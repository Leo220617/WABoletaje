 

namespace WATickets.Models
{

    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("BitacoraMovimientosSAP")]
    public partial class BitacoraMovimientosSAP
    {
        public int id { get; set; }
        public int idEncabezado { get; set; }
        public int idDetalle { get; set; }
        public decimal Cantidad { get; set; }
        public string DocEntry { get; set; }
        public bool ProcesadaSAP { get; set; }
    }
}