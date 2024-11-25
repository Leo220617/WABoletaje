 

namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("MetodosPagosFacturas")]
    public partial class MetodosPagosFacturas
    {
        public int id { get; set; }
        public int idEncabezado { get; set; }
        public DateTime Fecha { get; set; }
        public int idCuentaBancaria { get; set; }
        public decimal Monto { get; set; }
        public string BIN { get; set; }
        public string NumReferencia { get; set; }
        public string NumCheque { get; set; }
        public string Metodo { get; set; }
        public string Moneda { get; set; }  
        public string MonedaVuelto { get; set; }
        public decimal PagadoCon { get; set; }
    }
}