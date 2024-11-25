

namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DetFacturas")]
    public partial class DetFacturas
    {
        public int id { get; set; }
        public int idEncabezado { get; set; }
        public int idImpuesto { get; set; }
        public int NumLinea { get; set; }
        public string ItemCode { get; set; }
        public string CodBodega { get; set; }
        public string Cabys { get; set; }
        public string ListaPrecios { get; set; }
        public decimal Cantidad { get; set; }
        public string UnidadMedida { get; set; }
        public string NomPro { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal PorDescto { get; set; }
        public decimal TotalDescuento { get; set; }
        public decimal TotalImpuestos { get; set; }
        public decimal TotalLinea { get; set; }
        public int idDocumentoExoneracion { get; set; }
    }
}