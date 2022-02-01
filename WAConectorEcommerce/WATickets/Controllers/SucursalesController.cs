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

    public class SucursalesController : ApiController
    {
        ModelCliente db = new ModelCliente();


        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {

                var Sucursales = db.Sucursales.ToList();

                if (!string.IsNullOrEmpty(filtro.Texto))
                {
                    Sucursales = Sucursales.Where(a => a.Nombre.ToUpper().Contains(filtro.Texto.ToUpper())).ToList();
                }



                return Request.CreateResponse(HttpStatusCode.OK, Sucursales);

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [Route("api/Sucursales/Consultar")]
        public HttpResponseMessage GetOne([FromUri]int id)
        {
            try
            {



                var Sucursales = db.Sucursales.Where(a => a.id == id).FirstOrDefault();


                if (Sucursales == null)
                {
                    throw new Exception("Este Sucursales no se encuentra registrado");
                }

                return Request.CreateResponse(HttpStatusCode.OK, Sucursales);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        public HttpResponseMessage Post([FromBody] Sucursales sucursales)
        {
            try
            {


                var Sucursales = db.Sucursales.Where(a => a.id == sucursales.id).FirstOrDefault();

                if (Sucursales == null)
                {
                    Sucursales = new Sucursales();
                    Sucursales.idSAP = sucursales.idSAP;
                    Sucursales.Nombre = sucursales.Nombre;

                    db.Sucursales.Add(Sucursales);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Este Sucursales  YA existe");
                }


                return Request.CreateResponse(HttpStatusCode.OK, Sucursales);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPut]
        [Route("api/Sucursales/Actualizar")]
        public HttpResponseMessage Put([FromBody] Sucursales sucursales)
        {
            try
            {


                var Sucursales = db.Sucursales.Where(a => a.id == sucursales.id).FirstOrDefault();

                if (Sucursales != null)
                {
                    db.Entry(Sucursales).State = EntityState.Modified;
                    Sucursales.idSAP = sucursales.idSAP;
                    Sucursales.Nombre = sucursales.Nombre;

                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Sucursales no existe");
                }

                return Request.CreateResponse(HttpStatusCode.OK, Sucursales);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpDelete]
        [Route("api/Sucursales/Eliminar")]
        public HttpResponseMessage Delete([FromUri] int id)
        {
            try
            {


                var Sucursales = db.Sucursales.Where(a => a.id == id).FirstOrDefault();

                if (Sucursales != null)
                {


                    db.Sucursales.Remove(Sucursales);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Sucursales no existe");
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