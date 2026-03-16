using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace QUIZTEAM
{
    public partial class Form1 : Form
    {
        private Image _imgBienvenida;
        private Rectangle _zonaComenzon;
        private Rectangle _zonaSalir;

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.ClientSize = new Size(780, 500);
            this.Text = "QUIZTEAM";
            this.BackColor = Color.FromArgb(26, 26, 46);

            _zonaComenzon = new Rectangle(160, 355, 460, 44);
            _zonaSalir = new Rectangle(280, 412, 220, 36);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string ruta = Path.Combine(Application.StartupPath, "Imagenes", "bienvenida.png");
            if (File.Exists(ruta))
                _imgBienvenida = Image.FromFile(ruta);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Fondo
            g.Clear(Color.FromArgb(26, 26, 46));

            // Área imagen
            Rectangle rectImg = new Rectangle(80, 50, 620, 220);
            DrawRoundRect(g, rectImg, 12, Color.FromArgb(15, 33, 62), Color.FromArgb(233, 69, 96));
            if (_imgBienvenida != null)
                g.DrawImage(_imgBienvenida, rectImg);

            // Título
            using (Font fTitulo = new Font("Georgia", 22, FontStyle.Bold))
            using (SolidBrush brTitulo = new SolidBrush(Color.FromArgb(234, 234, 234)))
            {
                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString("BIENVENIDO A NUESTRO QUIZ", fTitulo, brTitulo,
                    new RectangleF(0, 292, 780, 40), sf);
            }

            // Línea decorativa
            using (Pen p = new Pen(Color.FromArgb(233, 69, 96), 1.5f))
                g.DrawLine(p, 180, 340, 600, 340);

            // Botón Comenzar
            DrawBoton(g, _zonaComenzon, "CLICK AQUÍ PARA COMENZAR",
                Color.FromArgb(15, 52, 96), Color.FromArgb(233, 69, 96),
                Color.FromArgb(234, 234, 234));

            // Botón Salir
            DrawBoton(g, _zonaSalir, "SALIR",
                Color.Transparent, Color.FromArgb(85, 85, 85),
                Color.FromArgb(136, 146, 164));
        }

        private void DrawBoton(Graphics g, Rectangle r, string texto,
            Color fill, Color borde, Color colorTexto)
        {
            using (SolidBrush br = new SolidBrush(fill))
            using (Pen p = new Pen(borde, 1.5f))
            using (Font f = new Font("Georgia", 13, FontStyle.Bold))
            using (SolidBrush brT = new SolidBrush(colorTexto))
            {
                DrawRoundRect(g, r, 22, fill, borde);
                StringFormat sf = new StringFormat
                { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(texto, f, brT, r, sf);
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

            using (SolidBrush br = new SolidBrush(fill))
                g.FillPath(br, path);
            using (Pen p = new Pen(borde, 1.5f))
                g.DrawPath(p, path);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (_zonaComenzon.Contains(e.Location))
            {
                var cats = new Categorias();
                cats.FormClosed += (s, args) => this.Show();
                cats.Show();
                this.Hide();
            }
            else if (_zonaSalir.Contains(e.Location))
            {
                Application.Exit();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            this.Cursor = (_zonaComenzon.Contains(e.Location) || _zonaSalir.Contains(e.Location))
                ? Cursors.Hand : Cursors.Default;
        }
    }
}