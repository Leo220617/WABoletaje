﻿ 

namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DetOferta")]
    public partial class DetOferta
    {
        public int id { get; set; }
        public int idEncabezado { get; set; }
        public int NumLinea { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Bodega { get; set; }
        public decimal PorcentajeDescuento { get; set; }
        public decimal Cantidad { get; set; }
        public string Impuesto { get; set; }
        public bool TaxOnly { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Total { get; set; }
        public string TaxCode { get; set; }

    }
}