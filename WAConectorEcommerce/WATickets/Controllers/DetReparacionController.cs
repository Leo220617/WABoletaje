using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using WATickets.Models;
using WATickets.Models.Cliente;

namespace WATickets.Controllers
{
    [Authorize]

    public class DetReparacionController : ApiController
    {
        ModelCliente db = new ModelCliente();


        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {

                var DetReparacion = db.DetReparacion.ToList();

                if(filtro.Codigo1 > 0)
                {
                    DetReparacion = DetReparacion.Where(a => a.idEncabezado == filtro.Codigo1).ToList();
                }



                return Request.CreateResponse(HttpStatusCode.OK, DetReparacion);

            }
            catch (Exception ex)
            {
                BitacoraErrores be = new BitacoraErrores();
                be.Descripcion = ex.Message;
                be.StackTrace = ex.StackTrace;
                be.Fecha = DateTime.Now;
                db.BitacoraErrores.Add(be);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

       
    }
}