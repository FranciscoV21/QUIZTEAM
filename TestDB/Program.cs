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

                    // 🔥 LIMPIAR Y REINICIAR IDS
                    new MySqlCommand("DELETE FROM preguntas;", conn).ExecuteNonQuery();
                    new MySqlCommand("ALTER TABLE preguntas AUTO_INCREMENT = 1;", conn).ExecuteNonQuery();

                    // =========================
                    // TEXTO
                    // =========================
                    string sqlTexto = @"
INSERT INTO preguntas 
(categoria_id, texto, tipo, opcion1, opcion2, opcion3, opcion4, correcta) VALUES

(1,'¿En qué año llegó Cristóbal Colón a América?','texto','1392','1492','1592','1292',2),
(1,'¿Qué civilización construyó las pirámides de Giza?','texto','La romana','La griega','La egipcia','La mesopotámica',3),
(1,'¿Quién fue el primer presidente de los Estados Unidos?','texto','Abraham Lincoln','Thomas Jefferson','Benjamin Franklin','George Washington',4),
(1,'¿En qué año comenzó la Primera Guerra Mundial?','texto','1914','1918','1905','1939',1),
(1,'¿Qué muro separó a Alemania del Este y Oeste?','texto','Muro de Berlín','Gran Muralla','Muro de Varsovia','Muro de Adriano',1),
(1,'¿A qué país pertenecía México antes de su independencia?','texto','Portugal','Francia','Reino Unido','España',4),

(2,'¿Cuántos jugadores tiene un equipo de fútbol?','texto','9','10','11','12',3),
(2,'¿Dónde se originó el béisbol?','texto','Cuba','México','Canadá','Estados Unidos',4),
(2,'¿Cada cuántos años es el Mundial?','texto','2','4','6','3',2),
(2,'¿Deporte con raqueta y pelota amarilla?','texto','Bádminton','Squash','Tenis','Ping-pong',3),
(2,'¿Quién ganó Qatar 2022?','texto','Brasil','Francia','Alemania','Argentina',4),
(2,'¿Cuántos anillos olímpicos hay?','texto','4','6','5','3',3),

(3,'¿Cuántas cuerdas tiene una guitarra?','texto','4','5','7','6',4),
(3,'¿Quiénes son los Fab Four?','texto','Rolling Stones','The Beatles','Queen','U2',2),
(3,'¿Quién compuso la Quinta Sinfonía?','texto','Mozart','Bach','Beethoven','Chopin',3),
(3,'¿Qué instrumento toca un pianista?','texto','Violín','Clarinete','Piano','Arpa',3),
(3,'¿Origen del flamenco?','texto','Argentina','México','Italia','España',4),

(4,'Capital de Australia','texto','Sídney','Melbourne','Canberra','Brisbane',3),
(4,'Río más largo','texto','Amazonas','Nilo','Yangtsé','Mississippi',2),
(4,'¿Dónde está Egipto?','texto','Asia','Europa','América','África',4),
(4,'Océano más grande','texto','Atlántico','Índico','Pacífico','Ártico',3),
(4,'País más grande','texto','China','Canadá','EEUU','Rusia',4),
(4,'¿Cuántos países hay en Sudamérica?','texto','10','12','14','9',2),

(5,'¿Quién pintó la Mona Lisa?','texto','Miguel Ángel','Rafael','Leonardo da Vinci','Botticelli',3),
(5,'¿Dónde está la Mona Lisa?','texto','Prado','Louvre','Hermitage','MoMA',2),
(5,'¿Quién es cubista?','texto','Dalí','Goya','Miró','Picasso',4),
(5,'Pintura sobre yeso','texto','Óleo','Acuarela','Fresco','Acrílico',3),
(5,'¿Quién hizo el David?','texto','Donatello','Bernini','Rodin','Miguel Ángel',4);
";
                    new MySqlCommand(sqlTexto, conn).ExecuteNonQuery();

                    // =========================
                    // IMÁGENES COMPLETAS 🔥
                    // =========================
                    string sqlImagen = @"
INSERT INTO preguntas 
(categoria_id, texto, tipo, opcion1, opcion2, opcion3, opcion4, img1, img2, img3, img4, correcta) VALUES

