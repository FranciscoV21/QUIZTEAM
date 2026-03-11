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
        public Categorias()
        {
            InitializeComponent();
        }
        private void Categorias_Load(object sender, EventArgs e)
        {
            CargarCategoriasDesdeBD();
        }
        private void CargarCategoriasDesdeBD()
        {
            // 1. Usamos tu clase DB para obtener la conexión
            using (MySqlConnection conexion = DB.GetConnection())
            {
                try
                {
                    conexion.Open();
                    // 2. Traemos el ID y el Nombre de tus categorías en Railway
                    string query = "SELECT id, nombre FROM categorias";
                    MySqlCommand cmd = new MySqlCommand(query, conexion);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    // Limpiamos el panel por si acaso
                    flowLayoutPanel1.Controls.Clear();

                    while (reader.Read())
                    {
                        // 3. Creamos un botón "en el aire" por cada fila de la BD
                        Button btn = new Button();
                        btn.Text = reader["nombre"].ToString().ToUpper();
                        btn.Name = "btnCat_" + reader["id"].ToString();
                        btn.Tag = reader["id"]; // Guardamos el ID aquí para saber cuál eligió

                        // 4. Diseño del botón (puedes ajustarlo a tu gusto)
                        btn.Size = new Size(180, 60);
                        btn.BackColor = Color.WhiteSmoke;
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.Font = new Font("Arial", 10, FontStyle.Bold);

                        // 5. Le asignamos un evento de "Clic"
                        btn.Click += Boton_Click;

                        // 6. ¡Lo metemos al FlowLayoutPanel!
                        flowLayoutPanel1.Controls.Add(btn);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar categorías: " + ex.Message);
                }
            }
        }
        // Este método se ejecuta cuando hagas clic en CUALQUIER botón generado
        private void Boton_Click(object sender, EventArgs e)
        {
            Button botonPresionado = (Button)sender;
            int idCategoria = Convert.ToInt32(botonPresionado.Tag);

            MessageBox.Show("Elegiste: " + botonPresionado.Text + " (ID: " + idCategoria + ")");

            // Aquí es donde luego llamaremos al FormJuego pasando el ID
        }
    }
}
