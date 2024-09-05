using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WATickets.Models
{
    public class LlamadasServicioViewModel
    {
        public int id { get; set; }

        [StringLength(2)]
        public string TipoLlamada { get; set; }

        public int? Series { get; set; }

        public int? Status { get; set; }

        [StringLength(50)]
        public string CardCode { get; set; }

        public int? DocEntry { get; set; }

        public int? DocNum { get; set; }

        [StringLength(100)]
        public string SerieFabricante { get; set; }

        [StringLength(100)]
        public string ItemCode { get; set; }

        [StringLength(500)]
        public string Asunto { get; set; }

        public int? TipoCaso { get; set; }

        public DateTime? FechaSISO { get; set; }

        public int? LugarReparacion { get; set; }

        public int? SucRecibo { get; set; }

        public int? SucRetiro { get; set; }

        public string Comentarios { get; set; }

        public int? TratadoPor { get; set; }

        public int? Garantia { get; set; }

        public int? Tecnico { get; set; }

        public bool? ProcesadaSAP { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string Firma { get; set; }
        public int Horas { get; set; }

        public string PersonaContacto { get; set; }
        public string EmailPersonaContacto { get; set; }
        public string NumeroPersonaContacto { get; set; }
        public bool PIN { get; set; }
        public bool SinRepuestos { get; set; }

        public List<AdjuntosViewModel> Adjuntos { get; set; }
        public List<AdjuntosViewModel> AdjuntosIdentificacion { get; set; }

    }
}