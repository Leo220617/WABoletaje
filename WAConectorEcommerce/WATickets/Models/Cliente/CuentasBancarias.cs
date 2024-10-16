 

namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("CuentasBancarias")]
    public partial class CuentasBancarias
    {
        public int id { get; set; }
        public string Nombre { get; set; }
        public string CuentaSAP { get; set; }
        public bool Estado { get; set; }
        public string Banco { get; set; }
        public string Moneda { get; set; }
        public string Tipo { get; set; }
    }
}