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

    public class MovimientosController: ApiController
    {
        G g = new G();
        ModelCliente db = new ModelCliente();


        [Route("api/Movimientos/Reenvio")]
        [HttpGet]
        public HttpResponseMessage GetCorreo([FromUri]int id, string correo)
        {
            try
            {///Alt + 125 }
                var EncMovimiento = db.EncMovimiento.Where(a => a.id == id).FirstOrDefault();

                if(EncMovimiento != null)
                {
                    if (EncMovimiento.TipoMovimiento == 1)
                    {
                         

                            ////Enviar Correo
                            ///
                            try
                            {
                                var EmailDestino = "";
                                Parametros parametros = db.Parametros.FirstOrDefault();
                                var CorreoEnvio = db.CorreoEnvio.FirstOrDefault();
                                var conexion = g.DevuelveCadena(db);

                                var SQL = parametros.HtmlLlamada + "'" + EncMovimiento.CardCode + "'";

                                SqlConnection Cn = new SqlConnection(conexion);
                                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                                DataSet Ds = new DataSet();
                                Cn.Open();
                                Da.Fill(Ds, "Encabezado");

                                List<System.Net.Mail.Attachment> adjuntos = new List<System.Net.Mail.Attachment>();
                                html Html = new html();
                                var bodyH = Html.textoOferta;
                                bodyH = bodyH.Replace("@NombreCliente", Ds.Tables["Encabezado"].Rows[0]["CardName"].ToString());
                                bodyH = bodyH.Replace("@NombreCliente2", Ds.Tables["Encabezado"].Rows[0]["CardName"].ToString());
                                bodyH = bodyH.Replace("@Email", Ds.Tables["Encabezado"].Rows[0]["E_Mail"].ToString());


                                bodyH = bodyH.Replace("@TelefonoCliente", Ds.Tables["Encabezado"].Rows[0]["Phone1"].ToString());

                                bodyH = bodyH.Replace("@DocEntry", EncMovimiento.DocEntry.ToString());



                                bodyH = bodyH.Replace("@Fecha", EncMovimiento.Fecha.ToString("dd/MM/yyyy"));
                                EmailDestino = Ds.Tables["Encabezado"].Rows[0]["E_Mail"].ToString();

                                bodyH = bodyH.Replace("@PorDesc", EncMovimiento.PorDescuento.ToString());

                                bodyH = bodyH.Replace("@Subtotal", EncMovimiento.Subtotal.ToString());
                                bodyH = bodyH.Replace("@Descuento", EncMovimiento.Descuento.ToString());
                                bodyH = bodyH.Replace("@Impuestos", EncMovimiento.Impuestos.ToString());
                                bodyH = bodyH.Replace("@Total", EncMovimiento.TotalComprobante.ToString());



                                Cn.Close();
                                Cn.Dispose();

                                var inyectado = "";
                                var z = 0;
                                var top1 = 290;

                                var diagnosticos = "";
                            var DetalleSAP = db.DetMovimiento.Where(a => a.idEncabezado == id).ToList();
                                foreach (var item in DetalleSAP)
                                {
                                    if (z == 0)
                                    {

                                        inyectado = Html.InyectadoOferta.Replace("@NumLinea", (z + 1).ToString()).Replace("@ItemCode", item.ItemCode).Replace("@ItemName", item.ItemName).Replace("@Cantidad", item.Cantidad.ToString()).Replace("@PrecioUnitario", item.PrecioUnitario.ToString()).Replace("@TotalLinea", item.TotalLinea.ToString()).Replace("top1", top1.ToString()).Replace("top2", top1.ToString()).Replace("top3", top1.ToString()).Replace("top4", top1.ToString()).Replace("top5", top1.ToString()).Replace("top5", top1.ToString());
                                        diagnosticos += db.Errores.Where(a => a.id == item.idError).FirstOrDefault() == null ? "" : db.Errores.Where(a => a.id == item.idError).FirstOrDefault().Diagnostico + "<br/>";
                                    }
                                    else
                                    {
                                        top1 += 20;

                                        inyectado += Html.InyectadoOferta.Replace("@NumLinea", (z + 1).ToString()).Replace("@ItemCode", item.ItemCode).Replace("@ItemName", item.ItemName).Replace("@Cantidad", item.Cantidad.ToString()).Replace("@PrecioUnitario", item.PrecioUnitario.ToString()).Replace("@TotalLinea", item.TotalLinea.ToString()).Replace("top1", top1.ToString()).Replace("top2", top1.ToString()).Replace("top3", top1.ToString()).Replace("top4", top1.ToString()).Replace("top5", top1.ToString()).Replace("top6", top1.ToString());
                                        diagnosticos += db.Errores.Where(a => a.id == item.idError).FirstOrDefault() == null ? "" : db.Errores.Where(a => a.id == item.idError).FirstOrDefault().Diagnostico + "<br/>";

                                    }


                                    z++;
                                }

                                diagnosticos += EncMovimiento.Comentarios + "<br/>";
                                bodyH = bodyH.Replace("@INYECTADO", inyectado);


                                bodyH = bodyH.Replace("@Diagnosticos", diagnosticos);



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

                                System.Net.Mail.Attachment att3 = new System.Net.Mail.Attachment(new MemoryStream(bytes), "Oferta_Venta.pdf");
                                adjuntos.Add(att3);

                                var NumLlamada = Convert.ToInt32(EncMovimiento.NumLlamada);
                                var Llamada = db.LlamadasServicios.Where(a => a.DocEntry == NumLlamada).FirstOrDefault();
                                var EncReparacion = db.EncReparacion.Where(a => a.idLlamada == Llamada.id).FirstOrDefault();
                                var Adjuntos = db.Adjuntos.Where(a => a.idEncabezado == EncReparacion.id).ToList();
                                var ui = 1;
                                foreach (var det in Adjuntos)
                                {

                                    {
                                        System.Net.Mail.Attachment att2 = new System.Net.Mail.Attachment(new MemoryStream(det.base64), ui.ToString() + ".png");
                                        adjuntos.Add(att2);
                                        ui++;
                                    }
                                }


                                var resp = G.SendV2(correo, "larce@dydconsultorescr.com", "", CorreoEnvio.RecepcionEmail, "Oferta de Venta", "Oferta de Venta", "<!DOCTYPE html> <html> <head> <meta charset='utf-8'> <meta name='viewport' content='width=device-width, initial-scale=1'> <title></title> </head> <body> <h1>Oferta de Venta</h1> <p> En el presente correo se le hace el envio de la ofeta de venta, favor no responder a este correo </p> </body> </html>", CorreoEnvio.RecepcionHostName, CorreoEnvio.EnvioPort, CorreoEnvio.RecepcionUseSSL, CorreoEnvio.RecepcionEmail, CorreoEnvio.RecepcionPassword, adjuntos);

                                g.GuardarTxt("html.txt", bodyH);

                                if (!resp)
                                {
                                    throw new Exception("No se ha podido enviar el correo con la oferta de venta");
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

                    }
                    else
                    {
                        
                            


                            ////Enviar Correo
                            ///
                            try
                            {
                                var EmailDestino = "";
                                Parametros parametros = db.Parametros.FirstOrDefault();
                                var CorreoEnvio = db.CorreoEnvio.FirstOrDefault();
                                var conexion = g.DevuelveCadena(db);

                                var SQL = parametros.HtmlLlamada + "'" + EncMovimiento.CardCode + "'";

                                SqlConnection Cn = new SqlConnection(conexion);
                                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                                DataSet Ds = new DataSet();
                                Cn.Open();
                                Da.Fill(Ds, "Encabezado");

                                List<System.Net.Mail.Attachment> adjuntos = new List<System.Net.Mail.Attachment>();
                                html Html = new html();
                                var bodyH = Html.textoEntrega;
                                bodyH = bodyH.Replace("@NombreCliente", Ds.Tables["Encabezado"].Rows[0]["CardName"].ToString());
                                bodyH = bodyH.Replace("@Telefono", Ds.Tables["Encabezado"].Rows[0]["Phone1"].ToString());
                                bodyH = bodyH.Replace("@Celular", "      ");
                                bodyH = bodyH.Replace("@DocEntry", EncMovimiento.DocEntry.ToString());
                                bodyH = bodyH.Replace("@NumBoleta", EncMovimiento.NumLlamada);


                                bodyH = bodyH.Replace("@Fecha", EncMovimiento.Fecha.ToString("dd/MM/yyyy"));
                                EmailDestino = Ds.Tables["Encabezado"].Rows[0]["E_Mail"].ToString();

                                bodyH = bodyH.Replace("@NumContacto", Ds.Tables["Encabezado"].Rows[0]["Tel1"].ToString());

                                bodyH = bodyH.Replace("@SubTotal", EncMovimiento.Subtotal.ToString());
                                bodyH = bodyH.Replace("@Descuento", EncMovimiento.Descuento.ToString());
                                bodyH = bodyH.Replace("@Impuestos", EncMovimiento.Impuestos.ToString());
                                bodyH = bodyH.Replace("@TotalEntrega", EncMovimiento.TotalComprobante.ToString());



                                Cn.Close();
                                Cn.Dispose();

                                var inyectado = "";
                                var z = 0;
                                var top1 = 454;
                                var top2 = 453;

                            var DetalleSAP = db.DetMovimiento.Where(a => a.idEncabezado == id).ToList();

                            foreach (var item in DetalleSAP)
                                {
                                    if (z == 0)
                                    {
                                        inyectado = Html.Inyectado.Replace("@ItemCode", item.ItemCode).Replace("@ItemName", item.ItemName).Replace("@Cantidad", item.Cantidad.ToString()).Replace("@PrecioUnitario", item.PrecioUnitario.ToString()).Replace("@TotalLinea", item.TotalLinea.ToString()).Replace("@Top1", top1.ToString()).Replace("@Top1.1", top1.ToString()).Replace("@Top1.2", top1.ToString()).Replace("@Top1.3", top1.ToString()).Replace("@Top2", top2.ToString());

                                    }
                                    else
                                    {
                                        top1 += 23;
                                        top2 += 23;
                                        inyectado += Html.Inyectado.Replace("@ItemCode", item.ItemCode).Replace("@ItemName", item.ItemName).Replace("@Cantidad", item.Cantidad.ToString()).Replace("@PrecioUnitario", item.PrecioUnitario.ToString()).Replace("@TotalLinea", item.TotalLinea.ToString()).Replace("@Top1", top1.ToString()).Replace("@Top1.1", top1.ToString()).Replace("@Top1.2", top1.ToString()).Replace("@Top1.3", top1.ToString()).Replace("@Top2", top2.ToString());
                                    }


                                    z++;
                                }
                                bodyH = bodyH.Replace("@Inyectado", inyectado);





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

                                System.Net.Mail.Attachment att3 = new System.Net.Mail.Attachment(new MemoryStream(bytes), "Entrega_Mercaderia.pdf");
                                adjuntos.Add(att3);

                                var NumLlamada = Convert.ToInt32(EncMovimiento.NumLlamada);
                                var Llamada = db.LlamadasServicios.Where(a => a.DocEntry == NumLlamada).FirstOrDefault();
                                var EncReparacion = db.EncReparacion.Where(a => a.idLlamada == Llamada.id).FirstOrDefault();
                                var Adjuntos = db.Adjuntos.Where(a => a.idEncabezado == EncReparacion.id).ToList();
                                var ui = 1;
                                foreach (var det in Adjuntos)
                                {

                                    {
                                        System.Net.Mail.Attachment att2 = new System.Net.Mail.Attachment(new MemoryStream(det.base64), ui.ToString() + ".png");
                                        adjuntos.Add(att2);
                                        ui++;
                                    }
                                }


                                var resp = G.SendV2(correo, "larce@dydconsultorescr.com", "", CorreoEnvio.RecepcionEmail, "Entrega de Mercaderia", "Entrega de Producto", "<!DOCTYPE html> <html> <head> <meta charset='utf-8'> <meta name='viewport' content='width=device-width, initial-scale=1'> <title></title> </head> <body> <h1>Entrega Mercaderia</h1> <p> En el presente correo se le hace el envio de la entrega de mercaderia, favor no responder a este correo </p> </body> </html>", CorreoEnvio.RecepcionHostName, CorreoEnvio.EnvioPort, CorreoEnvio.RecepcionUseSSL, CorreoEnvio.RecepcionEmail, CorreoEnvio.RecepcionPassword, adjuntos);

                                g.GuardarTxt("html.txt", bodyH);

                                if (!resp)
                                {
                                    throw new Exception("No se ha podido enviar el correo con la entrega");
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


                        
                    }
                }
                else
                {
                    throw new Exception("No existe el encabezado");
                }

                return Request.CreateResponse(HttpStatusCode.OK);

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }


        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {///Alt + 125 }
                var time = new DateTime();
                if (filtro.FechaFinal != time)
                {
                    filtro.FechaFinal = filtro.FechaFinal.AddDays(1);
                }

                var encMovimientos = db.EncMovimiento.Where(a => (filtro.FechaInicial != time ? a.Fecha >= filtro.FechaInicial : true) && (filtro.FechaFinal != time ? a.Fecha <= filtro.FechaFinal : true))
                    .Select(a => new
                    {
                        a.id,
                        a.DocEntry,
                        a.CardCode,
                        a.CardName,
                        a.NumLlamada,
                        a.Fecha,
                        a.TipoMovimiento,
                        a.Comentarios,
                        a.CreadoPor,
                        a.Subtotal,
                        a.PorDescuento,
                        a.Descuento,
                        a.Impuestos,
                        a.TotalComprobante,
                        Detalle = db.DetMovimiento.Where(b => b.idEncabezado == a.id).ToList()

                    }

                        ).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, encMovimientos);

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }


        [Route("api/Movimientos/Consultar")]
        public HttpResponseMessage GetOne([FromUri]int id)
        {
            try
            {



                var EncMovimiento = db.EncMovimiento.Where(a => a.id == id).Select(a => new
                {
                    a.id,
                    a.DocEntry,

                    a.CardCode,
                    a.CardName,
                    a.NumLlamada,
                    a.Fecha,
                    a.TipoMovimiento,
                    a.Comentarios,
                    a.CreadoPor,
                    a.Subtotal,
                    a.PorDescuento,
                    a.Descuento,
                    a.Impuestos,
                    a.TotalComprobante,
                    Detalle = db.DetMovimiento.Where(b => b.idEncabezado == a.id).ToList()

                }

                        ).FirstOrDefault();


                if (EncMovimiento == null)
                {
                    throw new Exception("Este EncMovimiento no se encuentra registrado");
                }

                return Request.CreateResponse(HttpStatusCode.OK, EncMovimiento);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }


        [HttpPost]
        [Route("api/Movimientos/Actualizar")]
        public HttpResponseMessage Put([FromBody] ColeccionMovimientos encMovimiento)
        {
            try
            {


                var EncMovimiento = db.EncMovimiento.Where(a => a.id == encMovimiento.id).FirstOrDefault();

                if (EncMovimiento != null)
                {
                    db.Entry(EncMovimiento).State = EntityState.Modified;
                    EncMovimiento.CreadoPor = encMovimiento.CreadoPor;
                    EncMovimiento.Descuento = encMovimiento.Descuento;
                    EncMovimiento.Impuestos = encMovimiento.Impuestos;
                    EncMovimiento.Subtotal = encMovimiento.Subtotal;
                    EncMovimiento.PorDescuento = encMovimiento.PorDescuento;
                    EncMovimiento.TotalComprobante = encMovimiento.TotalComprobante;
                    EncMovimiento.Comentarios = encMovimiento.Comentarios;
                    db.SaveChanges();


                    foreach(var item in encMovimiento.Detalle)
                    {
                        var Det = db.DetMovimiento.Where(a => a.id == item.id).FirstOrDefault();
                        if(Det != null)
                        {
                            db.Entry(Det).State = EntityState.Modified;
                            Det.PrecioUnitario = item.PrecioUnitario;
                            Det.Cantidad = item.Cantidad;
                            Det.PorDescuento = item.PorDescuento;
                            Det.Descuento = item.Descuento;
                            Det.Impuestos = item.Impuestos;
                            Det.TotalLinea = item.TotalLinea;
                            db.SaveChanges();
                        }
                        
                    }
                }
                else
                {
                    throw new Exception("EncMovimiento no existe");
                }


                if(encMovimiento.Generar)
                {
                    if(EncMovimiento.TipoMovimiento == 1)
                    {
                        var client = (Documents)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oQuotations);
                        client.DocObjectCode = BoObjectTypes.oQuotations;
                        client.CardCode = EncMovimiento.CardCode;
                        client.DocCurrency =   "COL";
                        client.DocDate = EncMovimiento.Fecha; //listo
                        client.DocDueDate = EncMovimiento.Fecha.AddDays(3); //listo
                        client.DocNum = 0; //automatico
                        client.DocType = BoDocumentTypes.dDocument_Items;
                        client.HandWritten = BoYesNoEnum.tNO;
                        client.NumAtCard = EncMovimiento.NumLlamada; //orderid               
                        client.ReserveInvoice = BoYesNoEnum.tNO;
                        client.Series = 11; //11 quemado
                        client.Comments = EncMovimiento.Comentarios; //direccion
                        client.DiscountPercent = Convert.ToDouble(EncMovimiento.PorDescuento); //direccion

                        client.DocumentsOwner = Convert.ToInt32(EncMovimiento.CreadoPor); //Quemado 47
                        client.UserFields.Fields.Item("U_DYD_Boleta").Value = EncMovimiento.NumLlamada.ToString() ;

                        var DetalleSAP = db.DetMovimiento.Where(a => a.idEncabezado == EncMovimiento.id).ToList();
                        var i = 0;
                        foreach (var item in DetalleSAP)
                        {
                            client.Lines.SetCurrentLine(i);
                            client.Lines.CostingCode = "";
                            client.Lines.CostingCode2 = "";
                            client.Lines.CostingCode3 = "TA-01";
                            client.Lines.CostingCode4 = "";
                            client.Lines.CostingCode5 = "";
                            client.Lines.Currency = "COL";
                            client.Lines.WarehouseCode = db.Parametros.FirstOrDefault().BodegaFinal;
                            client.Lines.DiscountPercent = Convert.ToDouble(item.PorDescuento);
                            client.Lines.ItemCode = item.ItemCode;
                            client.Lines.DiscountPercent = Convert.ToDouble(item.PorDescuento);
                            client.Lines.Quantity = Convert.ToDouble(item.Cantidad);
                            client.Lines.TaxCode = "IVA-13";
                            client.Lines.TaxOnly = BoYesNoEnum.tNO;
                            client.Lines.UnitPrice = Convert.ToDouble(item.PrecioUnitario);
                            client.Lines.Add();


                            i++;
                        }

                        var respuesta = client.Add();

                        if (respuesta == 0)
                        {
                            db.Entry(EncMovimiento).State = EntityState.Modified;
                            EncMovimiento.DocEntry = Convert.ToInt32(Conexion.Company.GetNewObjectKey());
                            db.SaveChanges();


                            

                            Conexion.Desconectar();


                            ////Enviar Correo
                            ///
                            try
                            {
                                var EmailDestino = "";
                                Parametros parametros = db.Parametros.FirstOrDefault();
                                var CorreoEnvio = db.CorreoEnvio.FirstOrDefault();
                                var conexion = g.DevuelveCadena(db);

                                var SQL = parametros.HtmlLlamada + "'" + EncMovimiento.CardCode + "'";

                                SqlConnection Cn = new SqlConnection(conexion);
                                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                                DataSet Ds = new DataSet();
                                Cn.Open();
                                Da.Fill(Ds, "Encabezado");

                                List<System.Net.Mail.Attachment> adjuntos = new List<System.Net.Mail.Attachment>();
                                html Html = new html();
                                var bodyH = Html.textoOferta;
                                bodyH = bodyH.Replace("@NombreCliente", Ds.Tables["Encabezado"].Rows[0]["CardName"].ToString()); 
                                bodyH = bodyH.Replace("@NombreCliente2", Ds.Tables["Encabezado"].Rows[0]["CardName"].ToString());
                                bodyH = bodyH.Replace("@Email", Ds.Tables["Encabezado"].Rows[0]["E_Mail"].ToString());

                                
                                 bodyH = bodyH.Replace("@TelefonoCliente", Ds.Tables["Encabezado"].Rows[0]["Phone1"].ToString());
                                
                                bodyH = bodyH.Replace("@DocEntry", EncMovimiento.DocEntry.ToString());
                             


                                bodyH = bodyH.Replace("@Fecha", EncMovimiento.Fecha.ToString("dd/MM/yyyy"));
                                EmailDestino = Ds.Tables["Encabezado"].Rows[0]["E_Mail"].ToString();

                                bodyH = bodyH.Replace("@PorDesc", EncMovimiento.PorDescuento.ToString());

                                bodyH = bodyH.Replace("@Subtotal", EncMovimiento.Subtotal.ToString());
                                bodyH = bodyH.Replace("@Descuento", EncMovimiento.Descuento.ToString());
                                bodyH = bodyH.Replace("@Impuestos", EncMovimiento.Impuestos.ToString());
                                bodyH = bodyH.Replace("@Total", EncMovimiento.TotalComprobante.ToString());



                                Cn.Close();
                                Cn.Dispose();

                                var inyectado = "";
                                var z = 0;
                                var top1 = 290;

                                var diagnosticos = "";

                                foreach (var item in DetalleSAP)
                                {
                                    if (z == 0)
                                    {
                                        
                                        inyectado = Html.InyectadoOferta.Replace("@NumLinea", (z+1).ToString()).Replace("@ItemCode", item.ItemCode).Replace("@ItemName", item.ItemName).Replace("@Cantidad", item.Cantidad.ToString()).Replace("@PrecioUnitario", item.PrecioUnitario.ToString()).Replace("@TotalLinea", item.TotalLinea.ToString()).Replace("top1", top1.ToString()).Replace("top2", top1.ToString()).Replace("top3", top1.ToString()).Replace("top4", top1.ToString()).Replace("top5", top1.ToString()).Replace("top5", top1.ToString());
                                        diagnosticos += db.Errores.Where(a => a.id == item.idError).FirstOrDefault() == null ? "" : db.Errores.Where(a => a.id == item.idError).FirstOrDefault().Diagnostico + "<br/>";
                                    }
                                    else
                                    {
                                        top1 += 20;
                                        
                                        inyectado += Html.InyectadoOferta.Replace("@NumLinea", (z + 1).ToString()).Replace("@ItemCode", item.ItemCode).Replace("@ItemName", item.ItemName).Replace("@Cantidad", item.Cantidad.ToString()).Replace("@PrecioUnitario", item.PrecioUnitario.ToString()).Replace("@TotalLinea", item.TotalLinea.ToString()).Replace("top1", top1.ToString()).Replace("top2", top1.ToString()).Replace("top3", top1.ToString()).Replace("top4", top1.ToString()).Replace("top5", top1.ToString()).Replace("top6", top1.ToString());
                                        diagnosticos += db.Errores.Where(a => a.id == item.idError).FirstOrDefault() == null ? "" : db.Errores.Where(a => a.id == item.idError).FirstOrDefault().Diagnostico + "<br/>";

                                    }


                                    z++;
                                }

                                diagnosticos += EncMovimiento.Comentarios + "<br/>";
                                bodyH = bodyH.Replace("@INYECTADO", inyectado);


                                bodyH = bodyH.Replace("@Diagnosticos", diagnosticos);



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

                                System.Net.Mail.Attachment att3 = new System.Net.Mail.Attachment(new MemoryStream(bytes), "Oferta_Venta.pdf");
                                adjuntos.Add(att3);

                                var NumLlamada = Convert.ToInt32(EncMovimiento.NumLlamada);
                                var Llamada = db.LlamadasServicios.Where(a => a.DocEntry == NumLlamada).FirstOrDefault();
                                var EncReparacion = db.EncReparacion.Where(a => a.idLlamada == Llamada.id).FirstOrDefault();
                                var Adjuntos = db.Adjuntos.Where(a => a.idEncabezado == EncReparacion.id).ToList();
                                var ui = 1;
                                foreach (var det in Adjuntos)
                                {

                                    {
                                        System.Net.Mail.Attachment att2 = new System.Net.Mail.Attachment(new MemoryStream(det.base64), ui.ToString() + ".png");
                                        adjuntos.Add(att2);
                                        ui++;
                                    }
                                }


                                var resp = G.SendV2(EmailDestino, "larce@dydconsultorescr.com", "", CorreoEnvio.RecepcionEmail, "Oferta de Venta", "Oferta de Venta", "<!DOCTYPE html> <html> <head> <meta charset='utf-8'> <meta name='viewport' content='width=device-width, initial-scale=1'> <title></title> </head> <body> <h1>Oferta de Venta</h1> <p> En el presente correo se le hace el envio de la ofeta de venta, favor no responder a este correo </p> </body> </html>", CorreoEnvio.RecepcionHostName, CorreoEnvio.EnvioPort, CorreoEnvio.RecepcionUseSSL, CorreoEnvio.RecepcionEmail, CorreoEnvio.RecepcionPassword, adjuntos);

                                g.GuardarTxt("html.txt", bodyH);

                                if (!resp)
                                {
                                    throw new Exception("No se ha podido enviar el correo con la oferta de venta");
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

                        }
                        else
                        {
                            BitacoraErrores be = new BitacoraErrores();

                            be.Descripcion = Conexion.Company.GetLastErrorDescription();
                            be.StackTrace = "Movimientos";
                            be.Fecha = DateTime.Now;

                            db.BitacoraErrores.Add(be);
                            db.SaveChanges();
                            Conexion.Desconectar();
                        }
                    }
                    else
                    {
                        var client = (Documents)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDeliveryNotes);
                        client.DocObjectCode = BoObjectTypes.oDeliveryNotes;
                        client.CardCode = EncMovimiento.CardCode;
                        client.DocCurrency = "COL";
                        client.DocDate = EncMovimiento.Fecha; //listo
                        client.DocDueDate = EncMovimiento.Fecha.AddDays(3); //listo
                        client.DocNum = 0; //automatico
                        client.DocType = BoDocumentTypes.dDocument_Items;
                        client.HandWritten = BoYesNoEnum.tNO;
                        client.NumAtCard = EncMovimiento.NumLlamada; //orderid               
                        client.ReserveInvoice = BoYesNoEnum.tNO;
                        client.Series = 3; //3 quemado
                        client.Comments = EncMovimiento.Comentarios; //direccion
                        client.DiscountPercent = Convert.ToDouble(EncMovimiento.PorDescuento); //direccion

                        client.DocumentsOwner = Convert.ToInt32(EncMovimiento.CreadoPor); //Quemado 47
                        client.UserFields.Fields.Item("U_DYD_Boleta").Value = EncMovimiento.NumLlamada.ToString();

                        var DetalleSAP = db.DetMovimiento.Where(a => a.idEncabezado == EncMovimiento.id).ToList();
                        var i = 0;
                        foreach (var item in DetalleSAP)
                        {
                            client.Lines.SetCurrentLine(i);
                            client.Lines.CostingCode = "";
                            client.Lines.CostingCode2 = "";
                            client.Lines.CostingCode3 = "TA-01";
                            client.Lines.CostingCode4 = "";
                            client.Lines.CostingCode5 = "";
                            client.Lines.Currency = "COL";
                            client.Lines.WarehouseCode = db.Parametros.FirstOrDefault().BodegaFinal;
                            client.Lines.DiscountPercent = Convert.ToDouble(item.PorDescuento);
                            client.Lines.ItemCode = item.ItemCode;
                            client.Lines.DiscountPercent = Convert.ToDouble(item.PorDescuento);
                            client.Lines.Quantity = Convert.ToDouble(item.Cantidad);
                            client.Lines.TaxCode = "IVA-13";
                            client.Lines.TaxOnly = BoYesNoEnum.tNO;
                            client.Lines.UnitPrice = Convert.ToDouble(item.PrecioUnitario);
                            client.Lines.Add();


                            i++;
                        }

                        var respuesta = client.Add();

                        if (respuesta == 0)
                        {
                            db.Entry(EncMovimiento).State = EntityState.Modified;
                            EncMovimiento.DocEntry = Convert.ToInt32(Conexion.Company.GetNewObjectKey());
                            db.SaveChanges();
                            var idEntry = Convert.ToInt32(Conexion.Company.GetNewObjectKey());
                            var client2 = (ServiceCalls)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oServiceCalls);
                            if (client2.GetByKey(Convert.ToInt32(EncMovimiento.NumLlamada)))
                            {
                                if (client2.Expenses.Count > 0)
                                {
                                    client2.Expenses.Add();
                                }
                                client2.Expenses.DocumentType = BoSvcEpxDocTypes.edt_Delivery;
                                client2.Expenses.DocumentNumber = idEntry;
                                client2.Expenses.DocEntry = idEntry;

                                if (client2.Expenses.Count == 0)
                                {
                                    client2.Expenses.Add();
                                }
                                client2.Expenses.Add();
                                var respuesta2 = client2.Update();
                                if (respuesta2 == 0)
                                {
                                    Conexion.Desconectar();
                                }
                                else
                                {
                                    BitacoraErrores be = new BitacoraErrores();

                                    be.Descripcion = Conexion.Company.GetLastErrorDescription();
                                    be.StackTrace = "Llamada de Servicio - Actualizar";
                                    be.Fecha = DateTime.Now;

                                    db.BitacoraErrores.Add(be);
                                    db.SaveChanges();
                                    Conexion.Desconectar();
                                    throw new Exception(be.Descripcion);

                                }
                            }

                            Conexion.Desconectar();



                            ////Enviar Correo
                            ///
                            try
                            {
                                var EmailDestino = "";
                                Parametros parametros = db.Parametros.FirstOrDefault();
                                var CorreoEnvio = db.CorreoEnvio.FirstOrDefault();
                                var conexion = g.DevuelveCadena(db);

                                var SQL = parametros.HtmlLlamada + "'" + EncMovimiento.CardCode + "'";

                                SqlConnection Cn = new SqlConnection(conexion);
                                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                                DataSet Ds = new DataSet();
                                Cn.Open();
                                Da.Fill(Ds, "Encabezado");

                                List<System.Net.Mail.Attachment> adjuntos = new List<System.Net.Mail.Attachment>();
                                html Html = new html();
                                var bodyH = Html.textoEntrega;
                                bodyH = bodyH.Replace("@NombreCliente", Ds.Tables["Encabezado"].Rows[0]["CardName"].ToString());
                                bodyH = bodyH.Replace("@Telefono", Ds.Tables["Encabezado"].Rows[0]["Phone1"].ToString());
                                bodyH = bodyH.Replace("@Celular", "      ");
                                bodyH = bodyH.Replace("@DocEntry", EncMovimiento.DocEntry.ToString());
                                bodyH = bodyH.Replace("@NumBoleta", EncMovimiento.NumLlamada);

                                
                                bodyH = bodyH.Replace("@Fecha", EncMovimiento.Fecha.ToString("dd/MM/yyyy"));
                                EmailDestino = Ds.Tables["Encabezado"].Rows[0]["E_Mail"].ToString();
                               
                                bodyH = bodyH.Replace("@NumContacto", Ds.Tables["Encabezado"].Rows[0]["Tel1"].ToString());

                                bodyH = bodyH.Replace("@SubTotal", EncMovimiento.Subtotal.ToString());
                                bodyH = bodyH.Replace("@Descuento", EncMovimiento.Descuento.ToString());
                                bodyH = bodyH.Replace("@Impuestos", EncMovimiento.Impuestos.ToString());
                                bodyH = bodyH.Replace("@TotalEntrega", EncMovimiento.TotalComprobante.ToString());



                                Cn.Close();
                                Cn.Dispose();

                                    var inyectado = "";
                                var z = 0;
                                var top1 = 454;
                                var top2 = 453;


                                foreach (var item in DetalleSAP)
                                {
                                    if (z == 0)
                                    {
                                         inyectado = Html.Inyectado.Replace("@ItemCode", item.ItemCode).Replace("@ItemName", item.ItemName).Replace("@Cantidad", item.Cantidad.ToString()).Replace("@PrecioUnitario", item.PrecioUnitario.ToString()).Replace("@TotalLinea", item.TotalLinea.ToString()).Replace("@Top1", top1.ToString()).Replace("@Top1.1", top1.ToString()).Replace("@Top1.2", top1.ToString()).Replace("@Top1.3", top1.ToString()).Replace("@Top2", top2.ToString());
                                         
                                    }
                                    else
                                    {
                                        top1 += 23;
                                        top2 += 23;
                                        inyectado += Html.Inyectado.Replace("@ItemCode", item.ItemCode).Replace("@ItemName", item.ItemName).Replace("@Cantidad", item.Cantidad.ToString()).Replace("@PrecioUnitario", item.PrecioUnitario.ToString()).Replace("@TotalLinea", item.TotalLinea.ToString()).Replace("@Top1", top1.ToString()).Replace("@Top1.1", top1.ToString()).Replace("@Top1.2", top1.ToString()).Replace("@Top1.3", top1.ToString()).Replace("@Top2", top2.ToString());
                                    }


                                    z++;
                                }
                                        bodyH = bodyH.Replace("@Inyectado", inyectado); 





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

                                System.Net.Mail.Attachment att3 = new System.Net.Mail.Attachment(new MemoryStream(bytes), "Entrega_Mercaderia.pdf");
                                adjuntos.Add(att3);

                                var NumLlamada = Convert.ToInt32(EncMovimiento.NumLlamada);
                                var Llamada = db.LlamadasServicios.Where(a => a.DocEntry == NumLlamada).FirstOrDefault();
                                var EncReparacion = db.EncReparacion.Where(a => a.idLlamada == Llamada.id).FirstOrDefault();
                                var Adjuntos = db.Adjuntos.Where(a => a.idEncabezado == EncReparacion.id).ToList();
                                var ui = 1;
                                foreach (var det in Adjuntos)
                                {
                                    
                                    {
                                        System.Net.Mail.Attachment att2 = new System.Net.Mail.Attachment(new MemoryStream(det.base64),ui.ToString() + ".png");
                                        adjuntos.Add(att2);
                                        ui++;
                                    }
                                }


                                var resp = G.SendV2(EmailDestino, "larce@dydconsultorescr.com", "", CorreoEnvio.RecepcionEmail, "Entrega de Mercaderia", "Entrega de Producto", "<!DOCTYPE html> <html> <head> <meta charset='utf-8'> <meta name='viewport' content='width=device-width, initial-scale=1'> <title></title> </head> <body> <h1>Entrega Mercaderia</h1> <p> En el presente correo se le hace el envio de la entrega de mercaderia, favor no responder a este correo </p> </body> </html>", CorreoEnvio.RecepcionHostName, CorreoEnvio.EnvioPort, CorreoEnvio.RecepcionUseSSL, CorreoEnvio.RecepcionEmail, CorreoEnvio.RecepcionPassword, adjuntos);

                                g.GuardarTxt("html.txt", bodyH);

                                if (!resp)
                                {
                                    throw new Exception("No se ha podido enviar el correo con la entrega");
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


                        }
                        else
                        {
                            BitacoraErrores be = new BitacoraErrores();

                            be.Descripcion = Conexion.Company.GetLastErrorDescription();
                            be.StackTrace = "Movimientos";
                            be.Fecha = DateTime.Now;

                            db.BitacoraErrores.Add(be);
                            db.SaveChanges();
                            Conexion.Desconectar();
                        }
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, EncMovimiento);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }



    }
}