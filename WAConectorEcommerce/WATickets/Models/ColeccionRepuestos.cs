using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WATickets.Models.Cliente;

namespace WATickets.Models
{
    public class ColeccionRepuestos
    {
        public EncReparacion EncReparacion { get; set; }
        public DetReparacion[] DetReparacion { get; set; }
        public AdjuntosViewModel[] Adjuntos { get; set; }
        public int EstadoLlamada { get; set; }
        public int TipoCasoLlamada { get; set; }
        public int TipoGarantiaLlamada { get; set; }
        public bool Semaforo { get; set; }
    }
}