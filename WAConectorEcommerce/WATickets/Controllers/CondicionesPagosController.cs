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
    public class CondicionesPagosController: ApiController
    {
        G g = new G();
        ModelCliente db = new ModelCliente();

        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            { 
                var CondicionesP = db.CondicionesPagos.ToList();
                 
                return Request.CreateResponse(HttpStatusCode.OK, CondicionesP);

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


        [Route("api/CondicionesPagos/Consultar")]
        public HttpResponseMessage GetOne([FromUri]int id)
        {
            try
            {



                var CondP = db.CondicionesPagos.Where(a => a.id == id).FirstOrDefault();


                if (CondP == null)
                {
                    throw new Exception("Esta condicion no se encuentra registrado");
                }

                return Request.CreateResponse(HttpStatusCode.OK, CondP);
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

        [HttpPost]
        public HttpResponseMessage Post([FromBody] CondicionesPagos cond)
        {
            try
            {


                var CondP = db.CondicionesPagos.Where(a => a.id == cond.id).FirstOrDefault();

                if (CondP == null)
                {
                    CondP = new CondicionesPagos();
                    CondP.codSAP = cond.codSAP;
                    CondP.Nombre = cond.Nombre;

                    db.CondicionesPagos.Add(CondP);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Esta condicion YA existe");
                }


                return Request.CreateResponse(HttpStatusCode.OK, CondP);
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

        [HttpPost]
        [Route("api/CondicionesPagos/Actualizar")]
        public HttpResponseMessage Put([FromBody] CondicionesPagos cond)
        {
            try
            {


                var CondP = db.CondicionesPagos.Where(a => a.id == cond.id).FirstOrDefault();

                if (CondP != null)
                {
                    db.Entry(CondP).State = EntityState.Modified;

                    CondP.codSAP = cond.codSAP;
                    CondP.Nombre = cond.Nombre;

                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Condicion no existe");
                }

                return Request.CreateResponse(HttpStatusCode.OK, CondP);
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

        [HttpPost]
        [Route("api/CondicionesPagos/Eliminar")]
        public HttpResponseMessage Delete([FromUri] int id)
        {
            try
            {


                var CondP = db.CondicionesPagos.Where(a => a.id == id).FirstOrDefault();


                if (CondP != null)
                {


                    db.CondicionesPagos.Remove(CondP);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Condicion no existe");
                }

                return Request.CreateResponse(HttpStatusCode.OK);
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