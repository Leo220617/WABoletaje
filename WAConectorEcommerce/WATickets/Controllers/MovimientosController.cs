using SAPbobsCOM;
using SelectPdf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
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

    public class MovimientosController : ApiController
    {
        G g = new G();
        ModelCliente db = new ModelCliente();

        [Route("api/Movimientos/GenerarAprobacion")]
        [HttpGet]
        public HttpResponseMessage GetAprobacion([FromUri]int id)
        {
            try
            {///Alt + 125 }
                var EncMovimiento = db.EncMovimiento.Where(a => a.id == id).FirstOrDefault();

                if (EncMovimiento != null)
                {
                    var DetalleReparaciones = db.DetMovimiento.Where(a => a.idEncabezado == EncMovimiento.id).ToList();
                    var NumLlamada = Convert.ToInt32(EncMovimiento.NumLlamada);
                    var CotizacionesAnteriores = db.CotizacionesAprobadas.Where(a => a.idEncabezado == NumLlamada).ToList();
                    if (CotizacionesAnteriores.Count() > 0)
                    {
                        foreach (var item in CotizacionesAnteriores)
                        {
                            db.CotizacionesAprobadas.Remove(item);
                            db.SaveChanges();
                        }
                    }

                    foreach (var item in DetalleReparaciones)
                    {

                        CotizacionesAprobadas coti = new CotizacionesAprobadas();
                        coti.idEncabezado = Convert.ToInt32(EncMovimiento.NumLlamada);
                        coti.ItemCode = item.ItemCode + " | " + item.ItemName;
                        coti.Cantidad = item.Cantidad;
                        coti.Opcional = item.Opcional;
                        coti.idError = item.idError;
                        db.CotizacionesAprobadas.Add(coti);
                        db.SaveChanges();
                    }


                    db.Entry(EncMovimiento).State = EntityState.Modified;
                    EncMovimiento.Aprobada = true;
                    db.SaveChanges();

                    var MovimientosAnteriores = db.EncMovimiento.Where(a => a.NumLlamada == EncMovimiento.NumLlamada && (a.TipoMovimiento == 1 || a.TipoMovimiento == 3) && a.id != EncMovimiento.id).ToList();
                    foreach (var item in MovimientosAnteriores)
                    {
                        db.Entry(item).State = EntityState.Modified;
                        item.Aprobada = false;
                        db.SaveChanges();
                    }

                    var Parametros = db.Parametros.FirstOrDefault();
                    if (Parametros.SetearAprobado)
                    {
                        var Llamada = db.LlamadasServicios.Where(a => a.DocEntry == NumLlamada).FirstOrDefault();
                        var EncReparacion = db.EncReparacion.Where(a => a.idLlamada == Llamada.id).FirstOrDefault();
                        BitacoraMovimientos bts = new BitacoraMovimientos();
                        bts.idLlamada = EncReparacion.idLlamada;
                        bts.idEncabezado = EncReparacion.id;
                        bts.DocEntry = 0;
                        bts.Fecha = DateTime.Now;
                        bts.TipoMovimiento = 1;


                        bts.BodegaInicial = Parametros.BodegaInicial;
                        bts.BodegaFinal = Parametros.BodegaFinal; 
                        bts.idTecnico = EncReparacion.idTecnico;
                        bts.Status = "0";
                        bts.ProcesadaSAP = false;
                        db.BitacoraMovimientos.Add(bts);
                        db.SaveChanges();

                        foreach (var item in DetalleReparaciones.Where(a => !a.ItemCode.ToLower().Contains("mano de obra")))
                        {
                            DetBitacoraMovimientos dbt = new DetBitacoraMovimientos();
                            dbt.idEncabezado = bts.id;
                            var ProductoHijo = db.ProductosHijos.Where(a => a.codSAP == item.ItemCode).FirstOrDefault();
                            dbt.idProducto = ProductoHijo != null ? ProductoHijo.id : 0;
                            dbt.Cantidad = item.Cantidad;
                            dbt.ItemCode = item.ItemCode + " | " + ProductoHijo.Nombre;
                            dbt.idError = item.idError;
                            dbt.CantidadEnviar = 0;
                            dbt.CantidadFaltante = item.Cantidad;
                            db.DetBitacoraMovimientos.Add(dbt);
                            db.SaveChanges();

                             
                        }
                        try
                        {
                            if (!string.IsNullOrEmpty(Parametros.StatusLlamadaAprobado))
                            {
                                db.Entry(Llamada).State = EntityState.Modified;
                                Llamada.Status = Convert.ToInt32(Parametros.StatusLlamadaAprobado);
                                db.SaveChanges();
                                LlamadasServicioViewModel llamada = new LlamadasServicioViewModel();
                                llamada.id = Llamada.id;
                                llamada.Status = Llamada.Status;
                                llamada.TipoCaso = Llamada.TipoCaso;
                                llamada.FechaSISO = Llamada.FechaSISO;
                                llamada.LugarReparacion = Llamada.LugarReparacion;
                                llamada.PIN = Llamada.PIN;
                                LlamadasServicioController llamadasServicioController = new LlamadasServicioController();
                                llamadasServicioController.Put(llamada);
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

                try
                {
                    G Ge = new G();

                    var conexion = Ge.DevuelveCadena(db);
                    var Cn5 = new SqlConnection(conexion);
                    var Cmd5 = new SqlCommand();

                    Cn5.Open();

                    Cmd5.Connection = Cn5;


                    Cmd5.CommandText = " Update OQUT set U_DYD_Aprobado = '2' where DocEntry = '" + EncMovimiento.DocEntry + "' ";



                    Cmd5.ExecuteNonQuery();
                    Cn5.Close();
                    Cn5.Dispose();


                    var MovimientosAnteriores = db.EncMovimiento.Where(a => a.NumLlamada == EncMovimiento.NumLlamada && (a.TipoMovimiento == 1 || a.TipoMovimiento == 3) && a.id != EncMovimiento.id).ToList();

                    foreach (var item in MovimientosAnteriores)
                    {
                        try
                        {
                            Cn5 = new SqlConnection(conexion);
                            Cmd5 = new SqlCommand();

                            Cn5.Open();

                            Cmd5.Connection = Cn5;


                            Cmd5.CommandText = " Update OQUT set U_DYD_Aprobado = '1' where DocEntry = '" + item.DocEntry + "' ";



                            Cmd5.ExecuteNonQuery();
                            Cn5.Close();
                            Cn5.Dispose();
                        }
                        catch (Exception ex)
                        {

                            BitacoraErrores be = new BitacoraErrores();

                            be.Descripcion = "Aprobacion Llamada: " + ex.Message;
                            be.StackTrace = ex.StackTrace;
                            be.Fecha = DateTime.Now;
                            db.BitacoraErrores.Add(be);
                            db.SaveChanges();
                        }

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


        [Route("api/Movimientos/Reenvio")]
        [HttpGet]
        public HttpResponseMessage GetCorreo([FromUri]int id, string correo)
        {
            try
            {///Alt + 125 }
                NumberFormatInfo formato = new CultureInfo("en-US").NumberFormat;
                formato.CurrencyGroupSeparator = ",";
                formato.NumberDecimalSeparator = ".";
                var EncMovimiento = db.EncMovimiento.Where(a => a.id == id).FirstOrDefault();
                var Moneda = EncMovimiento.Moneda == "COL" ? "₡" : "$";
                if (EncMovimiento != null)
                {
                    if (EncMovimiento.TipoMovimiento == 1 || EncMovimiento.TipoMovimiento == 3)
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
                            var bodyH = G.ObtenerConfig("Empresa") == "G" ? Html.textoOfertaGermantec : Html.textoOfertaAlsaraBackOffice;
                            bodyH = bodyH.Replace("@NombreCliente", Ds.Tables["Encabezado"].Rows[0]["CardName"].ToString());
                            bodyH = bodyH.Replace("@NombreCliente2", Ds.Tables["Encabezado"].Rows[0]["CardName"].ToString());
                            bodyH = bodyH.Replace("@Email", Ds.Tables["Encabezado"].Rows[0]["E_Mail"].ToString());


                            bodyH = bodyH.Replace("@TelefonoCliente", Ds.Tables["Encabezado"].Rows[0]["Phone1"].ToString());

                            bodyH = bodyH.Replace("@DocEntry", EncMovimiento.id.ToString());
                            bodyH = bodyH.Replace("@NumBoleta", EncMovimiento.NumLlamada);



                            bodyH = bodyH.Replace("@Fecha", EncMovimiento.Fecha.ToString("dd/MM/yyyy"));
                            EmailDestino = Ds.Tables["Encabezado"].Rows[0]["E_Mail"].ToString();

                            bodyH = bodyH.Replace("@PorDesc", Math.Round(EncMovimiento.PorDescuento, 2).ToString());


                            bodyH = bodyH.Replace("@Subtotal", Moneda + Math.Round(EncMovimiento.Subtotal, 2).ToString("N", formato));
                            bodyH = bodyH.Replace("@Descuento", Moneda + Math.Round(EncMovimiento.Descuento, 2).ToString("N", formato));
                            bodyH = bodyH.Replace("@Impuestos", Moneda + Math.Round(EncMovimiento.Impuestos, 2).ToString("N", formato));
                            bodyH = bodyH.Replace("@Total", Moneda + Math.Round(EncMovimiento.TotalComprobante, 2).ToString("N", formato));



                            Cn.Close();
                            Cn.Dispose();
                            var NumLlamada = Convert.ToInt32(EncMovimiento.NumLlamada);
                            var Llamada = db.LlamadasServicios.Where(a => a.DocEntry == NumLlamada).FirstOrDefault();

                            var SQLANTES = parametros.SQLProductosBoleta.Replace("and t0.validFor = 'Y' ", " ");
                            SQLANTES = SQLANTES.Replace("and t0.validFor = 'Y'", " ");
                            SQL = SQLANTES + " and ItemCode = '" + Llamada.ItemCode + "'";

                            Cn = new SqlConnection(conexion);
                            Cmd = new SqlCommand(SQL, Cn);
                            Da = new SqlDataAdapter(Cmd);
                            Ds = new DataSet();
                            Cn.Open();
                            Da.Fill(Ds, "Encabezado");
                            var NombreProducto = "";
                            try
                            {
                                NombreProducto = Ds.Tables["Encabezado"].Rows[0]["ItemName"].ToString();
                            }
                            catch (Exception)
                            {


                            }
                            Cn.Close();

                            bodyH = bodyH.Replace("@ItemCodeProd", Llamada.ItemCode + " - " + NombreProducto);
                            bodyH = bodyH.Replace("@SerieProd", Llamada.SerieFabricante);

                            bodyH = bodyH.Replace("@ContactoReferencia", Llamada.PersonaContacto);
                            bodyH = bodyH.Replace("@Referencia", "");

                            bodyH = bodyH.Replace("@NombreUsuario", "");
                            bodyH = bodyH.Replace("@TelefonoUsuario", "");
                            bodyH = bodyH.Replace("@CorreoVentas", "");
                            var inyectado = "";
                            var z = 0;
                            var top1 = 290;

                            var diagnosticos = "";
                            var DetalleSAP = db.DetMovimiento.Where(a => a.idEncabezado == id && a.Garantia == false).ToList();

                            var PH2 = db.ProductosHijos.ToList();
                            foreach (var item in DetalleSAP)
                            {
                                var TieneStock = PH2.Where(a => a.codSAP == item.ItemCode).FirstOrDefault() == null ? 0 : PH2.Where(a => a.codSAP == item.ItemCode).FirstOrDefault().Stock;

                                if (z == 0)
                                {
                                    inyectado = G.ObtenerConfig("Empresa") == "G" ? Html.InyectadoOfertaGermantec.Replace("@NumLinea", (z + 1).ToString()).Replace("@ItemCode", item.ItemCode).Replace("@ItemName", item.ItemName).Replace("@Cantidad", Math.Round(item.Cantidad, 2).ToString("N", formato)).Replace("@Desc", Math.Round(item.PorDescuento, 2).ToString()).Replace("@PrecioUnitario", Moneda + Math.Round(item.PrecioUnitario, 2).ToString("N", formato)).Replace("@TotalLinea", Moneda + Math.Round((item.PrecioUnitario * item.Cantidad), 2).ToString("N", formato)).Replace("@Disponible", (TieneStock == 0 ? "***" : "")).Replace("top1", top1.ToString()).Replace("top2", top1.ToString()).Replace("top3", top1.ToString()).Replace("top4", top1.ToString()).Replace("top5", top1.ToString()).Replace("top5", top1.ToString()) : Html.InyectadoOfertaAlsara.Replace("@NumLinea", (z + 1).ToString()).Replace("@ItemCode", item.ItemCode).Replace("@ItemName", item.ItemName).Replace("@Cantidad", Math.Round(item.Cantidad, 2).ToString("N", formato)).Replace("@Desc", Math.Round(item.PorDescuento, 2).ToString()).Replace("@PrecioUnitario", Moneda + Math.Round(item.PrecioUnitario, 2).ToString("N", formato)).Replace("@TotalLinea", Moneda + Math.Round((item.PrecioUnitario * item.Cantidad), 2).ToString("N", formato)).Replace("@Disponible", (TieneStock == 0 ? "***" : "")).Replace("top1", top1.ToString()).Replace("top2", top1.ToString()).Replace("top3", top1.ToString()).Replace("top4", top1.ToString()).Replace("top5", top1.ToString()).Replace("top5", top1.ToString());
                                    diagnosticos += db.Errores.Where(a => a.id == item.idError).FirstOrDefault() == null ? "" : db.Errores.Where(a => a.id == item.idError).FirstOrDefault().Diagnostico + "<br/>";
                                }
                                else
                                {
                                    top1 += 20;

                                    inyectado += G.ObtenerConfig("Empresa") == "G" ? Html.InyectadoOfertaGermantec.Replace("@NumLinea", (z + 1).ToString()).Replace("@ItemCode", item.ItemCode).Replace("@ItemName", item.ItemName).Replace("@Cantidad", Math.Round(item.Cantidad, 2).ToString("N", formato)).Replace("@Desc", Math.Round(item.PorDescuento, 2).ToString()).Replace("@PrecioUnitario", Moneda + Math.Round(item.PrecioUnitario, 2).ToString("N", formato)).Replace("@TotalLinea", Moneda + Math.Round((item.PrecioUnitario * item.Cantidad), 2).ToString("N", formato)).Replace("@Disponible", (TieneStock == 0 ? "***" : "")).Replace("top1", top1.ToString()).Replace("top2", top1.ToString()).Replace("top3", top1.ToString()).Replace("top4", top1.ToString()).Replace("top5", top1.ToString()).Replace("top6", top1.ToString()) : Html.InyectadoOfertaAlsara.Replace("@NumLinea", (z + 1).ToString()).Replace("@ItemCode", item.ItemCode).Replace("@ItemName", item.ItemName).Replace("@Cantidad", Math.Round(item.Cantidad, 2).ToString("N", formato)).Replace("@Desc", Math.Round(item.PorDescuento, 2).ToString()).Replace("@PrecioUnitario", Moneda + Math.Round(item.PrecioUnitario, 2).ToString("N", formato)).Replace("@TotalLinea", Moneda + Math.Round((item.PrecioUnitario * item.Cantidad), 2).ToString("N", formato)).Replace("@Disponible", (TieneStock == 0 ? "***" : "")).Replace("top1", top1.ToString()).Replace("top2", top1.ToString()).Replace("top3", top1.ToString()).Replace("top4", top1.ToString()).Replace("top5", top1.ToString()).Replace("top6", top1.ToString());
                                    diagnosticos += db.Errores.Where(a => a.id == item.idError).FirstOrDefault() == null ? "" : db.Errores.Where(a => a.id == item.idError).FirstOrDefault().Diagnostico + "<br/>";

                                }


                                z++;
                            }

                            diagnosticos += EncMovimiento.Comentarios + "<br/>";
                            bodyH = bodyH.Replace("@INYECTADO", inyectado);


                            bodyH = bodyH.Replace("@Diagnosticos", diagnosticos);

                            var CondicionPago = EncMovimiento.idCondPago != 0 ? db.CondicionesPagos.Where(a => a.id == EncMovimiento.idCondPago).FirstOrDefault() == null ? throw new Exception("No se puede enviar el correo, falta la condicion de pago") : db.CondicionesPagos.Where(a => a.id == EncMovimiento.idCondPago).FirstOrDefault() : new CondicionesPagos();
                            var TiempoEntrega = EncMovimiento.idTiemposEntregas != 0 ? db.TiemposEntregas.Where(a => a.id == EncMovimiento.idTiemposEntregas).FirstOrDefault() == null ? throw new Exception("No se puede enviar el correo, falta el Tiempos de Entregas") : db.TiemposEntregas.Where(a => a.id == EncMovimiento.idTiemposEntregas).FirstOrDefault() : new TiemposEntregas();
                            var Garantia = EncMovimiento.idGarantia != 0 ? db.Garantias.Where(a => a.id == EncMovimiento.idGarantia).FirstOrDefault() == null ? throw new Exception("No se puede enviar el correo, falta la garantia") : db.Garantias.Where(a => a.id == EncMovimiento.idGarantia).FirstOrDefault() : new Garantias();
                            var DiasValidos = EncMovimiento.idDiasValidos != 0 ? db.DiasValidos.Where(a => a.id == EncMovimiento.idDiasValidos).FirstOrDefault() == null ? throw new Exception("No se puede enviar el correo, falta los dias validos") : db.DiasValidos.Where(a => a.id == EncMovimiento.idDiasValidos).FirstOrDefault() : new DiasValidos();

                            bodyH = bodyH.Replace("@CondicionPago", CondicionPago.Nombre);
                            bodyH = bodyH.Replace("@TiempoEntrega", TiempoEntrega.Nombre);
                            bodyH = bodyH.Replace("@Garantia", Garantia.Nombre);
                            bodyH = bodyH.Replace("@VigenciaOferta", DiasValidos.Nombre);

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

                            System.Net.Mail.Attachment att3 = new System.Net.Mail.Attachment(new MemoryStream(bytes), "Oferta_Venta_" + Llamada.DocEntry + ".pdf");
                            adjuntos.Add(att3);


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


                            var resp = G.SendV2(correo, "", "", CorreoEnvio.RecepcionEmail, "Oferta de Venta", "Oferta de Venta #" + Llamada.DocEntry, "<!DOCTYPE html> <html> <head> <meta charset='utf-8'> <meta name='viewport' content='width=device-width, initial-scale=1'> <title></title> </head> <body> <h1>Oferta de Venta</h1> <p> En el presente correo se le hace el envio de la oferta de venta, Estimado Cliente Agradecemos su pronta respuesta a este Correo </p> </body> </html>", CorreoEnvio.RecepcionHostName, CorreoEnvio.EnvioPort, CorreoEnvio.RecepcionUseSSL, CorreoEnvio.RecepcionEmail, CorreoEnvio.RecepcionPassword, adjuntos);

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
                            var bodyH = G.ObtenerConfig("Empresa") == "G" ? Html.textoEntregaGermantec : Html.textoEntregaAlsara;
                            bodyH = bodyH.Replace("@NombreCliente", Ds.Tables["Encabezado"].Rows[0]["CardName"].ToString());
                            bodyH = bodyH.Replace("@TelefonoCliente", Ds.Tables["Encabezado"].Rows[0]["Phone1"].ToString());
                            bodyH = bodyH.Replace("@Celular", "      ");
                            bodyH = bodyH.Replace("@DocEntry", EncMovimiento.id.ToString());


                            bodyH = bodyH.Replace("@NumBoleta", EncMovimiento.NumLlamada);


                            bodyH = bodyH.Replace("@Fecha", EncMovimiento.Fecha.ToString("dd/MM/yyyy"));
                            EmailDestino = Ds.Tables["Encabezado"].Rows[0]["E_Mail"].ToString();

                            bodyH = bodyH.Replace("@NumContacto", Ds.Tables["Encabezado"].Rows[0]["Tel1"].ToString());

                            bodyH = bodyH.Replace("@SubTotal", Moneda + Math.Round(EncMovimiento.Subtotal, 2).ToString("N", formato));
                            bodyH = bodyH.Replace("@Descuento", Moneda + Math.Round(EncMovimiento.Descuento, 2).ToString("N", formato));
                            bodyH = bodyH.Replace("@Impuestos", Moneda + Math.Round(EncMovimiento.Impuestos, 2).ToString("N", formato));
                            bodyH = bodyH.Replace("@TotalEntrega", Moneda + Math.Round(EncMovimiento.TotalComprobante, 2).ToString("N", formato));
                            bodyH = bodyH.Replace("@PorDesc", Math.Round(EncMovimiento.PorDescuento, 2).ToString("N", formato));



                            Cn.Close();
                            Cn.Dispose();

                            var inyectado = "";
                            var z = 0;
                            var top1 = 454;
                            var top2 = 453;

                            var DetalleSAP = db.DetMovimiento.Where(a => a.idEncabezado == id).ToList();
                            var PH2 = db.ProductosHijos.ToList();

                            var EncMovimientoGarantia = db.EncMovimiento.Where(a => a.NumLlamada == EncMovimiento.NumLlamada && a.TipoMovimiento == 2 && a.id != EncMovimiento.id).FirstOrDefault();
                            if (EncMovimientoGarantia != null)
                            {
                                DetalleSAP = DetalleSAP.Where(a => a.Garantia == false).ToList();
                            }
                            var diagnosticos = "";

                            foreach (var item in DetalleSAP)
                            {
                                var TieneStock = PH2.Where(a => a.codSAP == item.ItemCode).FirstOrDefault() == null ? 0 : PH2.Where(a => a.codSAP == item.ItemCode).FirstOrDefault().Stock;

                                if (z == 0)
                                {
                                    inyectado = G.ObtenerConfig("Empresa") == "G" ? Html.InyectadoGermantec.Replace("@ItemCode", item.ItemCode).Replace("@ItemName", item.ItemName).Replace("@Cantidad", Math.Round(item.Cantidad, 2).ToString("N", formato)).Replace("@PrecioUnitario", Moneda + Math.Round(item.PrecioUnitario, 2).ToString("N", formato)).Replace("@TotalLinea", Moneda + Math.Round(item.TotalLinea, 2).ToString("N", formato)).Replace("@Disponible", (TieneStock == 0 ? "***" : "")).Replace("@Top1", top1.ToString()).Replace("@Top1.1", top1.ToString()).Replace("@Top1.2", top1.ToString()).Replace("@Top1.3", top1.ToString()).Replace("@Top2", top2.ToString()) : Html.InyectadoAlsara.Replace("@ItemCode", item.ItemCode).Replace("@ItemName", item.ItemName).Replace("@Cantidad", Math.Round(item.Cantidad, 2).ToString("N", formato)).Replace("@PrecioUnitario", Moneda + Math.Round(item.PrecioUnitario, 2).ToString("N", formato)).Replace("@TotalLinea", Moneda + Math.Round(item.TotalLinea, 2).ToString("N", formato)).Replace("@Disponible", (TieneStock == 0 ? "***" : "")).Replace("@Top1", top1.ToString()).Replace("@Top1.1", top1.ToString()).Replace("@Top1.2", top1.ToString()).Replace("@Top1.3", top1.ToString()).Replace("@Top2", top2.ToString());
                                    diagnosticos += db.Errores.Where(a => a.id == item.idError).FirstOrDefault() == null ? "" : db.Errores.Where(a => a.id == item.idError).FirstOrDefault().Diagnostico + "<br/>";

                                }
                                else
                                {
                                    top1 += 23;
                                    top2 += 23;
                                    inyectado += G.ObtenerConfig("Empresa") == "G" ? Html.InyectadoGermantec.Replace("@ItemCode", item.ItemCode).Replace("@ItemName", item.ItemName).Replace("@Cantidad", Math.Round(item.Cantidad, 2).ToString("N", formato)).Replace("@PrecioUnitario", Moneda + Math.Round(item.PrecioUnitario, 2).ToString("N", formato)).Replace("@TotalLinea", Moneda + Math.Round(item.TotalLinea, 2).ToString("N", formato)).Replace("@Disponible", (TieneStock == 0 ? "***" : "")).Replace("@Top1", top1.ToString()).Replace("@Top1.1", top1.ToString()).Replace("@Top1.2", top1.ToString()).Replace("@Top1.3", top1.ToString()).Replace("@Top2", top2.ToString()) : Html.InyectadoAlsara.Replace("@ItemCode", item.ItemCode).Replace("@ItemName", item.ItemName).Replace("@Cantidad", Math.Round(item.Cantidad, 2).ToString("N", formato)).Replace("@PrecioUnitario", Moneda + Math.Round(item.PrecioUnitario, 2).ToString("N", formato)).Replace("@TotalLinea", Moneda + Math.Round(item.TotalLinea, 2).ToString("N", formato)).Replace("@Disponible", (TieneStock == 0 ? "***" : "")).Replace("@Top1", top1.ToString()).Replace("@Top1.1", top1.ToString()).Replace("@Top1.2", top1.ToString()).Replace("@Top1.3", top1.ToString()).Replace("@Top2", top2.ToString());
                                    diagnosticos += db.Errores.Where(a => a.id == item.idError).FirstOrDefault() == null ? "" : db.Errores.Where(a => a.id == item.idError).FirstOrDefault().Diagnostico + "<br/>";

                                }


                                z++;
                            }
                            diagnosticos += EncMovimiento.Comentarios + "<br/>";
                            bodyH = bodyH.Replace("@Inyectado", inyectado);

                            bodyH = bodyH.Replace("@Diagnosticos", diagnosticos);

                            var CondicionPago = EncMovimiento.idCondPago != 0 ? db.CondicionesPagos.Where(a => a.id == EncMovimiento.idCondPago).FirstOrDefault() == null ? throw new Exception("No se puede enviar el correo, falta la condicion de pago") : db.CondicionesPagos.Where(a => a.id == EncMovimiento.idCondPago).FirstOrDefault() : new CondicionesPagos();
                            var TiempoEntrega = EncMovimiento.idTiemposEntregas != 0 ? db.TiemposEntregas.Where(a => a.id == EncMovimiento.idTiemposEntregas).FirstOrDefault() == null ? throw new Exception("No se puede enviar el correo, falta el Tiempos de Entregas") : db.TiemposEntregas.Where(a => a.id == EncMovimiento.idTiemposEntregas).FirstOrDefault() : new TiemposEntregas();
                            var Garantia = EncMovimiento.idGarantia != 0 ? db.Garantias.Where(a => a.id == EncMovimiento.idGarantia).FirstOrDefault() == null ? throw new Exception("No se puede enviar el correo, falta la garantia") : db.Garantias.Where(a => a.id == EncMovimiento.idGarantia).FirstOrDefault() : new Garantias();
                            var DiasValidos = EncMovimiento.idDiasValidos != 0 ? db.DiasValidos.Where(a => a.id == EncMovimiento.idDiasValidos).FirstOrDefault() == null ? throw new Exception("No se puede enviar el correo, falta los dias validos") : db.DiasValidos.Where(a => a.id == EncMovimiento.idDiasValidos).FirstOrDefault() : new DiasValidos();

                            bodyH = bodyH.Replace("@CondicionPago", CondicionPago.Nombre);
                            bodyH = bodyH.Replace("@TiempoEntrega", TiempoEntrega.Nombre);
                            bodyH = bodyH.Replace("@Garantia", Garantia.Nombre);
                            bodyH = bodyH.Replace("@VigenciaOferta", DiasValidos.Nombre);

                            var NumLlamada = Convert.ToInt32(EncMovimiento.NumLlamada);
                            var Llamada = db.LlamadasServicios.Where(a => a.DocEntry == NumLlamada).FirstOrDefault();
                            var SQLANTES = parametros.SQLProductosBoleta.Replace("and t0.validFor = 'Y' ", " ");
                            SQLANTES = SQLANTES.Replace("and t0.validFor = 'Y'", " ");
                            SQL = SQLANTES + " and ItemCode = '" + Llamada.ItemCode + "'";

                            Cn = new SqlConnection(conexion);
                            Cmd = new SqlCommand(SQL, Cn);
                            Da = new SqlDataAdapter(Cmd);
                            Ds = new DataSet();
                            Cn.Open();
                            Da.Fill(Ds, "Encabezado");
                            var NombreProducto = "";
                            try
                            {
                                NombreProducto = Ds.Tables["Encabezado"].Rows[0]["ItemName"].ToString();
                            }
                            catch (Exception)
                            {


                            }

                            Cn.Close();

                            bodyH = bodyH.Replace("@ItemCodeProd", Llamada.ItemCode + " - " + NombreProducto);
                            bodyH = bodyH.Replace("@SerieProd", Llamada.SerieFabricante);


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

                            System.Net.Mail.Attachment att3 = new System.Net.Mail.Attachment(new MemoryStream(bytes), "Presupuesto_Reparacion_" + NumLlamada.ToString() + ".pdf");
                            adjuntos.Add(att3);


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


                            var resp = G.SendV2(correo, "", "", CorreoEnvio.RecepcionEmail, "Presupuesto de Reparación", "Presupuesto de Reparación #" + NumLlamada, "<!DOCTYPE html> <html> <head> <meta charset='utf-8'> <meta name='viewport' content='width=device-width, initial-scale=1'> <title></title> </head> <body> <h1>Presupuesto de Reparación</h1> <p> En el presente correo se le hace el envio del presupuesto de reparación. </br> Estimado Cliente Agradecemos su pronta respuesta a este Correo </p> </body> </html>", CorreoEnvio.RecepcionHostName, CorreoEnvio.EnvioPort, CorreoEnvio.RecepcionUseSSL, CorreoEnvio.RecepcionEmail, CorreoEnvio.RecepcionPassword, adjuntos);

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
            {///Alt + 125 }
                var time = new DateTime();
                if (filtro.FechaFinal != time)
                {
                    filtro.FechaFinal = filtro.FechaFinal.AddDays(1);
                }
                if (!filtro.FiltroEspecial)
                {
                    var encMovimientos = db.EncMovimiento.Where(a => (filtro.FechaInicial != time ? a.Fecha >= filtro.FechaInicial : true) && (filtro.FechaFinal != time ? a.Fecha <= filtro.FechaFinal : true))
                       .Select(a => new
                       {
                           a.id,
                           a.DocEntry,
                           a.CardCode,
                           a.CardName,
                           a.NumLlamada,
                           Llamada = db.LlamadasServicios
                    .Where(z => z.DocEntry.ToString() == a.NumLlamada)
                    .Select(z => new
                    {
                        z.id,
                        z.EmailPersonaContacto,
                        z.Status,
                        z.TipoCaso
                    })
                    .FirstOrDefault(),
                           a.Fecha,
                           a.TipoMovimiento,
                           a.Comentarios,
                           a.CreadoPor,
                           a.Subtotal,
                           a.PorDescuento,
                           a.Descuento,
                           a.Impuestos,
                           a.TotalComprobante,
                           a.Moneda,
                           a.AprobadaSuperior,
                           a.idCondPago,
                           a.idDiasValidos,
                           a.idGarantia,
                           a.idTiemposEntregas,
                           a.Aprobada,
                           Detalle = db.DetMovimiento.Where(b => b.idEncabezado == a.id).ToList()
                       })
                            .AsEnumerable() // Convert to in-memory collection before setting the properties that depend on null checks
                            .Select(a => new
                            {
                                a.id,
                                a.DocEntry,
                                a.CardCode,
                                a.CardName,
                                a.NumLlamada,
                                idLlamada = a.Llamada?.id ?? 0,
                                EmailPersonaContacto = a.Llamada?.EmailPersonaContacto ?? "",
                                StatusLlamada = a.Llamada?.Status ?? 0,
                                TipoCaso = a.Llamada?.TipoCaso ?? 0,
                                a.Fecha,
                                a.TipoMovimiento,
                                a.Comentarios,
                                a.CreadoPor,
                                a.Subtotal,
                                a.PorDescuento,
                                a.Descuento,
                                a.Impuestos,
                                a.TotalComprobante,
                                a.Moneda,
                                a.AprobadaSuperior,
                                a.idCondPago,
                                a.idDiasValidos,
                                a.idGarantia,
                                a.idTiemposEntregas,
                                a.Aprobada,
                                a.Detalle
                            })
                            .ToList();
                    if (filtro.Codigo1 > 0)
                    {
                        encMovimientos = encMovimientos.Where(a => a.TipoMovimiento == filtro.Codigo1).ToList();

                    }
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
                            var llamadasQuery = db.LlamadasServicios.AsQueryable();


                            // Filtrar por Status diferente a Codigo3
                            var llamadas = llamadasQuery.Where(a => !filtro.seleccionMultiple.Contains(a.Status.Value)).Select(a => a.DocEntry).ToHashSet();
                            // Filtrar por fechas si las fechas son diferentes al valor por defecto
                            if (filtro.FechaInicial != time)
                            {
                                llamadasQuery = llamadasQuery.Where(a => a.FechaCreacion >= filtro.FechaInicial);
                            }
                            if (filtro.FechaFinal != time)
                            {
                                llamadasQuery = llamadasQuery.Where(a => a.FechaCreacion <= filtro.FechaFinal);
                            }

                            // Remover reparaciones con idLlamada == 0 en una sola pasada
                            encMovimientos.RemoveAll(a => a.NumLlamada == "0");

                            // Remover reparaciones cuyas llamadas coinciden con las llamadas filtradas
                            encMovimientos.RemoveAll(a => llamadas.Contains(Convert.ToInt32(a.NumLlamada)));
                        }


                    }
                    //Si me estan filtrando por Status de la llamada
                    //if (filtro.Codigo2 != 0)
                    //{
                    //    var Llamadas = db.LlamadasServicios.Where(a => a.Status != filtro.Codigo2).ToList();
                    //    var ListadoReparacionesEnCero = encMovimientos.Where(a => a.NumLlamada == "0").ToList();

                    //    foreach (var item in ListadoReparacionesEnCero)
                    //    {
                    //        encMovimientos.Remove(item);

                    //    }


                    //    foreach (var item in Llamadas)
                    //    {
                    //        var DocEntry = item.DocEntry.ToString();
                    //        var EncReparacionSacar = encMovimientos.Where(a => a.NumLlamada == DocEntry).ToList();
                    //        if (EncReparacionSacar != null)
                    //        {
                    //            foreach (var item2 in EncReparacionSacar)
                    //            {
                    //                encMovimientos.Remove(item2);

                    //            }

                    //        }
                    //    }

                    //}
                    if (!string.IsNullOrEmpty(filtro.Texto) || !string.IsNullOrEmpty(filtro.CardCode))
                    {


                        encMovimientos = db.EncMovimiento
                       .Where(a => (!string.IsNullOrEmpty(filtro.Texto) ? a.NumLlamada == filtro.Texto : true)
                       && (!string.IsNullOrEmpty(filtro.CardCode) ? a.CardCode.Contains(filtro.CardCode) : true)
                       ).Select(a => new
                       {
                           a.id,
                           a.DocEntry,
                           a.CardCode,
                           a.CardName,
                           a.NumLlamada,
                           Llamada = db.LlamadasServicios
                .Where(z => z.DocEntry.ToString() == a.NumLlamada)
                .Select(z => new
                {
                    z.id,
                    z.EmailPersonaContacto,
                    z.Status,
                    z.TipoCaso
                })
                .FirstOrDefault(),
                           a.Fecha,
                           a.TipoMovimiento,
                           a.Comentarios,
                           a.CreadoPor,
                           a.Subtotal,
                           a.PorDescuento,
                           a.Descuento,
                           a.Impuestos,
                           a.TotalComprobante,
                           a.Moneda,
                           a.AprobadaSuperior,
                           a.idCondPago,
                           a.idDiasValidos,
                           a.idGarantia,
                           a.idTiemposEntregas,
                           a.Aprobada,
                           Detalle = db.DetMovimiento.Where(b => b.idEncabezado == a.id).ToList()
                       })
    .AsEnumerable() // Convert to in-memory collection before setting the properties that depend on null checks
    .Select(a => new
    {
        a.id,
        a.DocEntry,
        a.CardCode,
        a.CardName,
        a.NumLlamada,
        idLlamada = a.Llamada?.id ?? 0,
        EmailPersonaContacto = a.Llamada?.EmailPersonaContacto ?? "",
        StatusLlamada = a.Llamada?.Status ?? 0,
        TipoCaso = a.Llamada?.TipoCaso ?? 0,
        a.Fecha,
        a.TipoMovimiento,
        a.Comentarios,
        a.CreadoPor,
        a.Subtotal,
        a.PorDescuento,
        a.Descuento,
        a.Impuestos,
        a.TotalComprobante,
        a.Moneda,
        a.AprobadaSuperior,
        a.idCondPago,
        a.idDiasValidos,
        a.idGarantia,
        a.idTiemposEntregas,
        a.Aprobada,
        a.Detalle
    })
    .ToList();

                    }
                    return Request.CreateResponse(HttpStatusCode.OK, encMovimientos);
                }
                else
                {
                    if (!string.IsNullOrEmpty(filtro.Texto) || !string.IsNullOrEmpty(filtro.CardCode))
                    {


                        var encMovimientos = db.EncMovimiento
                         .Where(a => (!string.IsNullOrEmpty(filtro.Texto) ? a.NumLlamada == filtro.Texto : true)
                         && (!string.IsNullOrEmpty(filtro.CardCode) ? a.CardCode.Contains(filtro.CardCode) : true)
                         ).Select(a => new
                         {
                             a.id,
                             a.DocEntry,
                             a.CardCode,
                             a.CardName,
                             a.NumLlamada,
                             Llamada = db.LlamadasServicios
                .Where(z => z.DocEntry.ToString() == a.NumLlamada)
                .Select(z => new
                {
                    z.id,
                    z.EmailPersonaContacto,
                    z.Status,
                    z.TipoCaso
                })
                .FirstOrDefault(),
                             a.Fecha,
                             a.TipoMovimiento,
                             a.Comentarios,
                             a.CreadoPor,
                             a.Subtotal,
                             a.PorDescuento,
                             a.Descuento,
                             a.Impuestos,
                             a.TotalComprobante,
                             a.Moneda,
                             a.AprobadaSuperior,
                             a.idCondPago,
                             a.idDiasValidos,
                             a.idGarantia,
                             a.idTiemposEntregas,
                             a.Aprobada,
                             Detalle = db.DetMovimiento.Where(b => b.idEncabezado == a.id).ToList()
                         })
    .AsEnumerable() // Convert to in-memory collection before setting the properties that depend on null checks
    .Select(a => new
    {
        a.id,
        a.DocEntry,
        a.CardCode,
        a.CardName,
        a.NumLlamada,
        idLlamada = a.Llamada?.id ?? 0,
        EmailPersonaContacto = a.Llamada?.EmailPersonaContacto ?? "",
        StatusLlamada = a.Llamada?.Status ?? 0,
        TipoCaso = a.Llamada?.TipoCaso ?? 0,
        a.Fecha,
        a.TipoMovimiento,
        a.Comentarios,
        a.CreadoPor,
        a.Subtotal,
        a.PorDescuento,
        a.Descuento,
        a.Impuestos,
        a.TotalComprobante,
        a.Moneda,
        a.AprobadaSuperior,
        a.idCondPago,
        a.idDiasValidos,
        a.idGarantia,
        a.idTiemposEntregas,
        a.Aprobada,
        a.Detalle
    })
    .ToList(); ;




                        return Request.CreateResponse(HttpStatusCode.OK, encMovimientos);
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
                                var llamadasQuery = db.LlamadasServicios.AsQueryable();


                                // Filtrar por Status diferente a Codigo3
                                var llamadas = llamadasQuery.Where(a => !filtro.seleccionMultiple.Contains(a.Status.Value)).Select(a => a.DocEntry.ToString()).ToHashSet();
                                // Filtrar por fechas si las fechas son diferentes al valor por defecto
                                //if (filtro.FechaInicial != time)
                                //{
                                //    llamadasQuery = llamadasQuery.Where(a => a.FechaCreacion >= filtro.FechaInicial);
                                //}
                                //if (filtro.FechaFinal != time)
                                //{
                                //    llamadasQuery = llamadasQuery.Where(a => a.FechaCreacion <= filtro.FechaFinal);
                                //}


                                var encMovimientos = db.EncMovimiento.Where(a => !llamadas.Contains(a.NumLlamada) && (filtro.Codigo1 > 0 ? a.TipoMovimiento == filtro.Codigo1 : true))
                               .Select(a => new
                               {
                                   a.id,
                                   a.DocEntry,
                                   a.CardCode,
                                   a.CardName,
                                   a.NumLlamada,
                                   Llamada = db.LlamadasServicios
                    .Where(z => z.DocEntry.ToString() == a.NumLlamada)
                    .Select(z => new
                    {
                        z.id,
                        z.EmailPersonaContacto,
                        z.Status,
                        z.TipoCaso
                    })
                    .FirstOrDefault(),
                                   a.Fecha,
                                   a.TipoMovimiento,
                                   a.Comentarios,
                                   a.CreadoPor,
                                   a.Subtotal,
                                   a.PorDescuento,
                                   a.Descuento,
                                   a.Impuestos,
                                   a.TotalComprobante,
                                   a.Moneda,
                                   a.AprobadaSuperior,
                                   a.idCondPago,
                                   a.idDiasValidos,
                                   a.idGarantia,
                                   a.idTiemposEntregas,
                                   a.Aprobada,
                                   Detalle = db.DetMovimiento.Where(b => b.idEncabezado == a.id).ToList()
                               })
    .AsEnumerable() // Convert to in-memory collection before setting the properties that depend on null checks
    .Select(a => new
    {
        a.id,
        a.DocEntry,
        a.CardCode,
        a.CardName,
        a.NumLlamada,
        idLlamada = a.Llamada?.id ?? 0,
        EmailPersonaContacto = a.Llamada?.EmailPersonaContacto ?? "",
        StatusLlamada = a.Llamada?.Status ?? 0,
        TipoCaso = a.Llamada?.TipoCaso ?? 0,
        a.Fecha,
        a.TipoMovimiento,
        a.Comentarios,
        a.CreadoPor,
        a.Subtotal,
        a.PorDescuento,
        a.Descuento,
        a.Impuestos,
        a.TotalComprobante,
        a.Moneda,
        a.AprobadaSuperior,
        a.idCondPago,
        a.idDiasValidos,
        a.idGarantia,
        a.idTiemposEntregas,
        a.Aprobada,
        a.Detalle
    })
    .ToList();
                                return Request.CreateResponse(HttpStatusCode.OK, encMovimientos.ToList());

                            }
                            else
                            {
                                var encMovimientos = db.EncMovimiento.Where(a => (filtro.FechaInicial != time ? a.Fecha >= filtro.FechaInicial : true) && (filtro.FechaFinal != time ? a.Fecha <= filtro.FechaFinal : true))
                     .Select(a => new
                     {
                         a.id,
                         a.DocEntry,
                         a.CardCode,
                         a.CardName,
                         a.NumLlamada,
                         Llamada = db.LlamadasServicios
                    .Where(z => z.DocEntry.ToString() == a.NumLlamada)
                    .Select(z => new
                    {
                        z.id,
                        z.EmailPersonaContacto,
                        z.Status,
                        z.TipoCaso
                    })
                    .FirstOrDefault(),
                         a.Fecha,
                         a.TipoMovimiento,
                         a.Comentarios,
                         a.CreadoPor,
                         a.Subtotal,
                         a.PorDescuento,
                         a.Descuento,
                         a.Impuestos,
                         a.TotalComprobante,
                         a.Moneda,
                         a.AprobadaSuperior,
                         a.idCondPago,
                         a.idDiasValidos,
                         a.idGarantia,
                         a.idTiemposEntregas,
                         a.Aprobada,
                         Detalle = db.DetMovimiento.Where(b => b.idEncabezado == a.id).ToList()
                     })
    .AsEnumerable() // Convert to in-memory collection before setting the properties that depend on null checks
    .Select(a => new
    {
        a.id,
        a.DocEntry,
        a.CardCode,
        a.CardName,
        a.NumLlamada,
        idLlamada = a.Llamada?.id ?? 0,
        EmailPersonaContacto = a.Llamada?.EmailPersonaContacto ?? "",
        StatusLlamada = a.Llamada?.Status ?? 0,
        TipoCaso = a.Llamada?.TipoCaso ?? 0,
        a.Fecha,
        a.TipoMovimiento,
        a.Comentarios,
        a.CreadoPor,
        a.Subtotal,
        a.PorDescuento,
        a.Descuento,
        a.Impuestos,
        a.TotalComprobante,
        a.Moneda,
        a.AprobadaSuperior,
        a.idCondPago,
        a.idDiasValidos,
        a.idGarantia,
        a.idTiemposEntregas,
        a.Aprobada,
        a.Detalle
    })
    .ToList();
                                return Request.CreateResponse(HttpStatusCode.OK, encMovimientos.ToList());
                            }


                        }
                        else
                        {
                            var encMovimientos = db.EncMovimiento.Where(a => (filtro.FechaInicial != time ? a.Fecha >= filtro.FechaInicial : true) && (filtro.FechaFinal != time ? a.Fecha <= filtro.FechaFinal : true))
                                  .Select(a => new
                                  {
                                      a.id,
                                      a.DocEntry,
                                      a.CardCode,
                                      a.CardName,
                                      a.NumLlamada,
                                      Llamada = db.LlamadasServicios
                    .Where(z => z.DocEntry.ToString() == a.NumLlamada)
                    .Select(z => new
                    {
                        z.id,
                        z.EmailPersonaContacto,
                        z.Status,
                        z.TipoCaso
                    })
                    .FirstOrDefault(),
                                      a.Fecha,
                                      a.TipoMovimiento,
                                      a.Comentarios,
                                      a.CreadoPor,
                                      a.Subtotal,
                                      a.PorDescuento,
                                      a.Descuento,
                                      a.Impuestos,
                                      a.TotalComprobante,
                                      a.Moneda,
                                      a.AprobadaSuperior,
                                      a.idCondPago,
                                      a.idDiasValidos,
                                      a.idGarantia,
                                      a.idTiemposEntregas,
                                      a.Aprobada,
                                      Detalle = db.DetMovimiento.Where(b => b.idEncabezado == a.id).ToList()
                                  })
    .AsEnumerable() // Convert to in-memory collection before setting the properties that depend on null checks
    .Select(a => new
    {
        a.id,
        a.DocEntry,
        a.CardCode,
        a.CardName,
        a.NumLlamada,
        idLlamada = a.Llamada?.id ?? 0,
        EmailPersonaContacto = a.Llamada?.EmailPersonaContacto ?? "",
        StatusLlamada = a.Llamada?.Status ?? 0,
        TipoCaso = a.Llamada?.TipoCaso ?? 0,
        a.Fecha,
        a.TipoMovimiento,
        a.Comentarios,
        a.CreadoPor,
        a.Subtotal,
        a.PorDescuento,
        a.Descuento,
        a.Impuestos,
        a.TotalComprobante,
        a.Moneda,
        a.AprobadaSuperior,
        a.idCondPago,
        a.idDiasValidos,
        a.idGarantia,
        a.idTiemposEntregas,
        a.Aprobada,
        a.Detalle
    })
    .ToList();
                            return Request.CreateResponse(HttpStatusCode.OK, encMovimientos.ToList());
                        }


                    }
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
                    a.Moneda,
                    a.AprobadaSuperior,
                    a.idCondPago,
                    a.idDiasValidos,
                    a.idGarantia,
                    a.idTiemposEntregas,
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
        [Route("api/Movimientos/Actualizar")]
        public HttpResponseMessage Put([FromBody] ColeccionMovimientos encMovimiento)
        {
            var t = db.Database.BeginTransaction();
            try
            {

                var Parametros = db.Parametros.FirstOrDefault();
                var EncMovimiento = db.EncMovimiento.Where(a => a.id == encMovimiento.id).FirstOrDefault();

                if (EncMovimiento != null)
                {
                    if (!encMovimiento.Regenerar)
                    {
                        db.Entry(EncMovimiento).State = EntityState.Modified;
                        EncMovimiento.CreadoPor = encMovimiento.CreadoPor;
                        EncMovimiento.Descuento = encMovimiento.Descuento;
                        EncMovimiento.Impuestos = encMovimiento.Impuestos;
                        EncMovimiento.Subtotal = encMovimiento.Subtotal;
                        EncMovimiento.PorDescuento = encMovimiento.PorDescuento;
                        EncMovimiento.TotalComprobante = encMovimiento.TotalComprobante;
                        EncMovimiento.Comentarios = encMovimiento.Comentarios;
                        EncMovimiento.Moneda = encMovimiento.Moneda;
                        EncMovimiento.idCondPago = encMovimiento.idCondPago;
                        EncMovimiento.idDiasValidos = encMovimiento.idDiasValidos;
                        EncMovimiento.idGarantia = encMovimiento.idGarantia;
                        EncMovimiento.idTiemposEntregas = encMovimiento.idTiemposEntregas;
                        db.SaveChanges();

                        /// Probar funcionabilidad
                        /// 
                        var Detalles = db.DetMovimiento.Where(a => a.idEncabezado == EncMovimiento.id).ToList();

                        foreach (var item in Detalles)
                        {
                            try
                            {
                                encMovimiento.Detalle.Where(a => a.ItemCode == item.ItemCode).FirstOrDefault().idError = item.idError;
                            }
                            catch (Exception)
                            {


                            }
                        }


                        foreach (var item in Detalles)
                        {

                            db.DetMovimiento.Remove(item);
                            db.SaveChanges();
                        }



                        foreach (var item in encMovimiento.Detalle)
                        {
                            item.id = 0;
                            var Det = db.DetMovimiento.Where(a => a.id == item.id).FirstOrDefault();
                            if (Det != null)
                            {
                                db.Entry(Det).State = EntityState.Modified;
                                Det.PrecioUnitario = item.PrecioUnitario;
                                Det.Cantidad = item.Cantidad;
                                Det.PorDescuento = item.PorDescuento;
                                Det.Descuento = item.Descuento;
                                Det.Impuestos = item.Impuestos;
                                Det.TotalLinea = item.TotalLinea;
                                Det.Garantia = item.Garantia;
                                Det.idImpuesto = item.idImpuesto;
                                Det.Opcional = item.Opcional;
                                db.SaveChanges();
                            }
                            else
                            {
                                Det = new DetMovimiento();
                                Det.idEncabezado = EncMovimiento.id;
                                Det.idError = item.idError;
                                Det.NumLinea = item.NumLinea;
                                Det.ItemCode = item.ItemCode;
                                Det.ItemName = item.ItemName;
                                Det.PrecioUnitario = item.PrecioUnitario;
                                Det.Cantidad = item.Cantidad;
                                Det.PorDescuento = item.PorDescuento;
                                Det.Descuento = item.Descuento;
                                Det.Impuestos = item.Impuestos;
                                Det.TotalLinea = item.TotalLinea;
                                Det.Garantia = item.Garantia;
                                Det.idImpuesto = item.idImpuesto;
                                Det.Opcional = item.Opcional;

                                db.DetMovimiento.Add(Det);
                                db.SaveChanges();
                                try
                                {
                                    if (!item.ItemName.ToUpper().Contains("mano de obra".ToUpper()))
                                    {
                                        var Prod = db.ProductosHijos.Where(a => a.codSAP == item.ItemCode).FirstOrDefault();
                                        if (Prod != null)
                                        {
                                            var idLLamada = Convert.ToInt32(EncMovimiento.NumLlamada);
                                            var llamada = db.LlamadasServicios.Where(a => a.DocEntry == idLLamada).FirstOrDefault();
                                            var ProdPadre = db.ProductosPadres.Where(a => a.codSAP == llamada.ItemCode).FirstOrDefault();
                                            var ExisteEnPadre = db.PadresHijosProductos.Where(a => a.idProductoPadre == ProdPadre.id && a.idProductoHijo == Prod.id).FirstOrDefault();

                                            if (ExisteEnPadre == null)
                                            {
                                                ExisteEnPadre = new PadresHijosProductos();
                                                ExisteEnPadre.idProductoHijo = Prod.id;
                                                ExisteEnPadre.idProductoPadre = ProdPadre.id;
                                                ExisteEnPadre.Cantidad = item.Cantidad;
                                                db.PadresHijosProductos.Add(ExisteEnPadre);
                                                db.SaveChanges();
                                            }
                                        }
                                    }
                                }
                                catch (Exception)
                                {


                                }

                            }

                        }
                    }
                    else
                    {
                        var Detalles = db.DetMovimiento.Where(a => a.idEncabezado == EncMovimiento.id).ToList();
                        EncMovimiento.DocEntry = 0;
                        EncMovimiento.Fecha = DateTime.Now;
                        EncMovimiento.CreadoPor = encMovimiento.CreadoPor;
                        EncMovimiento.Descuento = encMovimiento.Descuento;
                        EncMovimiento.Impuestos = encMovimiento.Impuestos;
                        EncMovimiento.Subtotal = encMovimiento.Subtotal;
                        EncMovimiento.PorDescuento = encMovimiento.PorDescuento;
                        EncMovimiento.TotalComprobante = encMovimiento.TotalComprobante;
                        EncMovimiento.Comentarios = encMovimiento.Comentarios;
                        EncMovimiento.Moneda = encMovimiento.Moneda;
                        EncMovimiento.Aprobada = false;
                        EncMovimiento.AprobadaSuperior = false;
                        EncMovimiento.idCondPago = encMovimiento.idCondPago;
                        EncMovimiento.idDiasValidos = encMovimiento.idDiasValidos;
                        EncMovimiento.idGarantia = encMovimiento.idGarantia;
                        EncMovimiento.idTiemposEntregas = encMovimiento.idTiemposEntregas;
                        db.EncMovimiento.Add(EncMovimiento);
                        db.SaveChanges();

                        /// Probar funcionabilidad
                        /// 
                        foreach (var item in Detalles)
                        {
                            try
                            {
                                encMovimiento.Detalle.Where(a => a.ItemCode == item.ItemCode).FirstOrDefault().idError = item.idError;
                            }
                            catch (Exception)
                            {


                            }
                        }


                        foreach (var item in encMovimiento.Detalle)
                        {

                            var Det = new DetMovimiento();
                            Det.idEncabezado = EncMovimiento.id;
                            Det.idError = item.idError;
                            Det.NumLinea = item.NumLinea;
                            Det.ItemCode = item.ItemCode;
                            Det.ItemName = item.ItemName;
                            Det.PrecioUnitario = item.PrecioUnitario;
                            Det.Cantidad = item.Cantidad;
                            Det.PorDescuento = item.PorDescuento;
                            Det.Descuento = item.Descuento;
                            Det.Impuestos = item.Impuestos;
                            Det.TotalLinea = item.TotalLinea;
                            Det.Garantia = item.Garantia;
                            Det.idImpuesto = item.idImpuesto;
                            Det.Opcional = item.Opcional;

                            db.DetMovimiento.Add(Det);
                            db.SaveChanges();
                            try
                            {
                                if (!item.ItemName.ToUpper().Contains("mano de obra".ToUpper()))
                                {
                                    var Prod = db.ProductosHijos.Where(a => a.codSAP == item.ItemCode).FirstOrDefault();
                                    if (Prod != null)
                                    {
                                        var idLLamada = Convert.ToInt32(EncMovimiento.NumLlamada);
                                        var llamada = db.LlamadasServicios.Where(a => a.DocEntry == idLLamada).FirstOrDefault();
                                        var ProdPadre = db.ProductosPadres.Where(a => a.codSAP == llamada.ItemCode).FirstOrDefault();
                                        var ExisteEnPadre = db.PadresHijosProductos.Where(a => a.idProductoPadre == ProdPadre.id && a.idProductoHijo == Prod.id).FirstOrDefault();

                                        if (ExisteEnPadre == null)
                                        {
                                            ExisteEnPadre = new PadresHijosProductos();
                                            ExisteEnPadre.idProductoHijo = Prod.id;
                                            ExisteEnPadre.idProductoPadre = ProdPadre.id;
                                            ExisteEnPadre.Cantidad = item.Cantidad;
                                            db.PadresHijosProductos.Add(ExisteEnPadre);
                                            db.SaveChanges();
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {


                            }



                        }
                    }


                }
                else
                {
                    throw new Exception("EncMovimiento no existe");
                }
                t.Commit();
                // PAra mandar a SAP
                if (encMovimiento.Generar)
                {
                    if ((EncMovimiento.TipoMovimiento == 1 || EncMovimiento.TipoMovimiento == 3) && db.DetMovimiento.Where(a => a.idEncabezado == EncMovimiento.id && a.Garantia == false).Count() > 0)
                    {
                        var id = EncMovimiento.id;
                        //if (encMovimiento.Regenerar)
                        //{
                        //    var EncMovimiento2 = EncMovimiento;
                        //    EncMovimiento2.DocEntry = 0;
                        //    EncMovimiento2.Fecha = DateTime.Now;

                        //    db.EncMovimiento.Add(EncMovimiento2);
                        //    db.SaveChanges();
                        //    var Detalles2 = db.DetMovimiento.Where(a => a.idEncabezado == id).ToList();
                        //    id = EncMovimiento2.id;
                        //    foreach (var item in Detalles2)
                        //    {
                        //        item.idEncabezado = EncMovimiento2.id;
                        //        db.DetMovimiento.Add(item);
                        //        db.SaveChanges();

                        //    }
                        //}
                        var client = (Documents)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oQuotations);
                        client.DocObjectCode = BoObjectTypes.oQuotations;
                        client.CardCode = EncMovimiento.CardCode;
                        client.DocCurrency = EncMovimiento.Moneda; // "COL";
                        client.DocDate = DateTime.Now;//EncMovimiento.Fecha; //listo
                        client.DocDueDate = DateTime.Now.AddDays(3); //listo
                        client.DocNum = 0; //automatico
                        client.DocType = BoDocumentTypes.dDocument_Items;
                        client.HandWritten = BoYesNoEnum.tNO;
                        client.NumAtCard = EncMovimiento.id.ToString(); //orderid               
                        client.ReserveInvoice = BoYesNoEnum.tNO;
                        client.Series = Parametros.SerieOferta; //11; //11 quemado
                        client.Comments = g.TruncarString(EncMovimiento.Comentarios, 200); //direccion
                        client.DiscountPercent = Convert.ToDouble(EncMovimiento.PorDescuento); //direccion
                        var Llam = Convert.ToInt32(EncMovimiento.NumLlamada);
                        var Llamada2 = db.LlamadasServicios.Where(a => a.DocEntry == Llam).FirstOrDefault();
                        var Tec = Llamada2.Tecnico == null ? "" : Llamada2.Tecnico.ToString();
                        var Tecnico = db.Tecnicos.Where(a => a.idSAP == Tec).FirstOrDefault();

                        client.DocumentsOwner = Convert.ToInt32(Llamada2.Tecnico);

                        if (Tecnico.Letra > 0)
                        {
                            client.SalesPersonCode = Tecnico.Letra;
                        }

                        client.UserFields.Fields.Item("U_DYD_Boleta").Value = EncMovimiento.NumLlamada.ToString();

                        var DetalleSAP = db.DetMovimiento.Where(a => a.idEncabezado == EncMovimiento.id && a.Garantia == false).ToList();
                        var i = 0;
                        foreach (var item in DetalleSAP)
                        {
                            client.Lines.SetCurrentLine(i);
                            client.Lines.CostingCode = "";
                            client.Lines.CostingCode2 = "";
                            client.Lines.CostingCode3 = Parametros.CostingCode; //"TA-01";
                            client.Lines.CostingCode4 = "";
                            client.Lines.CostingCode5 = "";
                            client.Lines.Currency = EncMovimiento.Moneda;
                            client.Lines.WarehouseCode = db.Parametros.FirstOrDefault().BodegaFinal;
                            client.Lines.DiscountPercent = Convert.ToDouble(item.PorDescuento);
                            client.Lines.ItemCode = item.ItemCode;
                            client.Lines.DiscountPercent = Convert.ToDouble(item.PorDescuento);
                            client.Lines.Quantity = Convert.ToDouble(item.Cantidad);
                            client.Lines.TaxCode = db.Impuestos.Where(a => a.id == item.idImpuesto).FirstOrDefault() == null ? Parametros.TaxCode : db.Impuestos.Where(a => a.id == item.idImpuesto).FirstOrDefault().CodSAP;  //Parametros.TaxCode;//"IVA-13";
                            client.Lines.TaxOnly = BoYesNoEnum.tNO;
                            client.Lines.UnitPrice = Convert.ToDouble(item.PrecioUnitario);
                            client.Lines.Add();


                            i++;
                        }

                        var respuesta = client.Add();

                        if (respuesta == 0)
                        {
                            var enc = db.EncMovimiento.Where(a => a.id == id).FirstOrDefault();
                            db.Entry(enc).State = EntityState.Modified;
                            try
                            {
                                enc.DocEntry = Convert.ToInt32(Conexion.Company.GetNewObjectKey());
                                throw new Exception("");

                            }
                            catch (Exception)
                            {
                                try
                                {
                                   
                                    var conexion = g.DevuelveCadena(db);
                                    var valorAFiltrar = EncMovimiento.id.ToString();
                                    var filtroSQL = "NumAtCard like '%" + valorAFiltrar + "%'";
                                    var SQL = Parametros.SQLDocEntryDocs.Replace("@CampoBuscar", "DocEntry").Replace("@Tabla", "OQUT").Replace("@CampoWhere = @reemplazo", filtroSQL);

                                    SqlConnection Cn = new SqlConnection(conexion);
                                    SqlCommand Cmd = new SqlCommand(SQL, Cn);
                                    SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                                    DataSet Ds = new DataSet();
                                    Cn.Open();
                                    Da.Fill(Ds, "DocNum1");
                                    enc.DocEntry = Convert.ToInt32(Ds.Tables["DocNum1"].Rows[0]["DocEntry"]);

                                    Cn.Close();
                                }
                                catch (Exception ex1)
                                {
                                    BitacoraErrores be = new BitacoraErrores();

                                    be.Descripcion = "Error en la oferta #" + EncMovimiento.id + " , al conseguir el docEntry -> " + ex1.Message;
                                    be.StackTrace = ex1.StackTrace;
                                    be.Fecha = DateTime.Now;

                                    db.BitacoraErrores.Add(be);
                                    db.SaveChanges();

                                }

                            }
                          
                            db.SaveChanges();
                            Conexion.Desconectar();
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
                            throw new Exception("Error en SAP: " + be.Descripcion);
                        }



                    }
                    else
                    {
                        var DetalleSAP = db.DetMovimiento.Where(a => a.idEncabezado == EncMovimiento.id && a.Garantia == false).ToList();
                        //var DetalleSAP = db.DetMovimiento.Where(a => a.idEncabezado == EncMovimiento.id).ToList();

                        var CumpleParaOrden = (DetalleSAP.Count() == 1 && DetalleSAP.Where(a => a.ItemName.ToLower().Contains("mano de obra")).FirstOrDefault() != null);
                        if (CumpleParaOrden)
                        {
                            var orden = (Documents)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders);
                            orden.DocObjectCode = BoObjectTypes.oOrders;
                            orden.CardCode = EncMovimiento.CardCode;
                            orden.DocCurrency = EncMovimiento.Moneda; //"COL";
                            orden.DocDate = DateTime.Now;// EncMovimiento.Fecha; //listo
                            orden.DocDueDate = DateTime.Now.AddDays(3); //listo
                            orden.DocNum = 0; //automatico
                            orden.DocType = BoDocumentTypes.dDocument_Items;
                            orden.HandWritten = BoYesNoEnum.tNO;
                            orden.NumAtCard = EncMovimiento.id.ToString(); //orderid               
                            orden.ReserveInvoice = BoYesNoEnum.tNO;
                            orden.Series = Parametros.SeriesOrdenVenta;//3; //3 quemado
                            orden.Comments = g.TruncarString(EncMovimiento.Comentarios, 200); //direccion
                            orden.DiscountPercent = Convert.ToDouble(EncMovimiento.PorDescuento); //direccion 
                            var LlamO = Convert.ToInt32(EncMovimiento.NumLlamada);
                            var LlamadaO2 = db.LlamadasServicios.Where(a => a.DocEntry == LlamO).FirstOrDefault();
                            var Tec2 = LlamadaO2.Tecnico == null ? "" : LlamadaO2.Tecnico.ToString();
                            var Tecnico2 = db.Tecnicos.Where(a => a.idSAP == Tec2).FirstOrDefault();

                            orden.DocumentsOwner = Convert.ToInt32(LlamadaO2.Tecnico); // Convert.ToInt32(EncMovimiento.CreadoPor); //Quemado 47
                            if (Tecnico2.Letra > 0)
                            {
                                orden.SalesPersonCode = Tecnico2.Letra;
                            }
                            orden.UserFields.Fields.Item("U_DYD_Boleta").Value = EncMovimiento.NumLlamada.ToString();

                            var ii = 0;
                            foreach (var item in DetalleSAP)
                            {
                                orden.Lines.SetCurrentLine(ii);
                                orden.Lines.CostingCode = "";
                                orden.Lines.CostingCode2 = "";
                                orden.Lines.CostingCode3 = Parametros.CostingCode; //"TA-01";
                                orden.Lines.CostingCode4 = "";
                                orden.Lines.CostingCode5 = "";
                                orden.Lines.Currency = EncMovimiento.Moneda; //"COL";
                                orden.Lines.WarehouseCode = db.Parametros.FirstOrDefault().BodegaFinal;
                                orden.Lines.DiscountPercent = Convert.ToDouble(item.PorDescuento);
                                orden.Lines.ItemCode = item.ItemCode;
                                orden.Lines.DiscountPercent = Convert.ToDouble(item.PorDescuento);
                                orden.Lines.Quantity = Convert.ToDouble(item.Cantidad);
                                orden.Lines.TaxCode = db.Impuestos.Where(a => a.id == item.idImpuesto).FirstOrDefault() == null ? Parametros.TaxCode : db.Impuestos.Where(a => a.id == item.idImpuesto).FirstOrDefault().CodSAP;  //Parametros.TaxCode;// "IVA-13";
                                orden.Lines.TaxOnly = BoYesNoEnum.tNO;
                                orden.Lines.UnitPrice = Convert.ToDouble(item.PrecioUnitario);

                                orden.Lines.Add();


                                ii++;
                            }

                            var respuestaO = orden.Add();

                            if (respuestaO == 0)
                            {

                                var DocEntry = 0;//Convert.ToInt32(Conexion.Company.GetNewObjectKey());
                                try
                                {
                                    DocEntry = Convert.ToInt32(Conexion.Company.GetNewObjectKey());
                                }
                                catch (Exception)
                                {
                                    try
                                    {

                                        var conexion2 = g.DevuelveCadena(db);
                                        var valorAFiltrar = EncMovimiento.id.ToString();
                                        var filtroSQL = "NumAtCard like '%" + valorAFiltrar + "%'";
                                        var SQL2 = Parametros.SQLDocEntryDocs.Replace("@CampoBuscar", "DocEntry").Replace("@Tabla", "ORDR").Replace("@CampoWhere = @reemplazo", filtroSQL);

                                        SqlConnection Cn2 = new SqlConnection(conexion2);
                                        SqlCommand Cmd2 = new SqlCommand(SQL2, Cn2);
                                        SqlDataAdapter Da2 = new SqlDataAdapter(Cmd2);
                                        DataSet Ds2 = new DataSet();
                                        Cn2.Open();
                                        Da2.Fill(Ds2, "DocNum1");
                                        DocEntry = Convert.ToInt32(Ds2.Tables["DocNum1"].Rows[0]["DocEntry"]);

                                        Cn2.Close();
                                    }
                                    catch (Exception ex1)
                                    {
                                        BitacoraErrores be = new BitacoraErrores();

                                        be.Descripcion = "Error en la orden #" + EncMovimiento.id + " , al conseguir el docEntry -> " + ex1.Message;
                                        be.StackTrace = ex1.StackTrace;
                                        be.Fecha = DateTime.Now;

                                        db.BitacoraErrores.Add(be);
                                        db.SaveChanges();

                                    }

                                }
                                var DocNum = 0;

                                db.Entry(EncMovimiento).State = EntityState.Modified;
                                EncMovimiento.DocEntry = DocEntry;

                                db.SaveChanges();

                                var conexion = g.DevuelveCadena(db);

                                var SQL = " select top 1 DocNum from ORDR where DocEntry = '" + DocEntry + "'";

                                SqlConnection Cn = new SqlConnection(conexion);
                                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                                DataSet Ds = new DataSet();
                                Cn.Open();
                                Da.Fill(Ds, "Encabezado");
                                DocNum = Convert.ToInt32(Ds.Tables["Encabezado"].Rows[0]["DocNum"].ToString());
                                Cn.Close();
                                Cn.Dispose();

                                var idEntry = DocEntry;
                                var CantidadExpenses = 0;
                                var client2 = (ServiceCalls)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oServiceCalls);
                                if (client2.GetByKey(Convert.ToInt32(EncMovimiento.NumLlamada)))
                                {
                                    CantidadExpenses = client2.Expenses.Count;
                                    if (client2.Expenses.Count > 1)
                                    {
                                        client2.Expenses.Add();
                                    }
                                    client2.Expenses.DocumentType = BoSvcEpxDocTypes.edt_Order;
                                    client2.Expenses.DocumentNumber = DocNum;
                                    client2.Expenses.DocEntry = idEntry;



                                    if (client2.Expenses.Count == 1 || client2.Expenses.Count == 0)
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
                                        be.StackTrace = "Insercion de Liga EN - OR " + DocNum + " - " + idEntry + " - " + CantidadExpenses;
                                        be.Fecha = DateTime.Now;

                                        db.BitacoraErrores.Add(be);
                                        db.SaveChanges();
                                        Conexion.Desconectar();


                                    }
                                }






                            }
                            else
                            {
                                BitacoraErrores be = new BitacoraErrores();

                                be.Descripcion = Conexion.Company.GetLastErrorDescription();
                                be.StackTrace = "Movimientos Ordenes";
                                be.Fecha = DateTime.Now;

                                db.BitacoraErrores.Add(be);
                                db.SaveChanges();
                                Conexion.Desconectar();

                            }
                        }

                        var Llam = Convert.ToInt32(EncMovimiento.NumLlamada);
                        var Llamada2 = db.LlamadasServicios.Where(a => a.DocEntry == Llam).FirstOrDefault();
                        if (!CumpleParaOrden && DetalleSAP.Count() > 0)
                        {
                            var client = (Documents)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDeliveryNotes);
                            client.DocObjectCode = BoObjectTypes.oDeliveryNotes;
                            client.CardCode = EncMovimiento.CardCode;
                            client.DocCurrency = EncMovimiento.Moneda; //"COL";
                            client.DocDate = DateTime.Now; //EncMovimiento.Fecha; //listo
                            client.DocDueDate = DateTime.Now.AddDays(3); //listo
                            client.DocNum = 0; //automatico
                            client.DocType = BoDocumentTypes.dDocument_Items;
                            client.HandWritten = BoYesNoEnum.tNO;
                            client.NumAtCard = EncMovimiento.id.ToString(); //orderid               
                            client.ReserveInvoice = BoYesNoEnum.tNO;
                            client.Series = Parametros.SerieEntrega;//3; //3 quemado
                            client.Comments = g.TruncarString(EncMovimiento.Comentarios, 200); //direccion
                            client.DiscountPercent = Convert.ToDouble(EncMovimiento.PorDescuento); //direccion

                            var Tec = Llamada2.Tecnico == null ? "" : Llamada2.Tecnico.ToString();
                            var Tecnico = db.Tecnicos.Where(a => a.idSAP == Tec).FirstOrDefault();

                            client.DocumentsOwner = Convert.ToInt32(Llamada2.Tecnico); // Convert.ToInt32(EncMovimiento.CreadoPor); //Quemado 47
                            if (Tecnico.Letra > 0)
                            {
                                client.SalesPersonCode = Tecnico.Letra;
                            }
                            client.UserFields.Fields.Item("U_DYD_Boleta").Value = EncMovimiento.NumLlamada.ToString();

                            var i = 0;
                            foreach (var item in DetalleSAP)
                            {
                                client.Lines.SetCurrentLine(i);
                                client.Lines.CostingCode = "";
                                client.Lines.CostingCode2 = "";
                                client.Lines.CostingCode3 = Parametros.CostingCode; //"TA-01";
                                client.Lines.CostingCode4 = "";
                                client.Lines.CostingCode5 = "";
                                client.Lines.Currency = EncMovimiento.Moneda; //"COL";
                                client.Lines.WarehouseCode = db.Parametros.FirstOrDefault().BodegaFinal;
                                client.Lines.DiscountPercent = Convert.ToDouble(item.PorDescuento);
                                client.Lines.ItemCode = item.ItemCode;
                                client.Lines.DiscountPercent = Convert.ToDouble(item.PorDescuento);
                                client.Lines.Quantity = Convert.ToDouble(item.Cantidad);
                                client.Lines.TaxCode = db.Impuestos.Where(a => a.id == item.idImpuesto).FirstOrDefault() == null ? Parametros.TaxCode : db.Impuestos.Where(a => a.id == item.idImpuesto).FirstOrDefault().CodSAP;  //Parametros.TaxCode;// "IVA-13";
                                client.Lines.TaxOnly = BoYesNoEnum.tNO;
                                client.Lines.UnitPrice = Convert.ToDouble(item.PrecioUnitario);

                                client.Lines.Add();


                                i++;
                            }

                            var respuesta = client.Add();

                            if (respuesta == 0)
                            {
                                db.Entry(EncMovimiento).State = EntityState.Modified;
                               
                                try
                                {
                                    EncMovimiento.DocEntry = Convert.ToInt32(Conexion.Company.GetNewObjectKey());
                                    throw new Exception("");
                                }
                                catch (Exception)
                                {
                                    try
                                    {

                                        var conexion = g.DevuelveCadena(db);
                                        var valorAFiltrar = EncMovimiento.id.ToString();
                                        var filtroSQL = "NumAtCard like '%" + valorAFiltrar + "%'";
                                        var SQL = Parametros.SQLDocEntryDocs.Replace("@CampoBuscar", "DocEntry").Replace("@Tabla", "ODLN").Replace("@CampoWhere = @reemplazo", filtroSQL);

                                        SqlConnection Cn = new SqlConnection(conexion);
                                        SqlCommand Cmd = new SqlCommand(SQL, Cn);
                                        SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                                        DataSet Ds = new DataSet();
                                        Cn.Open();
                                        Da.Fill(Ds, "DocNum1");
                                        EncMovimiento.DocEntry = Convert.ToInt32(Ds.Tables["DocNum1"].Rows[0]["DocEntry"]);

                                        Cn.Close();
                                    }
                                    catch (Exception ex1)
                                    {
                                        BitacoraErrores be = new BitacoraErrores();

                                        be.Descripcion = "Error en la entrega #" + EncMovimiento.id + " , al conseguir el docEntry -> " + ex1.Message;
                                        be.StackTrace = ex1.StackTrace;
                                        be.Fecha = DateTime.Now;

                                        db.BitacoraErrores.Add(be);
                                        db.SaveChanges();

                                    }

                                }
                                db.SaveChanges();
                                var idEntry = EncMovimiento.DocEntry;//Convert.ToInt32(Conexion.Company.GetNewObjectKey());
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
                                        be.StackTrace = "Insercion de Liga EN - LL " + client.DocNum;
                                        be.Fecha = DateTime.Now;

                                        db.BitacoraErrores.Add(be);
                                        db.SaveChanges();
                                        Conexion.Desconectar();
                                        throw new Exception(be.Descripcion);

                                    }
                                }

                                Conexion.Desconectar();




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
                                throw new Exception("Error en SAP: " + be.Descripcion);

                            }
                        }


                        //Pregunto si existe algun producto con garantia, para generar entonces una entrega
                        if (db.DetMovimiento.Where(a => a.idEncabezado == EncMovimiento.id && a.Garantia == true).Count() > 0)
                        {
                            //var clientEntrega = (Documents)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDeliveryNotes);
                            //clientEntrega.DocObjectCode = BoObjectTypes.oDeliveryNotes;
                            //clientEntrega.CardCode = EncMovimiento.CardCode;
                            //clientEntrega.DocCurrency = EncMovimiento.Moneda; //"COL";
                            //clientEntrega.DocDate = EncMovimiento.Fecha; //listo
                            //clientEntrega.DocDueDate = EncMovimiento.Fecha.AddDays(3); //listo
                            //clientEntrega.DocNum = 0; //automatico
                            //clientEntrega.DocType = BoDocumentTypes.dDocument_Items;
                            //clientEntrega.HandWritten = BoYesNoEnum.tNO;
                            //clientEntrega.NumAtCard = EncMovimiento.NumLlamada; //orderid               
                            //clientEntrega.ReserveInvoice = BoYesNoEnum.tNO;
                            //clientEntrega.Series = Parametros.SerieEntrega;//3; //3 quemado
                            //clientEntrega.Comments = "Esta es la entrega de los productos por garantia"; //direccion
                            //clientEntrega.DiscountPercent = Convert.ToDouble(EncMovimiento.PorDescuento); //direccion
                            //                                                                              //var Llam = Convert.ToInt32(EncMovimiento.NumLlamada);
                            //                                                                              //var Llamada = db.LlamadasServicios.Where(a => a.DocEntry == Llam).FirstOrDefault();
                            //var Tec1 = Llamada2.Tecnico == null ? "" : Llamada2.Tecnico.ToString();
                            //var Tecnico1 = db.Tecnicos.Where(a => a.idSAP == Tec1).FirstOrDefault();
                            //clientEntrega.DocumentsOwner = Convert.ToInt32(Llamada2.Tecnico);
                            //if (Tecnico1.Letra > 0)
                            //{
                            //    clientEntrega.SalesPersonCode = Tecnico1.Letra;
                            //}

                            //clientEntrega.UserFields.Fields.Item("U_DYD_Boleta").Value = EncMovimiento.NumLlamada.ToString();

                            var DetalleSAPEntrega = db.DetMovimiento.Where(a => a.idEncabezado == EncMovimiento.id && a.Garantia == true).ToList();
                            //var iE = 0;
                            //foreach (var item in DetalleSAPEntrega)
                            //{
                            //    clientEntrega.Lines.SetCurrentLine(iE);
                            //    clientEntrega.Lines.CostingCode = "";
                            //    clientEntrega.Lines.CostingCode2 = "";
                            //    clientEntrega.Lines.CostingCode3 = Parametros.CostingCode; //"TA-01";
                            //    clientEntrega.Lines.CostingCode4 = "";
                            //    clientEntrega.Lines.CostingCode5 = "";
                            //    clientEntrega.Lines.Currency = EncMovimiento.Moneda; //"COL";
                            //    clientEntrega.Lines.WarehouseCode = db.Parametros.FirstOrDefault().BodegaFinal;
                            //    clientEntrega.Lines.DiscountPercent = Convert.ToDouble(item.PorDescuento);
                            //    clientEntrega.Lines.ItemCode = item.ItemCode;
                            //    clientEntrega.Lines.DiscountPercent = Convert.ToDouble(item.PorDescuento);
                            //    clientEntrega.Lines.Quantity = Convert.ToDouble(item.Cantidad);
                            //    clientEntrega.Lines.TaxCode = db.Impuestos.Where(a => a.id == item.idImpuesto).FirstOrDefault() == null ? Parametros.TaxCode : db.Impuestos.Where(a => a.id == item.idImpuesto).FirstOrDefault().CodSAP;  //Parametros.TaxCode;// "IVA-13";
                            //    clientEntrega.Lines.TaxOnly = BoYesNoEnum.tNO;
                            //    clientEntrega.Lines.UnitPrice = Convert.ToDouble(item.PrecioUnitario);
                            //    clientEntrega.Lines.Add();


                            //    iE++;
                            //}

                            //var respuestaE = clientEntrega.Add();
                            var respuestaE = 0;
                            if (respuestaE == 0)
                            {

                                var EncMovimientoEntrega = EncMovimiento;
                                EncMovimientoEntrega.DocEntry = 0; //Convert.ToInt32(Conexion.Company.GetNewObjectKey());
                                EncMovimientoEntrega.TipoMovimiento = 2;
                                EncMovimientoEntrega.Descuento = 0;
                                EncMovimientoEntrega.Impuestos = 0;
                                EncMovimientoEntrega.Subtotal = 0;
                                EncMovimientoEntrega.PorDescuento = 0;
                                EncMovimientoEntrega.TotalComprobante = 0;
                                EncMovimientoEntrega.Comentarios = "Esta es la entrega de los productos por garantia";
                                EncMovimientoEntrega.Aprobada = false;
                                EncMovimientoEntrega.AprobadaSuperior = false;
                                EncMovimientoEntrega.idCondPago = 0;
                                EncMovimientoEntrega.idDiasValidos = 0;
                                EncMovimientoEntrega.idGarantia = 0;
                                EncMovimientoEntrega.idTiemposEntregas = 0;
                                db.EncMovimiento.Add(EncMovimientoEntrega);
                                db.SaveChanges();

                                foreach (var item in DetalleSAPEntrega)
                                {
                                    item.idEncabezado = EncMovimientoEntrega.id;

                                    db.DetMovimiento.Add(item);
                                    db.SaveChanges();


                                    db.Entry(EncMovimientoEntrega).State = EntityState.Modified;

                                    EncMovimientoEntrega.Descuento += item.Descuento;
                                    EncMovimientoEntrega.Impuestos += item.Impuestos;
                                    EncMovimientoEntrega.Subtotal += item.TotalLinea - item.Impuestos;
                                    EncMovimientoEntrega.PorDescuento += item.PorDescuento;
                                    EncMovimientoEntrega.TotalComprobante += item.TotalLinea;

                                    db.SaveChanges();
                                }




                                //var idEntry = Convert.ToInt32(Conexion.Company.GetNewObjectKey());
                                //var client2 = (ServiceCalls)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oServiceCalls);
                                //if (client2.GetByKey(Convert.ToInt32(EncMovimiento.NumLlamada)))
                                //{
                                //    if (client2.Expenses.Count > 0)
                                //    {
                                //        client2.Expenses.Add();
                                //    }
                                //    client2.Expenses.DocumentType = BoSvcEpxDocTypes.edt_Delivery;
                                //    client2.Expenses.DocumentNumber = idEntry;
                                //    client2.Expenses.DocEntry = idEntry;

                                //    if (client2.Expenses.Count == 0)
                                //    {
                                //        client2.Expenses.Add();
                                //    }
                                //    client2.Expenses.Add();
                                //    var respuesta2 = client2.Update();
                                //    if (respuesta2 == 0)
                                //    {
                                //        Conexion.Desconectar();
                                //    }
                                //    else
                                //    {
                                //        BitacoraErrores be = new BitacoraErrores();

                                //        be.Descripcion = Conexion.Company.GetLastErrorDescription();
                                //        be.StackTrace = "Llamada de Servicio - Actualizar";
                                //        be.Fecha = DateTime.Now;

                                //        db.BitacoraErrores.Add(be);
                                //        db.SaveChanges();
                                //        Conexion.Desconectar();
                                //        throw new Exception(be.Descripcion);

                                //    }
                                //}

                                //Conexion.Desconectar();






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
                }

                return Request.CreateResponse(HttpStatusCode.OK, EncMovimiento);
            }
            catch (Exception ex)
            {
                try
                {
                    t.Rollback();
                }
                catch (Exception)
                {


                }

                BitacoraErrores be = new BitacoraErrores();

                be.Descripcion = "Error en el movimiento #" + encMovimiento.id + " -> " + ex.Message;
                be.StackTrace = ex.StackTrace;
                be.Fecha = DateTime.Now;
                db.BitacoraErrores.Add(be);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [Route("api/Movimientos/Eliminar")]
        public HttpResponseMessage Delete([FromUri] int id)
        {
            var t = db.Database.BeginTransaction();
            try
            {


                var EncMovimiento = db.EncMovimiento.Where(a => a.id == id).FirstOrDefault();

                if (EncMovimiento != null)
                {

                    var Detalles = db.DetMovimiento.Where(a => a.idEncabezado == EncMovimiento.id).ToList();

                    foreach (var item in Detalles)
                    {
                        db.DetMovimiento.Remove(item);
                        db.SaveChanges();
                    }

                    db.EncMovimiento.Remove(EncMovimiento);
                    db.SaveChanges();
                    t.Commit();
                }
                else
                {
                    throw new Exception("EncMovimmiento no existe");
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                t.Rollback();
                BitacoraErrores be = new BitacoraErrores();

                be.Descripcion = "Error en el Movimiento #" + id + " -> " + ex.Message;
                be.StackTrace = ex.StackTrace;
                be.Fecha = DateTime.Now;
                db.BitacoraErrores.Add(be);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }


        [HttpPost]
        [Route("api/Movimientos/AprobarSuperior")]
        public HttpResponseMessage AprobarSuperior([FromUri] int id)
        {
            var t = db.Database.BeginTransaction();
            try
            {


                var EncMovimiento = db.EncMovimiento.Where(a => a.id == id).FirstOrDefault();

                if (EncMovimiento != null)
                {

                    db.Entry(EncMovimiento).State = EntityState.Modified;
                    EncMovimiento.AprobadaSuperior = true;

                    db.SaveChanges();
                    t.Commit();
                }
                else
                {
                    throw new Exception("EncMovimmiento no existe");
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                t.Rollback();
                BitacoraErrores be = new BitacoraErrores();

                be.Descripcion = "Error en el Movimiento #" + id + " -> " + ex.Message;
                be.StackTrace = ex.StackTrace;
                be.Fecha = DateTime.Now;
                db.BitacoraErrores.Add(be);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

    }
}