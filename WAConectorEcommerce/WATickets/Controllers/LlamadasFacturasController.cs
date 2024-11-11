using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WATickets.Models;
using WATickets.Models.Cliente;

namespace WATickets.Controllers
{
    [Authorize]
    public class LlamadasFacturasController : ApiController
    {
        G g = new G();
        ModelCliente db = new ModelCliente();

        [HttpPost]
        public HttpResponseMessage Post([FromBody] LlamadasFacturas llf)
        {
            try
            {


                var LLF = db.LlamadasFacturas.Where(a => a.id == llf.id).FirstOrDefault();

                if (LLF == null)
                {

                    LLF = new LlamadasFacturas();

                    LLF.CardCode = llf.CardCode;
                    LLF.ItemCode = llf.ItemCode;
                    LLF.Serie = llf.Serie;
                    LLF.idFac = llf.idFac;
                    LLF.Fecha = DateTime.Now.Date;

                    db.LlamadasFacturas.Add(LLF);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Este LlamadasFacturas  YA existe");
                }


                return Request.CreateResponse(HttpStatusCode.OK, LLF);
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