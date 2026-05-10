using System.Collections.Generic;

namespace QUIZTEAM.Servidor
{
    public class Pregunta
    {
        public int id { get; set; }
        public string texto { get; set; }
        public string tipo { get; set; }
        public int correcta { get; set; }
        public string opcion1 { get; set; }
        public string opcion2 { get; set; }
        public string opcion3 { get; set; }
        public string opcion4 { get; set; }
        public string img1 { get; set; }
        public string img2 { get; set; }
        public string img3 { get; set; }
        public string img4 { get; set; }
    }

    public class Categoria
    {
        public int id { get; set; }
        public string nombre { get; set; }
    }

    public class PlayerScore
    {
        public string nombre { get; set; }
        public int puntos { get; set; }
        public int correctas { get; set; }
        public int incorrectas { get; set; }
    }
}