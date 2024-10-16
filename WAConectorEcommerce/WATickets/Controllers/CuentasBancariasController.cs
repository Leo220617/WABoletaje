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
    public class CuentasBancariasController : ApiController
    {
        ModelCliente db = new ModelCliente();

        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {

                var CuentasBancarias = db.CuentasBancarias.ToList(); 
                return Request.CreateResponse(HttpStatusCode.OK, CuentasBancarias);

            }
            catch (Exception ex)
            {
                BitacoraErrores bit = new BitacoraErrores();
                bit.Descripcion = ex.Message;
                bit.StackTrace = ex.StackTrace;
                bit.Fecha = DateTime.Now;
                bit.DocNum = "";

                db.BitacoraErrores.Add(bit);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [Route("api/CuentasBancarias/Consultar")]
        public HttpResponseMessage GetOne([FromUri]int id)
        {
            try
            {



                var Cuentas = db.CuentasBancarias.Where(a => a.id == id).FirstOrDefault();


                if (Cuentas == null)
                {
                    throw new Exception("Esta Cuenta no se encuentra registrado");
                }

                return Request.CreateResponse(HttpStatusCode.OK, Cuentas);
            }
            catch (Exception ex)
            {
                BitacoraErrores bit = new BitacoraErrores();
                bit.Descripcion = ex.Message;
                bit.StackTrace = ex.StackTrace;
                bit.Fecha = DateTime.Now;
                bit.DocNum = "";

                db.BitacoraErrores.Add(bit);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        public HttpResponseMessage Post([FromBody] CuentasBancarias cuentas)
        {
            try
            {


                var Cuentas = db.CuentasBancarias.Where(a => a.id == cuentas.id).FirstOrDefault();

                if (Cuentas == null)
                {
                    Cuentas = new CuentasBancarias();  
                    Cuentas.Nombre = cuentas.Nombre;
                    Cuentas.CuentaSAP = cuentas.CuentaSAP;
                    Cuentas.Estado = true;
                    Cuentas.Banco = cuentas.Banco;
                    Cuentas.Moneda = cuentas.Moneda;
                    Cuentas.Tipo = cuentas.Tipo;
                    db.CuentasBancarias.Add(Cuentas);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Esta Cuenta  YA existe");
                }


                return Request.CreateResponse(HttpStatusCode.OK, Cuentas);
            }
            catch (Exception ex)
            {
                BitacoraErrores bit = new BitacoraErrores();
                bit.Descripcion = ex.Message;
                bit.StackTrace = ex.StackTrace;
                bit.Fecha = DateTime.Now;
                bit.DocNum = "";

                db.BitacoraErrores.Add(bit);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [Route("api/CuentasBancarias/Actualizar")]
        public HttpResponseMessage Put([FromBody] CuentasBancarias cuentas)
        {
            try
            {


                var Cuentas = db.CuentasBancarias.Where(a => a.id == cuentas.id).FirstOrDefault();

                if (Cuentas != null)
                {
                    db.Entry(Cuentas).State = EntityState.Modified; 
                    Cuentas.Nombre = cuentas.Nombre;
                    Cuentas.CuentaSAP = cuentas.CuentaSAP;
                    Cuentas.Banco = cuentas.Banco;
                    Cuentas.Moneda = cuentas.Moneda;
                    Cuentas.Tipo = cuentas.Tipo;
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Cuenta no existe");
                }

                return Request.CreateResponse(HttpStatusCode.OK, Cuentas);
            }
            catch (Exception ex)
            {
                BitacoraErrores bit = new BitacoraErrores();
                bit.Descripcion = ex.Message;
                bit.StackTrace = ex.StackTrace;
                bit.Fecha = DateTime.Now;
                bit.DocNum = "";

                db.BitacoraErrores.Add(bit);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [Route("api/CuentasBancarias/Eliminar")]
        public HttpResponseMessage Delete([FromUri] int id)
        {
            try
            {


                var Cuentas = db.CuentasBancarias.Where(a => a.id == id).FirstOrDefault();

                if (Cuentas != null)
                {


                    db.CuentasBancarias.Remove(Cuentas);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Cuenta no existe");
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                BitacoraErrores bit = new BitacoraErrores();
                bit.Descripcion = ex.Message;
                bit.StackTrace = ex.StackTrace  ;
                bit.Fecha = DateTime.Now;
                bit.DocNum = "";

                db.BitacoraErrores.Add(bit);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}