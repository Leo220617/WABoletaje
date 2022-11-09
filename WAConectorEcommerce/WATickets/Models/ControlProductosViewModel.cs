using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WATickets.Models
{
    public class ControlProductosViewModel
    {
        public int idProducto { get; set; }
        public string Item { get; set; }
        public string ItemName { get; set; }
        public decimal CantidadUsado { get; set; }
         
    }
}