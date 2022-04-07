 

namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("BitacoraMovimientos")]
    public partial class BitacoraMovimientos
    {
        public int id { get; set; }
        public int idLlamada { get; set; }
        public int idEncabezado { get; set; }
        public int idTecnico { get; set; }
        public int DocEntry { get; set; }
        public DateTime Fecha { get; set; }
        public int TipoMovimiento { get; set; }
        public string BodegaInicial { get; set; }
        public string BodegaFinal { get; set; }
        public string Status { get; set; }
        public bool ProcesadaSAP { get; set; }
    }
}