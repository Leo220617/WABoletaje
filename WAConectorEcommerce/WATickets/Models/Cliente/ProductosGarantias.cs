 

namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    [Table("ProductosGarantias")]
    public partial class ProductosGarantias
    {
        public int id { get; set; }
        public string ItemCode { get; set; }
        public decimal CantidadInicial { get; set; }
        public decimal CantidadFinal { get; set; }
    }
}