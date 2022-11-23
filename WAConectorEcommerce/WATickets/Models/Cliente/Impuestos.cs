 

namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Impuestos")]
    public partial class Impuestos
    {
        public int id { get; set; }
        public string CodSAP { get; set; }
        public decimal Tarifa { get; set; }
    }
}