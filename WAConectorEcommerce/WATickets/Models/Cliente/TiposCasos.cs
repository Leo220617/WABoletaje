namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TiposCasos
    {
        public int id { get; set; }

        [StringLength(50)]
        public string idSAP { get; set; }

        [StringLength(500)]
        public string Nombre { get; set; }
    }
}
