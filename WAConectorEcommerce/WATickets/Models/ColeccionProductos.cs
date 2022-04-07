using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WATickets.Models.Cliente;

namespace WATickets.Models
{
    public class ColeccionProductos
    {
        
        public ProductosPadres ProductosPadres { get; set; }
        public ProductosHijos[] ProductosHijos { get; set; }
    }
}