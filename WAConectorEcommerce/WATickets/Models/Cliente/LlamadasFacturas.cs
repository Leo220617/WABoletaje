 

namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("LlamadasFacturas")]
    public partial class LlamadasFacturas
    {
        public int id { get; set; }
        public int idFac { get; set; }
        public string CardCode { get; set; }
        public string ItemCode { get; set; }
        public string Serie { get; set; }
        public DateTime Fecha { get; set; }
    }
}