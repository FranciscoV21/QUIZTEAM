using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QUIZTEAM
{
    //Hola
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            string nombreArchivo = "bienvenida.png";
            string carpeta = Path.Combine(Application.StartupPath, "Imagenes");
            string rutaConfigurada = Path.Combine(carpeta, nombreArchivo);

            // 1. ¿Existe la carpeta Imagenes en el Debug?
            if (!Directory.Exists(carpeta))
            {
                MessageBox.Show("ERROR: La carpeta 'Imagenes' no existe en: " + carpeta);
                return;
            }

            // 2. ¿Existe el archivo ahí dentro?
            if (File.Exists(rutaConfigurada))
            {
                ImagBienvenida.Image = Image.FromFile(rutaConfigurada);
                ImagBienvenida.SizeMode = PictureBoxSizeMode.Zoom;
            }
            else
            {
                MessageBox.Show("ERROR: No veo el archivo '" + nombreArchivo + "' en: " + carpeta);
            }
        }

        private void BotComenzar_Click(object sender, EventArgs e)
        {
            // 1. Creamos la instancia de la nueva ventana
            Categorias ventanaCategorias = new Categorias();
            // 2. La mostramos
            ventanaCategorias.FormClosed += (s, args) => this.Show();

            ventanaCategorias.Show();
            // 3. Ocultamos la ventana actual (la principal)
            this.Hide();
        }

        private void BotSalir_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void ImagBienvenida_Click(object sender, EventArgs e)
        {

        }
    }
}
