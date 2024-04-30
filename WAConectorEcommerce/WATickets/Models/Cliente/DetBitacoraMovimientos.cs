 

namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DetBitacoraMovimientos")]
    public partial class DetBitacoraMovimientos
    {
        public int id { get; set; }
        public int idEncabezado { get; set; }
        public int idProducto { get; set; }
        public int idError { get; set; }
        public decimal Cantidad { get; set; }
        public string ItemCode { get; set; }
        public decimal CantidadEnviar { get; set; }
        public decimal CantidadFaltante { get; set; }
    }
}