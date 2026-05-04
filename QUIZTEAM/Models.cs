using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace QUIZTEAM
{
    public class Pregunta
    {
        public int id { get; set; }
        public string texto { get; set; }
        public string tipo { get; set; }
        public int correcta { get; set; }

        // Campos reales que devuelve la API
        public string opcion1 { get; set; }
        public string opcion2 { get; set; }
        public string opcion3 { get; set; }
        public string opcion4 { get; set; }

        public string img1 { get; set; }
        public string img2 { get; set; }
        public string img3 { get; set; }
        public string img4 { get; set; }

        // Propiedades calculadas que usa el resto del código
        [JsonIgnore]
        public List<string> opciones => new List<string> { opcion1, opcion2, opcion3, opcion4 };

        [JsonIgnore]
        public List<string> imagenesOpciones => new List<string> { img1, img2, img3, img4 };
    }
}