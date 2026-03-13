using System;
using MySql.Data.MySqlClient;

namespace TestDB
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("===== TEST BASE DE DATOS =====");

            ProbarConexion();
            VerTablas();
            VerCategorias();

            InsertarPreguntasProgramacion();

            VerPreguntas();

            Console.WriteLine("\n✔ Fin de pruebas");
        }

        static void ProbarConexion()
        {
            try
            {
                using (MySqlConnection conexion = DB.GetConnection())
                {
                    conexion.Open();
                    Console.WriteLine("✅ Conectado a Railway");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error: " + ex.Message);
            }
        }

        static void VerTablas()
        {
            using (MySqlConnection conexion = DB.GetConnection())
            {
                conexion.Open();

                string query = "SHOW TABLES";

                MySqlCommand cmd = new MySqlCommand(query, conexion);
                MySqlDataReader reader = cmd.ExecuteReader();

                Console.WriteLine("\n📦 Tablas:");

                while (reader.Read())
                {
                    Console.WriteLine("- " + reader.GetString(0));
                }
            }
        }

        static void VerCategorias()
        {
            using (MySqlConnection conexion = DB.GetConnection())
            {
                conexion.Open();

                string query = "SELECT id, nombre FROM categorias";

                MySqlCommand cmd = new MySqlCommand(query, conexion);
                MySqlDataReader reader = cmd.ExecuteReader();

                Console.WriteLine("\n📚 Categorías:");

                while (reader.Read())
                {
                    Console.WriteLine(
                        reader.GetInt32("id") + " - " +
                        reader.GetString("nombre")
                    );
                }
            }
        }

        static void InsertarPreguntasProgramacion()
        {
            using (MySqlConnection conexion = DB.GetConnection())
            {
                conexion.Open();

                Console.WriteLine("\n🧠 Insertando preguntas de programación...");

                string[] preguntas =
                {
                    "¿Que es una variable en programación?",
                    "¿Que lenguaje usa .NET?",
                    "¿Que es un bucle?",
                    "¿Que hace un compilador?",
                    "¿Que es una función?",
                    "¿Que simbolo termina instrucciones en C#?",
                    "¿Que significa IDE?",
                    "¿Que es un array?",
                    "¿Que significa SQL?",
                    "¿Que es un objeto en programación?"
                };

                foreach (string pregunta in preguntas)
                {
                    string query = "INSERT INTO preguntas (texto_pregunta, imagen_nombre, categoria_id) VALUES (@texto, NULL, 1)";

                    MySqlCommand cmd = new MySqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@texto", pregunta);

                    cmd.ExecuteNonQuery();
                }

                Console.WriteLine("✅ 10 preguntas insertadas correctamente.");
            }
        }

        static void VerPreguntas()
        {
            using (MySqlConnection conexion = DB.GetConnection())
            {
                conexion.Open();

                string query = "SELECT id, texto_pregunta FROM preguntas";

                MySqlCommand cmd = new MySqlCommand(query, conexion);
                MySqlDataReader reader = cmd.ExecuteReader();

                Console.WriteLine("\n📖 Preguntas guardadas:");

                while (reader.Read())
                {
                    Console.WriteLine(
                        reader.GetInt32("id") + " - " +
                        reader.GetString("texto_pregunta")
                    );
                }
            }
        }
    }
}