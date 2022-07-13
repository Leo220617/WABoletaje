 

namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("EncMovimiento")]
    public partial class EncMovimiento
    {
        public int id { get; set; }
        public int DocEntry { get; set; }
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public string NumLlamada { get; set; }
        public DateTime Fecha { get; set; }
        public int TipoMovimiento { get; set; }
        public string Comentarios { get; set; }
        public string CreadoPor { get; set; }
        public decimal Subtotal { get; set; }
        public decimal PorDescuento { get; set; }
        public decimal Descuento { get; set; }
        public decimal Impuestos { get; set; }
        public decimal TotalComprobante { get; set; }
        public string Moneda { get; set; }

    }
}