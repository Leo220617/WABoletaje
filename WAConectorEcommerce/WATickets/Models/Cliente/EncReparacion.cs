 

namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("EncReparacion")]
    public partial class EncReparacion
    {
        public int id { get; set; }
        public int idLlamada { get; set; }
        public int idTecnico { get; set; }
        public int idDiagnostico { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaModificacion { get; set; }
        public int TipoReparacion { get; set; }
        public string idProductoArreglar { get; set; }
        public int Status { get; set; }
        public bool ProcesadaSAP { get; set; }
        public string Comentarios { get; set; }
        public string BodegaOrigen { get; set; }
        public string BodegaFinal { get; set; }
    }
}