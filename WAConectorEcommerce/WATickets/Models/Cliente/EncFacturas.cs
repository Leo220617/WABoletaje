 

namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("EncFacturas")]
    public partial class EncFacturas
    {
        public int id { get; set; }
        public int idSucursal { get; set; }
        public int idCondicionVenta { get; set; }
        public int idPlazoCredito { get; set; }
        public int idEntrega { get; set; }
        public string NumLlamada { get; set; }
        public string TipoDocumento { get; set; }
        public string CardCode { get; set; }
        public string Cedula { get; set; }
        public string NombreCliente { get; set; }
        public string Correo { get; set; }
        public DateTime Fecha { get; set; }
        public string Moneda { get; set; }
        public decimal TipoCambio { get; set; }
        public string Comentarios  { get; set; }
        public string DocEntry { get; set; }
        public bool ProcesadoSAP { get; set; }
        public DateTime FechaProcesado { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TotalImpuestos { get; set; }
        public decimal TotalDescuento { get; set; }
        public decimal TotalCompra { get; set; }
        public string ClaveHacienda { get; set; }
        public string ConsecutivoHacienda { get; set; }
        public int CreadoPor { get; set; }
        public decimal PorDesc { get; set; }
        public string DocEntryPago { get; set; }
        public bool ProcesadoSAPPago { get; set; }
        public decimal Redondeo { get; set; }

        public DateTime FechaProcesadoPago { get; set; }
    }

}