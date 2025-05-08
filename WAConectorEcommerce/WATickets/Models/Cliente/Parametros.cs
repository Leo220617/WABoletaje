namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Parametros
    {
        public int id { get; set; }

        public int? SerieBoleta { get; set; }
        public string SQLProductos { get; set; }
        public string SQLClientes { get; set; }
        public string SQLDocNum { get; set; }
        public string SQLItemPadres { get; set; }
        public string SQLItemHijos { get; set; }
        public string BodegaInicial { get; set; }
        public string BodegaFinal { get; set; }
        public string SQLProductosBoleta { get; set; }
        public string HtmlLlamada { get; set; }
        public string TaxCode { get; set; }
        public string CostingCode { get; set; }
        public int SerieOferta { get; set; }
        public int SerieEntrega { get; set; }
        public string SQLClientesOrdenes { get; set; }
        public string SQLProductosOrdenes { get; set; }
        public int SeriesOrdenVenta { get; set; }
        public string SQLPersonasContacto { get; set; }
        public int SerieCliente { get; set; }
        public string SQLInformacionLlamada { get; set; }
        public string SQLInformacionLlamadaDetallada { get; set; }
        public string StatusLlamadaAprobado { get; set; }
        public bool SetearAprobado { get; set; }
        public string SQLDocEntryDocs { get; set; }
        public string SQLClienteTOP { get; set; }
        public string SQLVerificaGarantia { get; set; }
        public string SQLArtSustituido { get; set; }
        public string SQLVerificaMeses { get; set; }
        public string SQLProveedorPredeterminado { get; set; }
        public string SQLPregArtCompra { get; set; }
        public string SQLTipoCambio { get; set; }
        public string SQLVistaPlanificador { get; set; }
    }
}
