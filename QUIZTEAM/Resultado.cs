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
        private Rectangle _zonaMenu, _zonaSalir;

        public Resultado(string categoria, int correctas, int total, List<PlayerScore> ranking)
        {
            _categoria = categoria; _correctas = correctas; _total = total;
            _ranking = ranking ?? new List<PlayerScore>();
            this.DoubleBuffered = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(26, 26, 46);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int midX = this.ClientSize.Width / 2;
            int groundY = this.ClientSize.Height - 150;

            // Título
            using (Font f = new Font("Georgia", 26, FontStyle.Bold))
            using (SolidBrush br = new SolidBrush(Color.White))
                g.DrawString($"RESULTADOS: {_correctas}/{_total}", f, br, new RectangleF(0, 50, this.Width, 60), new StringFormat { Alignment = StringAlignment.Center });

            // Podio
            if (_ranking.Count >= 2) DibujarPilar(g, _ranking[1], midX - 220, groundY, 160, "2°", Color.Silver);
            if (_ranking.Count >= 1) DibujarPilar(g, _ranking[0], midX - 90, groundY, 240, "1°", Color.Gold);
            if (_ranking.Count >= 3) DibujarPilar(g, _ranking[2], midX + 130, groundY, 100, "3°", Color.Chocolate);

            // Botones
            _zonaMenu = new Rectangle(midX - 110, this.Height - 80, 220, 45);
            Juego.DrawRoundRect(g, _zonaMenu, 10, Color.FromArgb(233, 69, 96), Color.Transparent);
            using (Font f = new Font("Georgia", 12, FontStyle.Bold))
                g.DrawString("VOLVER AL MENÚ", f, Brushes.White, _zonaMenu, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        }

        private void DibujarPilar(Graphics g, PlayerScore p, int x, int groundY, int alto, string rank, Color color)
        {
            Rectangle rect = new Rectangle(x, groundY - alto, 180, alto);
            using (LinearGradientBrush lgb = new LinearGradientBrush(rect, color, Color.FromArgb(20, 20, 40), 90f))
                g.FillRectangle(lgb, rect);
            g.DrawRectangle(new Pen(color, 2), rect);

            using (Font f = new Font("Segoe UI", 12, FontStyle.Bold))
            using (SolidBrush br = new SolidBrush(Color.White))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString(p.nombre, f, br, new RectangleF(x, rect.Y - 50, 180, 25), sf);
                g.DrawString($"{p.puntos} PTS", f, br, new RectangleF(x, rect.Y - 25, 180, 20), sf);
                g.DrawString(rank, new Font("Impact", 25), br, new RectangleF(x, rect.Y + 10, 180, 50), sf);
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (_zonaMenu.Contains(e.Location)) this.Close();
        }
    }
}