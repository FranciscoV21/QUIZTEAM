using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace QUIZTEAM.Servidor
{
    public static class BaseDatos
    {
        private const string ConnStr =
            "server=127.0.0.1;user=root;password=12345;database=quiz;";

        public static List<Categoria> ObtenerCategorias()
        {
            var lista = new List<Categoria>();
            using (var conn = new MySqlConnection(ConnStr))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT id, nombre FROM categorias", conn))
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                        lista.Add(new Categoria
                        {
                            id = rd.GetInt32("id"),
                            nombre = rd.GetString("nombre")
                        });
            }
            return lista;
        }

        public static List<Pregunta> ObtenerPreguntas(string categoria)
        {
            var lista = new List<Pregunta>();
            using (var conn = new MySqlConnection(ConnStr))
            {
                conn.Open();
                const string sql = @"
                    SELECT p.*
                    FROM preguntas p
                    JOIN categorias c ON p.categoria_id = c.id
                    WHERE c.nombre = @cat
                    ORDER BY RAND()
                    LIMIT 10";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@cat", categoria);
                    using (var rd = cmd.ExecuteReader())
                        while (rd.Read())
                        {
                            string Get(string col) =>
                                rd.IsDBNull(rd.GetOrdinal(col)) ? "" : rd.GetString(col);

                            lista.Add(new Pregunta
                            {
                                id = rd.GetInt32("id"),
                                texto = rd.GetString("texto"),
                                tipo = rd.GetString("tipo"),
                                correcta = rd.GetInt32("correcta"),
                                opcion1 = Get("opcion1"),
                                opcion2 = Get("opcion2"),
                                opcion3 = Get("opcion3"),
                                opcion4 = Get("opcion4"),
                                img1 = Get("img1"),
                                img2 = Get("img2"),
                                img3 = Get("img3"),
                                img4 = Get("img4"),
                            });
                        }
                }
            }
            return lista;
        }

        public static void GuardarPartida(string categoria, int correctas, int total)
        {
            using (var conn = new MySqlConnection(ConnStr))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(
                    "INSERT INTO partidas (categoria, correctas, total) VALUES (@c,@ok,@t)", conn))
                {
                    cmd.Parameters.AddWithValue("@c", categoria);
                    cmd.Parameters.AddWithValue("@ok", correctas);
                    cmd.Parameters.AddWithValue("@t", total);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}