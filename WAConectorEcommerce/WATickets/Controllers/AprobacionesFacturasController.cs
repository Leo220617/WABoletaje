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

    public class AprobacionesFacturasController : ApiController
    {
        G g = new G();
        ModelCliente db = new ModelCliente();

        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {

                var time = new DateTime();
                var af = db.AprobacionesFacturas.AsQueryable();

                if(filtro.FechaInicial != time)
                {
                    af = af.Where(a => a.Fecha >= filtro.FechaInicial && a.Fecha <= filtro.FechaFinal);
                }

                if(filtro.NoFacturado != null)
                {
                    af = af.Where(a => a.Aprobada == filtro.NoFacturado);
                }
                if(filtro.FechaBusqueda != time)
                {
                    filtro.FechaBusqueda = filtro.FechaBusqueda.Date;
                    af = af.Where(a => a.Fecha == filtro.FechaBusqueda) ;

                }

                if (!string.IsNullOrEmpty(filtro.ItemCode))
                {
                    af = af.Where(a => a.ItemCode.ToLower().Contains(filtro.ItemCode.ToLower()));
                }
                if(!string.IsNullOrEmpty(filtro.Texto))
                {
                    af = af.Where(a => a.Serie.ToLower().Contains(filtro.Texto.ToLower()));

                }
                if(!string.IsNullOrEmpty(filtro.CardCode))
                {
                    af = af.Where(a => a.CardCode.ToLower().Contains(filtro.CardCode.ToLower()));

                }


                return Request.CreateResponse(HttpStatusCode.OK, af.ToList());

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

        [Route("api/AprobacionesFacturas/Consultar")]
        public HttpResponseMessage GetOne([FromUri]int id)
        {
            try
            {



                var Aprobaciones = db.AprobacionesFacturas.Where(a => a.id == id).FirstOrDefault();


                if (Aprobaciones == null)
                {
                    throw new Exception("Este Aprobacion  no se encuentra registrado");
                }

                return Request.CreateResponse(HttpStatusCode.OK, Aprobaciones);
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
        public HttpResponseMessage Post([FromBody] AprobacionesFacturas af)
        {
            try
            {


                var AF = db.AprobacionesFacturas.Where(a => a.id == af.id).FirstOrDefault();

                if (AF == null)
                {
                    var fecha = DateTime.Now.Date;
                    if (db.AprobacionesFacturas.Where(a => a.ItemCode.ToLower().Contains(af.ItemCode.ToLower()) && a.Serie == af.Serie && a.CardCode == af.CardCode && a.Fecha == fecha && a.Aprobada == false).FirstOrDefault() == null)
                    {
                         
                        AF = new AprobacionesFacturas();
                        AF.ItemCode = af.ItemCode;
                        AF.Serie = af.Serie;
                        AF.CardCode = af.CardCode;
                        AF.Aprobada = af.Aprobada;
                        AF.Fecha = fecha;
                        AF.idLoginAprobador = af.idLoginAprobador;
                        AF.FechaAprobacion = DateTime.Now;
                        db.AprobacionesFacturas.Add(AF);
                        db.SaveChanges();
                    }
                    else
                    {
                        throw new Exception("Esta Aprobacion  YA existe");
                    }
                }
                else
                {
                    throw new Exception("Esta Aprobacion  YA existe");
                }


                return Request.CreateResponse(HttpStatusCode.OK, AF);
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
        [Route("api/AprobacionesFacturas/Eliminar")]
        public HttpResponseMessage Delete([FromUri] int id, int idLoginAceptador)
        {
            try
            {


                var aprobacionesFacturas = db.AprobacionesFacturas.Where(a => a.id == id).FirstOrDefault();

                if (aprobacionesFacturas != null)
                {


                    db.Entry(aprobacionesFacturas).State = EntityState.Modified;
                    aprobacionesFacturas.Aprobada = true;
                    aprobacionesFacturas.idLoginAprobador = idLoginAceptador;
                    aprobacionesFacturas.FechaAprobacion = DateTime.Now;
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Aprobaciones no existe");
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