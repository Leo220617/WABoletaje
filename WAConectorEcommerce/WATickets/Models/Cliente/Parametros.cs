namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Parametros
    {
        public int id { get; set; }

        public int? SerieBoleta { get; set; }
        public string SQLProductos { get; set; }
        public string SQLClientes { get; set; }
        public string SQLDocNum { get; set; }
        public string SQLItemPadres { get; set; }
        public string SQLItemHijos { get; set; }
        public string BodegaInicial { get; set; }
        public string BodegaFinal { get; set; }
    }
}
