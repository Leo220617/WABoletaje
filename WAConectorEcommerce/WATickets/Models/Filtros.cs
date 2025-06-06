﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WATickets.Models
{
    public class Filtros
    {
        public string Texto { get; set; }
        public string CardName { get; set; }
        public string CardCode { get; set; }
        public int Codigo1 { get; set; }
        public int Codigo2 { get; set; }
        public int Codigo3 { get; set; }
        public int Codigo4 { get; set; }
        public int Codigo5 { get; set; }
        public string ListPrice { get; set; }
        public string ItemCode { get; set; }
        public string Categoria { get; set; }
        public DateTime FechaInicial { get; set; }
        public DateTime FechaFinal { get; set; }
        public DateTime FechaBusqueda { get; set; }

        public bool FiltroEspecial { get; set; }
        public bool PIN { get; set; }
        public bool NoFacturado { get; set; }
        public int DocEntryGenerado { get; set; }
        public bool FiltrarFacturado { get; set; }
        public List<int> seleccionMultiple { get; set; }
    }
}