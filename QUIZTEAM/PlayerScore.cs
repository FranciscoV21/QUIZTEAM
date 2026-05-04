using System;

namespace QUIZTEAM
{
    // Esta clase sirve como un "molde" para los datos de los jugadores
    public class PlayerScore
    {
        public string nombre { get; set; }
        public int puntos { get; set; }
        public int correctas { get; set; }

        // Constructor vacío para que el deserializador de JSON no de problemas
        public PlayerScore() { }

        public PlayerScore(string nombre, int puntos, int correctas)
        {
            this.nombre = nombre;
            this.puntos = puntos;
            this.correctas = correctas;
        }
    }
}