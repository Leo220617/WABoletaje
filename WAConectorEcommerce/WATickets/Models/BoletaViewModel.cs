using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WATickets.Models
{
    public class BoletaViewModel
    {
        public string NoSerie { get; set; }
        public string NoSerieFabricante { get; set; }
        public string ItemCode { get; set; }
        public string CardCode { get; set; }
    }
}