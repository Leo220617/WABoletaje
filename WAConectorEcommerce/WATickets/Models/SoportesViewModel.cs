using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WATickets.Models
{
    public class SoportesViewModel
    {
        public int id { get; set; }
        public int idUsuarioCreador { get; set; }
        public string Asunto { get; set; }
        public string Mensaje { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string Pantalla { get; set; }
        public string Status { get; set; }
        public string NoBoleta { get; set; }
        public string base64 { get; set; }
        public string Comentarios { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }
}