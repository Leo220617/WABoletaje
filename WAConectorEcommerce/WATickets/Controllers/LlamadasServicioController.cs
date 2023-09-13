using SAPbobsCOM;
using SelectPdf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
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

        [Route("api/LlamadasServicio/Modifica")]
        public HttpResponseMessage GetOneModifica([FromUri]int DocEntry)
        {
            try
            {

                var conexion = g.DevuelveCadena(db);

                var SQL = db.Parametros.FirstOrDefault().SQLDocNum.Replace("@reemplazo", DocEntry.ToString());

                SqlConnection Cn = new SqlConnection(conexion);
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open();
                Da.Fill(Ds, "DocNum1");

                var Llamada = db.LlamadasServicios.Where(a => a.DocEntry == DocEntry).FirstOrDefault();
                if(Llamada != null)
                {
                    db.Entry(Llamada).State = EntityState.Modified;
                    try
                    {
                        Llamada.FechaSISO = Convert.ToDateTime(Ds.Tables["DocNum1"].Rows[0]["FechaSISO"]);
                    }
                    catch (Exception)
                    {

                        
                    }
                    
                    Llamada.Status = Convert.ToInt32(Ds.Tables["DocNum1"].Rows[0]["Status"]);
                    Llamada.TipoCaso = Convert.ToInt32(Ds.Tables["DocNum1"].Rows[0]["TP"]);
                    db.SaveChanges();
                }
               

                Cn.Close();



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

        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {
                var time = new DateTime();
                
                if(filtro.FechaFinal != time)
                {
                    filtro.FechaFinal = filtro.FechaFinal.AddDays(1);
                }

                var Llamada = db.LlamadasServicios.Where(a => (filtro.FechaInicial != time ? a.FechaCreacion >= filtro.FechaInicial : true) && (filtro.FechaFinal != time ? a.FechaCreacion <= filtro.FechaFinal : true) && (filtro.Codigo1 > 0 ? a.Tecnico == filtro.Codigo1 : true) && (filtro.Codigo2 != 0 ? a.Status.Value == filtro.Codigo2 : true))
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



                var LlamadasServicio = db.LlamadasServicios.Where(a => a.id == id ).FirstOrDefault();


                if (LlamadasServicio == null)
                {
                    LlamadasServicio = db.LlamadasServicios.Where(a => a.DocEntry == id).FirstOrDefault();
                    if (LlamadasServicio == null)
                    {
                        throw new Exception("Este LlamadasServicio no se encuentra registrado");

                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, LlamadasServicio);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }


        [HttpGet]
        [Route("api/LlamadasServicio/ConsultarPorDocEntry")]
        public HttpResponseMessage GetOneLLamada([FromUri]int id)
        {
            try
            {



                var LlamadasServicio = db.LlamadasServicios.Where(a => a.DocEntry == id).FirstOrDefault();


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
        //Reenviar correo
        [HttpGet]
        [Route("api/LlamadasServicio/Reenvio")]
        public HttpResponseMessage GeCorreo([FromUri]int id, string correo)
        {
            try
            {



                var Llamada = db.LlamadasServicios.Where(a => a.id == id).FirstOrDefault();


                if (Llamada == null)
                {
                    throw new Exception("Este LlamadasServicio no se encuentra registrado");
                }
                ////Enviar Correo
                ///
                try
                {
                    var EmailDestino = "";
                    Parametros parametros = db.Parametros.FirstOrDefault();
                    var CorreoEnvio = db.CorreoEnvio.FirstOrDefault();
                    var conexion = g.DevuelveCadena(db);

                    var SQL = parametros.HtmlLlamada + "'" + Llamada.CardCode + "'";

                    SqlConnection Cn = new SqlConnection(conexion);
                    SqlCommand Cmd = new SqlCommand(SQL, Cn);
                    SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                    DataSet Ds = new DataSet();
                    Cn.Open();
                    Da.Fill(Ds, "Encabezado");

                    List<System.Net.Mail.Attachment> adjuntos = new List<System.Net.Mail.Attachment>();
                    html Html = new html();
                    var bodyH = Html.texto;
                    bodyH = bodyH.Replace("@NombreCliente", Ds.Tables["Encabezado"].Rows[0]["CardName"].ToString());
                    bodyH = bodyH.Replace("@telefono",Llamada.NumeroPersonaContacto);
                    bodyH = bodyH.Replace("@celular", "      ");
                    bodyH = bodyH.Replace("@email", Llamada.EmailPersonaContacto);
                    EmailDestino = Llamada.EmailPersonaContacto;
                    bodyH = bodyH.Replace("@NombreContacto", Llamada.PersonaContacto);
                    bodyH = bodyH.Replace("@telcontacto", Llamada.NumeroPersonaContacto);

                    Cn.Close();
                    Cn.Dispose();


                    SQL = parametros.SQLProductos + " where itemCode = '" + Llamada.ItemCode + "'";
                    Cn = new SqlConnection(conexion);
                    Cmd = new SqlCommand(SQL, Cn);
                    Da = new SqlDataAdapter(Cmd);
                    Ds = new DataSet();
                    Cn.Open();
                    Da.Fill(Ds, "Producto");

                    bodyH = bodyH.Replace("@EquipoDelClie", Ds.Tables["Producto"].Rows[0]["itemName"].ToString());
                    bodyH = bodyH.Replace("@Serie", Ds.Tables["Producto"].Rows[0]["manufSN"].ToString());
                    bodyH = bodyH.Replace("@Fecha", Llamada.FechaCreacion.ToString("dd/MM/yyyy"));
                    var sucR = Llamada.SucRecibo.Value.ToString();
                    var sucE = Llamada.SucRetiro.Value.ToString();

                    bodyH = bodyH.Replace("@SucursalR", db.Sucursales.Where(a => a.idSAP == sucR).FirstOrDefault() == null ? "" : db.Sucursales.Where(a => a.idSAP == sucR).FirstOrDefault().Nombre);
                    bodyH = bodyH.Replace("@SE", db.Sucursales.Where(a => a.idSAP == sucE).FirstOrDefault() == null ? "" : db.Sucursales.Where(a => a.idSAP == sucE).FirstOrDefault().Nombre);

                    bodyH = bodyH.Replace("@DiagnosticosDelCliente", Llamada.Asunto);
                    bodyH = bodyH.Replace("@Observaciones", Llamada.Comentarios);
                    bodyH = bodyH.Replace("@NumBoleta", Llamada.DocEntry.ToString());
                  //  bodyH = bodyH.Replace("@Imagen", "<img src="+Llamada.Firma+" width='100' style='margin-left: -50%;' />");
                    bodyH = bodyH.Replace("@Imagen", "<img src=" + Llamada.Firma + " width='100'   />");


                    Cn.Close();
                    Cn.Dispose();




                    HtmlToPdf converter = new HtmlToPdf();

                    // set converter options
                    converter.Options.PdfPageSize = PdfPageSize.A4;
                    converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
                    converter.Options.MarginLeft = 5;
                    converter.Options.MarginRight = 5;

                    // create a new pdf document converting an html string
                    SelectPdf.PdfDocument doc = converter.ConvertHtmlString(bodyH);

                    var bytes = doc.Save();
                    doc.Close();

                    System.Net.Mail.Attachment att3 = new System.Net.Mail.Attachment(new MemoryStream(bytes), "Contrato_Servicio.pdf");
                    adjuntos.Add(att3);
                    var EncReparacion = db.EncReparacion.Where(a => a.idLlamada == Llamada.id).FirstOrDefault();
                    var Adjuntos = db.Adjuntos.Where(a => a.idEncabezado == EncReparacion.id).ToList();
                    var ui = 1;
                    foreach (var det in Adjuntos)
                    {


                        System.Net.Mail.Attachment att2 = new System.Net.Mail.Attachment(new MemoryStream(det.base64), ui.ToString() + ".png");
                        adjuntos.Add(att2);
                        ui++;

                    }

                    var resp = G.SendV2( correo, "", "", CorreoEnvio.RecepcionEmail, "Contrato de Servicio", "Contrato de Servicio para el cliente", "<!DOCTYPE html> <html> <head> <meta charset='utf-8'> <meta name='viewport' content='width=device-width, initial-scale=1'> <title></title> </head> <body> <h1>Contrato de servicio</h1> <p> En el presente correo se le hace entrega del contrato de servicio, favor no responder a este correo </p> </body> </html>", CorreoEnvio.RecepcionHostName, CorreoEnvio.EnvioPort, CorreoEnvio.RecepcionUseSSL, CorreoEnvio.RecepcionEmail, CorreoEnvio.RecepcionPassword, adjuntos);

                    if (!resp)
                    {
                        throw new Exception("No se ha podido enviar el correo con la liquidación");
                    }

                }
                catch (Exception ex)
                {
                    BitacoraErrores be = new BitacoraErrores();

                    be.Descripcion = ex.Message;
                    be.StackTrace = ex.StackTrace;
                    be.Fecha = DateTime.Now;

                    db.BitacoraErrores.Add(be);
                    db.SaveChanges();
                }

                return Request.CreateResponse(HttpStatusCode.OK, Llamada);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }



        [HttpPost]
        public HttpResponseMessage Post([FromBody] LlamadasServicioViewModel llamada)
        {
            try
            {
                if(llamada == null)
                {
                    throw new Exception("El objeto llamada viene null");
                }

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
                    Llamada.FechaSISO = DateTime.Now.AddDays(1); //llamada.FechaSISO;
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
                    Llamada.Horas = llamada.Horas;
                    Llamada.PersonaContacto = llamada.PersonaContacto;
                    Llamada.EmailPersonaContacto = llamada.EmailPersonaContacto;
                    Llamada.NumeroPersonaContacto = llamada.NumeroPersonaContacto;
                    db.LlamadasServicios.Add(Llamada);
                    db.SaveChanges();

                    try
                    {
                        EncReparacion enc = new EncReparacion();
                        enc.idLlamada = Llamada.id;
                        enc.idTecnico = Llamada.Tecnico.Value;
                        enc.FechaCreacion = DateTime.Now;
                        enc.FechaModificacion = DateTime.Now;
                        enc.idProductoArreglar = Llamada.ItemCode;
                        enc.TipoReparacion = 0;
                        enc.Status = 0;
                        enc.ProcesadaSAP = false;
                        enc.BodegaOrigen = "0";
                        enc.BodegaFinal = "0";
                        db.EncReparacion.Add(enc);
                        db.SaveChanges();

                        foreach (var item in llamada.Adjuntos)
                        {
                            Adjuntos adjunto = new Adjuntos();
                            adjunto.idEncabezado = enc.id;

                            byte[] hex = Convert.FromBase64String(item.base64.Replace("data:image/jpeg;base64,", "").Replace("data:image/png;base64,", ""));
                            adjunto.base64 = hex;
                            db.Adjuntos.Add(adjunto);
                            db.SaveChanges();
                        }
                        foreach (var item in llamada.AdjuntosIdentificacion)
                        {
                            AdjuntosIdentificacion adjunto = new AdjuntosIdentificacion();
                            adjunto.idEncabezado = enc.id;

                            byte[] hex = Convert.FromBase64String(item.base64.Replace("data:image/jpeg;base64,", "").Replace("data:image/png;base64,", ""));
                            adjunto.base64 = hex;
                            db.AdjuntosIdentificacion.Add(adjunto);
                            db.SaveChanges();
                        }
                    }
                    catch (Exception ex3)
                    {

                        BitacoraErrores be = new BitacoraErrores();

                        be.Descripcion = ex3.Message;
                        be.StackTrace = ex3.StackTrace;
                        be.Fecha = DateTime.Now;

                        db.BitacoraErrores.Add(be);
                        db.SaveChanges();
                    }


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
                        var Tratado = Llamada.TratadoPor.ToString();
                        client.UserFields.Fields.Item("U_UsuarioCreador").Value = db.Login.Where(a => a.CardCode == Tratado).FirstOrDefault() == null ? "" : db.Login.Where(a => a.CardCode == Tratado).FirstOrDefault().Nombre;

                        client.UserFields.Fields.Item("U_WATTS").Value = Llamada.LugarReparacion.Value.ToString();
                        client.UserFields.Fields.Item("U_CONTHRS").Value = Llamada.Horas.ToString();
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

                                be.Descripcion = "Error en la llamada #" + Llamada.id + " -> " + ex2.Message;
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

                            be.Descripcion = "Error en la llamada #" + Llamada.id + " -> " + Conexion.Company.GetLastErrorDescription();
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

                        be.Descripcion = "Error en la llamada #" + Llamada.id + " -> " + ex1.Message;
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
        public HttpResponseMessage Put([FromBody] LlamadasServicioViewModel llamada)
        {
            try
            {
                var Parametros = db.Parametros.FirstOrDefault();
                var Llamada = db.LlamadasServicios.Where(a => a.id == llamada.id).FirstOrDefault();

                if(Llamada != null)
                {
 

                    db.Entry(Llamada).State = EntityState.Modified;
                    if (llamada.Status != Llamada.Status)
                    {
                        Llamada.Status = llamada.Status;
                      

                    }
                    if (!string.IsNullOrEmpty(llamada.CardCode) && llamada.CardCode != Llamada.CardCode)
                    {
                        Llamada.CardCode = llamada.CardCode;
                      

                    }
                    if (!string.IsNullOrEmpty(llamada.Asunto) && llamada.Asunto != Llamada.Asunto)
                    {
                        Llamada.Asunto = llamada.Asunto; 

                    }

                    if (Llamada.TipoCaso != llamada.TipoCaso)
                    {
                        Llamada.TipoCaso = llamada.TipoCaso; 

                    }

                    DateTime time = new DateTime();

                    if (llamada.FechaSISO != time && llamada.FechaSISO != Llamada.FechaSISO)
                    {
                        Llamada.FechaSISO = llamada.FechaSISO; 

                    }

                    if (llamada.LugarReparacion != Llamada.LugarReparacion)
                    {
                        Llamada.LugarReparacion = llamada.LugarReparacion; 

                    }

                    if (llamada.SucRecibo != Llamada.SucRecibo)
                    {
                        var SucReb = llamada.SucRecibo.Value.ToString();
                        Llamada.SucRecibo = llamada.SucRecibo.Value; 

                    }

                    if (llamada.SucRetiro != Llamada.SucRetiro)
                    {
                        var SucRet = llamada.SucRetiro.Value.ToString();
                        Llamada.SucRetiro = llamada.SucRetiro; 

                    }

                    if (!string.IsNullOrEmpty(llamada.Comentarios) && Llamada.Comentarios != llamada.Comentarios)
                    {
                        Llamada.Comentarios = llamada.Comentarios; 

                    }


                    if (llamada.TratadoPor != Llamada.TratadoPor)
                    {
                        Llamada.TratadoPor = llamada.TratadoPor; 

                    }

                    if (Llamada.Garantia != llamada.Garantia)
                    {
                        Llamada.Garantia = llamada.Garantia;
                       

                    }

                    if (Llamada.Horas != llamada.Horas)
                    {
                        Llamada.Horas = llamada.Horas;
                        
                    }

                    if (Llamada.Tecnico != llamada.Tecnico)
                    {
                        Llamada.Tecnico = llamada.Tecnico;
                         

                        try
                        {
                            var enc = db.EncReparacion.Where(a => a.idLlamada == Llamada.id).FirstOrDefault();
                            db.Entry(enc).State = EntityState.Modified;

                            enc.idTecnico = Llamada.Tecnico.Value;

                            db.SaveChanges();


                        }
                        catch (Exception ex3)
                        {

                            BitacoraErrores be = new BitacoraErrores();

                            be.Descripcion = "Error en la llamada #" + Llamada.id + " -> " + ex3.Message;
                            be.StackTrace = ex3.StackTrace;
                            be.Fecha = DateTime.Now;

                            db.BitacoraErrores.Add(be);
                            db.SaveChanges();
                        }

                    }

                    if(Llamada.PersonaContacto != llamada.PersonaContacto)
                    {
                        Llamada.PersonaContacto = llamada.PersonaContacto;
                    }

                    if (Llamada.EmailPersonaContacto != llamada.EmailPersonaContacto)
                    {
                        Llamada.EmailPersonaContacto = llamada.EmailPersonaContacto;
                    }

                    if (Llamada.NumeroPersonaContacto != llamada.NumeroPersonaContacto)
                    {
                        Llamada.NumeroPersonaContacto = llamada.NumeroPersonaContacto;
                    }
                    Llamada.ProcesadaSAP = false;
                    db.SaveChanges();
                    var enc2 = db.EncReparacion.Where(a => a.idLlamada == llamada.id).FirstOrDefault();
                    if(llamada.AdjuntosIdentificacion != null)
                    {
                        foreach (var item in llamada.AdjuntosIdentificacion)
                        {
                            AdjuntosIdentificacion adjunto = new AdjuntosIdentificacion();
                            adjunto.idEncabezado = enc2.id;

                            byte[] hex = Convert.FromBase64String(item.base64.Replace("data:image/jpeg;base64,", "").Replace("data:image/png;base64,", ""));
                            adjunto.base64 = hex;
                            db.AdjuntosIdentificacion.Add(adjunto);
                            db.SaveChanges();
                        }
                    }
                  

                    try
                    {
                        var client = (ServiceCalls)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oServiceCalls);
                      
                        if (client.GetByKey(Llamada.DocEntry.Value))
                        {

                            client.Status = Llamada.Status.Value;

                            if (!string.IsNullOrEmpty(llamada.CardCode))
                            {

                                client.CustomerCode = Llamada.CardCode;

                            }




                            if (!string.IsNullOrEmpty(llamada.Asunto))
                            {
                                client.Subject = Llamada.Asunto;

                            }


                            client.UserFields.Fields.Item("U_TPCASO").Value = Llamada.TipoCaso.Value.ToString();



                            DateTime time2 = new DateTime();

                            if (llamada.FechaSISO != time2)
                            {

                                client.UserFields.Fields.Item("U_SISO").Value = Llamada.FechaSISO.Value;

                            }


                            client.UserFields.Fields.Item("U_WATTS").Value = Llamada.LugarReparacion.Value.ToString();



                            client.UserFields.Fields.Item("U_SRECIB").Value = db.Sucursales.Where(a => a.id == Llamada.SucRecibo).FirstOrDefault() == null ? "" : db.Sucursales.Where(a => a.id == Llamada.SucRecibo).FirstOrDefault().Nombre;


                            client.UserFields.Fields.Item("U_SENTRE").Value = db.Sucursales.Where(a => a.id == Llamada.SucRetiro).FirstOrDefault() == null ? "" : db.Sucursales.Where(a => a.id == Llamada.SucRetiro).FirstOrDefault().Nombre;


                            if (!string.IsNullOrEmpty(llamada.Comentarios))
                            {

                                client.Description = Llamada.Comentarios;

                            }



                            client.AssigneeCode = Llamada.TratadoPor.Value;

                            var Status = db.Status.Where(a => a.Nombre.ToLower().Contains("cerrado")).FirstOrDefault() == null ? "0" : db.Status.Where(a => a.Nombre.ToLower().Contains("cerrado")).FirstOrDefault().idSAP;
                            if (Convert.ToInt32(Status) == Llamada.Status)
                            {
                                var Reparacion = db.EncReparacion.Where(a => a.idLlamada == Llamada.id).FirstOrDefault();
                                if(Reparacion != null)
                                {
                                    client.Resolution = string.IsNullOrEmpty(Reparacion.Comentarios) ? "Favor revisar operaciones" : Reparacion.Comentarios;

                                }
                                else
                                {
                                    client.Resolution = string.IsNullOrEmpty(Llamada.Comentarios) ? "Favor revisar operaciones" : Llamada.Comentarios;

                                }

                            }
                             


                            client.UserFields.Fields.Item("U_CONTHRS").Value = Llamada.Horas.ToString();



                            client.TechnicianCode = Llamada.Tecnico.Value;
                             
                            try
                            {
                                var enc = db.EncReparacion.Where(a => a.idLlamada == Llamada.id).FirstOrDefault();
                                db.Entry(enc).State = EntityState.Modified;

                                enc.idTecnico = Llamada.Tecnico.Value;

                                db.SaveChanges();


                            }
                            catch (Exception ex3)
                            {

                                BitacoraErrores be = new BitacoraErrores();

                                be.Descripcion = "Error en la llamada #" + Llamada.id + " -> " + ex3.Message;
                                be.StackTrace = ex3.StackTrace;
                                be.Fecha = DateTime.Now;

                                db.BitacoraErrores.Add(be);
                                db.SaveChanges();
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

                                be.Descripcion = "Error en la llamada #" + Llamada.id + " -> " + Conexion.Company.GetLastErrorDescription();
                                be.StackTrace = "Llamada de Servicio";
                                be.Fecha = DateTime.Now;

                                db.BitacoraErrores.Add(be);
                                db.SaveChanges();
                                Conexion.Desconectar();
                            }



                        }
                    }
                    catch (Exception ex)
                    {
                        BitacoraErrores be = new BitacoraErrores();

                        be.Descripcion = "Error en la llamada #" + Llamada.id + " -> " + ex.Message;
                        be.StackTrace = ex.StackTrace;
                        be.Fecha = DateTime.Now;

                        db.BitacoraErrores.Add(be);
                        db.SaveChanges();

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
                        var Tratado = Llamada.TratadoPor.ToString();
                        client.UserFields.Fields.Item("U_UsuarioCreador").Value = db.Login.Where(a => a.CardCode == Tratado).FirstOrDefault() == null ? "" : db.Login.Where(a => a.CardCode == Tratado).FirstOrDefault().Nombre;
                        client.UserFields.Fields.Item("U_WATTS").Value = Llamada.LugarReparacion.Value.ToString();
                        client.UserFields.Fields.Item("U_CONTHRS").Value = Llamada.Horas.ToString();
                        client.UserFields.Fields.Item("U_SENTRE").Value = db.Sucursales.Where(a => a.id == Llamada.SucRetiro).FirstOrDefault() == null ? "" : db.Sucursales.Where(a => a.id == Llamada.SucRetiro).FirstOrDefault().Nombre;
                        client.UserFields.Fields.Item("U_SRECIB").Value = db.Sucursales.Where(a => a.id == Llamada.SucRecibo).FirstOrDefault() == null ? "" : db.Sucursales.Where(a => a.id == Llamada.SucRecibo).FirstOrDefault().Nombre;
                        client.Description = Llamada.Comentarios;
                        client.AssigneeCode = Llamada.TratadoPor.Value;
                        //client.CallType = Llamada.Garantia.Value;
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

                                be.Descripcion = "Error en la llamada #" + Llamada.id + " -> " + ex2.Message;
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

                            be.Descripcion = "Error en la llamada #" + Llamada.id + " -> " + Conexion.Company.GetLastErrorDescription();
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

                        be.Descripcion = "Error en la llamada #" + Llamada.id + " -> " + ex1.Message;
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