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
    public class PlazosCreditosController : ApiController
    {
        G g = new G();
        ModelCliente db = new ModelCliente();

        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {
                var Plazos = db.PlazosCreditos.ToList();

                return Request.CreateResponse(HttpStatusCode.OK, Plazos);

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


        [Route("api/PlazosCreditos/Consultar")]
        public HttpResponseMessage GetOne([FromUri]int id)
        {
            try
            {



                var plazo = db.PlazosCreditos.Where(a => a.id == id).FirstOrDefault();


                if (plazo == null)
                {
                    throw new Exception("Este plazo de credito no se encuentra registrado");
                }

                return Request.CreateResponse(HttpStatusCode.OK, plazo);
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
        public HttpResponseMessage Post([FromBody] PlazosCreditos cond)
        {
            try
            {


                var Plazo = db.PlazosCreditos.Where(a => a.id == cond.id).FirstOrDefault();

                if (Plazo == null)
                {
                    Plazo = new PlazosCreditos();
                    Plazo.codSAP = cond.codSAP;
                    Plazo.Nombre = cond.Nombre;
                    Plazo.Dias = cond.Dias;
                    db.PlazosCreditos.Add(Plazo);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Este Plazos Creditos YA existe");
                }


                return Request.CreateResponse(HttpStatusCode.OK, Plazo);
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
        [Route("api/PlazosCreditos/Actualizar")]
        public HttpResponseMessage Put([FromBody] PlazosCreditos plazo)
        {
            try
            {


                var Plazo = db.PlazosCreditos.Where(a => a.id == plazo.id).FirstOrDefault();

                if (Plazo != null)
                {
                    db.Entry(Plazo).State = EntityState.Modified;

                    Plazo.codSAP = plazo.codSAP;
                    Plazo.Nombre = plazo.Nombre;
                    Plazo.Dias = plazo.Dias;
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Plazo no existe");
                }

                return Request.CreateResponse(HttpStatusCode.OK, Plazo);
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
        [Route("api/PlazosCreditos/Eliminar")]
        public HttpResponseMessage Delete([FromUri] int id)
        {
            try
            {


                var Plazo = db.PlazosCreditos.Where(a => a.id == id).FirstOrDefault();


                if (Plazo != null)
                {


                    db.PlazosCreditos.Remove(Plazo);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Plazo no existe");
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