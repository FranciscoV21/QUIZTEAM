using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QUIZTEAM
{
    public partial class Categorias : Form
    {

        private List<(string nombre, Rectangle zona)> _categorias = new List<(string, Rectangle)>();

        public Categorias()
        {
            this.DoubleBuffered = true;
            this.ClientSize = new Size(780, 500);
            this.Text = "Categorías";
            this.BackColor = Color.FromArgb(26, 26, 46);
            this.Load += new EventHandler(Categorias_Load);
        }

        private void Categorias_Load(object sender, EventArgs e)
        {
            CargarCategoriasDesdeBD();
        }

        private void CargarCategoriasDesdeBD()
        {
            _categorias.Clear();
            try
            {
                using (var conn = DB.GetConnection())
                {
                    conn.Open();
                    var cmd = new MySqlCommand("SELECT nombre FROM categorias", conn);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                        _categorias.Add((reader.GetString(0), Rectangle.Empty));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error BD: " + ex.Message);
            }
            CalcularZonas();
            this.Invalidate();
        }
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
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.Clear(Color.FromArgb(26, 26, 46));

            // Título
            using (Font fT = new Font("Georgia", 20, FontStyle.Bold))
            using (SolidBrush br = new SolidBrush(Color.FromArgb(234, 234, 234)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString("Elige una categoría", fT, br, new RectangleF(0, 28, 780, 40), sf);
            }

            // Línea
            using (Pen p = new Pen(Color.FromArgb(233, 69, 96), 2))
                g.DrawLine(p, 200, 72, 580, 72);

            // Tarjetas de categorías
            string[] iconos = { "★", "◉", "♪", "⬡", "⊕", "◈", "⌘", "◆" };
            for (int i = 0; i < _categorias.Count; i++)
            {
                var (nombre, rect) = _categorias[i];
                DrawTarjetaCategoria(g, rect, nombre, iconos[i % iconos.Length]);
            }

            // Botón volver
            var rectVolver = new Rectangle(30, 455, 100, 30);
            DrawBotonSimple(g, rectVolver, "← Volver");
        }

        private void DrawTarjetaCategoria(Graphics g, Rectangle r, string nombre, string icono)
        {
            DrawRoundRect(g, r, 10, Color.FromArgb(15, 33, 62), Color.FromArgb(233, 69, 96));

            using (Font fIco = new Font("Arial", 18, FontStyle.Bold))
            using (SolidBrush brIco = new SolidBrush(Color.FromArgb(233, 69, 96)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString(icono, fIco, brIco, new RectangleF(r.X, r.Y + 8, r.Width, 30), sf);
            }

            using (Font fNombre = new Font("Georgia", 12, FontStyle.Bold))
            using (SolidBrush brN = new SolidBrush(Color.FromArgb(234, 234, 234)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString(nombre, fNombre, brN, new RectangleF(r.X, r.Y + 44, r.Width, 24), sf);
            }

            using (Font fSub = new Font("Consolas", 9))
            using (SolidBrush brS = new SolidBrush(Color.FromArgb(136, 146, 164)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString("10 preguntas", fSub, brS, new RectangleF(r.X, r.Y + 68, r.Width, 18), sf);
            }
        }

        private void DrawBotonSimple(Graphics g, Rectangle r, string texto)
        {
            DrawRoundRect(g, r, 14, Color.Transparent, Color.FromArgb(85, 85, 85));
            using (Font f = new Font("Georgia", 11))
            using (SolidBrush br = new SolidBrush(Color.FromArgb(136, 146, 164)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(texto, f, br, r, sf);
            }
        }

        private void DrawRoundRect(Graphics g, Rectangle r, int radio, Color fill, Color borde)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(r.X, r.Y, radio * 2, radio * 2, 180, 90);
            path.AddArc(r.Right - radio * 2, r.Y, radio * 2, radio * 2, 270, 90);
            path.AddArc(r.Right - radio * 2, r.Bottom - radio * 2, radio * 2, radio * 2, 0, 90);
            path.AddArc(r.X, r.Bottom - radio * 2, radio * 2, radio * 2, 90, 90);
            path.CloseAllFigures();
            if (fill != Color.Transparent)
                using (SolidBrush br = new SolidBrush(fill)) g.FillPath(br, path);
            using (Pen p = new Pen(borde, 1.5f)) g.DrawPath(p, path);
        }


        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            foreach (var (nombre, rect) in _categorias)
            {
                if (rect.Contains(e.Location))
                {
                    var juego = new Juego(nombre);
                    juego.FormClosed += (s, args) => { this.Show(); };
                    juego.Show();
                    this.Hide();
                    return;
                }
            }
            if (new Rectangle(30, 455, 100, 30).Contains(e.Location))
            {
                this.Close();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            bool sobreCategoria = false;
            foreach (var (_, rect) in _categorias)
                if (rect.Contains(e.Location)) { sobreCategoria = true; break; }
            this.Cursor = sobreCategoria ? Cursors.Hand : Cursors.Default;
        }
    }
}
