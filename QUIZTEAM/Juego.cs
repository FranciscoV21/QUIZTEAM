using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace QUIZTEAM
{
    public class Pregunta
    {
        public int Id { get; set; }
        public string Texto { get; set; }
        public string Tipo { get; set; } // "texto" o "imagen"
        public List<string> Opciones { get; set; } = new List<string>();
        public List<string> ImagenesOpciones { get; set; } = new List<string>();
        public int IndiceCorrecta { get; set; }
    }
}