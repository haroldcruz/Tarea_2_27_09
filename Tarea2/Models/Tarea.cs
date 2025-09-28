using System;
using System.Collections.Generic;
namespace Tarea2.Models
{
    // Archivo: Models/Tarea.cs
    public class Tarea
    {
        public string Id { get; set; }
        public string Autor { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string Lenguajes { get; set; }
        public string Categoria { get; set; }
        public List<Calificacion> Calificaciones { get; set; } = new List<Calificacion>();
        public string UrlRepositorio { get; set; }
        public double PromedioCalificacion { get; set; }
    }

    public class Calificacion
    {
        public string Usuario { get; set; } // quien calificó
        public int Puntuacion { get; set; } // 1 a 5
        public string Comentario { get; set; }
        public DateTime Fecha { get; set; }

    }


}
