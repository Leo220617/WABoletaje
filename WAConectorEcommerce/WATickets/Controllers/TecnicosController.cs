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
    public class TecnicosController: ApiController
    {
        ModelCliente db = new ModelCliente();


        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {

                var Tecnicos = db.Tecnicos.ToList();

                if (!string.IsNullOrEmpty(filtro.Texto))
                {
                    Tecnicos = Tecnicos.Where(a => a.Nombre.ToUpper().Contains(filtro.Texto.ToUpper())).ToList();
                }



                return Request.CreateResponse(HttpStatusCode.OK, Tecnicos);

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [Route("api/Tecnicos/Consultar")]
        public HttpResponseMessage GetOne([FromUri]int id)
        {
            try
            {



                var Tecnicos = db.Tecnicos.Where(a => a.id == id).FirstOrDefault();


                if (Tecnicos == null)
                {
                    throw new Exception("Este Tecnicos no se encuentra registrado");
                }

                return Request.CreateResponse(HttpStatusCode.OK, Tecnicos);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        public HttpResponseMessage Post([FromBody] Tecnicos tecnicos)
        {
            try
            {


                var Tecnicos = db.Tecnicos.Where(a => a.id == tecnicos.id).FirstOrDefault();

                if (Tecnicos == null)
                {
                    Tecnicos = new Tecnicos();
                    Tecnicos.idSAP = tecnicos.idSAP;
                    Tecnicos.Nombre = tecnicos.Nombre;
                    Tecnicos.Letra = tecnicos.Letra;
                    db.Tecnicos.Add(Tecnicos);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Este Tecnicos  YA existe");
                }


                return Request.CreateResponse(HttpStatusCode.OK, Tecnicos);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
        [HttpPost]

        [Route("api/Tecnicos/Actualizar")]
        public HttpResponseMessage Put([FromBody] Tecnicos tecnicos)
        {
            try
            {


                var Tecnicos = db.Tecnicos.Where(a => a.id == tecnicos.id).FirstOrDefault();

                if (Tecnicos != null)
                {
                    db.Entry(Tecnicos).State = EntityState.Modified;
                    Tecnicos.idSAP = tecnicos.idSAP;
                    Tecnicos.Nombre = tecnicos.Nombre;
                    Tecnicos.Letra = tecnicos.Letra;
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Tecnicos no existe");
                }

                return Request.CreateResponse(HttpStatusCode.OK, Tecnicos);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [Route("api/Tecnicos/Eliminar")]
        public HttpResponseMessage Delete([FromUri] int id)
        {
            try
            {


                var Tecnicos = db.Tecnicos.Where(a => a.id == id).FirstOrDefault();

                if (Tecnicos != null)
                {


                    db.Tecnicos.Remove(Tecnicos);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Tecnicos no existe");
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