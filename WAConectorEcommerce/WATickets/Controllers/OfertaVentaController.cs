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
using SelectPdf;
using System.IO;

namespace WATickets.Controllers
{
    [Authorize]

    public class OfertaVentaController : ApiController
    {
        G g = new G();

        ModelCliente db = new ModelCliente();
        object resp;


        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {
                var time = new DateTime();

                var Orden = db.EncOferta.Select(a => new
                {

                    a.id,
                    a.idTiemposEntregas,
                    a.idDiasValidos,
                    a.idCondPago,
                    a.idGarantia,
                    a.DocEntry,
                    a.DocNum,
                    a.CardCode,
                    a.Moneda,
                    a.FechaEntrega,
                    a.Fecha,
                    a.FechaVencimiento,
                    a.TipoDocumento,
                    a.NumAtCard,
                    a.Series,
                    a.Comentarios,
                    a.CodVendedor,
                    a.ProcesadaSAP,
                    a.Status,
                    a.PersonaContacto,
                    a.TelefonoContacto,
                    a.CorreoContacto,
                    Detalle = db.DetOferta.Where(d => d.idEncabezado == a.id).ToList()

                }).Where(a => (filtro.FechaInicial != time ? a.Fecha >= filtro.FechaInicial : true) && (filtro.FechaFinal != time ? a.Fecha <= filtro.FechaFinal : true)).ToList();

                if (!string.IsNullOrEmpty(filtro.Texto))
                {
                    Orden = Orden.Where(a => a.CardCode.ToUpper().Contains(filtro.Texto.ToUpper())).ToList();
                }



                return Request.CreateResponse(HttpStatusCode.OK, Orden);

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

        [Route("api/OfertaVenta/Consultar")]
        public HttpResponseMessage GetOne([FromUri]int id)
        {
            try
            {



                var Orden = db.EncOferta.Select(a => new
                {

                    a.id,
                    a.idTiemposEntregas,
                    a.idCondPago,
                    a.idGarantia,
                    a.idDiasValidos,
                    a.DocEntry,
                    a.DocNum,
                    a.CardCode,
                    a.FechaEntrega,
                    a.Moneda,
                    a.Fecha,
                    a.FechaVencimiento,
                    a.TipoDocumento,
                    a.NumAtCard,
                    a.Series,
                    a.Comentarios,
                    a.CodVendedor,
                    a.ProcesadaSAP,
                    a.Status,
                    a.PersonaContacto,
                    a.TelefonoContacto,
                    a.CorreoContacto,
                    Detalle = db.DetOferta.Where(d => d.idEncabezado == a.id).ToList()



                }).Where(a => a.id == id).FirstOrDefault();

                if (Orden == null)
                {
                    throw new Exception("Esta Orden no se encuentra registrado");
                }

                return Request.CreateResponse(HttpStatusCode.OK, Orden);
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
        public HttpResponseMessage Post([FromBody] OfertaVenta orden)
        {
            var t = db.Database.BeginTransaction();
            try
            {

                var Parametros = db.Parametros.FirstOrDefault();
                var EncOrden = db.EncOferta.Where(a => a.id == orden.id).FirstOrDefault();

                if (EncOrden == null)
                {
                    EncOrden = new EncOferta();

                    EncOrden.idCondPago = orden.idCondPago;
                    EncOrden.idGarantia = orden.idGarantia;
                    EncOrden.CardCode = orden.CardCode;
                    EncOrden.Moneda = orden.Moneda;
                    EncOrden.Fecha = orden.Fecha;
                    EncOrden.FechaVencimiento = orden.FechaVencimiento;
                    EncOrden.TipoDocumento = orden.TipoDocumento;
                    EncOrden.NumAtCard = orden.NumAtCard;
                    EncOrden.Series = Parametros.SerieOferta;
                    EncOrden.Comentarios = orden.Comentarios;
                    EncOrden.CodVendedor = orden.CodVendedor;
                    EncOrden.ProcesadaSAP = false;
                    EncOrden.FechaEntrega = orden.FechaEntrega;
                    EncOrden.idTiemposEntregas = orden.idTiemposEntregas;
                    EncOrden.Status = "O";
                    EncOrden.PersonaContacto = orden.PersonaContacto;
                    EncOrden.TelefonoContacto = orden.TelefonoContacto;
                    EncOrden.CorreoContacto = orden.CorreoContacto;
                    EncOrden.idDiasValidos = orden.idDiasValidos;
                    EncOrden.idUsuarioCreador = orden.idUsuarioCreador;
                    db.EncOferta.Add(EncOrden);
                    db.SaveChanges();
                    var i = 0;
                    foreach (var item in orden.Detalle)
                    {
                        var DetOrden = new DetOferta();
                        DetOrden.idEncabezado = EncOrden.id;
                        DetOrden.NumLinea = i;
                        DetOrden.ItemCode = item.ItemCode;
                        DetOrden.ItemName = item.ItemName;
                        DetOrden.Bodega = item.Bodega;
                        DetOrden.PorcentajeDescuento = item.PorcentajeDescuento;
                        DetOrden.Cantidad = item.Cantidad;
                        DetOrden.Impuesto = item.Impuesto;
                        DetOrden.TaxOnly = item.TaxOnly;
                        DetOrden.PrecioUnitario = item.PrecioUnitario;
                        DetOrden.Total = item.Total;
                        var Imp = Decimal.Parse(item.Impuesto);
                        DetOrden.TaxCode = db.Impuestos.Where(a => a.Tarifa == Imp).FirstOrDefault() == null ? "IVA-13" : db.Impuestos.Where(a => a.Tarifa == Imp).FirstOrDefault().CodSAP;


                        db.DetOferta.Add(DetOrden);
                        db.SaveChanges();
                        i++;
                    }

                    t.Commit();

                    try
                    {
                        var client = (Documents)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oQuotations);
                        client.DocObjectCode = BoObjectTypes.oOrders;
                        client.CardCode = EncOrden.CardCode;
                        client.DocCurrency = EncOrden.Moneda;
                        client.DocDate = EncOrden.Fecha;
                        client.DocDueDate = EncOrden.FechaVencimiento;
                        client.DocNum = 0;
                        if (EncOrden.TipoDocumento == "I")
                        {
                            client.DocType = BoDocumentTypes.dDocument_Items;
                        }
                        else
                        {
                            client.DocType = BoDocumentTypes.dDocument_Service;

                        }
                        client.HandWritten = BoYesNoEnum.tNO;
                        client.NumAtCard = EncOrden.NumAtCard;
                        client.ReserveInvoice = BoYesNoEnum.tNO;
                        client.Series = EncOrden.Series;
                        client.TaxDate = EncOrden.Fecha;
                        client.Comments = EncOrden.Comentarios != null ? EncOrden.Comentarios.Length > 200  ? EncOrden.Comentarios.Substring(0,199) : EncOrden.Comentarios : EncOrden.Comentarios;
                        client.SalesPersonCode = EncOrden.CodVendedor;
                        client.GroupNumber = db.CondicionesPagos.Where(a => a.id == EncOrden.idCondPago).FirstOrDefault() == null ? 0 : Convert.ToInt32(db.CondicionesPagos.Where(a => a.id == EncOrden.idCondPago).FirstOrDefault().codSAP);
                        client.UserFields.Fields.Item("U_DYD_TEntrega").Value = db.TiemposEntregas.Where(a => a.id == EncOrden.idTiemposEntregas).FirstOrDefault() == null ? "0" : db.TiemposEntregas.Where(a => a.id == EncOrden.idTiemposEntregas).FirstOrDefault().codSAP;
                        client.UserFields.Fields.Item("U_DYD_TGarantia").Value = db.Garantias.Where(a => a.id == EncOrden.idGarantia).FirstOrDefault() == null ? "0" : db.Garantias.Where(a => a.id == EncOrden.idGarantia).FirstOrDefault().idSAP;


                        var Detalle = db.DetOferta.Where(a => a.idEncabezado == EncOrden.id).ToList();

                        int z = 0;
                        foreach (var item in Detalle)
                        {
                            client.Lines.SetCurrentLine(z);

                            client.Lines.CostingCode = "";
                            client.Lines.CostingCode2 = "";
                            client.Lines.CostingCode3 = Parametros.CostingCode; //"TA-01";
                            client.Lines.CostingCode4 = "";
                            client.Lines.CostingCode5 = "";
                            client.Lines.Currency = EncOrden.Moneda;
                            client.Lines.DiscountPercent = Convert.ToDouble(item.PorcentajeDescuento);
                            client.Lines.ItemCode = item.ItemCode;
                            client.Lines.ItemDescription = item.ItemName.Length > 200 ? item.ItemName.Substring(0, 199) : item.ItemName ;
                            client.Lines.Quantity = Convert.ToDouble(item.Cantidad);
                            client.Lines.TaxCode = item.TaxCode;
                            client.Lines.TaxOnly = item.TaxOnly == true ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;


                            client.Lines.UnitPrice = Convert.ToDouble(item.PrecioUnitario);
                            client.Lines.WarehouseCode = item.Bodega;
                            client.Lines.Add();
                            z++;
                        }
                        var respuesta = client.Add();

                        if (respuesta == 0)
                        {

                            db.Entry(EncOrden).State = EntityState.Modified;
                            EncOrden.DocEntry = Convert.ToInt32(Conexion.Company.GetNewObjectKey());
                            EncOrden.ProcesadaSAP = true;
                            orden.ProcesadaSAP = true;
                            db.SaveChanges();

                            resp = new
                            {

                                Type = "Oferta de Venta",
                                Status = "Exitoso",
                                Message = "Orden creada exitosamente en SAP",
                                User = Conexion.Company.UserName,
                                DocEntry = Conexion.Company.GetNewObjectKey()
                            };
                            Conexion.Desconectar();
                        }
                        else
                        {
                            resp = new
                            {

                                Type = "Oferta de Venta",
                                Status = "Error",
                                Message = Conexion.Company.GetLastErrorDescription(),
                                User = Conexion.Company.UserName,
                                DocEntry = ""
                            };

                            BitacoraErrores be = new BitacoraErrores();

                            be.Descripcion = Conexion.Company.GetLastErrorDescription();
                            be.StackTrace = "Oferta de Venta";
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
                    try
                    {
                        var client = (Documents)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oQuotations);
                        client.DocObjectCode = BoObjectTypes.oOrders;
                        client.CardCode = EncOrden.CardCode;
                        client.DocCurrency = EncOrden.Moneda;
                        client.DocDate = EncOrden.Fecha;
                        client.DocDueDate = EncOrden.FechaVencimiento;
                        client.DocNum = 0;
                        if (EncOrden.TipoDocumento == "I")
                        {
                            client.DocType = BoDocumentTypes.dDocument_Items;
                        }
                        else
                        {
                            client.DocType = BoDocumentTypes.dDocument_Service;

                        }
                        client.HandWritten = BoYesNoEnum.tNO;
                        client.NumAtCard = EncOrden.NumAtCard;
                        client.ReserveInvoice = BoYesNoEnum.tNO;
                        client.Series = EncOrden.Series;
                        client.TaxDate = EncOrden.Fecha;
                        client.Comments = EncOrden.Comentarios != null ? EncOrden.Comentarios.Length > 200 ? EncOrden.Comentarios.Substring(0, 199) : EncOrden.Comentarios : EncOrden.Comentarios;
                        client.SalesPersonCode = EncOrden.CodVendedor;
                        client.GroupNumber = db.CondicionesPagos.Where(a => a.id == EncOrden.idCondPago).FirstOrDefault() == null ? 0 : Convert.ToInt32(db.CondicionesPagos.Where(a => a.id == EncOrden.idCondPago).FirstOrDefault().codSAP);
                        client.UserFields.Fields.Item("U_DYD_TEntrega").Value = db.TiemposEntregas.Where(a => a.id == EncOrden.idTiemposEntregas).FirstOrDefault() == null ? "0" : db.TiemposEntregas.Where(a => a.id == EncOrden.idTiemposEntregas).FirstOrDefault().codSAP;
                        client.UserFields.Fields.Item("U_DYD_TGarantia").Value = db.Garantias.Where(a => a.id == EncOrden.idGarantia).FirstOrDefault() == null ? "0" : db.Garantias.Where(a => a.id == EncOrden.idGarantia).FirstOrDefault().idSAP;


                        var Detalle = db.DetOferta.Where(a => a.idEncabezado == EncOrden.id).ToList();

                        int z = 0;
                        foreach (var item in Detalle)
                        {
                            client.Lines.SetCurrentLine(z);

                            client.Lines.CostingCode = "";
                            client.Lines.CostingCode2 = "";
                            client.Lines.CostingCode3 = Parametros.CostingCode; //"TA-01";
                            client.Lines.CostingCode4 = "";
                            client.Lines.CostingCode5 = "";
                            client.Lines.Currency = EncOrden.Moneda;
                            client.Lines.DiscountPercent = Convert.ToDouble(item.PorcentajeDescuento);
                            client.Lines.ItemCode = item.ItemCode;
                            client.Lines.ItemDescription = item.ItemName.Length > 200 ? item.ItemName.Substring(0, 199) : item.ItemName;
                            client.Lines.Quantity = Convert.ToDouble(item.Cantidad);
                            client.Lines.TaxCode = item.TaxCode;
                            client.Lines.TaxOnly = item.TaxOnly == true ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;


                            client.Lines.UnitPrice = Convert.ToDouble(item.PrecioUnitario);
                            client.Lines.WarehouseCode = item.Bodega;
                            client.Lines.Add();
                            z++;
                        }

                        var respuesta = client.Add();

                        if (respuesta == 0)
                        {

                            db.Entry(EncOrden).State = EntityState.Modified;
                            EncOrden.DocEntry = Convert.ToInt32(Conexion.Company.GetNewObjectKey());
                            EncOrden.ProcesadaSAP = true;
                            orden.ProcesadaSAP = true;
                            db.SaveChanges();

                            resp = new
                            {

                                Type = "Oferta de Venta",
                                Status = "Exitoso",
                                Message = "Oferta creada exitosamente en SAP",
                                User = Conexion.Company.UserName,
                                DocEntry = Conexion.Company.GetNewObjectKey()
                            };
                            Conexion.Desconectar();
                        }
                        else
                        {
                            resp = new
                            {

                                Type = "Oferta de Venta",
                                Status = "Error",
                                Message = Conexion.Company.GetLastErrorDescription(),
                                User = Conexion.Company.UserName,
                                DocEntry = ""
                            };
                            BitacoraErrores be = new BitacoraErrores();

                            be.Descripcion = Conexion.Company.GetLastErrorDescription();
                            be.StackTrace = "Oferta de Venta";
                            be.Fecha = DateTime.Now;

                            db.BitacoraErrores.Add(be);
                            db.SaveChanges();

                            Conexion.Desconectar();
                        }

                        t.Commit();
                        Conexion.Desconectar();
                    }
                    catch (Exception ex1)
                    {
                        Conexion.Desconectar();

                        t.Rollback();
                        BitacoraErrores be = new BitacoraErrores();

                        be.Descripcion = ex1.Message;
                        be.StackTrace = ex1.StackTrace;
                        be.Fecha = DateTime.Now;

                        db.BitacoraErrores.Add(be);
                        db.SaveChanges();


                    }
                }


                return Request.CreateResponse(HttpStatusCode.OK, orden);
            }
            catch (Exception ex)
            {
                Conexion.Desconectar();

                t.Rollback();
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
        [Route("api/OfertaVenta/Eliminar")]
        public HttpResponseMessage Delete([FromUri] int id)
        {
            try
            {


                var EncOferta = db.EncOferta.Where(a => a.id == id).FirstOrDefault();

                if (EncOferta != null)
                {


                    db.Entry(EncOferta).State = EntityState.Modified;
                    EncOferta.Status = "C";
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("EncOferta no existe");
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


        [HttpPost]
        [Route("api/OfertaVenta/EliminarOferta")]
        public HttpResponseMessage DeleteOferta([FromUri] int id)
        {
            var t = db.Database.BeginTransaction();
            try
            {


                var EncOferta = db.EncOferta.Where(a => a.id == id).FirstOrDefault();

                if (EncOferta != null)
                {
                    var Detalle = db.DetOferta.Where(a => a.idEncabezado == id).ToList();
                    foreach (var item in Detalle)
                    {
                        db.DetOferta.Remove(item);
                        db.SaveChanges();
                    }

                    db.EncOferta.Remove(EncOferta);
                    db.SaveChanges();
                    t.Commit();
                }
                else
                {
                    throw new Exception("EncOferta no existe");
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                t.Rollback();
                BitacoraErrores be = new BitacoraErrores();
                be.Descripcion = ex.Message;
                be.StackTrace = ex.StackTrace;
                be.Fecha = DateTime.Now;
                db.BitacoraErrores.Add(be);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [Route("api/OfertaVenta/Reenvio")]
        [HttpGet]
        public HttpResponseMessage GetCorreo([FromUri]int id, string correo)
        {
            try
            {///Alt + 125 }
                var EncMovimiento = db.EncOferta.Where(a => a.id == id).FirstOrDefault();
                var Moneda = EncMovimiento.Moneda == "COL" ? "₡" : "$";
                if (EncMovimiento != null)
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

                        bodyH = bodyH.Replace("@DocEntry", EncMovimiento.id.ToString());



                        bodyH = bodyH.Replace("@Fecha", EncMovimiento.Fecha.ToString("dd/MM/yyyy"));
                        EmailDestino = Ds.Tables["Encabezado"].Rows[0]["E_Mail"].ToString();
                        var DetalleSAP = db.DetOferta.Where(a => a.idEncabezado == id).ToList();

                        var Porcentaje = DetalleSAP.Sum(a => a.PorcentajeDescuento) / DetalleSAP.Count();
                        bodyH = bodyH.Replace("@PorDesc", Math.Round(Porcentaje, 2).ToString());


                        bodyH = bodyH.Replace("@Subtotal", Moneda + Math.Round(DetalleSAP.Sum(a => a.Cantidad * a.PrecioUnitario), 2).ToString());
                        bodyH = bodyH.Replace("@Descuento", Moneda + Math.Round(DetalleSAP.Sum(a => ((a.PrecioUnitario * a.Cantidad) * (a.PorcentajeDescuento / 100))), 2).ToString());
                        bodyH = bodyH.Replace("@Impuestos", Moneda + Math.Round(DetalleSAP.Sum(a => (((a.PrecioUnitario * a.Cantidad) - ((a.PrecioUnitario * a.Cantidad) * (a.PorcentajeDescuento / 100))) * (Convert.ToDecimal(a.Impuesto) / 100))), 2).ToString());
                        bodyH = bodyH.Replace("@Total", Moneda + Math.Round(DetalleSAP.Sum(a => a.Total), 2).ToString());



                        Cn.Close();
                        Cn.Dispose();


                        bodyH = bodyH.Replace("@ContactoReferencia", EncMovimiento.PersonaContacto);
                        bodyH = bodyH.Replace("@Referencia", EncMovimiento.NumAtCard);

                        var CondicionPago = db.CondicionesPagos.Where(a => a.id == EncMovimiento.idCondPago).FirstOrDefault() == null ? throw new Exception("No se puede enviar el correo, falta la condicion de pago") : db.CondicionesPagos.Where(a => a.id == EncMovimiento.idCondPago).FirstOrDefault();
                        var TiempoEntrega = db.TiemposEntregas.Where(a => a.id == EncMovimiento.idTiemposEntregas).FirstOrDefault() == null ? throw new Exception("No se puede enviar el correo, falta el Tiempos de Entregas") : db.TiemposEntregas.Where(a => a.id == EncMovimiento.idTiemposEntregas).FirstOrDefault();
                        var Garantia = db.Garantias.Where(a => a.id == EncMovimiento.idGarantia).FirstOrDefault() == null ? throw new Exception("No se puede enviar el correo, falta la garantia") : db.Garantias.Where(a => a.id == EncMovimiento.idGarantia).FirstOrDefault();
                        var DiasValidos = db.DiasValidos.Where(a => a.id == EncMovimiento.idDiasValidos).FirstOrDefault() == null ? throw new Exception("No se puede enviar el correo, falta los dias validos") : db.DiasValidos.Where(a => a.id == EncMovimiento.idDiasValidos).FirstOrDefault();
                        var UsuarioCreador = db.Login.Where(a => a.id == EncMovimiento.idUsuarioCreador).FirstOrDefault() == null ? throw new Exception("No se puede enviar el correo, falta el usuario creador") : db.Login.Where(a => a.id == EncMovimiento.idUsuarioCreador).FirstOrDefault();

                        bodyH = bodyH.Replace("@CondicionPago", CondicionPago.Nombre);
                        bodyH = bodyH.Replace("@TiempoEntrega", TiempoEntrega.Nombre);
                        bodyH = bodyH.Replace("@Garantia", Garantia.Nombre);
                        bodyH = bodyH.Replace("@VigenciaOferta", DiasValidos.Nombre);

                        bodyH = bodyH.Replace("@NombreUsuario", UsuarioCreador.Nombre);
                        bodyH = bodyH.Replace("@TelefonoUsuario", UsuarioCreador.Telefono);
                        bodyH = bodyH.Replace("@CorreoVentas", UsuarioCreador.CorreoVentas);


                        var inyectado = "";
                        var z = 0;
                        var top1 = 290;

                        var diagnosticos = "";


                        foreach (var item in DetalleSAP)
                        {
                            if (z == 0)
                            {

                                inyectado = Html.InyectadoOferta.Replace("@NumLinea", (z + 1).ToString()).Replace("@ItemCode", item.ItemCode).Replace("@ItemName", item.ItemName).Replace("@Cantidad", Math.Round(item.Cantidad, 2).ToString()).Replace("@PrecioUnitario", Moneda + Math.Round(item.PrecioUnitario, 2).ToString()).Replace("@TotalLinea", Moneda + Math.Round(item.Total, 2).ToString()).Replace("top1", top1.ToString()).Replace("top2", top1.ToString()).Replace("top3", top1.ToString()).Replace("top4", top1.ToString()).Replace("top5", top1.ToString()).Replace("top5", top1.ToString());
                               // diagnosticos += db.Errores.Where(a => a.id == item.idError).FirstOrDefault() == null ? "" : db.Errores.Where(a => a.id == item.idError).FirstOrDefault().Diagnostico + "<br/>";
                            }
                            else
                            {
                                top1 += 20;

                                inyectado += Html.InyectadoOferta.Replace("@NumLinea", (z + 1).ToString()).Replace("@ItemCode", item.ItemCode).Replace("@ItemName", item.ItemName).Replace("@Cantidad", Math.Round(item.Cantidad, 2).ToString()).Replace("@PrecioUnitario", Moneda + Math.Round(item.PrecioUnitario, 2).ToString()).Replace("@TotalLinea", Moneda + Math.Round(item.Total, 2).ToString()).Replace("top1", top1.ToString()).Replace("top2", top1.ToString()).Replace("top3", top1.ToString()).Replace("top4", top1.ToString()).Replace("top5", top1.ToString()).Replace("top6", top1.ToString());
                                //diagnosticos += db.Errores.Where(a => a.id == item.idError).FirstOrDefault() == null ? "" : db.Errores.Where(a => a.id == item.idError).FirstOrDefault().Diagnostico + "<br/>";

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

                        System.Net.Mail.Attachment att3 = new System.Net.Mail.Attachment(new MemoryStream(bytes), "Oferta_Venta_"+ EncMovimiento.id + ".pdf");
                        adjuntos.Add(att3);

                       

                        var resp = G.SendV2(correo, "", "", CorreoEnvio.RecepcionEmail, "Oferta de Venta", "Oferta de Venta #" + EncMovimiento.id, "<!DOCTYPE html> <html> <head> <meta charset='utf-8'> <meta name='viewport' content='width=device-width, initial-scale=1'> <title></title> </head> <body> <h1>Oferta de Venta</h1> <p> En el presente correo se le hace el envio de la oferta de venta, Estimado Cliente Agradecemos su pronta respuesta a este Correo </p> </body> </html>", CorreoEnvio.RecepcionHostName, CorreoEnvio.EnvioPort, CorreoEnvio.RecepcionUseSSL, CorreoEnvio.RecepcionEmail, CorreoEnvio.RecepcionPassword, adjuntos);

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
    }
}