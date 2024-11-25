 

namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("AprobacionesFacturas")]
    public class AprobacionesFacturas
    {
        public int id { get; set; }
        public string CardCode { get; set; }
        public string ItemCode { get; set; }
        public string Serie { get; set; }
        public bool Aprobada { get; set; }
        public DateTime Fecha { get; set; }
        public int idLoginAprobador { get; set; }
        public DateTime FechaAprobacion { get; set; }
    }
}