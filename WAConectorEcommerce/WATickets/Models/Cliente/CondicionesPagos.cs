 
namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("CondicionesPagos")]
    public partial class CondicionesPagos
    {
        public int id { get; set; }
        public string codSAP { get; set; }
        public string Nombre { get; set; }
        public int Dias { get; set; }
    }
}