 

namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ProductosHijos")]
    public partial class ProductosHijos
    {
        public int id { get; set; }
        public int idPadre { get; set; }
        public string codSAP { get; set; }
        public string Nombre { get; set; }
        public decimal Stock { get; set; }
        public int Cantidad { get; set; }
    }
}