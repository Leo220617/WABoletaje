using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
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
                var time = new DateTime();
                
                var Llamada = db.LlamadasServicios.Where(a => (filtro.FechaInicial != time ? a.FechaCreacion >= filtro.FechaInicial : true) && (filtro.FechaFinal != time ? a.FechaCreacion <= filtro.FechaFinal : true))
                 .ToList();

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
                   

                    Llamada.SucRecibo = llamada.SucRecibo.Value;
                    Llamada.SucRetiro = llamada.SucRetiro;
                    Llamada.Comentarios = llamada.Comentarios;
                    Llamada.TratadoPor = llamada.TratadoPor;
                    //Llamada.Garantia = llamada.Garantia;
                    Llamada.Tecnico = llamada.Tecnico;
                    Llamada.ProcesadaSAP = false;
                    Llamada.FechaCreacion = DateTime.Now;
                    Llamada.Firma = !string.IsNullOrEmpty(llamada.Firma) ? llamada.Firma : "";
                    db.LlamadasServicios.Add(Llamada);
                    db.SaveChanges();


                    try
                    {
                        //CompanyService companyService = Conexion.Company.GetCompanyService();

                        var client = (ServiceCalls)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oServiceCalls);
                       // var clientw  = (SAPbobsCOM.DepositsService)companyService.GetBusinessService(ServiceTypes.DepositsService);

                        client.CustomerCode = Llamada.CardCode;
                        //client.ServiceBPType = Llamada.TipoLlamada == "1" ?  ServiceTypeEnum.srvcSales: ServiceTypeEnum.srvcPurchasing ;
                        client.Series = Llamada.Series.Value;
                        client.Status = Llamada.Status.Value;
                        client.ManufacturerSerialNum = Llamada.SerieFabricante;
                        client.Subject = Llamada.Asunto;
                        client.ItemCode = Llamada.ItemCode;
                        client.UserFields.Fields.Item("U_TPCASO").Value = Llamada.TipoCaso.Value.ToString();

                        if (Llamada.FechaSISO != null)
                        {
                            client.UserFields.Fields.Item("U_SISO").Value = Llamada.FechaSISO.Value;

                        }
                        client.UserFields.Fields.Item("U_WATTS").Value = Llamada.LugarReparacion.Value.ToString();

                        client.UserFields.Fields.Item("U_SENTRE").Value = db.Sucursales.Where(a => a.id == Llamada.SucRetiro).FirstOrDefault() == null ? "" : db.Sucursales.Where(a => a.id == Llamada.SucRetiro).FirstOrDefault().Nombre;
                        client.UserFields.Fields.Item("U_SRECIB").Value = db.Sucursales.Where(a => a.id == Llamada.SucRecibo).FirstOrDefault() == null ? "" : db.Sucursales.Where(a => a.id == Llamada.SucRecibo).FirstOrDefault().Nombre;
                        client.Description = Llamada.Comentarios;
                        client.AssigneeCode = Llamada.TratadoPor.Value;
                       // client.CallType = Llamada.Garantia.Value;
                        client.TechnicianCode = Llamada.Tecnico.Value;
                        //client.ProblemSubType =  

                        var respuesta = client.Add();

                        if (respuesta == 0)
                        {

                            db.Entry(Llamada).State = EntityState.Modified;
                            Llamada.DocEntry = Convert.ToInt32(Conexion.Company.GetNewObjectKey());
                            try
                            {
                                var conexion = g.DevuelveCadena(db);

                                var SQL = Parametros.SQLDocNum.Replace("@reemplazo", Llamada.DocEntry.ToString());

                                SqlConnection Cn = new SqlConnection(conexion);
                                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                                DataSet Ds = new DataSet();
                                Cn.Open();
                                Da.Fill(Ds, "DocNum1");
                                Llamada.DocNum = Convert.ToInt32(Ds.Tables["DocNum1"].Rows[0]["DocNum"]);

                                Cn.Close();
                            }
                            catch (Exception ex2)
                            {

                                BitacoraErrores be = new BitacoraErrores();

                                be.Descripcion = ex2.Message;
                                be.StackTrace = ex2.StackTrace;
                                be.Fecha = DateTime.Now;

                                db.BitacoraErrores.Add(be);
                                db.SaveChanges();
                            }
                            Llamada.ProcesadaSAP = true;

                            db.SaveChanges();


                            Conexion.Desconectar();
                        }
                        else
                        {
                            BitacoraErrores be = new BitacoraErrores();

                            be.Descripcion = Conexion.Company.GetLastErrorDescription();
                            be.StackTrace = "Llamada de Servicio";
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
                bit.StackTrace = ex.StackTrace + " Caida general";
                bit.Fecha = DateTime.Now;
                bit.DocNum = "";

                db.BitacoraErrores.Add(bit);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }


        [HttpPost]
        [Route("api/LlamadasServicio/Actualizar")]
        public HttpResponseMessage Put([FromBody] LlamadasServicios llamada)
        {
            try
            {
                var Parametros = db.Parametros.FirstOrDefault();
                var Llamada = db.LlamadasServicios.Where(a => a.id == llamada.id).FirstOrDefault();

                if(Llamada != null)
                {

                    db.Entry(Llamada).State = EntityState.Modified;
                    var client = (ServiceCalls)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oServiceCalls);

                    if(client.GetByKey(Llamada.DocEntry.Value))
                    {
                        if (llamada.Status != Llamada.Status)
                        {
                            Llamada.Status = llamada.Status;
                            client.Status = Llamada.Status.Value;

                        }
                        if (!string.IsNullOrEmpty(llamada.CardCode) && llamada.CardCode != Llamada.CardCode)
                        {
                            Llamada.CardCode = llamada.CardCode;
                            client.CustomerCode = Llamada.CardCode;

                        }


                        // Llamada.SerieFabricante = llamada.SerieFabricante;
                        // Llamada.ItemCode = llamada.ItemCode;

                        if (!string.IsNullOrEmpty(llamada.Asunto) && llamada.Asunto != Llamada.Asunto)
                        {
                            Llamada.Asunto = llamada.Asunto;
                            client.Subject = Llamada.Asunto;

                        }

                        if (Llamada.TipoCaso != llamada.TipoCaso)
                        {
                            Llamada.TipoCaso = llamada.TipoCaso;
                            client.UserFields.Fields.Item("U_TPCASO").Value = Llamada.TipoCaso.Value.ToString();

                        }

                        DateTime time = new DateTime();

                        if (llamada.FechaSISO != time && llamada.FechaSISO != Llamada.FechaSISO)
                        {
                            Llamada.FechaSISO = llamada.FechaSISO;
                            client.UserFields.Fields.Item("U_SISO").Value = Llamada.FechaSISO.Value;

                        }

                        if (llamada.LugarReparacion != Llamada.LugarReparacion)
                        {
                            Llamada.LugarReparacion = llamada.LugarReparacion;
                            client.UserFields.Fields.Item("U_WATTS").Value = Llamada.LugarReparacion.Value.ToString();

                        }

                        if (llamada.SucRecibo != Llamada.SucRecibo)
                        {
                            var SucReb = llamada.SucRecibo.Value.ToString();
                            Llamada.SucRecibo = llamada.SucRecibo.Value;
                            client.UserFields.Fields.Item("U_SRECIB").Value = db.Sucursales.Where(a => a.id == Llamada.SucRecibo).FirstOrDefault() == null ? "": db.Sucursales.Where(a => a.id == Llamada.SucRecibo).FirstOrDefault().Nombre;

                        }

                        if (llamada.SucRetiro != Llamada.SucRetiro)
                        {
                            var SucRet = llamada.SucRetiro.Value.ToString();
                            Llamada.SucRetiro = llamada.SucRetiro;
                            client.UserFields.Fields.Item("U_SENTRE").Value = db.Sucursales.Where(a => a.id == Llamada.SucRetiro).FirstOrDefault() == null ? "" : db.Sucursales.Where(a => a.id == Llamada.SucRetiro).FirstOrDefault().Nombre;

                        }

                        if (!string.IsNullOrEmpty(llamada.Comentarios) && Llamada.Comentarios != llamada.Comentarios)
                        {
                            Llamada.Comentarios = llamada.Comentarios;
                            client.Description = Llamada.Comentarios;

                        }


                        if (llamada.TratadoPor != Llamada.TratadoPor)
                        {
                            Llamada.TratadoPor = llamada.TratadoPor;
                            client.AssigneeCode = Llamada.TratadoPor.Value;

                        }

                        if (Llamada.Garantia != llamada.Garantia)
                        {
                            Llamada.Garantia = llamada.Garantia;
                            client.CallType = Llamada.Garantia.Value;

                        }

                        if (Llamada.Tecnico != llamada.Tecnico)
                        {
                            Llamada.Tecnico = llamada.Tecnico;
                            client.TechnicianCode = Llamada.Tecnico.Value;

                        }

                        Llamada.ProcesadaSAP = false;

                        var respuesta = client.Update();

                        if (respuesta == 0)
                        {

                            db.SaveChanges();
                            db.Entry(Llamada).State = EntityState.Modified;
                            
                            Llamada.ProcesadaSAP = true;

                            db.SaveChanges();


                            Conexion.Desconectar();
                        }
                        else
                        {
                            BitacoraErrores be = new BitacoraErrores();

                            be.Descripcion = Conexion.Company.GetLastErrorDescription();
                            be.StackTrace = "Llamada de Servicio";
                            be.Fecha = DateTime.Now;

                            db.BitacoraErrores.Add(be);
                            db.SaveChanges();
                            Conexion.Desconectar();
                        }


                       
                    }

                   

                }
                else
                {
                    throw new Exception("Llamada no existe");
                }

                return Request.CreateResponse(HttpStatusCode.OK, Llamada);

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);

            }
        }



        [HttpPost]
        [Route("api/LlamadasServicio/EnviarSAP")]
        public HttpResponseMessage PostEnviarSAP([FromBody] LlamadasServicios llamada)
        {
            try
            {
                var Parametros = db.Parametros.FirstOrDefault();
                var Llamada = db.LlamadasServicios.Where(a => a.id == llamada.id).FirstOrDefault();

                if(Llamada != null)
                {
                    try
                    {
                        var client = (ServiceCalls)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oServiceCalls);
                        client.CustomerCode = Llamada.CardCode;
                        //client.ServiceBPType = Llamada.TipoLlamada == "1" ?  ServiceTypeEnum.srvcSales: ServiceTypeEnum.srvcPurchasing ;
                        client.Series = Llamada.Series.Value;
                        client.Status = Llamada.Status.Value;
                        client.ManufacturerSerialNum = Llamada.SerieFabricante;
                        client.Subject = Llamada.Asunto;
                        client.ItemCode = Llamada.ItemCode;
                        client.UserFields.Fields.Item("U_TPCASO").Value = Llamada.TipoCaso.Value.ToString();

                        if (Llamada.FechaSISO != null)
                        {
                            client.UserFields.Fields.Item("U_SISO").Value = Llamada.FechaSISO.Value;

                        }
                        client.UserFields.Fields.Item("U_WATTS").Value = Llamada.LugarReparacion.Value.ToString();
                     
                        client.UserFields.Fields.Item("U_SENTRE").Value = db.Sucursales.Where(a => a.id == Llamada.SucRetiro).FirstOrDefault() == null ? "" : db.Sucursales.Where(a => a.id == Llamada.SucRetiro).FirstOrDefault().Nombre;
                        client.UserFields.Fields.Item("U_SRECIB").Value = db.Sucursales.Where(a => a.id == Llamada.SucRecibo).FirstOrDefault() == null ? "" : db.Sucursales.Where(a => a.id == Llamada.SucRecibo).FirstOrDefault().Nombre;
                        client.Description = Llamada.Comentarios;
                        client.AssigneeCode = Llamada.TratadoPor.Value;
                        client.CallType = Llamada.Garantia.Value;
                        client.TechnicianCode = Llamada.Tecnico.Value;
                        //client.ProblemSubType =  

                        var respuesta = client.Add();

                        if (respuesta == 0)
                        {

                            db.Entry(Llamada).State = EntityState.Modified;
                            Llamada.DocEntry = Convert.ToInt32(Conexion.Company.GetNewObjectKey());
                            try
                            {
                                var conexion = g.DevuelveCadena(db);

                                var SQL = Parametros.SQLDocNum.Replace("@reemplazo", Llamada.DocEntry.ToString());

                                SqlConnection Cn = new SqlConnection(conexion);
                                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                                DataSet Ds = new DataSet();
                                Cn.Open();
                                Da.Fill(Ds, "DocNum1");
                                Llamada.DocNum = Convert.ToInt32(Ds.Tables["DocNum1"].Rows[0]["DocNum"]);

                                Cn.Close();
                            }
                            catch (Exception ex2)
                            {

                                BitacoraErrores be = new BitacoraErrores();

                                be.Descripcion = ex2.Message;
                                be.StackTrace = ex2.StackTrace;
                                be.Fecha = DateTime.Now;

                                db.BitacoraErrores.Add(be);
                                db.SaveChanges();
                            }
                            Llamada.ProcesadaSAP = true;

                            db.SaveChanges();


                            Conexion.Desconectar();
                        }
                        else
                        {
                            BitacoraErrores be = new BitacoraErrores();

                            be.Descripcion = Conexion.Company.GetLastErrorDescription();
                            be.StackTrace = "Llamada de Servicio";
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
                    throw new Exception("Esta LLamada de servicio NO existe");
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