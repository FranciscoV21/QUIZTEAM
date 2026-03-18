using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace QUIZTEAM
{
    public partial class Resultado : Form
    {
        private string _categoria;
        private int _correctas, _total;
        private Rectangle _zonaJugarOtro, _zonaCategorias;

        public Resultado(string categoria, int correctas, int total)
        {
            _categoria = categoria;
            _correctas = correctas;
            _total = total;
            this.DoubleBuffered = true;
            this.ClientSize = new Size(780, 480);
            this.Text = "QUIZTEAM — Resultado";
            this.BackColor = Color.FromArgb(26, 26, 46);
            _zonaJugarOtro = new Rectangle(160, 370, 190, 42);
            _zonaCategorias = new Rectangle(430, 370, 190, 42);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.Clear(Color.FromArgb(26, 26, 46));

            // Header
            using (Font f = new Font("Consolas", 10))
            using (SolidBrush br = new SolidBrush(Color.FromArgb(233, 69, 96)))
                g.DrawString("▶ QUIZTEAM  /  Resultado final", f, br, 30, 18);

            // Círculo de score
            double pct = (double)_correctas / _total;
            Rectangle circleRect = new Rectangle(290, 50, 200, 200);
            // Fondo círculo
            using (Pen p = new Pen(Color.FromArgb(40, 40, 60), 12))
                g.DrawEllipse(p, circleRect);
            // Arco de progreso
            Color colorArco = pct >= 0.7 ? Color.FromArgb(245, 166, 35)
                            : pct >= 0.5 ? Color.FromArgb(39, 174, 96)
                                         : Color.FromArgb(233, 69, 96);
            using (Pen p = new Pen(colorArco, 12))
            {
                p.StartCap = LineCap.Round;
                p.EndCap = LineCap.Round;
                g.DrawArc(p, circleRect, -90, (float)(360 * pct));
            }

            // Texto score
            using (Font fBig = new Font("Georgia", 36, FontStyle.Bold))
            using (SolidBrush br = new SolidBrush(Color.FromArgb(234, 234, 234)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString($"{_correctas}/{_total}", fBig, br,
                    new RectangleF(290, 50, 200, 200), sf);
            }
            using (Font fSub = new Font("Consolas", 10))
            using (SolidBrush br = new SolidBrush(Color.FromArgb(136, 146, 164)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString("preguntas correctas", fSub, br, new RectangleF(0, 258, 780, 20), sf);
            }

            // Mensaje motivador
            string msg = pct >= 0.9 ? "¡Excelente! 🏆"
                       : pct >= 0.7 ? "¡Buen trabajo!"
                       : pct >= 0.5 ? "Puedes mejorar"
                                     : "¡Sigue practicando!";
            using (Font fMsg = new Font("Georgia", 20, FontStyle.Bold))
            using (SolidBrush br = new SolidBrush(colorArco))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString(msg, fMsg, br, new RectangleF(0, 292, 780, 32), sf);
            }

            // Detalle
            using (Font fD = new Font("Georgia", 12))
            using (SolidBrush br = new SolidBrush(Color.FromArgb(136, 146, 164)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString($"Categoría: {_categoria}   •   {_incorrectas} incorrectas",
                    fD, br, new RectangleF(0, 330, 780, 24), sf);
            }

            // Botones
            DrawRoundRect(g, _zonaJugarOtro, 21, Color.FromArgb(15, 52, 96),
                Color.FromArgb(233, 69, 96));
            using (Font f = new Font("Georgia", 12, FontStyle.Bold))
            using (SolidBrush br = new SolidBrush(Color.FromArgb(234, 234, 234)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("Jugar de nuevo", f, br, _zonaJugarOtro, sf);
            }

            DrawRoundRect(g, _zonaCategorias, 21, Color.FromArgb(233, 69, 96),
                Color.Transparent);
            using (Font f = new Font("Georgia", 12, FontStyle.Bold))
            using (SolidBrush br = new SolidBrush(Color.White))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("Ver categorías", f, br, _zonaCategorias, sf);
            }
        }

        private int _incorrectas => _total - _correctas;

        private void DrawRoundRect(Graphics g, Rectangle r, int radio, Color fill, Color borde)
        {
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, radio * 2, radio * 2, 180, 90);
            path.AddArc(r.Right - radio * 2, r.Y, radio * 2, radio * 2, 270, 90);
            path.AddArc(r.Right - radio * 2, r.Bottom - radio * 2, radio * 2, radio * 2, 0, 90);
            path.AddArc(r.X, r.Bottom - radio * 2, radio * 2, radio * 2, 90, 90);
            path.CloseAllFigures();
            if (fill != Color.Transparent)
                using (SolidBrush br = new SolidBrush(fill)) g.FillPath(br, path);
            if (borde != Color.Transparent)
                using (Pen p = new Pen(borde, 1.5f)) g.DrawPath(p, path);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (_zonaJugarOtro.Contains(e.Location))
            {
                var juego = new Juego(_categoria);
                juego.Show();
                this.Close();
            }
            else if (_zonaCategorias.Contains(e.Location))
                this.Close();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            this.Cursor = (_zonaJugarOtro.Contains(e.Location) ||
                           _zonaCategorias.Contains(e.Location))
                ? Cursors.Hand : Cursors.Default;
        }
    }
}