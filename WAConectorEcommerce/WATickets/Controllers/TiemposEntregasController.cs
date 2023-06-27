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
    public class TiemposEntregasController : ApiController
    {
        G g = new G();
        ModelCliente db = new ModelCliente();

        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {
                var TiemposEntregas = db.TiemposEntregas.ToList();

                return Request.CreateResponse(HttpStatusCode.OK, TiemposEntregas);

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


        [Route("api/TiemposEntregas/Consultar")]
        public HttpResponseMessage GetOne([FromUri]int id)
        {
            try
            {



                var TiemposEntrega = db.TiemposEntregas.Where(a => a.id == id).FirstOrDefault();


                if (TiemposEntrega == null)
                {
                    throw new Exception("Esta Tiempos Entregas no se encuentra registrado");
                }

                return Request.CreateResponse(HttpStatusCode.OK, TiemposEntrega);
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
        public HttpResponseMessage Post([FromBody] TiemposEntregas tiemp)
        {
            try
            {


                var Tiemp = db.TiemposEntregas.Where(a => a.id == tiemp.id).FirstOrDefault();

                if (Tiemp == null)
                {
                    Tiemp = new TiemposEntregas();
                    Tiemp.codSAP = tiemp.codSAP;
                    Tiemp.Nombre = tiemp.Nombre;

                    db.TiemposEntregas.Add(Tiemp);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Esta Tiemp YA existe");
                }


                return Request.CreateResponse(HttpStatusCode.OK, Tiemp);
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
        [Route("api/TiemposEntregas/Actualizar")]
        public HttpResponseMessage Put([FromBody] TiemposEntregas tiemp)
        {
            try
            {


                var Tiemp = db.TiemposEntregas.Where(a => a.id == tiemp.id).FirstOrDefault();

                if (Tiemp != null)
                {
                    db.Entry(Tiemp).State = EntityState.Modified;

                    Tiemp.codSAP = tiemp.codSAP;
                    Tiemp.Nombre = tiemp.Nombre;

                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("TiemposEntregas no existe");
                }

                return Request.CreateResponse(HttpStatusCode.OK, Tiemp);
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