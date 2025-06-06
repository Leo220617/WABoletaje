﻿using SAPbobsCOM;
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

                        foreach (var item in DetalleReparaciones.Where(a => !a.ItemName.ToLower().Contains("mano de obra") && !a.ItemCode.ToLower().Contains("C0-000-001")))
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
                            dbt.SolicitudCompra = false;
                            dbt.SolicitudProcesada = false;
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

                GC.Collect();
                GC.WaitForPendingFinalizers();

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
                GC.Collect();
                GC.WaitForPendingFinalizers();
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
                            var bodyH = G.ObtenerConfig("Empresa") == "G" ? G.ObtenerConfig("Pais") == "P" ? Html.textoOfertaGermantecPanama : Html.textoOfertaGermantec : Html.textoOfertaAlsaraBackOffice;
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
                            var bodyH = G.ObtenerConfig("Empresa") == "G" ? G.ObtenerConfig("Pais") == "P" ? Html.textoEntregaGermantecPanama : Html.textoEntregaGermantec : Html.textoEntregaAlsara;
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
                GC.Collect();
                GC.WaitForPendingFinalizers();
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
                GC.Collect();
                GC.WaitForPendingFinalizers();
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
                    var encMovimientos = db.EncMovimiento.Where(a => (filtro.FechaInicial != time ? a.Fecha >= filtro.FechaInicial : true)
                    && (filtro.FechaFinal != time ? a.Fecha <= filtro.FechaFinal : true)
                    && (filtro.FiltrarFacturado ? (filtro.NoFacturado ? a.Facturado == false : a.Facturado == true) : true)
                    && (filtro.Codigo1 > 0 ? a.TipoMovimiento == filtro.Codigo1 : true)
                     && (filtro.DocEntryGenerado > 0 ? a.DocEntry > 0 : true)
                     && (filtro.Codigo5 > 0 ? !a.AprobadaSuperior : true)
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
                                z.TipoCaso,
                                z.PrioridadAtencion,
                                z.Garantia
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
                        a.Facturado,
                        a.DocEntryDevolucion,
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
                                PrioridadAtencion = a.Llamada?.PrioridadAtencion ?? "",
                                StatusLlamada = a.Llamada?.Status ?? 0,
                                TipoCaso = a.Llamada?.TipoCaso ?? 0,
                                GarantiaLlamada = a.Llamada?.Garantia ?? 0,
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
                                a.Facturado,
                                a.DocEntryDevolucion,
                                a.Detalle
                            })
                            .ToList();

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

                    if (!string.IsNullOrEmpty(filtro.Texto) || !string.IsNullOrEmpty(filtro.CardCode))
                    {


                        encMovimientos = db.EncMovimiento
                       .Where(a => (!string.IsNullOrEmpty(filtro.Texto) ? a.NumLlamada == filtro.Texto : true)
                       && (!string.IsNullOrEmpty(filtro.CardCode) ? a.CardCode.Contains(filtro.CardCode) : true)
                       && (filtro.Codigo1 > 0 ? a.TipoMovimiento == filtro.Codigo1 : true)
                       && (filtro.DocEntryGenerado > 0 ? a.DocEntry > 0 : true)
                        && (filtro.Codigo5 > 0 ? !a.AprobadaSuperior : true)
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
                    z.TipoCaso,
                    z.PrioridadAtencion,
                    z.Garantia
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
                           a.Facturado,
                           a.DocEntryDevolucion,
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
        PrioridadAtencion = a.Llamada?.PrioridadAtencion ?? "",
        StatusLlamada = a.Llamada?.Status ?? 0,
        TipoCaso = a.Llamada?.TipoCaso ?? 0,
        GarantiaLlamada = a.Llamada?.Garantia ?? 0,
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
        a.Facturado,
        a.DocEntryDevolucion,
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
                         && (filtro.Codigo1 > 0 ? a.TipoMovimiento == filtro.Codigo1 : true)
                        && (filtro.DocEntryGenerado > 0 ? a.DocEntry > 0 : true)
                         && (filtro.Codigo5 > 0 ? !a.AprobadaSuperior : true)
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
                    z.TipoCaso,
                    z.PrioridadAtencion,
                    z.Garantia
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
                             a.Facturado,
                             a.DocEntryDevolucion,
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
        PrioridadAtencion = a.Llamada?.PrioridadAtencion ?? "",
        StatusLlamada = a.Llamada?.Status ?? 0,
        TipoCaso = a.Llamada?.TipoCaso ?? 0,
        GarantiaLlamada = a.Llamada?.Garantia ?? 0,
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
        a.Facturado,
        a.DocEntryDevolucion,
        a.Detalle
    })
    .ToList(); ;




                        return Request.CreateResponse(HttpStatusCode.OK, encMovimientos);
                    }
                    else
                    {



                        if (!string.IsNullOrEmpty(filtro.CardName)) //Status
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


                                var encMovimientos = db.EncMovimiento
                                    .Where(a => !llamadas.Contains(a.NumLlamada) && (filtro.Codigo1 > 0 ? a.TipoMovimiento == filtro.Codigo1 : true)
                                    && (filtro.DocEntryGenerado > 0 ? a.DocEntry > 0 : true)
                                     && (filtro.Codigo5 > 0 ? !a.AprobadaSuperior : true))
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
                        z.TipoCaso,
                        z.PrioridadAtencion,
                        z.Garantia
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
                                   a.Facturado,
                                   a.DocEntryDevolucion,
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
        PrioridadAtencion = a.Llamada?.PrioridadAtencion ?? "",
        StatusLlamada = a.Llamada?.Status ?? 0,
        TipoCaso = a.Llamada?.TipoCaso ?? 0,
        GarantiaLlamada = a.Llamada?.Garantia ?? 0,
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
        a.DocEntryDevolucion,
        a.Detalle
    })
    .ToList();

                                return Request.CreateResponse(HttpStatusCode.OK, encMovimientos.ToList());

                            }
                            else
                            {
                                var encMovimientos = db.EncMovimiento.Where(a => (filtro.FechaInicial != time ? a.Fecha >= filtro.FechaInicial : true)
                                && (filtro.FechaFinal != time ? a.Fecha <= filtro.FechaFinal : true)
                                 && (filtro.Codigo1 > 0 ? a.TipoMovimiento == filtro.Codigo1 : true)
                       && (filtro.FiltrarFacturado ? (filtro.NoFacturado ? a.Facturado == false : a.Facturado == true) : true)
                       && (filtro.DocEntryGenerado > 0 ? a.DocEntry > 0 : true)
                        && (filtro.Codigo5 > 0 ? !a.AprobadaSuperior : true)
                                )

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
                        z.TipoCaso,
                        z.PrioridadAtencion,
                        z.Garantia
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
                         a.Facturado,
                         a.DocEntryDevolucion,
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
        PrioridadAtencion = a.Llamada?.PrioridadAtencion ?? "",
        StatusLlamada = a.Llamada?.Status ?? 0,
        TipoCaso = a.Llamada?.TipoCaso ?? 0,
        GarantiaLlamada = a.Llamada?.Garantia ?? 0,
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
        a.Facturado,
        a.DocEntryDevolucion,
        a.Detalle
    })
    .ToList();
                                return Request.CreateResponse(HttpStatusCode.OK, encMovimientos.ToList());
                            }


                        }
                        else
                        {
                            var encMovimientos = db.EncMovimiento.Where(a => (filtro.FechaInicial != time ? a.Fecha >= filtro.FechaInicial : true)
                            && (filtro.FechaFinal != time ? a.Fecha <= filtro.FechaFinal : true)
                            && (filtro.Codigo1 > 0 ? a.TipoMovimiento == filtro.Codigo1 : true)
                       && (filtro.FiltrarFacturado ? (filtro.NoFacturado ? a.Facturado == false : a.Facturado == true) : true)
                       && (filtro.DocEntryGenerado > 0 ? a.DocEntry > 0 : true)
                        && (filtro.Codigo5 > 0 ? !a.AprobadaSuperior : true)
                            )

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
                        z.TipoCaso,
                        z.PrioridadAtencion,
                        z.Garantia
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
                                      a.Facturado,
                                      a.DocEntryDevolucion,
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
        PrioridadAtencion = a.Llamada?.PrioridadAtencion ?? "",
        StatusLlamada = a.Llamada?.Status ?? 0,
        TipoCaso = a.Llamada?.TipoCaso ?? 0,
        GarantiaLlamada = a.Llamada?.Garantia ?? 0,
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
        a.Facturado,
        a.DocEntryDevolucion,
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

                    EmailPersonaContacto = db.LlamadasServicios
                    .Where(z => z.DocEntry.ToString() == a.NumLlamada).FirstOrDefault() == null ? "" : db.LlamadasServicios
                    .Where(z => z.DocEntry.ToString() == a.NumLlamada).FirstOrDefault().EmailPersonaContacto,
                    GarantiaLlamada = db.LlamadasServicios
                    .Where(z => z.DocEntry.ToString() == a.NumLlamada).FirstOrDefault() == null ? 0 : db.LlamadasServicios
                    .Where(z => z.DocEntry.ToString() == a.NumLlamada).FirstOrDefault().Garantia,
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
                    a.DocEntryDevolucion,
                    a.Redondeo,

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
                ParametrosFacturacion paramFac = new ParametrosFacturacion();

                if (EncMovimiento != null)
                {
                    if (!encMovimiento.Regenerar)
                    {
                        db.Entry(EncMovimiento).State = EntityState.Modified;
                        EncMovimiento.CreadoPor = encMovimiento.CreadoPor;
                        EncMovimiento.Descuento = encMovimiento.Descuento;
                        EncMovimiento.Impuestos = encMovimiento.Impuestos;
                        EncMovimiento.Subtotal = encMovimiento.Subtotal;
                        EncMovimiento.PorDescuento = Math.Round(encMovimiento.PorDescuento, 6);
                        EncMovimiento.TotalComprobante = encMovimiento.TotalComprobante;
                        EncMovimiento.Comentarios = encMovimiento.Comentarios;
                        EncMovimiento.Moneda = encMovimiento.Moneda;
                        EncMovimiento.idCondPago = encMovimiento.idCondPago;
                        EncMovimiento.idDiasValidos = encMovimiento.idDiasValidos;
                        // EncMovimiento.idGarantia = encMovimiento.idGarantia;
                        EncMovimiento.idTiemposEntregas = encMovimiento.idTiemposEntregas;
                        EncMovimiento.Redondeo = encMovimiento.Redondeo;
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
                                Det.idDocumentoExoneracion = item.idDocumentoExoneracion;
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
                                Det.idDocumentoExoneracion = item.idDocumentoExoneracion;
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
                        EncMovimiento.PorDescuento = Math.Round(encMovimiento.PorDescuento, 6);
                        EncMovimiento.TotalComprobante = encMovimiento.TotalComprobante;
                        EncMovimiento.Comentarios = encMovimiento.Comentarios;
                        EncMovimiento.Moneda = encMovimiento.Moneda;
                        EncMovimiento.Aprobada = false;
                        EncMovimiento.AprobadaSuperior = true;
                        EncMovimiento.idCondPago = encMovimiento.idCondPago;
                        EncMovimiento.idDiasValidos = encMovimiento.idDiasValidos;
                        //EncMovimiento.idGarantia = encMovimiento.idGarantia;
                        EncMovimiento.idTiemposEntregas = encMovimiento.idTiemposEntregas;
                        EncMovimiento.Facturado = false;
                        EncMovimiento.DocEntry = 0;
                        EncMovimiento.Redondeo = encMovimiento.Redondeo;
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
                            Det.idDocumentoExoneracion = item.idDocumentoExoneracion;
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
                        client.DocCurrency = EncMovimiento.Moneda == "USD" ? paramFac.MonedaDolaresSAP : paramFac.MonedaSAPColones; // "COL";
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
                        if (EncMovimiento.Redondeo != 0)
                        {
                            client.Rounding = BoYesNoEnum.tYES;
                            client.RoundingDiffAmount = Convert.ToDouble(EncMovimiento.Redondeo);
                        }
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
                            client.Lines.Currency = EncMovimiento.Moneda == "USD" ? paramFac.MonedaDolaresSAP : paramFac.MonedaSAPColones;
                            client.Lines.WarehouseCode = db.Parametros.FirstOrDefault().BodegaInicial;
                            client.Lines.DiscountPercent = Convert.ToDouble(item.PorDescuento);
                            client.Lines.ItemCode = item.ItemCode;
                            client.Lines.DiscountPercent = Convert.ToDouble(item.PorDescuento);
                            client.Lines.Quantity = Convert.ToDouble(item.Cantidad);
                            if (G.ObtenerConfig("Pais") != "P")
                            {
                                client.Lines.TaxCode = db.Impuestos.Where(a => a.id == item.idImpuesto).FirstOrDefault() == null ? Parametros.TaxCode : db.Impuestos.Where(a => a.id == item.idImpuesto).FirstOrDefault().CodSAP;  //Parametros.TaxCode;//"IVA-13";

                            }
                            else
                            {
                                client.Lines.VatGroup = db.Impuestos.Where(a => a.id == item.idImpuesto).FirstOrDefault() == null ? Parametros.TaxCode : db.Impuestos.Where(a => a.id == item.idImpuesto).FirstOrDefault().CodSAP;  //Parametros.TaxCode;//"IVA-13";

                            }
                            client.Lines.TaxOnly = BoYesNoEnum.tNO;
                            client.Lines.UnitPrice = Convert.ToDouble(item.PrecioUnitario);
                            if (item.idDocumentoExoneracion > 0)
                            {
                                var ParametrosFacturacion = db.ParametrosFacturacion.FirstOrDefault();
                                var conexion2 = g.DevuelveCadena(db);
                                var valorAFiltrar = item.idDocumentoExoneracion.ToString();

                                var SQL = ParametrosFacturacion.SQLDocumentoExoneracion + valorAFiltrar;

                                SqlConnection Cn = new SqlConnection(conexion2);
                                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                                DataSet Ds = new DataSet();
                                Cn.Open();
                                Da.Fill(Ds, "DocNum1");
                                client.Lines.UserFields.Fields.Item("U_Tipo_Doc").Value = Ds.Tables["DocNum1"].Rows[0]["TipoDocumento"].ToString();
                                client.Lines.UserFields.Fields.Item("U_NumDoc").Value = Ds.Tables["DocNum1"].Rows[0]["NumeroDocumento"].ToString();
                                client.Lines.UserFields.Fields.Item("U_NomInst").Value = Ds.Tables["DocNum1"].Rows[0]["Emisora"].ToString();
                                client.Lines.UserFields.Fields.Item("U_FecEmis").Value = Convert.ToDateTime(Ds.Tables["DocNum1"].Rows[0]["FechaEmision"].ToString());

                                Cn.Close();



                            }
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
                                    var filtroSQL = "NumAtCard = '" + valorAFiltrar + "' order by DocEntry desc";
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
                            orden.DocCurrency = EncMovimiento.Moneda == "USD" ? paramFac.MonedaDolaresSAP : paramFac.MonedaSAPColones; //"COL";
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
                            if (EncMovimiento.Redondeo != 0)
                            {
                                orden.Rounding = BoYesNoEnum.tYES;
                                orden.RoundingDiffAmount = Convert.ToDouble(EncMovimiento.Redondeo);
                            }
                            var ii = 0;
                            foreach (var item in DetalleSAP)
                            {
                                orden.Lines.SetCurrentLine(ii);
                                orden.Lines.CostingCode = "";
                                orden.Lines.CostingCode2 = "";
                                orden.Lines.CostingCode3 = Parametros.CostingCode; //"TA-01";
                                orden.Lines.CostingCode4 = "";
                                orden.Lines.CostingCode5 = "";
                                orden.Lines.Currency = EncMovimiento.Moneda == "USD" ? paramFac.MonedaDolaresSAP : paramFac.MonedaSAPColones; //"COL";
                                orden.Lines.WarehouseCode = db.Parametros.FirstOrDefault().BodegaFinal;
                                orden.Lines.DiscountPercent = Convert.ToDouble(item.PorDescuento);
                                orden.Lines.ItemCode = item.ItemCode;
                                orden.Lines.DiscountPercent = Convert.ToDouble(item.PorDescuento);
                                orden.Lines.Quantity = Convert.ToDouble(item.Cantidad);
                                if (G.ObtenerConfig("Pais") != "P")
                                {

                                    orden.Lines.TaxCode = db.Impuestos.Where(a => a.id == item.idImpuesto).FirstOrDefault() == null ? Parametros.TaxCode : db.Impuestos.Where(a => a.id == item.idImpuesto).FirstOrDefault().CodSAP;  //Parametros.TaxCode;// "IVA-13";
                                }
                                else
                                {
                                    orden.Lines.VatGroup = db.Impuestos.Where(a => a.id == item.idImpuesto).FirstOrDefault() == null ? Parametros.TaxCode : db.Impuestos.Where(a => a.id == item.idImpuesto).FirstOrDefault().CodSAP;  //Parametros.TaxCode;// "IVA-13";

                                }
                                orden.Lines.TaxOnly = BoYesNoEnum.tNO;
                                orden.Lines.UnitPrice = Convert.ToDouble(item.PrecioUnitario);
                                if (item.idDocumentoExoneracion > 0)
                                {
                                    var ParametrosFacturacion = db.ParametrosFacturacion.FirstOrDefault();
                                    var conexion2 = g.DevuelveCadena(db);
                                    var valorAFiltrar = item.idDocumentoExoneracion.ToString();

                                    var SQL = ParametrosFacturacion.SQLDocumentoExoneracion + valorAFiltrar;

                                    SqlConnection Cn = new SqlConnection(conexion2);
                                    SqlCommand Cmd = new SqlCommand(SQL, Cn);
                                    SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                                    DataSet Ds = new DataSet();
                                    Cn.Open();
                                    Da.Fill(Ds, "DocNum1");
                                    orden.Lines.UserFields.Fields.Item("U_Tipo_Doc").Value = Ds.Tables["DocNum1"].Rows[0]["TipoDocumento"].ToString();
                                    orden.Lines.UserFields.Fields.Item("U_NumDoc").Value = Ds.Tables["DocNum1"].Rows[0]["NumeroDocumento"].ToString();
                                    orden.Lines.UserFields.Fields.Item("U_NomInst").Value = Ds.Tables["DocNum1"].Rows[0]["Emisora"].ToString();
                                    orden.Lines.UserFields.Fields.Item("U_FecEmis").Value = Convert.ToDateTime(Ds.Tables["DocNum1"].Rows[0]["FechaEmision"].ToString());

                                    Cn.Close();



                                }
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
                                    throw new Exception("");
                                }
                                catch (Exception)
                                {
                                    try
                                    {

                                        var conexion2 = g.DevuelveCadena(db);
                                        var valorAFiltrar = EncMovimiento.id.ToString();
                                        var filtroSQL = "NumAtCard = '" + valorAFiltrar + "' order by DocEntry desc";
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
                            client.DocCurrency = EncMovimiento.Moneda == "USD" ? paramFac.MonedaDolaresSAP : paramFac.MonedaSAPColones; //"COL";
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
                            if (EncMovimiento.Redondeo != 0)
                            {
                                client.Rounding = BoYesNoEnum.tYES;
                                client.RoundingDiffAmount = Convert.ToDouble(EncMovimiento.Redondeo);
                            }
                            var i = 0;
                            foreach (var item in DetalleSAP)
                            {
                                client.Lines.SetCurrentLine(i);
                                client.Lines.CostingCode = "";
                                client.Lines.CostingCode2 = "";
                                client.Lines.CostingCode3 = Parametros.CostingCode; //"TA-01";
                                client.Lines.CostingCode4 = "";
                                client.Lines.CostingCode5 = "";
                                client.Lines.Currency = EncMovimiento.Moneda == "USD" ? paramFac.MonedaDolaresSAP : paramFac.MonedaSAPColones; //"COL";
                                client.Lines.WarehouseCode = db.Parametros.FirstOrDefault().BodegaFinal;
                                client.Lines.DiscountPercent = Convert.ToDouble(item.PorDescuento);
                                client.Lines.ItemCode = item.ItemCode;
                                client.Lines.DiscountPercent = Convert.ToDouble(item.PorDescuento);
                                client.Lines.Quantity = Convert.ToDouble(item.Cantidad);
                                if (G.ObtenerConfig("Pais") != "P")
                                {

                                    client.Lines.TaxCode = db.Impuestos.Where(a => a.id == item.idImpuesto).FirstOrDefault() == null ? Parametros.TaxCode : db.Impuestos.Where(a => a.id == item.idImpuesto).FirstOrDefault().CodSAP;  //Parametros.TaxCode;// "IVA-13";
                                }
                                else
                                {
                                    client.Lines.VatGroup = db.Impuestos.Where(a => a.id == item.idImpuesto).FirstOrDefault() == null ? Parametros.TaxCode : db.Impuestos.Where(a => a.id == item.idImpuesto).FirstOrDefault().CodSAP;  //Parametros.TaxCode;// "IVA-13";

                                }
                                client.Lines.TaxOnly = BoYesNoEnum.tNO;
                                client.Lines.UnitPrice = Convert.ToDouble(item.PrecioUnitario);
                                if (item.idDocumentoExoneracion > 0)
                                {
                                    var ParametrosFacturacion = db.ParametrosFacturacion.FirstOrDefault();
                                    var conexion2 = g.DevuelveCadena(db);
                                    var valorAFiltrar = item.idDocumentoExoneracion.ToString();

                                    var SQL = ParametrosFacturacion.SQLDocumentoExoneracion + valorAFiltrar;

                                    SqlConnection Cn = new SqlConnection(conexion2);
                                    SqlCommand Cmd = new SqlCommand(SQL, Cn);
                                    SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                                    DataSet Ds = new DataSet();
                                    Cn.Open();
                                    Da.Fill(Ds, "DocNum1");
                                    client.Lines.UserFields.Fields.Item("U_Tipo_Doc").Value = Ds.Tables["DocNum1"].Rows[0]["TipoDocumento"].ToString();
                                    client.Lines.UserFields.Fields.Item("U_NumDoc").Value = Ds.Tables["DocNum1"].Rows[0]["NumeroDocumento"].ToString();
                                    client.Lines.UserFields.Fields.Item("U_NomInst").Value = Ds.Tables["DocNum1"].Rows[0]["Emisora"].ToString();
                                    client.Lines.UserFields.Fields.Item("U_FecEmis").Value = Convert.ToDateTime(Ds.Tables["DocNum1"].Rows[0]["FechaEmision"].ToString());

                                    Cn.Close();



                                }
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
                                        var filtroSQL = "NumAtCard = '" + valorAFiltrar + "' order by DocEntry desc";
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
                                var count = -1;
                                var client2 = (ServiceCalls)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oServiceCalls);
                                if (client2.GetByKey(Convert.ToInt32(EncMovimiento.NumLlamada)))
                                {
                                    var idLlamada = Convert.ToInt32(EncMovimiento.NumLlamada);
                                    var Llamada = db.LlamadasServicios.Where(a => a.DocEntry == idLlamada).FirstOrDefault();
                                    //Que no existan bitacoraMovimientos aprobados 
                                    count = db.BitacoraMovimientosSAP.Where(a => a.idLlamada == Llamada.id && a.ProcesadaSAP == true).Distinct().GroupBy(a => a.DocEntry).Count();
                                    // Que no exista otra entrega asociada
                                    count += db.EncFacturas.Where(a => a.NumLlamada == EncMovimiento.NumLlamada && a.ProcesadoSAP == true).FirstOrDefault() == null ? 0 : db.EncFacturas.Where(a => a.NumLlamada == EncMovimiento.NumLlamada && a.ProcesadoSAP == true).Count();
                                    count += db.EncMovimiento.Where(a => a.NumLlamada == EncMovimiento.NumLlamada && a.id != EncMovimiento.id && a.TipoMovimiento == 2 && a.DocEntry > 0).Count();
                                    var bandera = false;
                                    if (count > 0)
                                    {
                                        bandera = true;
                                    }
                                    if (client2.Expenses.Count > 0)
                                    {
                                        if (bandera == true)
                                        {
                                            client2.Expenses.Add();

                                        }
                                    }
                                    client2.Expenses.DocumentType = BoSvcEpxDocTypes.edt_Delivery;


                                    client2.Expenses.DocumentNumber = idEntry;
                                    client2.Expenses.DocEntry = idEntry;



                                    if (client2.Expenses.Count == 0 || bandera == false)
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
                            if (DetalleSAP.Where(a => !a.ItemName.ToLower().Contains("mano de obra")).Count() <= 0)
                            {
                                var clientEntrega = (Documents)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDeliveryNotes);
                                clientEntrega.DocObjectCode = BoObjectTypes.oDeliveryNotes;
                                clientEntrega.CardCode = EncMovimiento.CardCode;
                                clientEntrega.DocCurrency = EncMovimiento.Moneda == "USD" ? paramFac.MonedaDolaresSAP : paramFac.MonedaSAPColones; //"COL";
                                clientEntrega.DocDate = EncMovimiento.Fecha; //listo
                                clientEntrega.DocDueDate = EncMovimiento.Fecha.AddDays(3); //listo
                                clientEntrega.DocNum = 0; //automatico
                                clientEntrega.DocType = BoDocumentTypes.dDocument_Items;
                                clientEntrega.HandWritten = BoYesNoEnum.tNO;
                                clientEntrega.NumAtCard = EncMovimiento.id.ToString(); //orderid               
                                clientEntrega.ReserveInvoice = BoYesNoEnum.tNO;
                                clientEntrega.Series = Parametros.SerieEntrega;//3; //3 quemado
                                clientEntrega.Comments = "Esta es la entrega de los productos por garantia"; //direccion
                                clientEntrega.DiscountPercent = Convert.ToDouble(EncMovimiento.PorDescuento); //direccion
                                                                                                              //var Llam = Convert.ToInt32(EncMovimiento.NumLlamada);
                                                                                                              //var Llamada = db.LlamadasServicios.Where(a => a.DocEntry == Llam).FirstOrDefault();
                                var Tec1 = Llamada2.Tecnico == null ? "" : Llamada2.Tecnico.ToString();
                                var Tecnico1 = db.Tecnicos.Where(a => a.idSAP == Tec1).FirstOrDefault();
                                clientEntrega.DocumentsOwner = Convert.ToInt32(Llamada2.Tecnico);
                                if (Tecnico1.Letra > 0)
                                {
                                    clientEntrega.SalesPersonCode = Tecnico1.Letra;
                                }

                                clientEntrega.UserFields.Fields.Item("U_DYD_Boleta").Value = EncMovimiento.NumLlamada.ToString();
                                var DetalleSAPEntrega = db.DetMovimiento.Where(a => a.idEncabezado == EncMovimiento.id && a.Garantia == true).ToList();
                                var iE = 0;
                                foreach (var item in DetalleSAPEntrega)
                                {
                                    clientEntrega.Lines.SetCurrentLine(iE);
                                    clientEntrega.Lines.CostingCode = "";
                                    clientEntrega.Lines.CostingCode2 = "";
                                    clientEntrega.Lines.CostingCode3 = Parametros.CostingCode; //"TA-01";
                                    clientEntrega.Lines.CostingCode4 = "";
                                    clientEntrega.Lines.CostingCode5 = "";
                                    clientEntrega.Lines.Currency = EncMovimiento.Moneda == "USD" ? paramFac.MonedaDolaresSAP : paramFac.MonedaSAPColones; //"COL";
                                    clientEntrega.Lines.WarehouseCode = db.Parametros.FirstOrDefault().BodegaFinal;
                                    clientEntrega.Lines.DiscountPercent = Convert.ToDouble(item.PorDescuento);
                                    clientEntrega.Lines.ItemCode = item.ItemCode;
                                    clientEntrega.Lines.DiscountPercent = Convert.ToDouble(item.PorDescuento);
                                    clientEntrega.Lines.Quantity = Convert.ToDouble(item.Cantidad);
                                    if (G.ObtenerConfig("Pais") != "P")
                                    {

                                        clientEntrega.Lines.TaxCode = db.Impuestos.Where(a => a.id == item.idImpuesto).FirstOrDefault() == null ? Parametros.TaxCode : db.Impuestos.Where(a => a.id == item.idImpuesto).FirstOrDefault().CodSAP;  //Parametros.TaxCode;// "IVA-13";
                                    }
                                    else
                                    {
                                        clientEntrega.Lines.VatGroup = db.Impuestos.Where(a => a.id == item.idImpuesto).FirstOrDefault() == null ? Parametros.TaxCode : db.Impuestos.Where(a => a.id == item.idImpuesto).FirstOrDefault().CodSAP;  //Parametros.TaxCode;// "IVA-13";

                                    }
                                    clientEntrega.Lines.TaxOnly = BoYesNoEnum.tNO;
                                    clientEntrega.Lines.UnitPrice = Convert.ToDouble(item.PrecioUnitario);
                                    if (item.idDocumentoExoneracion > 0)
                                    {
                                        var ParametrosFacturacion = db.ParametrosFacturacion.FirstOrDefault();
                                        var conexion2 = g.DevuelveCadena(db);
                                        var valorAFiltrar = item.idDocumentoExoneracion.ToString();

                                        var SQL = ParametrosFacturacion.SQLDocumentoExoneracion + valorAFiltrar;

                                        SqlConnection Cn = new SqlConnection(conexion2);
                                        SqlCommand Cmd = new SqlCommand(SQL, Cn);
                                        SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                                        DataSet Ds = new DataSet();
                                        Cn.Open();
                                        Da.Fill(Ds, "DocNum1");
                                        clientEntrega.Lines.UserFields.Fields.Item("U_Tipo_Doc").Value = Ds.Tables["DocNum1"].Rows[0]["TipoDocumento"].ToString();
                                        clientEntrega.Lines.UserFields.Fields.Item("U_NumDoc").Value = Ds.Tables["DocNum1"].Rows[0]["NumeroDocumento"].ToString();
                                        clientEntrega.Lines.UserFields.Fields.Item("U_NomInst").Value = Ds.Tables["DocNum1"].Rows[0]["Emisora"].ToString();
                                        clientEntrega.Lines.UserFields.Fields.Item("U_FecEmis").Value = Convert.ToDateTime(Ds.Tables["DocNum1"].Rows[0]["FechaEmision"].ToString());

                                        Cn.Close();



                                    }
                                    clientEntrega.Lines.Add();


                                    iE++;
                                }

                                var respuestaE = clientEntrega.Add();
                                if (respuestaE == 0)
                                {
                                    var idEntry = 0; //Convert.ToInt32(Conexion.Company.GetNewObjectKey());
                                    try
                                    {
                                        idEntry = Convert.ToInt32(Conexion.Company.GetNewObjectKey());
                                        throw new Exception("");
                                    }
                                    catch (Exception)
                                    {
                                        try
                                        {

                                            var conexion = g.DevuelveCadena(db);
                                            var valorAFiltrar = EncMovimiento.id.ToString();
                                            var filtroSQL = "NumAtCard = '" + valorAFiltrar + "' order by DocEntry desc";
                                            var SQL = Parametros.SQLDocEntryDocs.Replace("@CampoBuscar", "DocEntry").Replace("@Tabla", "ODLN").Replace("@CampoWhere = @reemplazo", filtroSQL);

                                            SqlConnection Cn = new SqlConnection(conexion);
                                            SqlCommand Cmd = new SqlCommand(SQL, Cn);
                                            SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                                            DataSet Ds = new DataSet();
                                            Cn.Open();
                                            Da.Fill(Ds, "DocNum1");
                                            idEntry = Convert.ToInt32(Ds.Tables["DocNum1"].Rows[0]["DocEntry"]);

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

                                    var count = -1;
                                    var client2 = (ServiceCalls)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oServiceCalls);
                                    if (client2.GetByKey(Convert.ToInt32(EncMovimiento.NumLlamada)))
                                    {
                                        var idLlamada = Convert.ToInt32(EncMovimiento.NumLlamada);
                                        var Llamada = db.LlamadasServicios.Where(a => a.DocEntry == idLlamada).FirstOrDefault();
                                        //Que no existan bitacoraMovimientos aprobados 
                                        count = db.BitacoraMovimientosSAP.Where(a => a.idLlamada == Llamada.id && a.ProcesadaSAP == true).Distinct().GroupBy(a => a.DocEntry).Count();
                                        // Que no exista otra entrega asociada
                                        count += db.EncMovimiento.Where(a => a.NumLlamada == EncMovimiento.NumLlamada && a.id != EncMovimiento.id && a.TipoMovimiento == 2 && a.DocEntry > 0).Count();
                                        var bandera = false;
                                        if (count > 0)
                                        {
                                            bandera = true;
                                        }

                                        var CantidadExpenses = 0;
                                        if (client2.Expenses.Count > 0)
                                        {
                                            if (bandera == true)
                                            {
                                                client2.Expenses.Add();

                                            }
                                        }
                                        client2.Expenses.DocumentType = BoSvcEpxDocTypes.edt_Delivery;
                                        client2.Expenses.DocumentNumber = idEntry;
                                        client2.Expenses.DocEntry = idEntry;

                                        if (client2.Expenses.Count == 0)
                                        {
                                            client2.Expenses.Add();
                                        }
                                        CantidadExpenses = client2.Expenses.Count;
                                        client2.Expenses.Add();
                                        var respuesta2 = client2.Update();
                                        if (respuesta2 == 0)
                                        {
                                            db.Entry(EncMovimiento).State = EntityState.Modified;
                                            EncMovimiento.DocEntry = idEntry;
                                            db.SaveChanges();
                                            Conexion.Desconectar();
                                        }
                                        else
                                        {
                                            BitacoraErrores be = new BitacoraErrores();

                                            be.Descripcion = Conexion.Company.GetLastErrorDescription();
                                            be.StackTrace = "Entrega Garantia - Actualizar " + idEntry + " Cantidad Expenses: " + CantidadExpenses + " count " + count;
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
                                }
                            }
                            else
                            {
                                var DetalleSAPEntrega = db.DetMovimiento.Where(a => a.idEncabezado == EncMovimiento.id && a.Garantia == true).ToList();

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
                                EncMovimientoEntrega.AprobadaSuperior = true;
                                EncMovimientoEntrega.idCondPago = 0;
                                EncMovimientoEntrega.idDiasValidos = 0;
                                EncMovimientoEntrega.idGarantia = 1;
                                EncMovimientoEntrega.idTiemposEntregas = 0;
                                EncMovimientoEntrega.Facturado = true;
                                EncMovimientoEntrega.DocEntryDevolucion = 0;
                                EncMovimientoEntrega.Redondeo = 0;
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
                catch (Exception ex1)
                {

                    g.GuardarTxt("errorTransaction.txt", ex1.Message);
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

        [HttpPost]
        [Route("api/Movimientos/Devolucion")]
        public HttpResponseMessage DevolucionEntrega([FromUri] int id)
        {

            try
            {


                var EncMovimiento = db.EncMovimiento.Where(a => a.id == id).FirstOrDefault();

                if (EncMovimiento != null)
                {
                    if (EncMovimiento.TipoMovimiento == 2)
                    {

                        var oEntrega = (Documents)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDeliveryNotes);
                        if (oEntrega.GetByKey(EncMovimiento.DocEntry))
                        { // Crear un nuevo documento de devolución basado en la entrega original 
                            Documents oDevolucion = (Documents)Conexion.Company.GetBusinessObject(BoObjectTypes.oReturns);
                            oDevolucion.CardCode = oEntrega.CardCode;
                            oDevolucion.DocDate = DateTime.Now;
                            oDevolucion.DocDueDate = DateTime.Now;
                            oDevolucion.TaxDate = DateTime.Now; // Copiar las líneas de la entrega original a la devolución
                            oDevolucion.NumAtCard = "Boletaje DEV: " + EncMovimiento.id;
                            for (int i = 0; i < oEntrega.Lines.Count; i++)
                            {
                                oEntrega.Lines.SetCurrentLine(i);
                                oDevolucion.Lines.BaseType = (int)BoObjectTypes.oDeliveryNotes;
                                oDevolucion.Lines.BaseEntry = oEntrega.DocEntry;
                                oDevolucion.Lines.BaseLine = oEntrega.Lines.LineNum;
                                oDevolucion.Lines.Quantity = oEntrega.Lines.Quantity;
                                oDevolucion.Lines.Add();
                            } // Agregar la devolución a SAP 
                            int retCode = oDevolucion.Add();
                            if (retCode != 0)
                            {
                                throw new Exception("Error al crear la devolución: " + Conexion.Company.GetLastErrorDescription());
                            }
                            else
                            {
                                try
                                {

                                    var conexion = g.DevuelveCadena(db);
                                    var valorAFiltrar = "Boletaje DEV: " + EncMovimiento.id.ToString();
                                    var filtroSQL = "NumAtCard like '%" + valorAFiltrar + "%' order by DocEntry desc";
                                    var SQL = db.Parametros.FirstOrDefault().SQLDocEntryDocs.Replace("@CampoBuscar", "DocEntry").Replace("@Tabla", "ORDN").Replace("@CampoWhere = @reemplazo", filtroSQL);

                                    SqlConnection Cn = new SqlConnection(conexion);
                                    SqlCommand Cmd = new SqlCommand(SQL, Cn);
                                    SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                                    DataSet Ds = new DataSet();
                                    Cn.Open();
                                    Da.Fill(Ds, "DocNum1");
                                    db.Entry(EncMovimiento).State = EntityState.Modified;
                                    EncMovimiento.DocEntryDevolucion = Convert.ToInt32(Ds.Tables["DocNum1"].Rows[0]["DocEntry"]);
                                    EncMovimiento.Facturado = true;
                                    db.SaveChanges();
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
                        }
                        else
                        {
                            throw new Exception("No se encontró la entrega con DocEntry: " + EncMovimiento.DocEntry);
                        }
                    }
                    else
                    {
                        throw new Exception("Esta no es una entrega");
                    }


                }
                else
                {
                    throw new Exception("EncMovimmiento no existe");
                }
                Conexion.Desconectar();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Conexion.Desconectar();
                BitacoraErrores be = new BitacoraErrores();

                be.Descripcion = "Error al hacer la devolucion en el Movimiento #" + id + " -> " + ex.Message;
                be.StackTrace = ex.StackTrace;
                be.Fecha = DateTime.Now;
                db.BitacoraErrores.Add(be);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}