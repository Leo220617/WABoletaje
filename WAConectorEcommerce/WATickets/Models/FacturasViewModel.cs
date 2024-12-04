using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WATickets.Models.Cliente;

namespace WATickets.Models
{
    public class FacturasViewModel
    {
        public int id { get; set; }
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
        public string Comentarios { get; set; }
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
        public string ItemCode { get; set; }
        public string Serie { get; set; }
        public DetFacturas[] DetFactura { get; set; }
        public MetodosPagosFacturas[] MetodosPagos { get; set; }

    }
}