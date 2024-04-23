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
    public class UbicacionesController : ApiController
    {
        G g = new G();
        ModelCliente db = new ModelCliente();

        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {
                var Ubicaciones = db.Ubicaciones.ToList();

                if(!string.IsNullOrEmpty(filtro.Texto))
                {
                    Ubicaciones = Ubicaciones.Where(a => a.Codigo == filtro.Texto).ToList();
                }

                return Request.CreateResponse(HttpStatusCode.OK, Ubicaciones);

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


        [Route("api/Ubicaciones/Consultar")]
        public HttpResponseMessage GetOne([FromUri]int id)
        {
            try
            {



                var Ubicacion = db.Ubicaciones.Where(a => a.id == id).FirstOrDefault();


                if (Ubicacion == null)
                {
                    throw new Exception("Esta Ubicacion no se encuentra registrado");
                }

                return Request.CreateResponse(HttpStatusCode.OK, Ubicacion);
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
        public HttpResponseMessage Post([FromBody] Ubicaciones ubicacion)
        {
            try
            {


                var Ubicacion = db.Ubicaciones.Where(a => a.id == ubicacion.id).FirstOrDefault();

                if (Ubicacion == null)
                {
                    Ubicacion = new Ubicaciones();
                    Ubicacion.Codigo = ubicacion.Codigo;
                    Ubicacion.Ubicacion = ubicacion.Ubicacion;

                    db.Ubicaciones.Add(Ubicacion);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Esta ubicacion YA existe");
                }


                return Request.CreateResponse(HttpStatusCode.OK, Ubicacion);
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
        [Route("api/Ubicaciones/Actualizar")]
        public HttpResponseMessage Put([FromBody] Ubicaciones ubicacion)
        {
            try
            {


                var Ubicacion = db.Ubicaciones.Where(a => a.id == ubicacion.id).FirstOrDefault();

                if (Ubicacion != null)
                {
                    db.Entry(Ubicacion).State = EntityState.Modified;

                    Ubicacion.Codigo = ubicacion.Codigo;
                    Ubicacion.Ubicacion = ubicacion.Ubicacion;

                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Ubicacion no existe");
                }

                return Request.CreateResponse(HttpStatusCode.OK, Ubicacion);
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
        [Route("api/Ubicaciones/Eliminar")]
        public HttpResponseMessage Delete([FromUri] int id)
        {
            try
            {


                var Ubicacion = db.Ubicaciones.Where(a => a.id == id).FirstOrDefault();


                if (Ubicacion != null)
                {


                    db.Ubicaciones.Remove(Ubicacion);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Ubicacion no existe");
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