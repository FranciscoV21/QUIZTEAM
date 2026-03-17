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



    }
}
