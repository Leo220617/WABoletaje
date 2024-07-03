using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
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
    public class SoportesController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G g = new G();

        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {

                DateTime time = new DateTime();
                if(filtro.FechaInicial != time)
                {
                    filtro.FechaFinal = filtro.FechaFinal.AddDays(1);
                }
                var Soportes = db.Soportes.Where(a => (filtro.FechaInicial != time ? a.FechaCreacion >= filtro.FechaInicial : true) &&
                (filtro.FechaFinal != time ? a.FechaCreacion <= filtro.FechaFinal : true)
                && (!string.IsNullOrEmpty(filtro.Texto) ? a.Status == filtro.Texto : true)).ToList();




                return Request.CreateResponse(HttpStatusCode.OK, Soportes);

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }



        [Route("api/Soportes/Consultar")]
        public HttpResponseMessage GetOne([FromUri]int id)
        {
            try
            {



                var Soportes = db.Soportes.Where(a => a.id == id).FirstOrDefault();


                if (Soportes == null)
                {

                    throw new Exception("Este Soporte no se encuentra registrado");


                }

                return Request.CreateResponse(HttpStatusCode.OK, Soportes);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        public HttpResponseMessage Post([FromBody] SoportesViewModel soporte)
        {
            try
            {


                var Soporte = db.Soportes.Where(a => a.id == soporte.id).FirstOrDefault();

                if (Soporte == null)
                {
                    Soporte = new Soportes();
                    Soporte.idUsuarioCreador = soporte.idUsuarioCreador;
                    Soporte.Asunto = soporte.Asunto;
                    Soporte.Mensaje = soporte.Mensaje;
                    Soporte.FechaCreacion = DateTime.Now;
                    Soporte.Pantalla = soporte.Pantalla;
                    Soporte.Status = soporte.Status;
                    Soporte.NoBoleta = soporte.NoBoleta;
                    if(!string.IsNullOrEmpty(soporte.base64))
                    {
                        byte[] hex = Convert.FromBase64String(soporte.base64.Replace("data:image/jpeg;base64,", "").Replace("data:image/png;base64,", ""));
                        Soporte.base64 = hex;
                    }

                    Soporte.FechaActualizacion = DateTime.Now;
                    Soporte.Comentarios = "";
                    db.Soportes.Add(Soporte);
                    db.SaveChanges();

                    try
                    {
                        List<System.Net.Mail.Attachment> adjuntos = new List<System.Net.Mail.Attachment>();
                        Parametros parametros = db.Parametros.FirstOrDefault();
                        var CorreoEnvio = db.CorreoEnvio.FirstOrDefault();

                        System.Net.Mail.Attachment att2 = new System.Net.Mail.Attachment(new MemoryStream(Soporte.base64), Soporte.id.ToString() + ".png");
                        adjuntos.Add(att2);

                        var resp = G.SendV2("soporte@dydconsultorescr.com", "", "", CorreoEnvio.RecepcionEmail, "Soporte Boletaje", "Soporte Boletaje # " + Soporte.id, "<!DOCTYPE html> <html> <head> <meta charset='utf-8'> <meta name='viewport' content='width=device-width, initial-scale=1'> <title></title> </head> <body> <h1>Contrato de servicio</h1> <p> En el presente correo se notifica el soporte tecnico reportado en la aplicacion con el id # "+Soporte.id+", favor no responder a este correo </p> </body> </html>", CorreoEnvio.RecepcionHostName, CorreoEnvio.EnvioPort, CorreoEnvio.RecepcionUseSSL, CorreoEnvio.RecepcionEmail, CorreoEnvio.RecepcionPassword, adjuntos);

                        if (!resp)
                        {
                            throw new Exception("No se ha podido enviar el correo con el soporte");
                        }
                    }
                    catch (Exception ex )
                    {

                        BitacoraErrores be = new BitacoraErrores();

                        be.Descripcion = ex.Message;
                        be.StackTrace = ex.StackTrace;
                        be.Fecha = DateTime.Now;
                        db.BitacoraErrores.Add(be);
                        db.SaveChanges();
                    }
                }
                else
                {
                    throw new Exception("Este Soporte YA existe");
                }


                return Request.CreateResponse(HttpStatusCode.OK, Soporte);
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
        [Route("api/Soportes/Actualizar")]
        public HttpResponseMessage Put([FromBody] SoportesViewModel soporte)
        {
            try
            {


                var Soporte = db.Soportes.Where(a => a.id == soporte.id).FirstOrDefault();

                if (Soporte != null)
                {
                    db.Entry(Soporte).State = EntityState.Modified;

                    Soporte.Asunto = soporte.Asunto;
                    Soporte.Mensaje = soporte.Mensaje; 
                    Soporte.Pantalla = soporte.Pantalla;
                    Soporte.Status = soporte.Status;
                    Soporte.NoBoleta = soporte.NoBoleta;
                    if (!string.IsNullOrEmpty(soporte.base64))
                    {
                        byte[] hex = Convert.FromBase64String(soporte.base64.Replace("data:image/jpeg;base64,", "").Replace("data:image/png;base64,", ""));
                        Soporte.base64 = hex;
                    }
                    Soporte.FechaActualizacion = DateTime.Now;
                    Soporte.Comentarios = soporte.Comentarios;
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Soporte no existe");
                }

                return Request.CreateResponse(HttpStatusCode.OK, Soporte);
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
        [Route("api/Soportes/Eliminar")]
        public HttpResponseMessage Delete([FromUri] int id)
        {
            try
            {


                var Soporte = db.Soportes.Where(a => a.id == id).FirstOrDefault();


                if (Soporte != null)
                {


                    db.Soportes.Remove(Soporte);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Soporte no existe");
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