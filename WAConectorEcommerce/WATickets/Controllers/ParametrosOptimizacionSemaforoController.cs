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
    public class ParametrosOptimizacionSemaforoController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();
        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {

                var CondicionesP = db.ParametrosOptimizacionSemaforo.Take(1).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, CondicionesP);

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
        [Route("api/ParametrosOptimizacionSemaforo/Actualizar")]
        public HttpResponseMessage Put([FromBody] ParametrosOptimizacionSemaforo param)
        {
            try
            {


                var Param = db.ParametrosOptimizacionSemaforo.Where(a => a.id == param.id).FirstOrDefault();

                if (Param != null)
                {
                    db.Entry(Param).State = EntityState.Modified;

                    Param.PorcentajeSemaforo = param.PorcentajeSemaforo;
                    if (!string.IsNullOrEmpty(param.CodigoProdCrear))
                    {
                        Param.CodigoProdCrear = param.CodigoProdCrear;
                    }
                    if (!string.IsNullOrEmpty(param.StatusEntregaGarantia))
                    {
                        Param.StatusEntregaGarantia = param.StatusEntregaGarantia;
                    }
                    if (!string.IsNullOrEmpty(param.TipoCasoEntregaGarantia))
                    {
                        Param.TipoCasoEntregaGarantia = param.TipoCasoEntregaGarantia;
                    }
                    if (!string.IsNullOrEmpty(param.StatusCotizacionSinGarantia))
                    {
                        Param.StatusCotizacionSinGarantia = param.StatusCotizacionSinGarantia;
                    }
                    if (!string.IsNullOrEmpty(param.StatusEntregaSinGarantia))
                    {
                        Param.StatusEntregaSinGarantia = param.StatusEntregaSinGarantia;

                    }
                    if (!string.IsNullOrEmpty(param.TipoCasoEntregaSinGarantia))
                    {
                        Param.TipoCasoEntregaSinGarantia = param.TipoCasoEntregaSinGarantia;
                    }
                    if (!string.IsNullOrEmpty(param.StatusCotizacionMGT))
                    {
                        Param.StatusCotizacionMGT = param.StatusCotizacionMGT;
                    }
                    if (!string.IsNullOrEmpty(param.TipoCasoCotizacionMGT))
                    {
                        Param.TipoCasoCotizacionMGT = param.TipoCasoCotizacionMGT;
                    }
                    if (!string.IsNullOrEmpty(param.StatusEntregaMGT))
                    {
                        Param.StatusEntregaMGT = param.StatusEntregaMGT;

                    }
                    if (!string.IsNullOrEmpty(param.TipoCasoEntregaMGT))
                    {
                        Param.TipoCasoEntregaMGT = param.TipoCasoEntregaMGT;
                    }
                    if (!string.IsNullOrEmpty(param.TipoGarantiaEntregaMGT))
                    {
                        Param.TipoGarantiaEntregaMGT = param.TipoGarantiaEntregaMGT;
                    }
                    if (!string.IsNullOrEmpty(param.StatusCotizacionGarantia))
                    {
                        Param.StatusCotizacionGarantia = param.StatusCotizacionGarantia;
                    }

                    if (!string.IsNullOrEmpty(param.TipoCasoCotizacionGarantiaV))
                    {
                        Param.TipoCasoCotizacionGarantiaV = param.TipoCasoCotizacionGarantiaV;
                    }
                    if (!string.IsNullOrEmpty(param.TipoCasoCotizacionSinGarantiaV))
                    {
                        Param.TipoCasoCotizacionSinGarantiaV = param.TipoCasoCotizacionSinGarantiaV;
                    }
                    if (!string.IsNullOrEmpty(param.TipoCasoEntregaGarantiaV))
                    {
                        Param.TipoCasoEntregaGarantiaV = param.TipoCasoEntregaGarantiaV;
                    }
                    if (!string.IsNullOrEmpty(param.TipoCasoEntregaSinGarantiaV))
                    {
                        Param.TipoCasoEntregaSinGarantiaV = param.TipoCasoEntregaSinGarantiaV;
                    }

                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Param no existe");
                }

                return Request.CreateResponse(HttpStatusCode.OK, Param);
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