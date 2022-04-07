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

    public class DiagnosticosController : ApiController
    {
        ModelCliente db = new ModelCliente();

        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {
               
                var Diagnosticos = db.Diagnosticos.ToList();





                return Request.CreateResponse(HttpStatusCode.OK, Diagnosticos);

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }


        [Route("api/Diagnosticos/Consultar")]
        public HttpResponseMessage GetOne([FromUri]int id)
        {
            try
            {



                var Diagnosticos = db.Diagnosticos.Where(a => a.id == id).FirstOrDefault();


                if (Diagnosticos == null)
                {
                    throw new Exception("Este Diagnostico no se encuentra registrado");
                }

                return Request.CreateResponse(HttpStatusCode.OK, Diagnosticos);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }


        [HttpPost]
        public HttpResponseMessage Post([FromBody] Diagnosticos diagnostico)
        {
            try
            {


                var Diagnostico = db.Diagnosticos.Where(a => a.id == diagnostico.id).FirstOrDefault();

                if (Diagnostico == null)
                {
                    Diagnostico = new Diagnosticos();
                    Diagnostico.Descripcion = diagnostico.Descripcion;

                    db.Diagnosticos.Add(Diagnostico);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Este Diagnostico  YA existe");
                }


                return Request.CreateResponse(HttpStatusCode.OK, Diagnostico);
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
        [Route("api/Diagnosticos/Actualizar")]
        public HttpResponseMessage Put([FromBody] Diagnosticos diagnostico)
        {
            try
            {


                var Diagnostico = db.Diagnosticos.Where(a => a.id == diagnostico.id).FirstOrDefault();

                if (Diagnostico != null)
                {
                    db.Entry(Diagnostico).State = EntityState.Modified;
                    Diagnostico.Descripcion = diagnostico.Descripcion;
                     

                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Diagnostico no existe");
                }

                return Request.CreateResponse(HttpStatusCode.OK, Diagnostico);
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
        [Route("api/Diagnosticos/Eliminar")]
        public HttpResponseMessage Delete([FromUri] int id)
        {
            try
            {


                var Diagnostico = db.Diagnosticos.Where(a => a.id == id).FirstOrDefault();

                if (Diagnostico != null)
                {


                    db.Diagnosticos.Remove(Diagnostico);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Diagnostico no existe");
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