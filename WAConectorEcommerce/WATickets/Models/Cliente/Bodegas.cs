 

namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Bodegas")]
    public partial class Bodegas
    {
        public int id { get; set; }
        public string codSAP { get; set; }
        public string Nombre { get; set; }
    }
}