using System;
using MySql.Data.MySqlClient;

namespace QUIZTEAM
{
    public class TestDB
    {
        public static void RunTests()
        {
            Console.WriteLine("===== TEST BASE DE DATOS =====");

            ProbarConexion();
            VerTablas();
            VerCategorias();

            Console.WriteLine("\n✔ Fin de pruebas");
        }

        public static void ProbarConexion()
        {
            try
            {
                using (MySqlConnection conexion = DB.GetConnection())
                {
                    conexion.Open();
                    Console.WriteLine("✅ Conectado a la base de datos");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error: " + ex.Message);
            }
        }

        public static void VerTablas()
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

        public static void VerCategorias()
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
    }
}