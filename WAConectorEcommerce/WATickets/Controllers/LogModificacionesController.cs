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
    public class LogModificacionesController : ApiController
    {
        G g = new G();
        ModelCliente db = new ModelCliente();

        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {


                var Logs = db.LogModificaciones.Select(a => new
                {
                    a.id,
                    a.idLlamada,
                    Usuario = db.Login.Where(b => b.id == a.idUsuario).FirstOrDefault() == null ? "" : db.Login.Where(b => b.id == a.idUsuario).FirstOrDefault().Nombre,
                    a.Accion,
                    a.Fecha
                })
                    .Where(a => (filtro.Codigo1 > 0 ? a.idLlamada == filtro.Codigo1 : true)).ToList();



                return Request.CreateResponse(HttpStatusCode.OK, Logs);

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
        public HttpResponseMessage Post([FromBody] LogModificaciones log)
        {
            try
            {


                var Log = db.LogModificaciones.Where(a => a.id == log.id).FirstOrDefault();

                if (Log == null)
                {
                    Log = new LogModificaciones();
                    Log.idLlamada = log.idLlamada;
                    Log.idUsuario = log.idUsuario;
                    Log.Accion = log.Accion;
                    Log.Fecha = DateTime.Now;
                    db.LogModificaciones.Add(Log);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Este LOG  YA existe");
                }


                return Request.CreateResponse(HttpStatusCode.OK, Log);
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