 
namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    [Table("Actividades")]

    public partial class Actividades
    {
        public int id { get; set; }
        public int idLlamada { get; set; }
        public int DocEntry { get; set; }
        public int TipoActividad { get; set; }
        public string Detalle { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool ProcesadaSAP { get; set; }
        public int UsuarioCreador { get; set; }
        public int idLogin { get; set; }
    }
}