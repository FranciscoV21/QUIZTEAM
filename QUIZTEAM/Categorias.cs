using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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




    }
}
