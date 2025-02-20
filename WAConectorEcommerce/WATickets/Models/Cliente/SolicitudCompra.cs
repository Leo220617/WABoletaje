 

namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    [Table("SolicitudCompra")] 
    public partial class SolicitudCompra
    {
        public int id { get; set; }
        public string CardCode { get; set; }
        public int idOfertaAprobada { get; set; }
        public int idEncabezadoBitacora { get; set; }
        public DateTime Fecha { get; set; }
        public string GrupoArticulo { get; set; }
        public int DocEntry { get; set; }
        public int DocNum { get; set; }
        public bool ProcesadaSAP { get; set; }
    }
}