(1,'¿Cuál es la Torre Eiffel?','imagen','Torre Eiffel','Big Ben','Coliseo','Sagrada Familia','eiffel.jpg','bigben.jpg','coliseo.jpg','sagrada.jpg',1),
(1,'¿Quién es Napoleón?','imagen','Napoleón','César','Alejandro','Carlomagno','napoleon.jpg','cesar.jpg','alejandro.jpg','carlomagno.jpg',1),
(1,'¿Bandera de Francia?','imagen','Alemania','Italia','Francia','Holanda','bandera_alemania.jpg','bandera_italia.jpg','bandera_francia.jpg','bandera_paisesbajos.jpg',3),
(1,'¿Estatua de la Libertad?','imagen','Cristo','Libertad','Marsellesa','Moai','cristo.jpg','libertad.jpg','marsellesa.jpg','moai.jpg',2),

(2,'¿Cancha de baloncesto?','imagen','Fútbol','Tenis','Basket','Béisbol','cancha_futbol.jpg','cancha_tenis.jpg','cancha_basket.jpg','cancha_beisbol.jpg',3),
(2,'¿Quién es Bolt?','imagen','Phelps','Bolt','Lewis','Farah','phelps.jpg','bolt.jpg','lewis.jpg','farah.jpg',2),
(2,'¿Pelota rugby?','imagen','Americano','Rugby','Volei','Waterpolo','pelota_americano.jpg','pelota_rugby.jpg','pelota_volei.jpg','pelota_water.jpg',2),
(2,'¿Logo Olímpico?','imagen','FIFA','UEFA','Olímpicos','NBA','logo_fifa.jpg','logo_uefa.jpg','logo_olimpicos.jpg','logo_nba.jpg',3),

(3,'¿Violín?','imagen','Cello','Viola','Violín','Contrabajo','cello.jpg','viola.jpg','violin.jpg','contrabajo.jpg',3),
(3,'¿Michael Jackson?','imagen','Prince','MJ','Brown','Wonder','prince.jpg','mjackson.jpg','jbrown.jpg','swonder.jpg',2),
(3,'¿Batería?','imagen','Piano','Batería','Órgano','Synth','piano.jpg','bateria.jpg','organo.jpg','sinte.jpg',2),
(3,'¿Spotify?','imagen','Apple','YT','Spotify','Deezer','applemusic.jpg','ytmusic.jpg','spotify.jpg','deezer.jpg',3),
(3,'¿Trompeta?','imagen','Trombón','Tuba','Corno','Trompeta','trombon.jpg','tuba.jpg','corno.jpg','trompeta.jpg',4),

(4,'¿Everest?','imagen','Mont Blanc','Kilimanjaro','Everest','Aconcagua','montblanc.jpg','kilimanjaro.jpg','everest.jpg','aconcagua.jpg',3),
(4,'¿Brasil?','imagen','Argentina','Colombia','Perú','Brasil','mapa_argentina.jpg','mapa_colombia.jpg','mapa_peru.jpg','mapa_brasil.jpg',4),
(4,'¿Sahara?','imagen','Gobi','Atacama','Sahara','Kalahari','desierto_gobi.jpg','desierto_atacama.jpg','desierto_sahara.jpg','desierto_kalahari.jpg',3),
(4,'¿Niágara?','imagen','Iguazú','Victoria','Niágara','Ángel','iguazu.jpg','victoria.jpg','niagara.jpg','angel.jpg',3),

(5,'¿Noche estrellada?','imagen','Girasoles','Noche','Grito','Persistencia','girasoles.jpg','noche_estrellada.jpg','grito.jpg','persistencia.jpg',2),
(5,'¿El grito?','imagen','Grito','Saturno','Balsa','Libertad','el_grito.jpg','saturno.jpg','balsa.jpg','libertad.jpg',1),
(5,'¿Dalí?','imagen','Persistencia','Guernica','Meninas','Jardín','persistencia.jpg','guernica.jpg','meninas.jpg','jardin.jpg',1),
(5,'¿El pensador?','imagen','Pensador','Piedad','Beso','Discóbolo','pensador.jpg','piedad.jpg','el_beso.jpg','discobolo.jpg',1),
(5,'¿Guernica?','imagen','Aviñón','Guernica','Comp VIII','No 31','aviñon.jpg','guernica.jpg','composicion.jpg','numero31.jpg',2);
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