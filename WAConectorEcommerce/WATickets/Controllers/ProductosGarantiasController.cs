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
    public class ProductosGarantiasController : ApiController
    {
        G g = new G();
        ModelCliente db = new ModelCliente();

        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {


                var pg = db.ProductosGarantias.AsQueryable();

                if(!string.IsNullOrEmpty(filtro.ItemCode))
                {
                    pg = pg.Where(a => a.ItemCode.ToLower().Contains(filtro.ItemCode.ToLower()));
                }


                return Request.CreateResponse(HttpStatusCode.OK, pg.ToList());

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

        [Route("api/ProductosGarantias/Consultar")]
        public HttpResponseMessage GetOne([FromUri]int id)
        {
            try
            {



                var ProductosGarantias = db.ProductosGarantias.Where(a => a.id == id).FirstOrDefault();


                if (ProductosGarantias == null)
                {
                    throw new Exception("Este Productos Garantias no se encuentra registrado");
                }

                return Request.CreateResponse(HttpStatusCode.OK, ProductosGarantias);
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
        public HttpResponseMessage Post([FromBody] ProductosGarantias pg)
        {
            try
            {


                var PG = db.ProductosGarantias.Where(a => a.id == pg.id).FirstOrDefault();

                if (PG == null)
                {
                    if(db.ProductosGarantias.Where(a => a.ItemCode.ToLower().Contains(pg.ItemCode.ToLower())) != null)
                    {
                        PG = new ProductosGarantias();
                        PG.ItemCode = pg.ItemCode;
                        PG.CantidadInicial = pg.CantidadInicial;
                        PG.CantidadFinal = pg.CantidadFinal;

                        db.ProductosGarantias.Add(PG);
                        db.SaveChanges();
                    }
                    else
                    {
                        throw new Exception("Este ProductosGarantias  YA existe");
                    } 
                }
                else
                {
                    throw new Exception("Este ProductosGarantias  YA existe");
                }


                return Request.CreateResponse(HttpStatusCode.OK, PG);
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
        [Route("api/ProductosGarantias/Actualizar")]
        public HttpResponseMessage Put([FromBody] ProductosGarantias pg)
        {
            try
            {


                var PG = db.ProductosGarantias.Where(a => a.id == pg.id).FirstOrDefault();

                if (PG != null)
                {
                    db.Entry(PG).State = EntityState.Modified;

                    PG.ItemCode = pg.ItemCode;
                    PG.CantidadInicial = pg.CantidadInicial;
                    PG.CantidadFinal = pg.CantidadFinal;

                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("ProductosGarantias no existe");
                }

                return Request.CreateResponse(HttpStatusCode.OK, PG);
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
        [Route("api/ProductosGarantias/Eliminar")]
        public HttpResponseMessage Delete([FromUri] int id)
        {
            try
            {


                var ProductosGarantias1 = db.ProductosGarantias.Where(a => a.id == id).FirstOrDefault();

                if (ProductosGarantias1 != null)
                {


                    db.ProductosGarantias.Remove(ProductosGarantias1);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("ProductosGarantias no existe");
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