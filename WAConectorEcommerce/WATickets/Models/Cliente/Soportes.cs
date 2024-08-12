 

namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    [Table("Soportes")]
    public partial class Soportes
    {
        public int id { get; set; }
        public int idUsuarioCreador { get; set; }
        public string Asunto { get; set; }
        public string Mensaje { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string Pantalla { get; set; }
        public string Status { get; set; }
        public string NoBoleta { get; set; }
        public byte[] base64 { get; set; }
        public string Comentarios { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }
}