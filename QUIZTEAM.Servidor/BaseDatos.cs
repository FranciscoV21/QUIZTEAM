using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace QUIZTEAM.Servidor
{
    public static class BaseDatos
    {
        private const string ConnStr =
            "server=127.0.0.1;user=root;password=;database=quiz;";

        // ── Categorías ──────────────────────────────────────────────
        public static List<Categoria> ObtenerCategorias()
        {
            var lista = new List<Categoria>();
            using (var conn = new MySqlConnection(ConnStr))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(
                    "SELECT id, nombre FROM categorias", conn))
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

        // ── Preguntas ────────────────────────────────────────────────
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
                            lista.Add(new Pregunta
                            {
                                id = rd.GetInt32("id"),
                                texto = rd.GetString("texto"),
                                tipo = rd.GetString("tipo"),
                                correcta = rd.GetInt32("correcta"),
                                opcion1 = rd.IsDBNull(rd.GetOrdinal("opcion1")) ? "" : rd.GetString("opcion1"),
                                opcion2 = rd.IsDBNull(rd.GetOrdinal("opcion2")) ? "" : rd.GetString("opcion2"),
                                opcion3 = rd.IsDBNull(rd.GetOrdinal("opcion3")) ? "" : rd.GetString("opcion3"),
                                opcion4 = rd.IsDBNull(rd.GetOrdinal("opcion4")) ? "" : rd.GetString("opcion4"),
                                img1 = rd.IsDBNull(rd.GetOrdinal("img1")) ? "" : rd.GetString("img1"),
                                img2 = rd.IsDBNull(rd.GetOrdinal("img2")) ? "" : rd.GetString("img2"),
                                img3 = rd.IsDBNull(rd.GetOrdinal("img3")) ? "" : rd.GetString("img3"),
                                img4 = rd.IsDBNull(rd.GetOrdinal("img4")) ? "" : rd.GetString("img4"),
                            });
                }
            }
            return lista;
        }

        // ── Guardar partida ──────────────────────────────────────────
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

        // ── Verificar respuesta ──────────────────────────────────────
        public static bool VerificarRespuesta(int idPregunta, int respuesta)
        {
            using (var conn = new MySqlConnection(ConnStr))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(
                    "SELECT correcta FROM preguntas WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", idPregunta);
                    var result = cmd.ExecuteScalar();
                    if (result == null) return false;
                    return Convert.ToInt32(result) == respuesta;
                }
            }
        }
    }
}