 

namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("EncOrden")]
    public partial class EncOrden
    {
        public int id { get; set; } 
        public int idCondPago { get; set; }
        public int idGarantia { get; set; }
        public int idTiemposEntregas { get; set; }
        public int BaseEntry { get; set; }
        public int DocEntry { get; set; }
        public int DocNum { get; set; }
        public string CardCode { get; set; }
        public string Moneda { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public DateTime FechaEntrega { get; set; }
        public string TipoDocumento { get; set; }
        public string NumAtCard { get; set; }
        public int Series { get; set; }
        public string Comentarios { get; set; }
        public int CodVendedor { get; set; }
        public bool ProcesadaSAP { get; set; }
        public string PersonaContacto { get; set; }
        public string TelefonoContacto { get; set; }
        public string CorreoContacto { get; set; }
        public int idDiasValidos { get; set; }
        public int idUsuarioCreador { get; set; }

    }
}