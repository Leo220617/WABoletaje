 
namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("BackOffice")]
    public partial class BackOffice
    {
        public int id { get; set; }
        public int idLlamada { get; set; }
        public int idEncabezadoReparacion { get; set; }
        public int TipoMovimiento { get; set; }
        public DateTime Fecha { get; set; }
    }
}