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
using System.Security.Claims;
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
                if (Llamada != null)
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
                var Llamada = new List<LlamadasServicios>();
                if (filtro.FechaFinal != time)
                {
                    filtro.FechaFinal = filtro.FechaFinal.AddDays(1);
                }

                if (!filtro.FiltroEspecial)
                {
                  
                    if (!string.IsNullOrEmpty(filtro.CardName))
                    {
                        var valores = filtro.CardName.Split('|');
                        foreach (var item in valores)
                        {
                            if (!string.IsNullOrEmpty(item))
                            {
                                filtro.seleccionMultiple.Add(Convert.ToInt32(item));

                            }

                        }

                        if (filtro.seleccionMultiple.Count > 0)
                        {
                             Llamada = db.LlamadasServicios
                             .Where(a => (filtro.FechaInicial != time ? a.FechaCreacion >= filtro.FechaInicial : true)
                             && (filtro.FechaFinal != time ? a.FechaCreacion <= filtro.FechaFinal : true)
                             && (filtro.Codigo1 > 0 ? a.Tecnico == filtro.Codigo1 : true)
                             && (filtro.Codigo2 != 0 ? a.Status.Value == filtro.Codigo2 : true) && a.PIN == filtro.PIN && filtro.seleccionMultiple.Contains(a.Status.Value))

                          .ToList();
                            

                        }


                    }
                    else if (!string.IsNullOrEmpty(filtro.Texto) || !string.IsNullOrEmpty(filtro.CardCode))
                    {
                        var DocEntry = 0;
                        try
                        {
                            DocEntry = Convert.ToInt32(filtro.Texto);
                        }
                        catch (Exception)
                        {

                        }

                        Llamada = db.LlamadasServicios
                       .Where(a => (DocEntry != 0 ? a.DocEntry == DocEntry : true)
                       && (!string.IsNullOrEmpty(filtro.CardCode) ? a.CardCode.Contains(filtro.CardCode) : true)
                       ).ToList();

                    }
                    else
                    {
                          Llamada = db.LlamadasServicios
                         .Where(a => (filtro.FechaInicial != time ? a.FechaCreacion >= filtro.FechaInicial : true)
                         && (filtro.FechaFinal != time ? a.FechaCreacion <= filtro.FechaFinal : true)
                         && (filtro.Codigo1 > 0 ? a.Tecnico == filtro.Codigo1 : true)
                         && (filtro.Codigo2 != 0 ? a.Status.Value == filtro.Codigo2 : true) && a.PIN == filtro.PIN)
                      .ToList();
                    }

                   


                    return Request.CreateResponse(HttpStatusCode.OK, Llamada);
                }
                else
                {
                    if (!string.IsNullOrEmpty(filtro.Texto) || !string.IsNullOrEmpty(filtro.CardCode))
                    {
                        var DocEntry = 0;
                        try
                        {
                            DocEntry = Convert.ToInt32(filtro.Texto);
                        }
                        catch (Exception)
                        {

                        }

                          Llamada = db.LlamadasServicios
                         .Where(a => (DocEntry != 0 ? a.DocEntry == DocEntry : true)
                         && (!string.IsNullOrEmpty(filtro.CardCode) ? a.CardCode.Contains(filtro.CardCode) : true)
                         ).ToList();




                        return Request.CreateResponse(HttpStatusCode.OK, Llamada);
                    }
                    else
                    {
                        

                        if (!string.IsNullOrEmpty(filtro.CardName))
                        {
                            var valores = filtro.CardName.Split('|');
                            foreach (var item in valores)
                            {
                                if (!string.IsNullOrEmpty(item))
                                {
                                    filtro.seleccionMultiple.Add(Convert.ToInt32(item));

                                }

                            }

                            if (filtro.seleccionMultiple.Count > 0)
                            {
                                Llamada = db.LlamadasServicios
                          .Where(a => (filtro.FechaInicial != time ? a.FechaCreacion >= filtro.FechaInicial : true)
                          && (filtro.FechaFinal != time ? a.FechaCreacion <= filtro.FechaFinal : true) && filtro.seleccionMultiple.Contains(a.Status.Value)
                          
                          ).ToList();
                                

                            }


                        }
                        else
                        {
                            Llamada = db.LlamadasServicios
                          .Where(a => (filtro.FechaInicial != time ? a.FechaCreacion >= filtro.FechaInicial : true)
                          && (filtro.FechaFinal != time ? a.FechaCreacion <= filtro.FechaFinal : true)  
                          ).ToList();
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, Llamada);
                    }


                }


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
                    var bodyH = G.ObtenerConfig("Empresa") == "G" ? Html.textoGermantec : Html.textoAlsara;
                    bodyH = bodyH.Replace("@NombreCliente", Ds.Tables["Encabezado"].Rows[0]["CardName"].ToString());
                    bodyH = bodyH.Replace("@telefono", Llamada.NumeroPersonaContacto);
                    bodyH = bodyH.Replace("@celular", "      ");
                    bodyH = bodyH.Replace("@email", Llamada.EmailPersonaContacto);
                    EmailDestino = Llamada.EmailPersonaContacto;
                    bodyH = bodyH.Replace("@NombreContacto", Llamada.PersonaContacto);
                    bodyH = bodyH.Replace("@telcontacto", Llamada.NumeroPersonaContacto);

                    Cn.Close();
                    Cn.Dispose();


                    SQL = parametros.SQLProductos + " where itemCode = '" + Llamada.ItemCode + "'";//+ " and customer= '" +Llamada.CardCode+ "' and ";
                    Cn = new SqlConnection(conexion);
                    Cmd = new SqlCommand(SQL, Cn);
                    Da = new SqlDataAdapter(Cmd);
                    Ds = new DataSet();
                    Cn.Open();
                    Da.Fill(Ds, "Producto");

                    bodyH = bodyH.Replace("@EquipoDelClie", Ds.Tables["Producto"].Rows[0]["itemName"].ToString());
                    bodyH = bodyH.Replace("@Serie", Llamada.SerieFabricante); //bodyH.Replace("@Serie", Ds.Tables["Producto"].Rows[0]["manufSN"].ToString());
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
                    // Set options
                    converter.Options.MaxPageLoadTime = 120; // Set timeout to 120 seconds
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

                    var Agregado = G.ObtenerConfig("Empresa") == "G" ? Llamada.SinRepuestos == true ? "<b style='font-size: 15px;'>   Todos nuestros equipos funcionan como un conjunto con sus accesorios (manguera, pistola, lanza y boquillas de alta presión). Muchos de los fallos del equipo pueden ser ocasionados por dichos accesorios. Por esta razón la reparación solo cuenta con garantía si se entregan los accesorios porque solo así podemos garantizar que el equipo funciona correctamente. </b>" : "" : "";
                    var resp = G.SendV2(correo, "", "", CorreoEnvio.RecepcionEmail, "Contrato de Servicio", "Contrato de Servicio para el cliente", "<!DOCTYPE html> <html> <head> <meta charset='utf-8'> <meta name='viewport' content='width=device-width, initial-scale=1'> <title></title> </head> <body> <h1>Contrato de servicio</h1> <p> En el presente correo se le hace entrega del contrato de servicio, favor no responder a este correo </p> </br> "+Agregado+" </body> </html>", CorreoEnvio.RecepcionHostName, CorreoEnvio.EnvioPort, CorreoEnvio.RecepcionUseSSL, CorreoEnvio.RecepcionEmail, CorreoEnvio.RecepcionPassword, adjuntos);

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
            var t = db.Database.BeginTransaction();
            try
            {
                if (llamada == null)
                {
                    throw new Exception("El objeto llamada viene null");
                }
                var FechaInicio = DateTime.Now.Date;
                var FechaFinal = DateTime.Now.AddDays(1).AddSeconds(-1);
                var Parametros = db.Parametros.FirstOrDefault();
                var Llamada = db.LlamadasServicios.Where(a => a.id == llamada.id 
                || (a.CardCode == llamada.CardCode 
                && a.ItemCode == llamada.ItemCode 
                && a.SerieFabricante == llamada.SerieFabricante
                && a.FechaCreacion >= FechaInicio && a.FechaCreacion <= FechaFinal
                )).FirstOrDefault();

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
                    Llamada.PIN = false;
                    Llamada.SinRepuestos = llamada.SinRepuestos;
                    Llamada.Prioridad = llamada.Prioridad;
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
                        LogModificaciones log = new LogModificaciones();
                        log.idLlamada = Llamada.id;
                        log.idUsuario = Convert.ToInt32(((ClaimsIdentity)User.Identity).Claims.Where(d => d.Type == ClaimTypes.Name).Select(s1 => s1.Value).FirstOrDefault());
                        log.Accion = "Usuario con el id " + log.idUsuario + " ha creado la llamada a la hora respectiva";
                        log.Fecha = DateTime.Now;
                        db.LogModificaciones.Add(log);
                        db.SaveChanges();
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
                        client.Priority = Llamada.Prioridad == "L" ? BoSvcCallPriorities.scp_Low : Llamada.Prioridad == "M" ? BoSvcCallPriorities.scp_Medium : BoSvcCallPriorities.scp_High;


                        var respuesta = client.Add();

                        if (respuesta == 0)
                        {

                            db.Entry(Llamada).State = EntityState.Modified;
                            Llamada.DocEntry = 0;
                            Llamada.DocNum = 0;
                            try
                            {
                                Llamada.DocEntry = Convert.ToInt32(Conexion.Company.GetNewObjectKey());
                                throw new Exception("");
                            }
                            catch (Exception)

                            {
                                try
                                {
                                    var conexion = g.DevuelveCadena(db);
                                    var filtroSQL = "customer = '" + Llamada.CardCode + "' and itemCode = '" + Llamada.ItemCode + "' and subject like '%" + Llamada.Asunto.TrimEnd() + "%' and createDate = '" + Llamada.FechaCreacion.Date + "' and manufSN = '" + Llamada.SerieFabricante + "'";
                                    var SQL = Parametros.SQLDocNum.Replace("callID = @reemplazo", filtroSQL);

                                    SqlConnection Cn = new SqlConnection(conexion);
                                    SqlCommand Cmd = new SqlCommand(SQL, Cn);
                                    SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                                    DataSet Ds = new DataSet();
                                    Cn.Open();
                                    Da.Fill(Ds, "DocNum1");
                                    Llamada.DocEntry = Convert.ToInt32(Ds.Tables["DocNum1"].Rows[0]["callID"]);

                                    Cn.Close();
                                }
                                catch (Exception ex1)
                                {
                                    BitacoraErrores be = new BitacoraErrores();

                                    be.Descripcion = "Error en la llamada #" + Llamada.id + " con el callID " + Llamada.DocEntry + " , al conseguir el docEntry -> " + ex1.Message;
                                    be.StackTrace = ex1.StackTrace;
                                    be.Fecha = DateTime.Now;

                                    db.BitacoraErrores.Add(be);
                                    db.SaveChanges();

                                }


                            }
                            if (Llamada.DocEntry != null || Llamada.DocEntry != 0)
                            {
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

                                    be.Descripcion = "Error en la llamada #" + Llamada.id + " con el callID " + Llamada.DocEntry + " -> " + ex2.Message;
                                    be.StackTrace = ex2.StackTrace;
                                    be.Fecha = DateTime.Now;

                                    db.BitacoraErrores.Add(be);
                                    db.SaveChanges();
                                }
                            }


                            if (Llamada.DocEntry != 0 && Llamada.DocEntry != null)
                            {
                                Llamada.ProcesadaSAP = true;

                            }

                            db.SaveChanges();


                            Conexion.Desconectar();

                            t.Commit();



                        }
                        else
                        {
                            BitacoraErrores be = new BitacoraErrores();

                            be.Descripcion = "Error en la llamada #" + Llamada.id + " con el callID " + Llamada.DocEntry + " -> " + Conexion.Company.GetLastErrorDescription();
                            be.StackTrace = "Llamada de Servicio";
                            be.Fecha = DateTime.Now;

                            db.BitacoraErrores.Add(be);
                            db.SaveChanges();
                            Conexion.Desconectar();
                            throw new Exception(be.Descripcion);
                        }



                    }
                    catch (Exception ex1)
                    {

                        Conexion.Desconectar();
                        BitacoraErrores be = new BitacoraErrores();
                        try
                        {
                            be.Descripcion = "Error en la llamada #" + Llamada.id + " -> " + ex1.Message + " -> " + Conexion.Company.GetNewObjectKey();
                        }
                        catch (Exception)
                        {

                        be.Descripcion = "Error en la llamada #" + Llamada.id + " con el callID " + Llamada.DocEntry +" -> " + ex1.Message;
                        be.StackTrace = ex1.StackTrace;
                        be.Fecha = DateTime.Now;

                        db.BitacoraErrores.Add(be);
                        db.SaveChanges();
                        throw new Exception(ex1.Message);
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
                t.Rollback();
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

                if (Llamada != null)
                {


                    db.Entry(Llamada).State = EntityState.Modified;
                    if (llamada.Status != Llamada.Status && llamada.Status != 0 && llamada.Status != null)
                    {
                        try
                        {
                            var StatusConvertido = Llamada.Status.ToString();
                            var StatusInicial = db.Status.Where(a => a.idSAP == StatusConvertido).FirstOrDefault() != null ? db.Status.Where(a => a.idSAP == StatusConvertido).FirstOrDefault().Nombre : "" ;
                            StatusConvertido = llamada.Status.ToString();
                            var StatusFinal = db.Status.Where(a => a.idSAP == StatusConvertido).FirstOrDefault() != null ? db.Status.Where(a => a.idSAP == StatusConvertido).FirstOrDefault().Nombre : "";
                            LogModificaciones log = new LogModificaciones();
                            log.idLlamada = Llamada.id;
                            log.idUsuario = Convert.ToInt32(((ClaimsIdentity)User.Identity).Claims.Where(d => d.Type == ClaimTypes.Name).Select(s1 => s1.Value).FirstOrDefault());
                            log.Accion = "Usuario con el id " + log.idUsuario + " ha modificado la llamada del Status " + StatusInicial + " al Status "+ StatusFinal +" a la hora respectiva";
                            log.Fecha = DateTime.Now;
                            db.LogModificaciones.Add(log);
                            db.SaveChanges();
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

                    if (Llamada.TipoCaso != llamada.TipoCaso && llamada.TipoCaso != 0 && llamada.TipoCaso != null)
                    {
                        var TipoCasoAnterior = Llamada.TipoCaso;
                        Llamada.TipoCaso = llamada.TipoCaso;
                        if(Llamada.TipoCaso == null || Llamada.TipoCaso == 0)
                        {
                            Llamada.TipoCaso = TipoCasoAnterior;
                        }
                    }

                    DateTime time = new DateTime();

                    if (llamada.FechaSISO != time && llamada.FechaSISO != Llamada.FechaSISO && llamada.FechaSISO != null)
                    {
                        Llamada.FechaSISO = llamada.FechaSISO;

                    }

                    if (llamada.LugarReparacion != Llamada.LugarReparacion && llamada.LugarReparacion != null)
                    {
                        Llamada.LugarReparacion = llamada.LugarReparacion;

                    }

                    if ( llamada.SucRecibo != null && llamada.SucRecibo != Llamada.SucRecibo)
                    {
                        var SucReb = llamada.SucRecibo.Value.ToString();
                        Llamada.SucRecibo = llamada.SucRecibo.Value;

                    }

                    if ( llamada.SucRetiro != null && llamada.SucRetiro != Llamada.SucRetiro)
                    {
                        var SucRet = llamada.SucRetiro.Value.ToString();
                        Llamada.SucRetiro = llamada.SucRetiro;

                    }

                    if (!string.IsNullOrEmpty(llamada.Comentarios) && Llamada.Comentarios != llamada.Comentarios)
                    {
                        Llamada.Comentarios = llamada.Comentarios;

                    }


                    if (llamada.TratadoPor != null && llamada.TratadoPor != Llamada.TratadoPor)
                    {
                        Llamada.TratadoPor = llamada.TratadoPor;

                    }

                    if (llamada.Garantia != null && Llamada.Garantia != llamada.Garantia)
                    {
                        Llamada.Garantia = llamada.Garantia;


                    }

                    if (llamada.Horas != null && (llamada.Horas > 0 && Llamada.Horas >= 0) && Llamada.Horas != llamada.Horas)
                    {
                        Llamada.Horas = llamada.Horas;

                    }

                    if (llamada.Tecnico != null && Llamada.Tecnico != llamada.Tecnico)
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

                            be.Descripcion = "Error en la llamada #" + Llamada.id  +" con el callID " + Llamada.DocEntry + " -> " + ex3.Message;
                            be.StackTrace = ex3.StackTrace;
                            be.Fecha = DateTime.Now;

                            db.BitacoraErrores.Add(be);
                            db.SaveChanges();
                            throw new Exception(be.Descripcion);
                        }

                    }

                    if (!string.IsNullOrEmpty(llamada.PersonaContacto) && Llamada.PersonaContacto != llamada.PersonaContacto)
                    {
                        Llamada.PersonaContacto = llamada.PersonaContacto;
                    }

                    if (!string.IsNullOrEmpty(llamada.EmailPersonaContacto) && Llamada.EmailPersonaContacto != llamada.EmailPersonaContacto)
                    {
                        Llamada.EmailPersonaContacto = llamada.EmailPersonaContacto;
                    }

                    if (!string.IsNullOrEmpty(llamada.NumeroPersonaContacto) && Llamada.NumeroPersonaContacto != llamada.NumeroPersonaContacto)
                    {
                        Llamada.NumeroPersonaContacto = llamada.NumeroPersonaContacto;
                    }

                    if(llamada.PIN != null)
                    {
                        Llamada.PIN = llamada.PIN;
                    }
                    
                    if(!string.IsNullOrEmpty(llamada.Prioridad) && Llamada.Prioridad != llamada.Prioridad)
                    {
                        Llamada.Prioridad = llamada.Prioridad;
                    }
                    Llamada.ProcesadaSAP = false;
                    db.SaveChanges();
                    var enc2 = db.EncReparacion.Where(a => a.idLlamada == llamada.id).FirstOrDefault();

                    if(llamada.Adjuntos != null)
                    {
                        foreach (var item in llamada.Adjuntos)
                        {
                            Adjuntos adjunto = new Adjuntos();
                            adjunto.idEncabezado = enc2.id;

                            byte[] hex = Convert.FromBase64String(item.base64.Replace("data:image/jpeg;base64,", "").Replace("data:image/png;base64,", ""));
                            adjunto.base64 = hex;
                            db.Adjuntos.Add(adjunto);
                            db.SaveChanges();
                        }
                    }

                    if (llamada.AdjuntosIdentificacion != null)
                    {
                        List<System.Net.Mail.Attachment> adjuntos = new List<System.Net.Mail.Attachment>();
                        foreach (var item in llamada.AdjuntosIdentificacion)
                        {
                           
                            AdjuntosIdentificacion adjunto = new AdjuntosIdentificacion();
                            adjunto.idEncabezado = enc2.id;

                            byte[] hex = Convert.FromBase64String(item.base64.Replace("data:image/jpeg;base64,", "").Replace("data:image/png;base64,", ""));
                            adjunto.base64 = hex;
                            db.AdjuntosIdentificacion.Add(adjunto);
                            db.SaveChanges();

                            System.Net.Mail.Attachment att2 = new System.Net.Mail.Attachment(new MemoryStream(adjunto.base64),  adjunto.id +".png");
                            adjuntos.Add(att2);
                        }

                        if(llamada.Status == -1)
                        {
                            var CorreoEnvio = db.CorreoEnvio.FirstOrDefault();
                            
                            var resp = G.SendV2(Llamada.EmailPersonaContacto, "", G.ObtenerConfig("CorreoEmpresa"), CorreoEnvio.RecepcionEmail, "Contrato de Servicio", "Contrato de Servicio para el cliente", "<!DOCTYPE html> <html> <head> <meta charset='utf-8'> <meta name='viewport' content='width=device-width, initial-scale=1'> <title></title> </head> <body> <h1>Contrato de servicio</h1> <p> En el presente correo se le hace constatar que el equipo se ha entregado correctamente del contrato de servicio, favor no responder a este correo </p> </body> </html>", CorreoEnvio.RecepcionHostName, CorreoEnvio.EnvioPort, CorreoEnvio.RecepcionUseSSL, CorreoEnvio.RecepcionEmail, CorreoEnvio.RecepcionPassword, adjuntos);

                            if (!resp)
                            {
                                throw new Exception("No se ha podido enviar el correo con la liquidación");
                            }
                        }
                    }


                    try
                    {
                        var client = (ServiceCalls)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oServiceCalls);

                        if (client.GetByKey(Llamada.DocEntry.Value))
                        {

                            client.Status = Llamada.Status.Value;

                            //if (!string.IsNullOrEmpty(llamada.CardCode))
                            //{

                                client.CustomerCode = Llamada.CardCode;

                            //}




                            if (!string.IsNullOrEmpty(llamada.Asunto))
                            {
                                client.Subject = Llamada.Asunto;

                            }

                            client.Priority = Llamada.Prioridad == "L" ? BoSvcCallPriorities.scp_Low : Llamada.Prioridad == "M" ? BoSvcCallPriorities.scp_Medium : BoSvcCallPriorities.scp_High;
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
                                if (Reparacion != null)
                                {
                                    var NumLlamada = Llamada.DocEntry.ToString();
                                    var EncMovimiento = db.EncMovimiento.Where(a => a.NumLlamada == NumLlamada && a.Aprobada).FirstOrDefault();
                                    if (EncMovimiento == null)
                                    {
                                        EncMovimiento = db.EncMovimiento.Where(a => a.NumLlamada == NumLlamada && a.TipoMovimiento == 2).FirstOrDefault();
                                    }
                                    if (EncMovimiento != null)
                                    {
                                        var DetalleMovimiento = db.DetMovimiento.Where(a => a.idEncabezado == EncMovimiento.id).ToList();
                                        var diagnosticosComentario = "";
                                        foreach (var item in DetalleMovimiento)
                                        {
                                            diagnosticosComentario += db.Errores.Where(a => a.id == item.idError).FirstOrDefault() == null ? "" : db.Errores.Where(a => a.id == item.idError).FirstOrDefault().Diagnostico + "\n";
                                        }
                                        diagnosticosComentario += EncMovimiento.Comentarios + "\n";
                                        client.Resolution = string.IsNullOrEmpty(diagnosticosComentario.Replace("\n", "").Replace("\r", "").Trim()) ? "Favor revisar operaciones" : diagnosticosComentario;

                                    }
                                    else
                                    {
                                        client.Resolution = string.IsNullOrEmpty(Llamada.Comentarios.Replace("\n", "").Replace("\r", "").Trim()) ? "Favor revisar operaciones" : Llamada.Comentarios;

                                    }

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

                                be.Descripcion = "Error en la llamada #" + Llamada.id + " con el callID " + Llamada.DocEntry + " -> " + ex3.Message;
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

                                be.Descripcion = "Error en la llamada #" + Llamada.id + " con el callID " + Llamada.DocEntry  + "-> " + Conexion.Company.GetLastErrorDescription();
                                be.StackTrace = "Llamada de Servicio";
                                be.Fecha = DateTime.Now;

                                db.BitacoraErrores.Add(be);
                                db.SaveChanges();
                                Conexion.Desconectar();
                                throw new Exception(be.Descripcion);
                            }



                        }
                    }
                    catch (Exception ex)
                    {
                        BitacoraErrores be = new BitacoraErrores();

                        be.Descripcion = "Error en la llamada #" + Llamada.id + " con el callID " + Llamada.DocEntry + " -> " + ex.Message;
                        be.StackTrace = ex.StackTrace;
                        be.Fecha = DateTime.Now;

                        db.BitacoraErrores.Add(be);
                        db.SaveChanges();
                        throw new Exception(ex.Message);

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
                var Llamada = db.LlamadasServicios.Where(a => a.id == llamada.id && a.ProcesadaSAP == false).FirstOrDefault();

                if (Llamada != null)
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
                            Llamada.DocEntry = 0;
                            Llamada.DocNum = 0;

                            try
                            {
                                var conexion = g.DevuelveCadena(db);
                                var filtroSQL = "customer = '" + Llamada.CardCode + "' and itemCode = '" + Llamada.ItemCode + "' and subject like '%" + Llamada.Asunto + "%' and createDate = '" + Llamada.FechaCreacion.Date + "' and manufSN = '" + Llamada.SerieFabricante + "'";
                                var SQL = Parametros.SQLDocNum.Replace("callID = @reemplazo", filtroSQL);

                                SqlConnection Cn = new SqlConnection(conexion);
                                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                                DataSet Ds = new DataSet();
                                Cn.Open();
                                Da.Fill(Ds, "DocNum1");
                                Llamada.DocEntry = Convert.ToInt32(Ds.Tables["DocNum1"].Rows[0]["callID"]);

                                Cn.Close();
                            }
                            catch (Exception)
                            {


                            }

                            try
                            {
                                if (Llamada.DocEntry == 0)
                                {
                                    Llamada.DocEntry = Convert.ToInt32(Conexion.Company.GetNewObjectKey());

                                }

                            }
                            catch (Exception)

                            {
                                try
                                {
                                    var conexion = g.DevuelveCadena(db);
                                    var filtroSQL = "customer = '" + Llamada.CardCode + "' and itemCode = '" + Llamada.ItemCode + "' and subject like '%" + Llamada.Asunto + "%' and createDate = '" + Llamada.FechaCreacion.Date + "'";
                                    var SQL = Parametros.SQLDocNum.Replace("callID = @reemplazo", filtroSQL);

                                    SqlConnection Cn = new SqlConnection(conexion);
                                    SqlCommand Cmd = new SqlCommand(SQL, Cn);
                                    SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                                    DataSet Ds = new DataSet();
                                    Cn.Open();
                                    Da.Fill(Ds, "DocNum1");
                                    Llamada.DocEntry = Convert.ToInt32(Ds.Tables["DocNum1"].Rows[0]["callID"]);

                                    Cn.Close();
                                }
                                catch (Exception)
                                {


                                }


                            }


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

                                be.Descripcion = "Error en la llamada #" + Llamada.id + " con el callID " + Llamada.DocEntry + " -> " + ex2.Message;
                                be.StackTrace = ex2.StackTrace;
                                be.Fecha = DateTime.Now;

                                db.BitacoraErrores.Add(be);
                                db.SaveChanges();
                            }
                            if (Llamada.DocEntry != 0)
                            {
                                Llamada.ProcesadaSAP = true;

                            }

                            db.SaveChanges();


                            Conexion.Desconectar();

                        }
                        else
                        {
                            BitacoraErrores be = new BitacoraErrores();

                            be.Descripcion = "Error en la llamada #" + Llamada.id + " con el callID " + Llamada.DocEntry + " -> " + Conexion.Company.GetLastErrorDescription();
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

                        be.Descripcion = "Error en la llamada #" + Llamada.id + " con el callID " + Llamada.DocEntry + " -> " + ex1.Message;
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