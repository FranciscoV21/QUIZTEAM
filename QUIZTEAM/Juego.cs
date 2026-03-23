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


    }