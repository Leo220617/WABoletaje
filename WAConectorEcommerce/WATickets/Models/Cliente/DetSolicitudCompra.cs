 

namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    [Table("DetSolicitudCompra")]
    public partial class DetSolicitudCompra
    {
        public int id { get; set; }
        public int idEncabezado { get; set; }
        public string ItemCode { get; set; }
        public int idProducto { get; set; }
        public decimal Cantidad { get; set; }
    }
}