 

namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    [Table("LogModificaciones")]
    public partial class LogModificaciones
    {
        public int id { get; set; }
        public int idLlamada { get; set; }
        public int idUsuario { get; set; }
        public string Accion { get; set; }
        public DateTime Fecha { get; set; }
    }
}