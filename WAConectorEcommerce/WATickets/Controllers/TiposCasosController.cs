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

    public class TiposCasosController : ApiController
    {
        ModelCliente db = new ModelCliente();

        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {

                var TP = db.TiposCasos.ToList();

                if (!string.IsNullOrEmpty(filtro.Texto))
                {
                    TP = TP.Where(a => a.Nombre.ToUpper().Contains(filtro.Texto.ToUpper())).ToList();
                }



                return Request.CreateResponse(HttpStatusCode.OK, TP);

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [Route("api/TiposCasos/Consultar")]
        public HttpResponseMessage GetOne([FromUri]int id)
        {
            try
            {



                var TP = db.TiposCasos.Where(a => a.id == id).FirstOrDefault();


                if (TP == null)
                {
                    throw new Exception("Este Tipo de caso no se encuentra registrado");
                }

                return Request.CreateResponse(HttpStatusCode.OK, TP);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        public HttpResponseMessage Post([FromBody] TiposCasos tipocaso)
        {
            try
            {


                var TP = db.TiposCasos.Where(a => a.id == tipocaso.id).FirstOrDefault();

                if (TP == null)
                {
                    TP = new TiposCasos();
                    TP.idSAP = tipocaso.idSAP;
                    TP.Nombre = tipocaso.Nombre;

                    db.TiposCasos.Add(TP);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Este Tipo de caso  YA existe");
                }


                return Request.CreateResponse(HttpStatusCode.OK, TP);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]

        [Route("api/TiposCasos/Actualizar")]
        public HttpResponseMessage Put([FromBody] TiposCasos tipocaso)
        {
            try
            {


                var TP = db.TiposCasos.Where(a => a.id == tipocaso.id).FirstOrDefault();

                if (TP != null)
                {
                    db.Entry(TP).State = EntityState.Modified;
                    TP.idSAP = tipocaso.idSAP;
                    TP.Nombre = tipocaso.Nombre;

                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Tipo de caso no existe");
                }

                return Request.CreateResponse(HttpStatusCode.OK, TP);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [Route("api/TiposCasos/Eliminar")]
        public HttpResponseMessage Delete([FromUri] int id)
        {
            try
            {


                var TP = db.TiposCasos.Where(a => a.id == id).FirstOrDefault();

                if (TP != null)
                {


                    db.TiposCasos.Remove(TP);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Tipo de Caso no existe");
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}