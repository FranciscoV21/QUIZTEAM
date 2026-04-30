using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QUIZTEAM
{
    public partial class Juego : Form
    {
        private string _categoria;
        private List<Pregunta> _preguntas = new List<Pregunta>();
        private int _indiceActual = 0, _correctas = 0, _incorrectas = 0, _seleccion = -1;
        private bool _respondida = false;

        private Rectangle[] _zonasOpciones = new Rectangle[4];
        private Rectangle _zonaSiguiente, _zonaSalir;
        private List<Image> _imagenesOpciones = new List<Image>();
        private static readonly HttpClient client = new HttpClient();

        public Juego(string categoria)
        {
            _categoria = categoria;
            this.DoubleBuffered = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(26, 26, 46);

            this.Load += async (s, e) => await InicializarJuego();
        }

        private async Task InicializarJuego()
        {
            await CargarPreguntasAPI();
            CalcularZonas();
            CargarImagenesActual();
            this.Invalidate();
        }

        private async Task CargarPreguntasAPI()
        {
            try
            {
                string url = $"{Config.ApiUrl}/preguntas/{_categoria}";
                string response = await client.GetStringAsync(url);
                _preguntas = JsonSerializer.Deserialize<List<Pregunta>>(response);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error de conexión: " + ex.Message);
                this.Close();
            }
        }

        private void CalcularZonas()
        {
            int W = this.ClientSize.Width, H = this.ClientSize.Height;
            int ox = 40, gx = 28, gy = 20, headerH = 230, btnH = 60;
            int oh = (H - headerH - btnH - gy) / 2;
            int ow = (W - ox * 2 - gx) / 2;

            _zonasOpciones[0] = new Rectangle(ox, headerH, ow, oh);
            _zonasOpciones[1] = new Rectangle(ox + ow + gx, headerH, ow, oh);
            _zonasOpciones[2] = new Rectangle(ox, headerH + oh + gy, ow, oh);
            _zonasOpciones[3] = new Rectangle(ox + ow + gx, headerH + oh + gy, ow, oh);

            _zonaSiguiente = new Rectangle((W - 300) / 2, H - 55, 300, 44);
            _zonaSalir = new Rectangle(ox, H - 52, 110, 40);
        }

        private void CargarImagenesActual()
        {
            foreach (var img in _imagenesOpciones) img?.Dispose();
            _imagenesOpciones.Clear();
            if (_indiceActual >= _preguntas.Count) return;

            var p = _preguntas[_indiceActual];
            if (p.tipo == "imagen" && p.imagenesOpciones != null)
            {
                foreach (var ruta in p.imagenesOpciones)
                {
                    string path = Path.Combine(Application.StartupPath, "Imagenes", ruta);
                    _imagenesOpciones.Add(File.Exists(path) ? Image.FromFile(path) : null);
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_preguntas == null || _preguntas.Count == 0) return;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var p = _preguntas[_indiceActual];

            // 1. Header
            using (Font f = new Font("Consolas", 10))
            using (SolidBrush br = new SolidBrush(Color.FromArgb(233, 69, 96)))
                g.DrawString($"▶ QUIZTEAM / {_categoria} - {(_indiceActual + 1)} de {_preguntas.Count}", f, br, 30, 20);

            // 2. Progreso
            int barW = this.ClientSize.Width - 60;
            DrawRoundRect(g, new Rectangle(30, 45, barW, 8), 4, Color.FromArgb(51, 51, 68), Color.Transparent);
            int progreso = (int)(barW * (_indiceActual + 1.0) / _preguntas.Count);
            DrawRoundRect(g, new Rectangle(30, 45, progreso, 8), 4, Color.FromArgb(233, 69, 96), Color.Transparent);

            // 3. Pregunta
            DrawRoundRect(g, new Rectangle(30, 65, barW, 130), 10, Color.FromArgb(22, 33, 62), Color.FromArgb(233, 69, 96));
            using (Font f = new Font("Georgia", 16, FontStyle.Bold))
            using (SolidBrush br = new SolidBrush(Color.White))
                g.DrawString(p.texto, f, br, new RectangleF(50, 65, barW - 40, 130), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

            // 4. Opciones
            if (p.tipo == "imagen") DibujarOpcionesImagen(g, p);
            else DibujarOpcionesTexto(g, p);

            // 5. Botones Control
            if (_respondida)
            {
                DrawRoundRect(g, _zonaSiguiente, 20, Color.FromArgb(233, 69, 96), Color.Transparent);
                using (Font f = new Font("Georgia", 12, FontStyle.Bold))
                using (SolidBrush br = new SolidBrush(Color.White))
                    g.DrawString(_indiceActual == _preguntas.Count - 1 ? "VER PODIO ▶" : "SIGUIENTE →", f, br, _zonaSiguiente, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            }

            DrawRoundRect(g, _zonaSalir, 17, Color.Transparent, Color.FromArgb(85, 85, 85));
            using (Font f = new Font("Georgia", 10))
            using (SolidBrush br = new SolidBrush(Color.Gray))
                g.DrawString("ESC - SALIR", f, br, _zonaSalir, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        }

        private void DibujarOpcionesTexto(Graphics g, Pregunta p)
        {
            for (int i = 0; i < 4; i++)
            {
                var r = _zonasOpciones[i];
                Color fill = Color.FromArgb(15, 52, 96), borde = Color.FromArgb(51, 51, 85);

                if (_respondida)
                {
                    if (i == p.correcta - 1) { fill = Color.FromArgb(27, 77, 46); borde = Color.FromArgb(39, 174, 96); }
                    else if (i == _seleccion) { fill = Color.FromArgb(77, 26, 26); borde = Color.FromArgb(233, 69, 96); }
                }
                else if (i == _seleccion) borde = Color.FromArgb(233, 69, 96);

                DrawRoundRect(g, r, 10, fill, borde);
                using (Font f = new Font("Georgia", 12))
                using (SolidBrush br = new SolidBrush(Color.White))
                    g.DrawString(p.opciones[i], f, br, r, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            }
        }

        private void DibujarOpcionesImagen(Graphics g, Pregunta p)
        {
            for (int i = 0; i < 4; i++)
            {
                var r = _zonasOpciones[i];
                Color borde = (i == _seleccion) ? Color.FromArgb(233, 69, 96) : Color.FromArgb(51, 51, 85);
                if (_respondida && i == p.correcta - 1) borde = Color.FromArgb(39, 174, 96);

                DrawRoundRect(g, r, 10, Color.FromArgb(15, 33, 62), borde);
                if (i < _imagenesOpciones.Count && _imagenesOpciones[i] != null)
                {
                    Rectangle imgRect = new Rectangle(r.X + 8, r.Y + 8, r.Width - 16, r.Height - 16);
                    g.DrawImage(_imagenesOpciones[i], imgRect);
                }
            }
        }

        public static void DrawRoundRect(Graphics g, Rectangle r, int radio, Color fill, Color borde)
        {
            if (r.Width <= 0 || r.Height <= 0) return;
            GraphicsPath path = new GraphicsPath();
            int d = radio * 2;
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();

            if (fill != Color.Transparent) using (SolidBrush br = new SolidBrush(fill)) g.FillPath(br, path);
            if (borde != Color.Transparent) using (Pen p = new Pen(borde, 2)) g.DrawPath(p, path);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (_zonaSalir.Contains(e.Location)) this.Close();
            if (_respondida && _zonaSiguiente.Contains(e.Location))
            {
                if (_indiceActual == _preguntas.Count - 1) FinalizarJuego();
                else { _indiceActual++; _seleccion = -1; _respondida = false; CargarImagenesActual(); this.Invalidate(); }
            }
            else if (!_respondida)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (_zonasOpciones[i].Contains(e.Location))
                    {
                        _seleccion = i; _respondida = true;
                        if (i == _preguntas[_indiceActual].correcta - 1) _correctas++;
                        else _incorrectas++;
                        this.Invalidate(); break;
                    }
                }
            }
        }

        private async void FinalizarJuego()
        {
            try
            {
                var resAPI = await client.PostAsync($"{Config.ApiUrl}/finalizar",
                    new StringContent(
                        JsonSerializer.Serialize(new { nombre = "Usuario", correctas = _correctas }),
                        System.Text.Encoding.UTF8,
                        "application/json"
                    ));

                var ranking = JsonSerializer.Deserialize<List<PlayerScore>>(
                    await resAPI.Content.ReadAsStringAsync()
                );

                var formRes = new Resultado(_categoria, _correctas, _preguntas.Count, ranking);
                formRes.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo obtener el podio: " + ex.Message);
                this.Close();
            }
        }
    }
}