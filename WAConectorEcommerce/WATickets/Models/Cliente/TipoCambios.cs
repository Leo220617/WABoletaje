 

namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TipoCambios")]
    public partial class TipoCambios
    {
        public int id { get; set; }
        public decimal TipoCambio { get; set; }
        public string Moneda { get; set; }
        public DateTime Fecha { get; set; }
    }
}