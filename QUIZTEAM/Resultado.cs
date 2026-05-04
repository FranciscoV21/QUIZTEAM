using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace QUIZTEAM
{
    public partial class Resultado : Form
    {
        private string _categoria;
        private int _correctas, _total;
        private List<PlayerScore> _ranking;
        private Rectangle _zonaMenu;

        public Resultado(string categoria, int correctas, int total, List<PlayerScore> ranking)
        {
            _categoria = categoria;
            _correctas = correctas;
            _total = total;
            _ranking = ranking ?? new List<PlayerScore>();
            if (_ranking.Count > 5) _ranking = _ranking.GetRange(0, 5);

            this.DoubleBuffered = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(26, 26, 46);
            this.KeyPreview = true;

            // Calcular zona del botón AQUÍ, no en OnPaint
            this.Load += (s, e) => RecalcularZonas();
        }

        private void RecalcularZonas()
        {
            int W = this.ClientSize.Width;
            int H = this.ClientSize.Height;
            _zonaMenu = new Rectangle(W / 2 - 120, H - 70, 240, 48);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RecalcularZonas();
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int W = this.ClientSize.Width;
            int H = this.ClientSize.Height;
            int midX = W / 2;
            int groundY = H - 160;

            g.Clear(Color.FromArgb(26, 26, 46));

            // Título
            using (Font f = new Font("Georgia", 24, FontStyle.Bold))
            using (SolidBrush br = new SolidBrush(Color.White))
                g.DrawString($"PODIO — {_categoria}  |  Tu resultado: {_correctas}/{_total}",
                    f, br, new RectangleF(0, 30, W, 50),
                    new StringFormat { Alignment = StringAlignment.Center });

            // Podio
            DibujarPodio(g, midX, groundY);

            // Botón volver — usa _zonaMenu que ya está calculado
            Juego.DrawRoundRect(g, _zonaMenu, 24, Color.FromArgb(233, 69, 96), Color.Transparent);
            using (Font f = new Font("Georgia", 13, FontStyle.Bold))
            using (SolidBrush br = new SolidBrush(Color.White))
                g.DrawString("← VOLVER AL MENÚ", f, br, _zonaMenu,
                    new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        }

        private void DibujarPodio(Graphics g, int midX, int groundY)
        {
            var configs = new (int offsetX, int alto, string rank, Color color)[]
            {
                (0,    240, "1°", Color.Gold),
                (-220, 180, "2°", Color.Silver),
                (220,  130, "3°", Color.Chocolate),
                (-380, 100, "4°", Color.SteelBlue),
                (380,   80, "5°", Color.MediumPurple),
            };

            int count = Math.Min(_ranking.Count, 5);
            for (int i = 0; i < count; i++)
            {
                var (offsetX, alto, rank, color) = configs[i];
                DibujarPilar(g, _ranking[i], midX + offsetX - 90, groundY, alto, rank, color);
            }
        }

        private void DibujarPilar(Graphics g, PlayerScore p, int x, int groundY, int alto, string rank, Color color)
        {
            Rectangle rect = new Rectangle(x, groundY - alto, 180, alto);

            using (LinearGradientBrush lgb = new LinearGradientBrush(
                rect, color, Color.FromArgb(20, 20, 40), 90f))
                g.FillRectangle(lgb, rect);
            g.DrawRectangle(new Pen(color, 2), rect);

            using (Font f = new Font("Segoe UI", 11, FontStyle.Bold))
            using (SolidBrush br = new SolidBrush(Color.White))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString(p.nombre, f, br, new RectangleF(x, rect.Y - 55, 180, 25), sf);
                g.DrawString($"{p.puntos} PTS", f, br, new RectangleF(x, rect.Y - 28, 180, 22), sf);
                g.DrawString($"{p.correctas} ✓", new Font("Segoe UI", 9), br,
                    new RectangleF(x, rect.Y - 10, 180, 18), sf);
                g.DrawString(rank, new Font("Impact", 28), br,
                    new RectangleF(x, rect.Y + 8, 180, 55), sf);
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (_zonaMenu.Contains(e.Location))
            {
                var cats = new Categorias();
                cats.Show();
                this.Close();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            this.Cursor = _zonaMenu.Contains(e.Location) ? Cursors.Hand : Cursors.Default;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Escape)
            {
                var cats = new Categorias();
                cats.Show();
                this.Close();
            }
        }
    }
}