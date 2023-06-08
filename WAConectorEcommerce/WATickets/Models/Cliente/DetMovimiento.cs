 
namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DetMovimiento")]
    public partial class DetMovimiento
    {
        public int id { get; set; }
        public int idEncabezado { get; set; }
        public int idError { get; set; }
        public int idImpuesto { get; set; }
        public int NumLinea { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PorDescuento { get; set; }
        public decimal Descuento { get; set; }
        public decimal Impuestos { get; set; }
        public decimal TotalLinea { get; set; }
        public bool Garantia { get; set; }
        public bool Opcional { get; set; }

    }
}