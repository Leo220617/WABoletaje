 

namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;


    [Table("Errores")]
    public partial class Errores
    {
        public int id { get; set; }
        public int idDiagnostico { get; set; }
        public string Descripcion { get; set; }
        public string Diagnostico { get; set; }
    }
}