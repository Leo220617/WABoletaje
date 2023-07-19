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

    public class DiasValidosController : ApiController
    {
        G g = new G();
        ModelCliente db = new ModelCliente();

        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {
                var DiasValidos = db.DiasValidos.ToList();

                return Request.CreateResponse(HttpStatusCode.OK, DiasValidos);

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


        [Route("api/DiasValidos/Consultar")]
        public HttpResponseMessage GetOne([FromUri]int id)
        {
            try
            {



                var DiasValidos = db.DiasValidos.Where(a => a.id == id).FirstOrDefault();


                if (DiasValidos == null)
                {
                    throw new Exception("Este DiasValidos no se encuentra registrado");
                }

                return Request.CreateResponse(HttpStatusCode.OK, DiasValidos);
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
        public HttpResponseMessage Post([FromBody] DiasValidos item)
        {
            try
            {


                var Item = db.DiasValidos.Where(a => a.id == item.id).FirstOrDefault();

                if (Item == null)
                {
                    Item = new DiasValidos();
                    Item.Nombre = item.Nombre;
                    Item.Dias = item.Dias;

                    db.DiasValidos.Add(Item);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Este DiasValidos YA existe");
                }


                return Request.CreateResponse(HttpStatusCode.OK, Item);
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
        [Route("api/DiasValidos/Actualizar")]
        public HttpResponseMessage Put([FromBody] DiasValidos item)
        {
            try
            {


                var Item = db.DiasValidos.Where(a => a.id == item.id).FirstOrDefault();

                if (Item != null)
                {
                    db.Entry(Item).State = EntityState.Modified;

                    Item.Nombre = item.Nombre;
                    Item.Dias = item.Dias;

                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("DiasValidos no existe");
                }

                return Request.CreateResponse(HttpStatusCode.OK, Item);
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