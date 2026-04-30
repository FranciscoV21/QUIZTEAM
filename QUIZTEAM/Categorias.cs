using System;

using System.Collections.Generic;

using System.Drawing;

using System.Drawing.Drawing2D;

using System.Net.Http;

using System.Text.Json;

using System.Windows.Forms;

namespace QUIZTEAM

{

    public partial class Categorias : Form

    {

        private List<(string nombre, Rectangle zona)> _categorias =

            new List<(string, Rectangle)>();

        private HttpClient client = new HttpClient();

        private string apiUrl = Config.ApiUrl;
        public Categorias()

        {

            InitializeComponent();

            this.DoubleBuffered = true;

            this.ClientSize = new Size(780, 500);

            this.Text = "Categorías";

            this.BackColor = Color.FromArgb(26, 26, 46);

            this.Load += async (s, e) => await CargarCategoriasDesdeAPI();

        }

        // =========================

        // API CALL

        // =========================

        private async System.Threading.Tasks.Task CargarCategoriasDesdeAPI()

        {

            _categorias.Clear();

            try

            {

                string json = await client.GetStringAsync($"{apiUrl}/categorias");

                var categorias = JsonSerializer.Deserialize<List<Categoria>>(json);

                if (categorias != null)

                {

                    foreach (var c in categorias)

                    {

                        _categorias.Add((c.nombre, Rectangle.Empty));

                    }

                }

            }

            catch (Exception ex)

            {

                MessageBox.Show("Error API: " + ex.Message);

            }

            CalcularZonas();

            this.Invalidate();

        }

        // =========================

        // UI

        // =========================

        private void CalcularZonas()

        {

            int cols = 4, cw = 160, ch = 90, gx = 20, gy = 16;

            int startX = (780 - (cols * cw + (cols - 1) * gx)) / 2;

            int startY = 100;

            for (int i = 0; i < _categorias.Count; i++)

            {

                int col = i % cols, row = i / cols;

                int x = startX + col * (cw + gx);

                int y = startY + row * (ch + gy);

                var (nombre, _) = _categorias[i];

                _categorias[i] = (nombre, new Rectangle(x, y, cw, ch));

            }

        }

        protected override void OnPaint(PaintEventArgs e)

        {

            base.OnPaint(e);

            Graphics g = e.Graphics;

            g.SmoothingMode = SmoothingMode.AntiAlias;

            g.Clear(Color.FromArgb(26, 26, 46));

            using (Font fT = new Font("Georgia", 20, FontStyle.Bold))

            using (SolidBrush br = new SolidBrush(Color.White))

            {

                g.DrawString("Elige una categoría",

                    fT, br,

                    new RectangleF(0, 20, 780, 40),

                    new StringFormat { Alignment = StringAlignment.Center });

            }

            string[] iconos = { "★", "◉", "♪", "⬡", "⊕", "◈", "⌘", "◆" };

            for (int i = 0; i < _categorias.Count; i++)

            {

                var (nombre, rect) = _categorias[i];

                DrawCard(g, rect, nombre, iconos[i % iconos.Length]);

            }

        }

        private void DrawCard(Graphics g, Rectangle r, string nombre, string icono)

        {

            GraphicsPath path = new GraphicsPath();

            int radius = 10;

            path.AddArc(r.X, r.Y, radius * 2, radius * 2, 180, 90);

            path.AddArc(r.Right - radius * 2, r.Y, radius * 2, radius * 2, 270, 90);

            path.AddArc(r.Right - radius * 2, r.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);

            path.AddArc(r.X, r.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);

            path.CloseAllFigures();

            using (SolidBrush b = new SolidBrush(Color.FromArgb(15, 33, 62)))

                g.FillPath(b, path);

            using (Pen p = new Pen(Color.FromArgb(233, 69, 96), 2))

                g.DrawPath(p, path);

            using (Font f = new Font("Arial", 16, FontStyle.Bold))

            using (SolidBrush br = new SolidBrush(Color.White))

            {

                g.DrawString(icono, f, br, r.X + 60, r.Y + 10);

            }

            using (Font f = new Font("Georgia", 11, FontStyle.Bold))

            using (SolidBrush br = new SolidBrush(Color.White))

            {

                g.DrawString(nombre, f, br, r.X + 20, r.Y + 50);

            }

        }

        protected override void OnMouseClick(MouseEventArgs e)

        {

            base.OnMouseClick(e);

            foreach (var (nombre, rect) in _categorias)

            {

                if (rect.Contains(e.Location))

                {

                    MessageBox.Show("Seleccionaste: " + nombre);

                    var juego = new Juego(nombre);

                    juego.Show();

                    this.Hide();

                    return;

                }

            }

        }

    }

    // =========================

    // MODELO API

    // =========================

    public class Categoria

    {

        public int id { get; set; }

        public string nombre { get; set; }

    }

}
