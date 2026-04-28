using System.Collections.Generic;

namespace QUIZTEAM
{
    public class Pregunta
    {
        public int id { get; set; }
        public string texto { get; set; }
        public string tipo { get; set; } // "texto" o "imagen"
        public List<string> opciones { get; set; }
        public List<string> imagenesOpciones { get; set; }
        public int correcta { get; set; } // Valor de 1 a 4
    }

    public class PlayerScore
    {
        public string nombre { get; set; }
        public int puntos { get; set; }
        public int correctas { get; set; }
    }
}