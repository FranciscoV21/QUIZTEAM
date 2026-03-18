using System;
using MySql.Data.MySqlClient;

namespace TestDB
{
    class Program
    {
        static void Main(string[] args)
        {
            EjecutarScript();      // crea la base y llena datos
            MostrarPreguntas();    // muestra las preguntas en consola
        }

        static void EjecutarScript()
        {
            try
            {
                using (var conn = DB.GetConnection())
                {
                    conn.Open();

                    // =============================================
                    // CREAR TABLAS
                    // =============================================
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
  correcta TINYINT NOT NULL COMMENT '1=opcion1, 2=opcion2, 3=opcion3, 4=opcion4',
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
                    using (var cmd = new MySqlCommand(sqlTables, conn))
                        cmd.ExecuteNonQuery();

                    // =============================================
                    // CATEGORIAS
                    // =============================================
                    string sqlCategorias = @"
INSERT IGNORE INTO categorias (id, nombre) VALUES
(1,'Historia'),
(2,'Deporte'),
(3,'Música'),
(4,'Geografía'),
(5,'Arte');
";
                    using (var cmd = new MySqlCommand(sqlCategorias, conn))
                        cmd.ExecuteNonQuery();

                    // =============================================
                    // PREGUNTAS DE TEXTO
                    // =============================================
                    string sqlPreguntasTexto = @"
DELETE FROM preguntas;

INSERT INTO preguntas (categoria_id, texto, tipo, opcion1, opcion2, opcion3, opcion4, correcta) VALUES
(1,'¿En qué año llegó Cristóbal Colón a América?','texto','1392','1492','1592','1292',2),
(1,'¿Qué civilización construyó las pirámides de Giza?','texto','La romana','La griega','La egipcia','La mesopotámica',3),
(1,'¿Quién fue el primer presidente de los Estados Unidos?','texto','Abraham Lincoln','Thomas Jefferson','Benjamin Franklin','George Washington',4),
(1,'¿En qué año comenzó la Primera Guerra Mundial?','texto','1914','1918','1905','1939',1),
(1,'¿Qué muro separó a Alemania del Este y Oeste durante la Guerra Fría?','texto','El Muro de Adriano','La Gran Muralla','El Muro de Berlín','El Muro de Varsovia',3),
(1,'¿A qué país perteneció México antes de su independencia en 1821?','texto','Portugal','Francia','Reino Unido','España',4),
(2,'¿Cuántos jugadores tiene un equipo de fútbol en el campo?','texto','9','10','11','12',3),
(2,'¿En qué país se originó el béisbol?','texto','Cuba','México','Canadá','Estados Unidos',4),
(2,'¿Cada cuántos años se celebra el Mundial de Fútbol?','texto','2 años','4 años','6 años','3 años',2),
(2,'¿Cuál deporte se juega con una raqueta y una pelota amarilla sobre césped?','texto','Bádminton','Squash','Tenis','Ping-pong',3),
(2,'¿Qué país ganó el Mundial de Fútbol de 2022 en Qatar?','texto','Brasil','Francia','Alemania','Argentina',4),
(2,'¿Cuántos anillos tiene el símbolo olímpico?','texto','4','6','5','3',3),
(3,'¿Cuántas cuerdas tiene una guitarra estándar?','texto','4','5','7','6',4),
(3,'¿Qué banda británica fue conocida como Los Fab Four?','texto','The Rolling Stones','The Beatles','Led Zeppelin','The Who',2),
(3,'¿Quién compuso la Quinta Sinfonía?','texto','Mozart','Bach','Beethoven','Chopin',3),
(3,'¿Qué instrumento toca un pianista?','texto','Violín','Clarinete','Piano','Arpa',3),
(3,'¿De qué país es originario el flamenco?','texto','Argentina','México','Italia','España',4),
(4,'¿Cuál es la capital de Australia?','texto','Sídney','Melbourne','Canberra','Brisbane',3),
(4,'¿Cuál es el río más largo del mundo?','texto','Amazonas','Nilo','Yangtsé','Mississippi',2),
(4,'¿En qué continente se encuentra Egipto?','texto','Asia','Europa','América','África',4),
(4,'¿Cuál es el océano más grande del mundo?','texto','Atlántico','Índico','Pacífico','Ártico',3),
(4,'¿Cuál es el país más grande del mundo?','texto','China','Canadá','EEUU','Rusia',4),
(4,'¿Cuántos países forman América del Sur?','texto','10','12','14','9',2),
(5,'¿Quién pintó la Mona Lisa?','texto','Miguel Ángel','Rafael','Leonardo da Vinci','Botticelli',3),
(5,'¿En qué museo se exhibe la Mona Lisa?','texto','Prado','Louvre','Hermitage','MoMA',2),
(5,'¿Qué artista español es famoso por el cubismo?','texto','Dalí','Goya','Miró','Picasso',4),
(5,'¿Cómo se llama la técnica de pintura sobre yeso húmedo?','texto','Óleo','Acuarela','Fresco','Acrílico',3),
(5,'¿Quién esculpió el David?','texto','Donatello','Bernini','Rodin','Miguel Ángel',4);
";
                    using (var cmd = new MySqlCommand(sqlPreguntasTexto, conn))
                        cmd.ExecuteNonQuery();

                    // =============================================
                    // PREGUNTAS DE IMAGEN
                    // =============================================
                    string sqlPreguntasImagen = @"
INSERT IGNORE INTO preguntas (categoria_id, texto, tipo, opcion1, opcion2, opcion3, opcion4, img1, img2, img3, img4, correcta) VALUES
(1,'¿Cuál de estas imágenes muestra la Torre Eiffel?','imagen','Torre Eiffel','Big Ben','Coliseo','Sagrada Familia','eiffel.jpg','bigben.jpg','coliseo.jpg','sagrada.jpg',1),
(1,'¿Cuál imagen corresponde a Napoleón Bonaparte?','imagen','Napoleón','Julio César','Alejandro Magno','Carlomagno','napoleon.jpg','cesar.jpg','alejandro.jpg','carlomagno.jpg',1),
(1,'¿Cuál de estas banderas pertenece a Francia?','imagen','Alemania','Italia','Francia','Países Bajos','bandera_alemania.jpg','bandera_italia.jpg','bandera_francia.jpg','bandera_paisesbajos.jpg',3);
";
                    using (var cmd = new MySqlCommand(sqlPreguntasImagen, conn))
                        cmd.ExecuteNonQuery();

                    Console.WriteLine("BD creada y todas las preguntas agregadas correctamente");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        // =============================================
        // FUNCIÓN PARA VISUALIZAR PREGUNTAS
        // =============================================
        static void MostrarPreguntas()
        {
            try
            {
                using (var conn = DB.GetConnection())
                {
                    conn.Open();

                    string sql = @"
SELECT p.id, c.nombre AS categoria, p.texto, p.tipo,
       p.opcion1, p.opcion2, p.opcion3, p.opcion4,
       p.img1, p.img2, p.img3, p.img4, p.correcta
FROM preguntas p
JOIN categorias c ON p.categoria_id = c.id
ORDER BY p.id
LIMIT 50; -- limita a las primeras 50 preguntas
";

                    using (var cmd = new MySqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        Console.WriteLine("ID | Categoria | Tipo | Texto | Correcta");
                        Console.WriteLine("---------------------------------------------------------");

                        while (reader.Read())
                        {
                            int id = reader.GetInt32("id");
                            string categoria = reader.GetString("categoria");
                            string tipo = reader.GetString("tipo");
                            string texto = reader.GetString("texto");
                            int correcta = reader.GetInt32("correcta");

                            Console.WriteLine($"{id} | {categoria} | {tipo} | {texto} | {correcta}");
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