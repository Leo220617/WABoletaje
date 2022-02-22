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
    public class StatusController : ApiController
    {
        ModelCliente db = new ModelCliente();

        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {

                var Status = db.Status.ToList();

                if (!string.IsNullOrEmpty(filtro.Texto))
                {
                    Status = Status.Where(a => a.Nombre.ToUpper().Contains(filtro.Texto.ToUpper())).ToList();
                }



                return Request.CreateResponse(HttpStatusCode.OK, Status);

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [Route("api/Status/Consultar")]
        public HttpResponseMessage GetOne([FromUri]int id)
        {
            try
            {



                var Status = db.Status.Where(a => a.id == id).FirstOrDefault();


                if (Status == null)
                {
                    throw new Exception("Este Status no se encuentra registrado");
                }

                return Request.CreateResponse(HttpStatusCode.OK, Status);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        public HttpResponseMessage Post([FromBody] Status status)
        {
            try
            {


                var Status = db.Status.Where(a => a.id == status.id).FirstOrDefault();

                if (Status == null)
                {
                    Status = new Status();
                    Status.idSAP = status.idSAP;
                    Status.Nombre = status.Nombre;

                    db.Status.Add(Status);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Este Status  YA existe");
                }


                return Request.CreateResponse(HttpStatusCode.OK, Status);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]

        [Route("api/Status/Actualizar")]
        public HttpResponseMessage Put([FromBody] Status status)
        {
            try
            {


                var Status = db.Status.Where(a => a.id == status.id).FirstOrDefault();

                if (Status != null)
                {
                    db.Entry(Status).State = EntityState.Modified;
                    Status.idSAP = status.idSAP;
                    Status.Nombre = status.Nombre;

                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Status no existe");
                }

                return Request.CreateResponse(HttpStatusCode.OK, Status);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [Route("api/Status/Eliminar")]
        public HttpResponseMessage Delete([FromUri] int id)
        {
            try
            {


                var Status = db.Status.Where(a => a.id == id).FirstOrDefault();

                if (Status != null)
                {


                    db.Status.Remove(Status);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Status no existe");
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