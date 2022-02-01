using SAPbobsCOM;
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

    public class LlamadasServicioController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G g = new G();

        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {

                var Llamada = db.LlamadasServicios.ToList();

                if (!string.IsNullOrEmpty(filtro.Texto))
                {
                    Llamada = Llamada.Where(a => a.Asunto.ToUpper().Contains(filtro.Texto.ToUpper())).ToList();
                }



                return Request.CreateResponse(HttpStatusCode.OK, Llamada);

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }



        [Route("api/LlamadasServicio/Consultar")]
        public HttpResponseMessage GetOne([FromUri]int id)
        {
            try
            {



                var LlamadasServicio = db.LlamadasServicios.Where(a => a.id == id).FirstOrDefault();


                if (LlamadasServicio == null)
                {
                    throw new Exception("Este LlamadasServicio no se encuentra registrado");
                }

                return Request.CreateResponse(HttpStatusCode.OK, LlamadasServicio);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        public HttpResponseMessage Post([FromBody] LlamadasServicios llamada)
        {
            try
            {

                var Parametros = db.Parametros.FirstOrDefault();
                var Llamada = db.LlamadasServicios.Where(a => a.id == llamada.id).FirstOrDefault();

                if (Llamada == null)
                {
                    Llamada = new LlamadasServicios();
                    Llamada.TipoLlamada = llamada.TipoLlamada;
                    Llamada.Series = Parametros.SerieBoleta;
                    Llamada.Status = llamada.Status;
                    Llamada.CardCode = llamada.CardCode;
                    Llamada.SerieFabricante = llamada.SerieFabricante;
                    Llamada.ItemCode = llamada.ItemCode;
                    Llamada.Asunto = llamada.Asunto;
                    Llamada.TipoCaso = llamada.TipoCaso;
                    Llamada.FechaSISO = llamada.FechaSISO;
                    Llamada.LugarReparacion = llamada.LugarReparacion;
                    var SucReb = llamada.SucRecibo.Value.ToString();
                    var SucRet = llamada.SucRetiro.Value.ToString();

                    Llamada.SucRecibo = llamada.SucRecibo.Value ;
                    Llamada.SucRetiro = llamada.SucRetiro;
                    Llamada.Comentarios = llamada.Comentarios;
                    Llamada.TratadoPor = llamada.TratadoPor;
                    Llamada.Garantia = llamada.Garantia;
                    Llamada.Tecnico = llamada.Tecnico;
                    Llamada.ProcesadaSAP = false;

                    db.LlamadasServicios.Add(Llamada);
                    db.SaveChanges();


                    try
                    {
                        var client = (ServiceCalls)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oServiceCalls);
                        client.CustomerCode = Llamada.CardCode;
                       // client.ServiceBPType = Llamada.TipoLlamada == "1" ?  ServiceTypeEnum.srvcSales: ServiceTypeEnum.srvcPurchasing ;
                        client.Series = Llamada.Series.Value;
                        client.Status = Llamada.Status.Value;
                        client.ManufacturerSerialNum = Llamada.SerieFabricante;
                        client.Subject = Llamada.Asunto;
                        client.ItemCode = Llamada.ItemCode;
                        client.UserFields.Fields.Item("U_TPCASO").Value = Llamada.TipoCaso.Value.ToString();

                        if(Llamada.FechaSISO != null)
                        {
                            client.UserFields.Fields.Item("U_SISO").Value = Llamada.FechaSISO.Value;

                        }
                        client.UserFields.Fields.Item("U_WATTS").Value = Llamada.LugarReparacion.Value.ToString();
                        g.GuardarTxt("Bitacora.txt", SucRet+ " " + db.Sucursales.Where(a => a.idSAP == SucRet).FirstOrDefault().Nombre) ;
                        client.UserFields.Fields.Item("U_SENTRE").Value = db.Sucursales.Where(a => a.idSAP == SucRet).FirstOrDefault() == null ? "": db.Sucursales.Where(a => a.idSAP == SucRet).FirstOrDefault().Nombre;
                        client.UserFields.Fields.Item("U_SRECIB").Value = db.Sucursales.Where(a => a.idSAP == SucReb).FirstOrDefault() == null ? "": db.Sucursales.Where(a => a.idSAP == SucReb).FirstOrDefault().Nombre;
                        client.Description = Llamada.Comentarios;
                        client.AssigneeCode = Llamada.TratadoPor.Value;
                        client.CallType = Llamada.Garantia.Value ;
                        client.TechnicianCode = Llamada.Tecnico.Value;
                        //client.ProblemSubType =  

                        var respuesta = client.Add();

                        if (respuesta == 0)
                        {

                            db.Entry(Llamada).State = EntityState.Modified;
                            Llamada.DocEntry = Convert.ToInt32(Conexion.Company.GetNewObjectKey());
                            Llamada.ProcesadaSAP = true;
                         
                            db.SaveChanges();

                            
                            Conexion.Desconectar();
                        }
                        else
                        {
                            BitacoraErrores be = new BitacoraErrores();

                            be.Descripcion = Conexion.Company.GetLastErrorDescription();
                            be.StackTrace = "Orden de Venta";
                            be.Fecha = DateTime.Now;

                            db.BitacoraErrores.Add(be);
                            db.SaveChanges();
                            Conexion.Desconectar();
                        }


                    }
                    catch (Exception ex1)
                    {

                        Conexion.Desconectar();
                        BitacoraErrores be = new BitacoraErrores();

                        be.Descripcion = ex1.Message;
                        be.StackTrace = ex1.StackTrace;
                        be.Fecha = DateTime.Now;

                        db.BitacoraErrores.Add(be);
                        db.SaveChanges();
                    }

                }
                else
                {
                    throw new Exception("Esta LLamada de servicio YA existe");
                }


                return Request.CreateResponse(HttpStatusCode.OK, Llamada);
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



    }
}