using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WATickets.Models.Cliente;

namespace WATickets.Models
{
    public class BitacoraMovimientosViewModel
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
        public List<DetBitacoraMovimientos> Detalle { get; set; }
    }
}