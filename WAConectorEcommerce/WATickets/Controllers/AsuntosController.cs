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

    public class AsuntosController : ApiController
    {
        G g = new G();
        ModelCliente db = new ModelCliente();

        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {


                var asuntos = db.Asuntos.ToList();



                return Request.CreateResponse(HttpStatusCode.OK, asuntos);

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

        [Route("api/Asuntos/Consultar")]
        public HttpResponseMessage GetOne([FromUri]int id)
        {
            try
            {



                var Asuntos = db.Asuntos.Where(a => a.id == id).FirstOrDefault();


                if (Asuntos == null)
                {
                    throw new Exception("Este Asunto no se encuentra registrado");
                }

                return Request.CreateResponse(HttpStatusCode.OK, Asuntos);
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
        public HttpResponseMessage Post([FromBody] Asuntos asunto)
        {
            try
            {


                var Asunto = db.Asuntos.Where(a => a.id == asunto.id).FirstOrDefault();

                if (Asunto == null)
                {
                    Asunto = new Asuntos();
                    Asunto.Asunto = asunto.Asunto;

                    db.Asuntos.Add(Asunto);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Este Asunto  YA existe");
                }


                return Request.CreateResponse(HttpStatusCode.OK, Asunto);
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
        [Route("api/Asuntos/Actualizar")]
        public HttpResponseMessage Put([FromBody] Asuntos asunto)
        {
            try
            {


                var Asunto = db.Asuntos.Where(a => a.id == asunto.id).FirstOrDefault();

                if (Asunto != null)
                {
                    db.Entry(Asunto).State = EntityState.Modified;

                    Asunto.Asunto = asunto.Asunto;

                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Asunto no existe");
                }

                return Request.CreateResponse(HttpStatusCode.OK, Asunto);
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
        [Route("api/Asuntos/Eliminar")]
        public HttpResponseMessage Delete([FromUri] int id)
        {
            try
            {


                var Asunto = db.Asuntos.Where(a => a.id == id).FirstOrDefault();

                if (Asunto != null)
                {


                    db.Asuntos.Remove(Asunto);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Asunto no existe");
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