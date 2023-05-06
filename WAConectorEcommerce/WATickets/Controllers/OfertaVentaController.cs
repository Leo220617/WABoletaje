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

    public class OfertaVentaController : ApiController
    {
        ModelCliente db = new ModelCliente();
        object resp;


        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {
                var time = new DateTime();

                var Orden = db.EncOferta.Select(a => new {

                    a.id,
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


                    db.EncOferta.Add(EncOrden);
                    db.SaveChanges();
                    var i = 1;
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
                        client.Comments = EncOrden.Comentarios;
                        client.SalesPersonCode = EncOrden.CodVendedor;


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
                            client.Lines.ItemDescription = item.ItemName;
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
                        client.Comments = EncOrden.Comentarios;
                        client.SalesPersonCode = EncOrden.CodVendedor;


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
                            client.Lines.ItemDescription = item.ItemName;
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
    }
}