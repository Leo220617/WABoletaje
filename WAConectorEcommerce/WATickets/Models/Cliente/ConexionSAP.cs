 

namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ConexionSAP")]
    public partial class ConexionSAP
    {
        public string id { get; set; }
        public string SAPUser { get; set; }
        public string SAPPass { get; set; }
        public string SQLUser { get; set; }
        public string ServerSQL { get; set; }
        public string ServerLicense { get; set; }
        public string SQLPass { get; set; }
        public string SQLType { get; set; }
        public string SQLBD { get; set; }
    }
}