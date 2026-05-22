using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QUIZTEAM
{
    public partial class Categorias : Form
    {
        private List<(string nombre, Rectangle zona)> _categorias =
            new List<(string, Rectangle)>();

        public Categorias()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(26, 26, 46);
            this.KeyPreview = true;
            this.Load += async (s, e) => await CargarCategorias();
        }

        private async Task CargarCategorias()
        {
            _categorias.Clear();
            ConexionServidor conn = null;
            try
            {
                conn = new ConexionServidor();
                var tcs = new TaskCompletionSource<List<Categoria>>();

                conn.OnMensaje += msg =>
                {
                    if (msg.GetProperty("type").GetString() == "categorias")
                    {
                        var lista = JsonSerializer.Deserialize<List<Categoria>>(
                            msg.GetProperty("data").GetRawText());
                        tcs.TrySetResult(lista ?? new List<Categoria>());
                    }
                };

                await conn.ConectarAsync();
                await conn.EnviarAsync(new { type = "get_categorias" });

                await Task.WhenAny(tcs.Task, Task.Delay(5000));

                if (tcs.Task.IsCompleted)
                    foreach (var c in tcs.Task.Result)
                        _categorias.Add((c.nombre, Rectangle.Empty));
                else
                    MessageBox.Show("El servidor no respondió. Verifica la conexión.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al conectar: " + ex.Message);
            }
            finally
            {
                conn?.Dispose();
            }

            CalcularZonas();
            if (!this.IsDisposed)
                this.Invoke((Action)(() => this.Invalidate()));
        }

        private void CalcularZonas()
        {
            int W = this.ClientSize.Width, H = this.ClientSize.Height;
            int cols = 4, cw = 200, ch = 110, gx = 30, gy = 20;
            int startX = (W - (cols * cw + (cols - 1) * gx)) / 2;
            int startY = (int)(H * 0.28);

            for (int i = 0; i < _categorias.Count; i++)
            {
                int col = i % cols, row = i / cols;
                int x = startX + col * (cw + gx);
                int y = startY + row * (ch + gy);
                var (nombre, _) = _categorias[i];
                _categorias[i] = (nombre, new Rectangle(x, y, cw, ch));
            }
        }

        protected override void OnResize(EventArgs e)
        { base.OnResize(e); CalcularZonas(); this.Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.Clear(Color.FromArgb(26, 26, 46));

            int W = this.ClientSize.Width, H = this.ClientSize.Height;

            using (Font fT = new Font("Georgia", 26, FontStyle.Bold))
            using (SolidBrush br = new SolidBrush(Color.White))
                g.DrawString("Elige una categoría", fT, br,
                    new RectangleF(0, (int)(H * 0.10), W, 50),
                    new StringFormat { Alignment = StringAlignment.Center });

            using (Pen p = new Pen(Color.FromArgb(233, 69, 96), 2))
                g.DrawLine(p, W * 0.25f, H * 0.20f, W * 0.75f, H * 0.20f);

            using (Font fS = new Font("Consolas", 11))
            using (SolidBrush br = new SolidBrush(Color.FromArgb(136, 146, 164)))
                g.DrawString("Selecciona un tema para comenzar", fS, br,
                    new RectangleF(0, (int)(H * 0.22), W, 24),
                    new StringFormat { Alignment = StringAlignment.Center });

            string[] iconos = { "★", "◉", "♪", "⬡", "⊕", "◈", "⌘", "◆" };
            for (int i = 0; i < _categorias.Count; i++)
            {
                var (nombre, rect) = _categorias[i];
                DrawCard(g, rect, nombre, iconos[i % iconos.Length]);
            }

            var zonaEsc = new Rectangle(30, H - 52, 120, 38);
            DrawRoundRect(g, zonaEsc, 17, Color.Transparent, Color.FromArgb(85, 85, 85));
            using (Font f = new Font("Georgia", 10))
            using (SolidBrush br = new SolidBrush(Color.Gray))
                g.DrawString("ESC - SALIR", f, br, zonaEsc,
                    new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        }

        private void DrawCard(Graphics g, Rectangle r, string nombre, string icono)
        {
            DrawRoundRect(g, r, 12, Color.FromArgb(15, 33, 62), Color.FromArgb(233, 69, 96));
            using (Font f = new Font("Arial", 20, FontStyle.Bold))
            using (SolidBrush br = new SolidBrush(Color.FromArgb(233, 69, 96)))
                g.DrawString(icono, f, br, new RectangleF(r.X, r.Y + 10, r.Width, 36),
                    new StringFormat { Alignment = StringAlignment.Center });
            using (Font f = new Font("Georgia", 12, FontStyle.Bold))
            using (SolidBrush br = new SolidBrush(Color.White))
                g.DrawString(nombre, f, br, new RectangleF(r.X, r.Y + 58, r.Width, 30),
                    new StringFormat { Alignment = StringAlignment.Center });
            using (Font f = new Font("Consolas", 9))
            using (SolidBrush br = new SolidBrush(Color.FromArgb(136, 146, 164)))
                g.DrawString("10 preguntas", f, br, new RectangleF(r.X, r.Y + 84, r.Width, 20),
                    new StringFormat { Alignment = StringAlignment.Center });
        }

        private void DrawRoundRect(Graphics g, Rectangle r, int radio, Color fill, Color borde)
        {
            if (r.Width <= 0 || r.Height <= 0) return;
            GraphicsPath path = new GraphicsPath();
            int d = radio * 2;
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseAllFigures();
            if (fill != Color.Transparent)
                using (SolidBrush br = new SolidBrush(fill)) g.FillPath(br, path);
            using (Pen p = new Pen(borde, 2)) g.DrawPath(p, path);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            foreach (var (nombre, rect) in _categorias)
                if (rect.Contains(e.Location)) { AbrirSalaEspera(nombre); return; }
        }

        private void AbrirSalaEspera(string categoria)
        {
            var espera = new Espera(categoria);
            espera.Show();
            this.Hide();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            bool sobre = false;
            foreach (var (_, rect) in _categorias)
                if (rect.Contains(e.Location)) { sobre = true; break; }
            this.Cursor = sobre ? Cursors.Hand : Cursors.Default;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        { base.OnKeyDown(e); if (e.KeyCode == Keys.Escape) this.Close(); }
    }

    public class Categoria
    {
        public int id { get; set; }
        public string nombre { get; set; }
    }
}