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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void BotComenzar_Click(object sender, EventArgs e)
        {
            // 1. Creamos la instancia de la nueva ventana
            Categorias ventanaCategorias = new Categorias();

            // 2. La mostramos
            ventanaCategorias.Show();

            // 3. Ocultamos la ventana actual (la principal)
            this.Hide();
        }

        private void BotSalir_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
