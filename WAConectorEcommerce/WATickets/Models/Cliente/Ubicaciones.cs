 

namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Ubicaciones")]
    public partial class Ubicaciones
    {
        public int id { get; set; }
        public string Codigo { get; set; }
        public string Ubicacion { get; set; }
    }
}