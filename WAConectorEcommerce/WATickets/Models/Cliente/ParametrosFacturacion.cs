using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ParametrosFacturacion")]
    public partial class ParametrosFacturacion
    {
        public int id { get; set; }
        public string Sucursal { get; set; }
        public string SQLProductosBuscar { get; set; }
        public string UrlFacturar { get; set; }
        public string UrlDocumentos { get; set; }
        public string SQLDocumentosExoneracion { get; set; }
        public string MonedaSAPColones { get; set; }
        public int SerieFECO { get; set; }
        public int SerieFECR { get; set; }
        public string CampoConsecutivo { get; set; }
        public string CampoClave { get; set; }
        public int Dimension { get; set; }
        public string Norma { get; set; }
        public string SQLDocumentoExoneracion { get; set; }
        public int SeriePago { get; set; }
        public string SQLProductosFacturar { get; set; }
        public string MonedaDolaresSAP { get; set; }
    }
}