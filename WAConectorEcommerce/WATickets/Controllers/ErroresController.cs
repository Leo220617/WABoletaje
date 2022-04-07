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
    public class ErroresController : ApiController
    {

        ModelCliente db = new ModelCliente();

        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {

                var Errores = db.Errores.ToList();

                if(filtro.Codigo1 > 0)
                {
                    Errores = Errores.Where(a => a.idDiagnostico == filtro.Codigo1).ToList();
                }
                
                return Request.CreateResponse(HttpStatusCode.OK, Errores);

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }


        [Route("api/Errores/Consultar")]
        public HttpResponseMessage GetOne([FromUri]int id)
        {
            try
            {



                var Error = db.Errores.Where(a => a.id == id).FirstOrDefault();


                if (Error == null)
                {
                    throw new Exception("Este Error no se encuentra registrado");
                }

                return Request.CreateResponse(HttpStatusCode.OK, Error);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        public HttpResponseMessage Post([FromBody] Errores error)
        {
            try
            {


                var Error = db.Errores.Where(a => a.id == error.id).FirstOrDefault();

                if (Error == null)
                {
                    Error = new Errores();
                    Error.idDiagnostico = error.idDiagnostico;
                    Error.Descripcion = error.Descripcion;
                    Error.Diagnostico = error.Diagnostico;

                    db.Errores.Add(Error);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Este Error  YA existe");
                }


                return Request.CreateResponse(HttpStatusCode.OK, Error);
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
        [Route("api/Errores/Actualizar")]
        public HttpResponseMessage Put([FromBody] Errores error)
        {
            try
            {


                var Error = db.Errores.Where(a => a.id == error.id).FirstOrDefault();

                if (Error != null)
                {
                    db.Entry(Error).State = EntityState.Modified;
                    Error.idDiagnostico = error.idDiagnostico;
                    Error.Descripcion = error.Descripcion;
                    Error.Diagnostico = error.Diagnostico;


                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Error no existe");
                }

                return Request.CreateResponse(HttpStatusCode.OK, Error);
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
        [Route("api/Errores/Eliminar")]
        public HttpResponseMessage Delete([FromUri] int id)
        {
            try
            {


                var Error = db.Errores.Where(a => a.id == id).FirstOrDefault();

                if (Error != null)
                {


                    db.Errores.Remove(Error);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Error no existe");
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