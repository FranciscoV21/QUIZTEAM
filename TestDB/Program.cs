using System;
using MySql.Data.MySqlClient;

namespace TestDB
{
    class Program
    {
        static void Main(string[] args)
        {
            EjecutarScript();
            MostrarPreguntas();
        }

        static void EjecutarScript()
        {
            try
            {
                using (var conn = DB.GetConnection())
                {
                    conn.Open();

                    string sqlTables = @"
CREATE TABLE IF NOT EXISTS categorias (
  id INT AUTO_INCREMENT PRIMARY KEY,
  nombre VARCHAR(100) NOT NULL
);

CREATE TABLE IF NOT EXISTS preguntas (
  id INT AUTO_INCREMENT PRIMARY KEY,
  categoria_id INT NOT NULL,
  texto TEXT NOT NULL,
  tipo ENUM('texto','imagen') DEFAULT 'texto',
  opcion1 VARCHAR(200), opcion2 VARCHAR(200),
  opcion3 VARCHAR(200), opcion4 VARCHAR(200),
  img1 VARCHAR(200), img2 VARCHAR(200),
  img3 VARCHAR(200), img4 VARCHAR(200),
  correcta TINYINT NOT NULL,
  FOREIGN KEY (categoria_id) REFERENCES categorias(id)
);

CREATE TABLE IF NOT EXISTS partidas (
  id INT AUTO_INCREMENT PRIMARY KEY,
  categoria VARCHAR(100),
  correctas INT,
  total INT,
  fecha DATETIME DEFAULT NOW()
);
";
                    new MySqlCommand(sqlTables, conn).ExecuteNonQuery();

                    string sqlCategorias = @"
INSERT IGNORE INTO categorias (id, nombre) VALUES
(1,'Historia'),
(2,'Deporte'),
(3,'Música'),
(4,'Geografía'),
(5,'Arte');
";
                    new MySqlCommand(sqlCategorias, conn).ExecuteNonQuery();

                    // 🔥 SIEMPRE BORRA Y REINICIA
                    new MySqlCommand("DELETE FROM preguntas;", conn).ExecuteNonQuery();
                    new MySqlCommand("ALTER TABLE preguntas AUTO_INCREMENT = 1;", conn).ExecuteNonQuery();

                    string sqlTexto = @"INSERT INTO preguntas 
(categoria_id, texto, tipo, opcion1, opcion2, opcion3, opcion4, correcta) VALUES

(1,'¿En qué año llegó Cristóbal Colón a América?','texto','1392','1492','1592','1292',2),
(1,'¿Qué civilización construyó las pirámides de Giza?','texto','La romana','La griega','La egipcia','La mesopotámica',3),
(1,'¿Quién fue el primer presidente de los Estados Unidos?','texto','Abraham Lincoln','Thomas Jefferson','Benjamin Franklin','George Washington',4),
(1,'¿En qué año comenzó la Primera Guerra Mundial?','texto','1914','1918','1905','1939',1),
(1,'¿Qué muro separó a Alemania del Este y Oeste durante la Guerra Fría?','texto','El Muro de Adriano','La Gran Muralla','El Muro de Berlín','El Muro de Varsovia',3),

(2,'¿Cuántos jugadores tiene un equipo de fútbol en el campo?','texto','9','10','11','12',3),
(2,'¿En qué país se originó el béisbol?','texto','Cuba','México','Canadá','Estados Unidos',4),
(2,'¿Cada cuántos años se celebra el Mundial?','texto','2','4','6','3',2),

(3,'¿Cuántas cuerdas tiene una guitarra?','texto','4','5','7','6',4),
(3,'¿Qué banda fue Los Fab Four?','texto','Rolling Stones','The Beatles','Queen','U2',2),

(4,'¿Capital de Australia?','texto','Sídney','Melbourne','Canberra','Brisbane',3),
(4,'¿Río más largo?','texto','Amazonas','Nilo','Yangtsé','Mississippi',2),

(5,'¿Quién pintó la Mona Lisa?','texto','Miguel Ángel','Rafael','Leonardo da Vinci','Botticelli',3);
";
                    new MySqlCommand(sqlTexto, conn).ExecuteNonQuery();

                    // 🔥 IMÁGENES BIEN DEFINIDAS
                    string sqlImagen = @"INSERT INTO preguntas 
(categoria_id, texto, tipo, opcion1, opcion2, opcion3, opcion4, img1, img2, img3, img4, correcta) VALUES

(1,'¿Cuál imagen es la Torre Eiffel?','imagen',
'Torre Eiffel','Big Ben','Coliseo','Sagrada Familia',
'eiffel.jpg','bigben.jpg','coliseo.jpg','sagrada.jpg',1),

(2,'¿Quién es Usain Bolt?','imagen',
'Michael Phelps','Usain Bolt','Carl Lewis','Mo Farah',
'phelps.jpg','bolt.jpg','lewis.jpg','farah.jpg',2),

(3,'¿Cuál es el logo de Spotify?','imagen',
'Apple Music','YouTube Music','Spotify','Deezer',
'applemusic.jpg','ytmusic.jpg','spotify.jpg','deezer.jpg',3),

(4,'¿Dónde está el Everest?','imagen',
'Mont Blanc','Kilimanjaro','Everest','Aconcagua',
'montblanc.jpg','kilimanjaro.jpg','everest.jpg','aconcagua.jpg',3),

(5,'¿Cuál es La noche estrellada?','imagen',
'Girasoles','Noche estrellada','El grito','Persistencia de la memoria',
'girasoles.jpg','noche_estrellada.jpg','grito.jpg','persistencia.jpg',2);
";
                    new MySqlCommand(sqlImagen, conn).ExecuteNonQuery();

                    Console.WriteLine("BD limpia y datos insertados correctamente");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        static void MostrarPreguntas()
        {
            try
            {
                using (var conn = DB.GetConnection())
                {
                    conn.Open();

                    string sql = @"
SELECT p.id, c.nombre AS categoria, p.texto, p.tipo, p.correcta
FROM preguntas p
JOIN categorias c ON p.categoria_id = c.id
ORDER BY p.id;
";

                    using (var cmd = new MySqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        Console.WriteLine("ID | Categoria | Tipo | Texto | Correcta");
                        Console.WriteLine("---------------------------------------------------------");

                        while (reader.Read())
                        {
                            Console.WriteLine($"{reader["id"]} | {reader["categoria"]} | {reader["tipo"]} | {reader["texto"]} | {reader["correcta"]}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al mostrar preguntas: " + ex.Message);
            }
        }
    }
}