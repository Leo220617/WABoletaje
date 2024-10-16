using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WATickets.Models
{
    public class RecibidoFacturacion
    {
        public string ClaveHacienda { get; set; }
        public string ConsecutivoHacienda { get; set; }
        public string DocEntry { get; set; }
        public int code { get; set; }
    }
}