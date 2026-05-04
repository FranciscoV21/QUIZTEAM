using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace QUIZTEAM
{
    public partial class Resultado : Form
    {
        private string _categoria;
        private int _correctas, _total;
        private List<PlayerScore> _ranking;

        public Resultado(string categoria, int correctas, int total, List<PlayerScore> ranking)
        {
            InitializeComponent();
            _categoria = categoria;
            _correctas = correctas;
            _total = total;
            // Ordenamos por puntos de mayor a menor por seguridad
            _ranking = ranking?.OrderByDescending(x => x.puntos).ToList() ?? new List<PlayerScore>();

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

            int W = this.ClientSize.Width;
            int H = this.ClientSize.Height;

            // 1. TÍTULO
            using (Font fTitulo = new Font("Georgia", 26, FontStyle.Bold))
            {
                string txt = $"FINALIZADO: {_categoria}";
                SizeF size = g.MeasureString(txt, fTitulo);
                g.DrawString(txt, fTitulo, Brushes.Crimson, (W - size.Width) / 2, 40);
            }

            // 2. DIBUJAR EL PODIO (TOP 3)
            DibujarPodio(g, W, H);

            // 3. DIBUJAR EL RESTO (DEL 4 EN ADELANTE)
            if (_ranking.Count > 3)
            {
                int startY = H / 2 + 100;
                for (int i = 3; i < Math.Min(_ranking.Count, 7); i++)
                {
                    Rectangle rect = new Rectangle((W - 500) / 2, startY + ((i - 3) * 55), 500, 45);
                    Juego.DrawRoundRect(g, rect, 10, Color.FromArgb(34, 40, 70), Color.FromArgb(85, 85, 85));

                    using (Font f = new Font("Segoe UI", 11, FontStyle.Bold))
                    {
                        g.DrawString($"{i + 1}. {_ranking[i].nombre}", f, Brushes.Silver, rect.X + 20, rect.Y + 12);
                        g.DrawString($"{_ranking[i].puntos} pts", f, Brushes.Gray, rect.Right - 100, rect.Y + 12);
                    }
                }
            }

            // 4. BOTÓN SALIR
            Rectangle btnSalir = new Rectangle((W - 200) / 2, H - 80, 200, 45);
            Juego.DrawRoundRect(g, btnSalir, 20, Color.FromArgb(233, 69, 96), Color.Transparent);
            using (Font fBtn = new Font("Segoe UI", 11, FontStyle.Bold))
                g.DrawString("CONTINUAR", fBtn, Brushes.White, btnSalir, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        }

        private void DibujarPodio(Graphics g, int W, int H)
        {
            int basePodio = H / 2 + 50;
            int anchoBloque = 160;
            int centroX = W / 2;

            // Definición de alturas de los bloques
            int alto1 = 220; // Oro
            int alto2 = 160; // Plata
            int alto3 = 110; // Bronce

            // 2DO LUGAR (Izquierda)
            if (_ranking.Count >= 2)
                DibujarBloquePodio(g, "2", _ranking[1], centroX - anchoBloque - 10, basePodio, anchoBloque, alto2, Color.Silver);

            // 3ER LUGAR (Derecha)
            if (_ranking.Count >= 3)
                DibujarBloquePodio(g, "3", _ranking[2], centroX + 10, basePodio, anchoBloque, alto3, Color.Chocolate);

            // 1ER LUGAR (Centro - Se dibuja al final para que resalte)
            if (_ranking.Count >= 1)
                DibujarBloquePodio(g, "1", _ranking[0], centroX - (anchoBloque / 2), basePodio, anchoBloque, alto1, Color.Gold);
        }

        private void DibujarBloquePodio(Graphics g, string rank, PlayerScore player, int x, int baseY, int w, int h, Color color)
        {
            Rectangle rectBloque = new Rectangle(x, baseY - h, w, h);

            // Dibujar el bloque físico
            Juego.DrawRoundRect(g, rectBloque, 15, Color.FromArgb(40, 45, 80), color);

            // Dibujar el número grande (1, 2 o 3)
            using (Font fNum = new Font("Impact", 40))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(100, color)))
                g.DrawString(rank, fNum, b, x + (w / 2) - 20, baseY - h + 10);

            // Dibujar Nombre sobre el bloque
            using (Font fNom = new Font("Segoe UI", 12, FontStyle.Bold))
            {
                string n = player.nombre.ToUpper();
                SizeF s = g.MeasureString(n, fNom);
                g.DrawString(n, fNom, Brushes.White, x + (w - s.Width) / 2, baseY - h - 60);
            }

            // Dibujar Puntos sobre el nombre
            using (Font fPts = new Font("Consolas", 11, FontStyle.Bold))
            {
                string p = $"{player.puntos} PTS";
                SizeF s = g.MeasureString(p, fPts);
                g.DrawString(p, fPts, new SolidBrush(color), x + (w - s.Width) / 2, baseY - h - 35);
            }

            // Corona o icono simple para el 1er lugar
            if (rank == "1")
            {
                g.DrawString("👑", new Font("Segoe UI", 24), Brushes.Gold, x + (w / 2) - 22, baseY - h - 110);
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            Rectangle btnSalir = new Rectangle((this.ClientSize.Width - 200) / 2, this.ClientSize.Height - 80, 200, 45);
            if (btnSalir.Contains(e.Location))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}