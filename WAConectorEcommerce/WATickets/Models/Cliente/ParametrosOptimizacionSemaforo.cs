 

namespace WATickets.Models.Cliente
{
     
     using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ParametrosOptimizacionSemaforo")]
    public partial class ParametrosOptimizacionSemaforo
    {
        public int id { get; set; }
        public string CodigoProdCrear { get; set; }
        public string StatusEntregaGarantia { get; set; }
        public string TipoCasoEntregaGarantia { get; set; }
        public string StatusCotizacionSinGarantia { get; set; }
        public string StatusEntregaSinGarantia { get; set; }
        public string TipoCasoEntregaSinGarantia { get; set; }
        public string StatusCotizacionMGT { get; set; }
        public string TipoCasoCotizacionMGT { get; set; }
        public string StatusEntregaMGT { get; set; }
        public string TipoCasoEntregaMGT { get; set; }
        public string TipoGarantiaEntregaMGT { get; set; }
        public string StatusCotizacionGarantia { get; set; }
        public decimal PorcentajeSemaforo { get; set; }
        public string TipoCasoCotizacionGarantiaV { get; set; }
        public string TipoCasoCotizacionSinGarantiaV { get; set; }
        public string TipoCasoEntregaGarantiaV { get; set; }
        public string TipoCasoEntregaSinGarantiaV { get; set; }
        public int idLoginActividad { get; set; }
    }
}