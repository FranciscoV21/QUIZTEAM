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

    public class Juego : Form
    {
        private string _categoria;
        private List<Pregunta> _preguntas = new List<Pregunta>();
        private int _indiceActual = 0;
        private int _correctas = 0;
        private int _incorrectas = 0;
        private int _seleccion = -1;
        private bool _respondida = false;

        private Rectangle[] _zonasOpciones = new Rectangle[4];
        private Rectangle _zonaSiguiente;
        private Rectangle _zonaSalir;

        private List<Image> _imagenesOpciones = new List<Image>();

        public Juego(string categoria)
        {
            _categoria = categoria;
            this.DoubleBuffered = true;
            this.ClientSize = new Size(780, 520);
            this.Text = "QUIZTEAM — " + categoria;
            this.BackColor = Color.FromArgb(26, 26, 46);
            this.Load += Juego_Load;
        }
    }