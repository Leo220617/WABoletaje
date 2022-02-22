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

    public class GarantiasController : ApiController
    {
        ModelCliente db = new ModelCliente();


        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {

                var Garantias = db.Garantias.ToList();

                if (!string.IsNullOrEmpty(filtro.Texto))
                {
                    Garantias = Garantias.Where(a => a.Nombre.ToUpper().Contains(filtro.Texto.ToUpper())).ToList();
                }



                return Request.CreateResponse(HttpStatusCode.OK, Garantias);

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [Route("api/Garantias/Consultar")]
        public HttpResponseMessage GetOne([FromUri]int id)
        {
            try
            {



                var Garantias = db.Garantias.Where(a => a.id == id).FirstOrDefault();


                if (Garantias == null)
                {
                    throw new Exception("Este Garantias no se encuentra registrado");
                }

                return Request.CreateResponse(HttpStatusCode.OK, Garantias);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        public HttpResponseMessage Post([FromBody] Garantias garantias)
        {
            try
            {


                var Garantias = db.Garantias.Where(a => a.id == garantias.id).FirstOrDefault();

                if (Garantias == null)
                {
                    Garantias = new Garantias();
                    Garantias.idSAP = garantias.idSAP;
                    Garantias.Nombre = garantias.Nombre;

                    db.Garantias.Add(Garantias);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Este Garantias  YA existe");
                }


                return Request.CreateResponse(HttpStatusCode.OK, Garantias);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [Route("api/Garantias/Actualizar")]
        public HttpResponseMessage Put([FromBody] Garantias garantias)
        {
            try
            {


                var Garantias = db.Garantias.Where(a => a.id == garantias.id).FirstOrDefault();

                if (Garantias != null)
                {
                    db.Entry(Garantias).State = EntityState.Modified;
                    Garantias.idSAP = garantias.idSAP;
                    Garantias.Nombre = garantias.Nombre;

                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Garantias no existe");
                }

                return Request.CreateResponse(HttpStatusCode.OK, Garantias);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [Route("api/Garantias/Eliminar")]
        public HttpResponseMessage Delete([FromUri] int id)
        {
            try
            {


                var Garantias = db.Garantias.Where(a => a.id == id).FirstOrDefault();

                if (Garantias != null)
                {


                    db.Garantias.Remove(Garantias);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Garantias no existe");
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