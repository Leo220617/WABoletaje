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

    public class ImpuestosController : ApiController
    {
        G g = new G();
        ModelCliente db = new ModelCliente();

        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {


                var impuestos = db.Impuestos.ToList();



                return Request.CreateResponse(HttpStatusCode.OK, impuestos);

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

        [Route("api/Impuestos/Consultar")]
        public HttpResponseMessage GetOne([FromUri]int id)
        {
            try
            {



                var Impuesto = db.Impuestos.Where(a => a.id == id).FirstOrDefault();


                if (Impuesto == null)
                {
                    throw new Exception("Este Impuesto no se encuentra registrado");
                }

                return Request.CreateResponse(HttpStatusCode.OK, Impuesto);
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
        public HttpResponseMessage Post([FromBody] Impuestos impuesto)
        {
            try
            {


                var Impuesto = db.Impuestos.Where(a => a.id == impuesto.id).FirstOrDefault();

                if (Impuesto == null)
                {
                    Impuesto = new Impuestos();
                    Impuesto.id = impuesto.id;
                    Impuesto.CodSAP = impuesto.CodSAP;
                    Impuesto.Tarifa = impuesto.Tarifa;
                    db.Impuestos.Add(Impuesto);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Este Impuesto  YA existe");
                }


                return Request.CreateResponse(HttpStatusCode.OK, Impuesto);
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
        [Route("api/Impuestos/Actualizar")]
        public HttpResponseMessage Put([FromBody] Impuestos impuesto)
        {
            try
            {


                var Impuesto = db.Impuestos.Where(a => a.id == impuesto.id).FirstOrDefault();

                if (Impuesto != null)
                {
                    db.Entry(Impuesto).State = EntityState.Modified;

                    Impuesto.CodSAP = impuesto.CodSAP;
                    Impuesto.Tarifa = impuesto.Tarifa;

                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Impuesto no existe");
                }

                return Request.CreateResponse(HttpStatusCode.OK, Impuesto);
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
        [Route("api/Impuestos/Eliminar")]
        public HttpResponseMessage Delete([FromUri] int id)
        {
            try
            {


                var Impuestos = db.Impuestos.Where(a => a.id == id).FirstOrDefault();

                if (Impuestos != null)
                {


                    db.Impuestos.Remove(Impuestos);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Impuesto no existe");
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