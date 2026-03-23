using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace QUIZTEAM
{
    public class Pregunta
    {
        public int Id { get; set; }
        public string Texto { get; set; }
        public string Tipo { get; set; } // "texto" o "imagen"
        public List<string> Opciones { get; set; } = new List<string>();
        public List<string> ImagenesOpciones { get; set; } = new List<string>();
        public int IndiceCorrecta { get; set; }
    }

    public class Juego : Form
    {
        private string _categoria;
        private List<Pregunta> _preguntas = new List<Pregunta>();
        private int _indiceActual = 0;
        private int _correctas = 0;
        private int _incorrectas = 0;
        private int _seleccion = -1;
        private bool _respondida = false;

        private Rectangle[] _zonasOpciones = new Rectangle[4];
        private Rectangle _zonaSiguiente;
        private Rectangle _zonaSalir;

        private List<Image> _imagenesOpciones = new List<Image>();

        public Juego(string categoria)
        {
            _categoria = categoria;
            this.DoubleBuffered = true;
            this.ClientSize = new Size(780, 520);
            this.Text = "QUIZTEAM — " + categoria;
            this.BackColor = Color.FromArgb(26, 26, 46);
            this.Load += Juego_Load;
        }

        private void Juego_Load(object sender, EventArgs e)
        {
            CargarPreguntas();
            CalcularZonas();
            CargarImagenesActual();
            this.Invalidate();
        }

        private void CargarPreguntas()
        {
            _preguntas.Clear();
            try
            {
                using (var conn = DB.GetConnection())
                {
                    conn.Open();
                    // Obtener id de categoría
                    int catId = 0;
                    var cmdCat = new MySqlCommand(
                        "SELECT id FROM categorias WHERE nombre=@n LIMIT 1", conn);
                    cmdCat.Parameters.AddWithValue("@n", _categoria);
                    var r = cmdCat.ExecuteReader();
                    if (r.Read()) catId = r.GetInt32(0);
                    r.Close();

                    // Obtener preguntas aleatorias (10)
                    var cmd = new MySqlCommand(
                        @"SELECT id, texto, tipo, opcion1, opcion2, opcion3, opcion4,
                          img1, img2, img3, img4, correcta
                          FROM preguntas WHERE categoria_id=@c
                          ORDER BY RAND() LIMIT 10", conn);
                    cmd.Parameters.AddWithValue("@c", catId);
                    var rd = cmd.ExecuteReader();
                    while (rd.Read())
                    {
                        var p = new Pregunta
                        {
                            Id = rd.GetInt32("id"),
                            Texto = rd.GetString("texto"),
                            Tipo = rd.GetString("tipo"),
                            IndiceCorrecta = rd.GetInt32("correcta") - 1
                        };
                        p.Opciones.Add(rd.GetString("opcion1"));
                        p.Opciones.Add(rd.GetString("opcion2"));
                        p.Opciones.Add(rd.GetString("opcion3"));
                        p.Opciones.Add(rd.GetString("opcion4"));
                        p.ImagenesOpciones.Add(rd.IsDBNull(rd.GetOrdinal("img1")) ? "" : rd.GetString("img1"));
                        p.ImagenesOpciones.Add(rd.IsDBNull(rd.GetOrdinal("img2")) ? "" : rd.GetString("img2"));
                        p.ImagenesOpciones.Add(rd.IsDBNull(rd.GetOrdinal("img3")) ? "" : rd.GetString("img3"));
                        p.ImagenesOpciones.Add(rd.IsDBNull(rd.GetOrdinal("img4")) ? "" : rd.GetString("img4"));
                        _preguntas.Add(p);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando preguntas: " + ex.Message);
            }
        }

        private void CalcularZonas()
        {
            // 2x2 grid de opciones
            int ox = 30, ow = 355, oh = 100, gx = 20, gy = 14, startY = 230;
            _zonasOpciones[0] = new Rectangle(ox, startY, ow, oh);
            _zonasOpciones[1] = new Rectangle(ox + ow + gx, startY, ow, oh);
            _zonasOpciones[2] = new Rectangle(ox, startY + oh + gy, ow, oh);
            _zonasOpciones[3] = new Rectangle(ox + ow + gx, startY + oh + gy, ow, oh);
            _zonaSiguiente = new Rectangle(270, 460, 240, 40);
            _zonaSalir = new Rectangle(30, 462, 90, 34);
        }

        private void CargarImagenesActual()
        {
            foreach (var img in _imagenesOpciones) img?.Dispose();
            _imagenesOpciones.Clear();
            if (_indiceActual >= _preguntas.Count) return;
            var p = _preguntas[_indiceActual];
            if (p.Tipo == "imagen")
            {
                foreach (var ruta in p.ImagenesOpciones)
                {
                    string fullPath = Path.Combine(Application.StartupPath, "Imagenes", ruta);
                    _imagenesOpciones.Add(File.Exists(fullPath) ? Image.FromFile(fullPath) : null);
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_preguntas.Count == 0) return;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.Clear(Color.FromArgb(26, 26, 46));

            var p = _preguntas[_indiceActual];

            // Header
            DibujarHeader(g, p);

            // Barra de progreso
            DibujarProgreso(g);

            // Tarjeta de pregunta
            DibujarPregunta(g, p);

            // Opciones
            if (p.Tipo == "imagen")
                DibujarOpcionesImagen(g, p);
            else
                DibujarOpcionesTexto(g, p);

            // Botón siguiente (solo si respondió)
            if (_respondida)
            {
                DrawRoundRect(g, _zonaSiguiente, 20,
                    Color.FromArgb(233, 69, 96), Color.Transparent);
                using (Font f = new Font("Georgia", 13, FontStyle.Bold))
                using (SolidBrush br = new SolidBrush(Color.White))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    string label = _indiceActual == _preguntas.Count - 1 ? "Ver resultado ▶" : "Siguiente →";
                    g.DrawString(label, f, br, _zonaSiguiente, sf);
                }
            }

            // Botón volver
            DrawRoundRect(g, _zonaSalir, 17, Color.Transparent, Color.FromArgb(85, 85, 85));
            using (Font f = new Font("Georgia", 11))
            using (SolidBrush br = new SolidBrush(Color.FromArgb(136, 146, 164)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("← Salir", f, br, _zonaSalir, sf);
            }
        }

        private void DibujarHeader(Graphics g, Pregunta p)
        {
            using (Font f = new Font("Consolas", 10))
            using (SolidBrush br = new SolidBrush(Color.FromArgb(233, 69, 96)))
                g.DrawString($"▶ QUIZTEAM  /  {_categoria}  —  Pregunta {_indiceActual + 1} de {_preguntas.Count}",
                    f, br, 30, 18);

            // Score
            string scoreText = $"✓ {_correctas}   ✗ {_incorrectas}";
            using (Font f = new Font("Consolas", 11, FontStyle.Bold))
            using (SolidBrush br = new SolidBrush(Color.FromArgb(245, 166, 35)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Far };
                g.DrawString(scoreText, f, br, new RectangleF(0, 14, 755, 22), sf);
            }
        }

        private void DibujarProgreso(Graphics g)
        {
            DrawRoundRect(g, new Rectangle(30, 42, 720, 8), 4,
                Color.FromArgb(51, 51, 68), Color.Transparent);
            int ancho = (int)(720.0 * (_indiceActual + 1) / _preguntas.Count);
            if (ancho > 0)
                DrawRoundRect(g, new Rectangle(30, 42, ancho, 8), 4,
                    Color.FromArgb(233, 69, 96), Color.Transparent);
        }

        private void DibujarPregunta(Graphics g, Pregunta p)
        {
            DrawRoundRect(g, new Rectangle(30, 60, 720, 110), 10,
                Color.FromArgb(22, 33, 62), Color.FromArgb(233, 69, 96));
            using (Font f = new Font("Georgia", 15, FontStyle.Bold))
            using (SolidBrush br = new SolidBrush(Color.FromArgb(234, 234, 234)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(p.Texto, f, br, new RectangleF(50, 60, 680, 110), sf);
            }
            // Badge tipo
            string badge = p.Tipo == "imagen" ? "  Selecciona la imagen correcta  " : "  Opción múltiple  ";
            using (Font f = new Font("Consolas", 9))
            using (SolidBrush br = new SolidBrush(Color.FromArgb(136, 146, 164)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString(badge, f, br, new RectangleF(0, 174, 780, 20), sf);
            }
        }
        private void DibujarOpcionesTexto(Graphics g, Pregunta p)
        {
            string[] letras = { "A", "B", "C", "D" };
            for (int i = 0; i < 4; i++)
            {
                var r = _zonasOpciones[i];
                Color fill = Color.FromArgb(15, 52, 96);
                Color borde = Color.FromArgb(51, 51, 85);
                Color colorTexto = Color.FromArgb(234, 234, 234);

                if (_respondida)
                {
                    if (i == p.IndiceCorrecta)
                    { fill = Color.FromArgb(27, 77, 46); borde = Color.FromArgb(39, 174, 96); }
                    else if (i == _seleccion)
                    { fill = Color.FromArgb(77, 26, 26); borde = Color.FromArgb(233, 69, 96); }
                }
                else if (i == _seleccion)
                {
                    fill = Color.FromArgb(15, 52, 110); borde = Color.FromArgb(233, 69, 96);
                }

                DrawRoundRect(g, r, 10, fill, borde);

                // Letra
                using (Font fL = new Font("Georgia", 13, FontStyle.Bold))
                using (SolidBrush brL = new SolidBrush(Color.FromArgb(233, 69, 96)))
                    g.DrawString(letras[i], fL, brL, r.X + 14, r.Y + (r.Height - 20) / 2);

                // Texto opción
                using (Font fO = new Font("Georgia", 13))
                using (SolidBrush brO = new SolidBrush(colorTexto))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString(p.Opciones[i], fO, brO,
                        new RectangleF(r.X + 34, r.Y, r.Width - 34, r.Height), sf);
                }

                // Icono correcto/incorrecto
                if (_respondida)
                {
                    string icono = i == p.IndiceCorrecta ? "✓" : (i == _seleccion ? "✗" : "");
                    if (icono != "")
                    {
                        Color cIco = i == p.IndiceCorrecta
                            ? Color.FromArgb(39, 174, 96) : Color.FromArgb(233, 69, 96);
                        using (Font fIco = new Font("Arial", 16, FontStyle.Bold))
                        using (SolidBrush brIco = new SolidBrush(cIco))
                            g.DrawString(icono, fIco, brIco, r.Right - 28, r.Y + (r.Height - 22) / 2);
                    }
                }
            }
        }
    }