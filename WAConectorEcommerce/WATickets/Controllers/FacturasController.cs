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
using WATickets.Models;
using WATickets.Models.Cliente;


namespace WATickets.Controllers
{
    [Authorize]
    public class FacturasController : ApiController
    {
        G g = new G();
        ModelCliente db = new ModelCliente();

        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {///Alt + 125 }
                var time = new DateTime();
                if (filtro.FechaFinal != time)
                {
                    filtro.FechaFinal = filtro.FechaFinal.AddDays(1);
                }

                if (!string.IsNullOrEmpty(filtro.Texto) || !string.IsNullOrEmpty(filtro.CardCode))
                {


                    var Facturas = db.EncFacturas
                     .Where(a => (!string.IsNullOrEmpty(filtro.Texto) ? a.NumLlamada == filtro.Texto : true)
                     ).Select(a => new
                     {
                         a.id,
                         a.idSucursal,
                         a.idCondicionVenta,
                         a.idPlazoCredito,
                         a.TipoDocumento,
                         a.idEntrega,
                         a.CardCode,
                         a.Cedula,
                         a.NombreCliente,
                         a.Correo,
                         a.Fecha,
                         a.Moneda,
                         a.TipoCambio,
                         a.Comentarios,
                         a.DocEntry,
                         a.ProcesadoSAP,
                         a.FechaProcesado,
                         a.Subtotal,
                         a.TotalImpuestos,
                         a.TotalDescuento,
                         a.TotalCompra,
                         a.NumLlamada,
                         a.CreadoPor,
                         a.ConsecutivoHacienda,
                         a.ClaveHacienda,
                         DetFactura = db.DetFacturas.Where(b => b.idEncabezado == a.id).ToList(),
                         Entrega = db.EncMovimiento.Where(b => b.TipoMovimiento == 2 && b.id == a.idEntrega).FirstOrDefault(),
                         MetodosPagos = db.MetodosPagosFacturas.Where(c => c.idEncabezado == a.id).ToList()

                     }).ToList(); ;




                    return Request.CreateResponse(HttpStatusCode.OK, Facturas);
                }
                else
                {

                    var Facturas = db.EncFacturas.Where(a => (filtro.FechaInicial != time ? a.Fecha >= filtro.FechaInicial : true)
                        && (filtro.FechaFinal != time ? a.Fecha <= filtro.FechaFinal : true)
                        && ((filtro.Codigo2 > 0 && filtro.Codigo2!= null) ? a.idSucursal == filtro.Codigo2 : true)
                        ).Select(a => new
                        {
                            a.id,
                            a.idSucursal,
                            a.idCondicionVenta,
                            a.idPlazoCredito,
                            a.TipoDocumento,
                            a.idEntrega,
                            a.CardCode,
                            a.Cedula,
                            a.NombreCliente,
                            a.Correo,
                            a.Fecha,
                            a.Moneda,
                            a.TipoCambio,
                            a.Comentarios,
                            a.DocEntry,
                            a.ProcesadoSAP,
                            a.FechaProcesado,
                            a.Subtotal,
                            a.TotalImpuestos,
                            a.TotalDescuento,
                            a.TotalCompra,
                            a.NumLlamada,
                            a.CreadoPor,
                            a.ConsecutivoHacienda,
                            a.ClaveHacienda,
                            DetFactura = db.DetFacturas.Where(b => b.idEncabezado == a.id).ToList(),
                            Entrega = db.EncMovimiento.Where(b => b.TipoMovimiento == 2 && b.id == a.idEntrega).FirstOrDefault(),
                            MetodosPagos = db.MetodosPagosFacturas.Where(c => c.idEncabezado == a.id).ToList()
                        })
                    .ToList();

                    return Request.CreateResponse(HttpStatusCode.OK, Facturas);



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

        [Route("api/Facturas/Consultar")]
        public HttpResponseMessage GetOne([FromUri]int id)
        {
            try
            {



                var Factura = db.EncFacturas.Select(a => new
                {
                    a.id,
                    a.idSucursal,
                    a.idCondicionVenta,
                    a.idPlazoCredito,
                    a.TipoDocumento,
                    a.idEntrega,
                    a.CardCode,
                    a.Cedula,
                    a.NombreCliente,
                    a.Correo,
                    a.Fecha,
                    a.Moneda,
                    a.TipoCambio,
                    a.Comentarios,
                    a.DocEntry,
                    a.ProcesadoSAP,
                    a.FechaProcesado,
                    a.Subtotal,
                    a.TotalImpuestos,
                    a.TotalDescuento,
                    a.TotalCompra,
                    a.NumLlamada,
                    a.CreadoPor,
                    a.ConsecutivoHacienda,
                    a.ClaveHacienda,
                    a.PorDesc,
                    DetFactura = db.DetFacturas.Where(b => b.idEncabezado == a.id).ToList(),
                    Entrega = db.EncMovimiento.Where(b => b.TipoMovimiento == 2 && b.id == a.idEntrega).FirstOrDefault(),
                    MetodosPagos = db.MetodosPagosFacturas.Where(c => c.idEncabezado == a.id).ToList()
                }).Where(a => a.id == id).FirstOrDefault();


                if (Factura == null)
                {
                    throw new Exception("Esta factura no se encuentra registrado");
                }

                return Request.CreateResponse(HttpStatusCode.OK, Factura);
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

        [Route("api/Facturas/SincronizarSAPPagos")]
        public HttpResponseMessage GetSincronizarSAPDocumentosPagosMasivos()
        {
            try
            {
                var Facturas = db.EncFacturas.Where(a => a.ProcesadoSAP == true && a.ProcesadoSAPPago == false).OrderByDescending(a => a.id).ToList();
                foreach (var Factura in Facturas)
                {
                    //Empieza a mandar a SAP
                    try
                    {
                        var ParametrosFacturacion = db.ParametrosFacturacion.FirstOrDefault();
                        //Procesamos el pago
                        var CondicionPago = db.CondicionesPagos.Where(a => a.id == Factura.idCondicionVenta).FirstOrDefault() == null ? db.CondicionesPagos.FirstOrDefault() : db.CondicionesPagos.Where(a => a.id == Factura.idCondicionVenta).FirstOrDefault();
                        //Procesamos el pago
                        if (CondicionPago.Dias == 0)
                        {

                            try
                            {


                                var Fecha = Factura.Fecha.Date;
                                var TipoCambio = Factura.TipoCambio;
                                var MetodosPagos = db.MetodosPagosFacturas.Where(a => a.idEncabezado == Factura.id && a.Monto > 0).FirstOrDefault() == null ? new List<MetodosPagosFacturas>() : db.MetodosPagosFacturas.Where(a => a.idEncabezado == Factura.id && a.Monto > 0).ToList();

                                var MetodosPagosColones = MetodosPagos.Where(a => a.Moneda == "COL").ToList();
                                var MetodosPagosDolares = MetodosPagos.Where(a => a.Moneda == "USD").ToList();

                                bool pagoColonesProcesado = false;
                                bool pagoDolaresProcesado = false;


                                var contador = 0;
                                if (MetodosPagosColones.Count() > 0)
                                {
                                    try
                                    {

                                        var pagoProcesado = (SAPbobsCOM.Payments)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);
                                        pagoProcesado.DocType = BoRcptTypes.rCustomer;
                                        pagoProcesado.CardCode = Factura.CardCode;
                                        pagoProcesado.DocDate = Factura.Fecha;
                                        pagoProcesado.DueDate = DateTime.Now;
                                        pagoProcesado.TaxDate = DateTime.Now;
                                        pagoProcesado.VatDate = DateTime.Now;
                                        pagoProcesado.Remarks = "Pago procesado por Boletaje";
                                        pagoProcesado.CounterReference = "APP FAC" + Factura.id;
                                        pagoProcesado.DocCurrency = ParametrosFacturacion.MonedaSAPColones;
                                        pagoProcesado.HandWritten = BoYesNoEnum.tNO;
                                        pagoProcesado.Invoices.InvoiceType = BoRcptInvTypes.it_Invoice;
                                        pagoProcesado.Invoices.DocEntry = Convert.ToInt32(Factura.DocEntry);

                                        if (Factura.Moneda != "COL")
                                        {
                                            var SumatoriaPagoColones = MetodosPagosColones.Sum(a => a.Monto) / TipoCambio;
                                            pagoProcesado.Invoices.AppliedFC = Convert.ToDouble(SumatoriaPagoColones);
                                        }
                                        else
                                        {
                                            var SumatoriaPagoColones = MetodosPagosColones.Sum(a => a.Monto);

                                            pagoProcesado.Invoices.SumApplied = Convert.ToDouble(SumatoriaPagoColones);

                                        }
                                        pagoProcesado.Series = ParametrosFacturacion.SeriePago;//154; 161;


                                        var SumatoriaEfectivo = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "Efectivo".ToUpper()).Sum(a => a.Monto);
                                        var PagosTarjetas = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "Tarjeta".ToUpper()).ToList(); //.Sum(a => a.Monto);
                                        var SumatoriaTransferencia = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "Transferencia".ToUpper()).Sum(a => a.Monto);

                                        if (SumatoriaEfectivo > 0)
                                        {
                                            var idcuenta = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "efectivo".ToUpper()).FirstOrDefault().idCuentaBancaria;
                                            var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;


                                            pagoProcesado.CashAccount = Cuenta;
                                            pagoProcesado.CashSum = Convert.ToDouble(SumatoriaEfectivo);

                                        }


                                        foreach (var item in PagosTarjetas)
                                        {

                                            if (item.Monto > 0)
                                            {
                                                var idcuenta = item.idCuentaBancaria;
                                                var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;
                                                if (contador > 0)
                                                {
                                                    pagoProcesado.CreditCards.Add();
                                                }
                                                else
                                                {
                                                    pagoProcesado.CreditCards.SetCurrentLine(contador);

                                                }
                                                pagoProcesado.CreditCards.CardValidUntil = new DateTime(Factura.Fecha.Year, Factura.Fecha.Month, 28); //Fecha en la que se mete el pago 
                                                pagoProcesado.CreditCards.CreditCard = 1;
                                                pagoProcesado.CreditCards.CreditType = BoRcptCredTypes.cr_Regular;
                                                pagoProcesado.CreditCards.PaymentMethodCode = 1; //Quemado
                                                pagoProcesado.CreditCards.CreditCardNumber = item.BIN; // Ultimos 4 digitos
                                                pagoProcesado.CreditCards.VoucherNum = item.NumReferencia;// 
                                                pagoProcesado.CreditCards.CreditAcct = Cuenta;
                                                pagoProcesado.CreditCards.CreditSum = Convert.ToDouble(item.Monto);
                                                contador++;

                                            }
                                        }


                                        if (SumatoriaTransferencia > 0)
                                        {
                                            var idcuenta = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "transferencia".ToUpper()).FirstOrDefault().idCuentaBancaria;
                                            var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;

                                            pagoProcesado.TransferAccount = Cuenta;
                                            pagoProcesado.TransferDate = DateTime.Now; //Fecha en la que se mete el pago 
                                            pagoProcesado.TransferReference = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "transferencia".ToUpper()).FirstOrDefault().NumReferencia;
                                            pagoProcesado.TransferSum = Convert.ToDouble(SumatoriaTransferencia);
                                        }

                                        var respuestaPago = pagoProcesado.Add();
                                        if (respuestaPago == 0)
                                        {
                                            pagoColonesProcesado = true;

                                        }
                                        else
                                        {
                                            var error = "Hubo un error en el pago de la factura #" + Factura.id + " -> " + Conexion.Company.GetLastErrorDescription();
                                            BitacoraErrores be = new BitacoraErrores();
                                            be.Descripcion = error;

                                            be.Fecha = DateTime.Now;
                                            db.BitacoraErrores.Add(be);
                                            db.SaveChanges();
                                            Conexion.Desconectar();

                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                        BitacoraErrores be = new BitacoraErrores();
                                        be.Descripcion = ex.Message;
                                        be.Fecha = DateTime.Now;
                                        db.BitacoraErrores.Add(be);
                                        db.SaveChanges();
                                        Conexion.Desconectar();
                                    }
                                }
                                else
                                {
                                    pagoColonesProcesado = true;

                                }


                                if (MetodosPagosDolares.Count() > 0)
                                {
                                    try
                                    {


                                        var pagoProcesado = (SAPbobsCOM.Payments)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);
                                        pagoProcesado.DocType = BoRcptTypes.rCustomer;
                                        pagoProcesado.CardCode = Factura.CardCode;
                                        pagoProcesado.DocDate = Factura.Fecha;
                                        pagoProcesado.DueDate = DateTime.Now;
                                        pagoProcesado.TaxDate = DateTime.Now;
                                        pagoProcesado.VatDate = DateTime.Now;
                                        pagoProcesado.Remarks = "Pago procesado por Boletaje";
                                        pagoProcesado.CounterReference = "APP FAC" + Factura.id;
                                        pagoProcesado.DocCurrency = "USD";
                                        pagoProcesado.HandWritten = BoYesNoEnum.tNO;
                                        pagoProcesado.Invoices.InvoiceType = BoRcptInvTypes.it_Invoice;
                                        pagoProcesado.Invoices.DocEntry = Convert.ToInt32(Factura.DocEntry);


                                        var SumatoriaPagod = MetodosPagosDolares.Sum(a => a.Monto);
                                        pagoProcesado.Invoices.AppliedFC = Convert.ToDouble(SumatoriaPagod);
                                        pagoProcesado.Series = ParametrosFacturacion.SeriePago;//154; 161;


                                        var SumatoriaEfectivo = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "Efectivo".ToUpper()).Sum(a => a.Monto);
                                        var PagosTarjetas = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "Tarjeta".ToUpper()).ToList();//.Sum(a => a.Monto);
                                        var SumatoriaTransferencia = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "Transferencia".ToUpper()).Sum(a => a.Monto);

                                        if (SumatoriaEfectivo > 0)
                                        {
                                            var idcuenta = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "efectivo".ToUpper()).FirstOrDefault().idCuentaBancaria;
                                            var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;


                                            pagoProcesado.CashAccount = Cuenta;
                                            pagoProcesado.CashSum = Convert.ToDouble(SumatoriaEfectivo);

                                        }


                                        foreach (var item in PagosTarjetas)
                                        {

                                            if (item.Monto > 0)
                                            {
                                                var idcuenta = item.idCuentaBancaria;
                                                var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;
                                                if (contador > 0)
                                                {
                                                    pagoProcesado.CreditCards.Add();
                                                }
                                                else
                                                {
                                                    pagoProcesado.CreditCards.SetCurrentLine(contador);

                                                }
                                                pagoProcesado.CreditCards.CardValidUntil = new DateTime(Factura.Fecha.Year, Factura.Fecha.Month, 28); //Fecha en la que se mete el pago 
                                                pagoProcesado.CreditCards.CreditCard = 1;
                                                pagoProcesado.CreditCards.CreditType = BoRcptCredTypes.cr_Regular;
                                                pagoProcesado.CreditCards.PaymentMethodCode = 1; //Quemado
                                                pagoProcesado.CreditCards.CreditCardNumber = item.BIN; // Ultimos 4 digitos
                                                pagoProcesado.CreditCards.VoucherNum = item.NumReferencia;// 
                                                pagoProcesado.CreditCards.CreditAcct = Cuenta;
                                                pagoProcesado.CreditCards.CreditSum = Convert.ToDouble(item.Monto);

                                                contador++;
                                            }
                                        }



                                        if (SumatoriaTransferencia > 0)
                                        {
                                            var idcuenta = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "transferencia".ToUpper()).FirstOrDefault().idCuentaBancaria;
                                            var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;


                                            pagoProcesado.TransferAccount = Cuenta;
                                            pagoProcesado.TransferDate = DateTime.Now; //Fecha en la que se mete el pago 
                                            pagoProcesado.TransferReference = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "transferencia".ToUpper()).FirstOrDefault().NumReferencia;
                                            pagoProcesado.TransferSum = Convert.ToDouble(SumatoriaTransferencia);
                                        }
                                        var respuestaPago = pagoProcesado.Add();
                                        if (respuestaPago == 0)
                                        {
                                            pagoDolaresProcesado = true;

                                        }
                                        else
                                        {
                                            var error = "Hubo un error en el pago de la factura # " + Factura.id + " -> " + Conexion.Company.GetLastErrorDescription();
                                            BitacoraErrores be = new BitacoraErrores();
                                            be.Descripcion = error;
                                            be.Fecha = DateTime.Now;
                                            db.BitacoraErrores.Add(be);
                                            db.SaveChanges();
                                            Conexion.Desconectar();

                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                        BitacoraErrores be = new BitacoraErrores();
                                        be.Descripcion = ex.Message;
                                        be.Fecha = DateTime.Now;
                                        db.BitacoraErrores.Add(be);
                                        db.SaveChanges();
                                        Conexion.Desconectar();
                                    }
                                }
                                else
                                {
                                    pagoDolaresProcesado = true;

                                }

                                if (pagoColonesProcesado && pagoDolaresProcesado)
                                {
                                    db.Entry(Factura).State = EntityState.Modified;
                                    Factura.DocEntryPago = Conexion.Company.GetNewObjectKey().ToString();
                                    Factura.ProcesadoSAPPago = true;
                                    Factura.FechaProcesadoPago = DateTime.Now;
                                    db.SaveChanges();
                                }


                            }
                            catch (Exception ex)
                            {

                                BitacoraErrores be = new BitacoraErrores();
                                be.Descripcion = ex.Message;
                                be.Fecha = DateTime.Now;
                                db.BitacoraErrores.Add(be);
                                db.SaveChanges();
                            }



                        }



                        Conexion.Desconectar();


                    }
                    catch (Exception ex)
                    {
                        BitacoraErrores be = new BitacoraErrores();
                        be.Descripcion = ex.Message;

                        be.Fecha = DateTime.Now;
                        be.StackTrace = ex.StackTrace;
                        db.BitacoraErrores.Add(be);
                        db.SaveChanges();
                        Conexion.Desconectar();


                    }

                    //
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

        [Route("api/Facturas/SincronizarSAP")]
        public HttpResponseMessage GetSincronizarSAPDocumentosMasivos()
        {
            try
            {
                var Facturas = db.EncFacturas.Where(a => a.ProcesadoSAP == false).OrderByDescending(a => a.id).ToList();

                foreach (var Factura in Facturas)
                {
                    //Empieza a mandar a SAP
                    try
                    {
                        var ParametrosFacturacion = db.ParametrosFacturacion.FirstOrDefault();
                        var documentoSAP = (Documents)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices);

                        //Encabezado

                        documentoSAP.DocObjectCode = BoObjectTypes.oInvoices;
                        documentoSAP.CardCode = Factura.CardCode;
                        documentoSAP.DocCurrency = Factura.Moneda == "COL" ? ParametrosFacturacion.MonedaSAPColones : Factura.Moneda;
                        var Dias = db.CondicionesPagos.Where(a => a.id == Factura.idCondicionVenta).FirstOrDefault() == null ? 0 : db.CondicionesPagos.Where(a => a.id == Factura.idCondicionVenta).FirstOrDefault().Dias;

                        documentoSAP.DocDate = Factura.Fecha;
                        documentoSAP.DocDueDate = Factura.Fecha.AddDays(Dias);
                        documentoSAP.DocType = BoDocumentTypes.dDocument_Items;
                        documentoSAP.NumAtCard = "Boletaje FAC:" + " " + Factura.id;
                        documentoSAP.Comments = g.TruncarString(Factura.Comentarios, 200);
                        documentoSAP.PaymentGroupCode = Convert.ToInt32(db.CondicionesPagos.Where(a => a.id == Factura.idCondicionVenta).FirstOrDefault() == null ? "0" : db.CondicionesPagos.Where(a => a.id == Factura.idCondicionVenta).FirstOrDefault().codSAP);
                        var CondPago = db.CondicionesPagos.Where(a => a.id == Factura.idCondicionVenta).FirstOrDefault() == null ? "0" : db.CondicionesPagos.Where(a => a.id == Factura.idCondicionVenta).FirstOrDefault().Nombre;
                        documentoSAP.Series = CondPago.ToLower().Contains("contado") ? ParametrosFacturacion.SerieFECO : ParametrosFacturacion.SerieFECR;  //4;  //param.SerieProforma; //Quemada





                        if (Factura.Moneda == "USD")
                        {
                            documentoSAP.DocTotalFc = Convert.ToDouble(Factura.TotalCompra);
                        }
                        else
                        {
                            documentoSAP.DocTotal = Convert.ToDouble(Factura.TotalCompra);
                        }
                        var EncMovimiento = new EncMovimiento();
                        if (Factura.idEntrega > 0)
                        {
                            EncMovimiento = db.EncMovimiento.Where(a => a.id == Factura.idEntrega).FirstOrDefault();
                            var Llam = Convert.ToInt32(EncMovimiento.NumLlamada);
                            var Llamada2 = db.LlamadasServicios.Where(a => a.DocEntry == Llam).FirstOrDefault();
                            var Tec = Llamada2.Tecnico == null ? "" : Llamada2.Tecnico.ToString();
                            var Tecnico = db.Tecnicos.Where(a => a.idSAP == Tec).FirstOrDefault();



                            if (Tecnico.Letra > 0)
                            {
                                documentoSAP.SalesPersonCode = Tecnico.Letra;
                            }
                        }


                        documentoSAP.UserFields.Fields.Item(ParametrosFacturacion.CampoConsecutivo).Value = Factura.ConsecutivoHacienda; //"U_LDT_NumeroGTI"
                        documentoSAP.UserFields.Fields.Item(ParametrosFacturacion.CampoClave).Value = Factura.ClaveHacienda;       //"U_LDT_FiscalDoc"
                                                                                                                                   //documentoSAP.UserFields.Fields.Item("U_DYD_Estado").Value = "A";

                        //Detalle
                        int z = 0;
                        var Detalle = db.DetFacturas.Where(a => a.idEncabezado == Factura.id).ToList();
                        foreach (var item in Detalle)
                        {

                            documentoSAP.Lines.SetCurrentLine(z);

                            documentoSAP.Lines.Currency = Factura.Moneda == "COL" ? ParametrosFacturacion.MonedaSAPColones : Factura.Moneda;
                            documentoSAP.Lines.Quantity = Convert.ToDouble(item.Cantidad);
                            documentoSAP.Lines.DiscountPercent = Convert.ToDouble(item.PorDescto);
                            documentoSAP.Lines.ItemCode = item.ItemCode;



                            var idImp = item.idImpuesto;




                            documentoSAP.Lines.TaxCode = item.idDocumentoExoneracion > 0 ? (db.Impuestos.Where(a => a.CodSAP.ToLower().Contains("EXE".ToLower())).FirstOrDefault() == null ? "EXE" : db.Impuestos.Where(a => a.CodSAP.ToLower().Contains("EXE".ToLower())).FirstOrDefault().CodSAP) : db.Impuestos.Where(a => a.id == idImp).FirstOrDefault() == null ? "IV" : db.Impuestos.Where(a => a.id == idImp).FirstOrDefault().CodSAP;
                            if (item.idDocumentoExoneracion > 0)
                            {
                                var conexion2 = g.DevuelveCadena(db);
                                var valorAFiltrar = item.idDocumentoExoneracion.ToString();

                                var SQL = ParametrosFacturacion.SQLDocumentoExoneracion + valorAFiltrar;

                                SqlConnection Cn = new SqlConnection(conexion2);
                                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                                DataSet Ds = new DataSet();
                                Cn.Open();
                                Da.Fill(Ds, "DocNum1");
                                documentoSAP.Lines.UserFields.Fields.Item("U_Tipo_Doc").Value = Ds.Tables["DocNum1"].Rows[0]["TipoDocumento"].ToString();
                                documentoSAP.Lines.UserFields.Fields.Item("U_NumDoc").Value = Ds.Tables["DocNum1"].Rows[0]["NumeroDocumento"].ToString();
                                documentoSAP.Lines.UserFields.Fields.Item("U_NomInst").Value = Ds.Tables["DocNum1"].Rows[0]["Emisora"].ToString();
                                documentoSAP.Lines.UserFields.Fields.Item("U_FecEmis").Value = Convert.ToDateTime(Ds.Tables["DocNum1"].Rows[0]["FechaEmision"].ToString());

                                Cn.Close();



                            }

                            documentoSAP.Lines.TaxOnly = BoYesNoEnum.tNO;



                            documentoSAP.Lines.UnitPrice = Convert.ToDouble(item.PrecioUnitario);

                            documentoSAP.Lines.WarehouseCode = item.CodBodega;
                            if (!string.IsNullOrEmpty(ParametrosFacturacion.Norma))
                            {
                                switch (ParametrosFacturacion.Dimension)
                                {
                                    case 1:
                                        {
                                            documentoSAP.Lines.CostingCode = ParametrosFacturacion.Norma;

                                            break;
                                        }
                                    case 2:
                                        {
                                            documentoSAP.Lines.CostingCode2 = ParametrosFacturacion.Norma;
                                            break;
                                        }
                                    case 3:
                                        {
                                            documentoSAP.Lines.CostingCode3 = ParametrosFacturacion.Norma;
                                            break;
                                        }
                                    case 4:
                                        {
                                            documentoSAP.Lines.CostingCode4 = ParametrosFacturacion.Norma;
                                            break;
                                        }
                                    case 5:
                                        {
                                            documentoSAP.Lines.CostingCode5 = ParametrosFacturacion.Norma;
                                            break;
                                        }

                                }
                            }

                            try
                            {
                                // Get the delivery
                                if (Factura.idEntrega > 0)
                                {

                                    Documents delivery = (Documents)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDeliveryNotes);
                                    if (delivery.GetByKey(EncMovimiento.DocEntry))
                                    {
                                        for (int j = 0; j < delivery.Lines.Count; j++)
                                        {
                                            delivery.Lines.SetCurrentLine(j);

                                            if (item.ItemCode == delivery.Lines.ItemCode) // Match criteria can be adjusted
                                            {
                                                documentoSAP.Lines.BaseEntry = delivery.DocEntry;
                                                documentoSAP.Lines.BaseType = (int)BoObjectTypes.oDeliveryNotes;
                                                documentoSAP.Lines.BaseLine = delivery.Lines.LineNum;
                                                break;
                                            }
                                        }
                                    }
                                }

                            }
                            catch (Exception ex)
                            {
                                BitacoraErrores be = new BitacoraErrores();
                                be.Descripcion = ex.Message;

                                be.Fecha = DateTime.Now;

                                db.BitacoraErrores.Add(be);
                                db.SaveChanges();

                            }
                            documentoSAP.Lines.Add();
                            z++;
                        }






                        var respuesta = documentoSAP.Add();
                        if (respuesta == 0) //se creo exitorsamente 
                        {
                            var idEntry = 0;

                            try
                            {
                                idEntry = Convert.ToInt32(Conexion.Company.GetNewObjectKey());
                                throw new Exception("");
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    var Parametros = db.Parametros.FirstOrDefault();
                                    var conexion = g.DevuelveCadena(db);
                                    var valorAFiltrar = "Boletaje FAC: " + Factura.id.ToString();
                                    var filtroSQL = "NumAtCard like '%" + valorAFiltrar + "%' order by DocEntry desc";
                                    var SQL = Parametros.SQLDocEntryDocs.Replace("@CampoBuscar", "DocEntry").Replace("@Tabla", "OINV").Replace("@CampoWhere = @reemplazo", filtroSQL);

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


                            db.Entry(Factura).State = EntityState.Modified;
                            Factura.DocEntry = idEntry.ToString();
                            Factura.ProcesadoSAP = true;
                            Factura.FechaProcesado = DateTime.Now;
                            db.SaveChanges();
                            if(Factura.idEntrega > 0)
                            {
                                var count = -1;
                                try
                                {
                                    var client2 = (ServiceCalls)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oServiceCalls);

                                    if (client2.GetByKey(Convert.ToInt32(Factura.NumLlamada)))
                                    {
                                        var idLlamada = Convert.ToInt32(EncMovimiento.NumLlamada);
                                        var Llamada = db.LlamadasServicios.Where(a => a.DocEntry == idLlamada).FirstOrDefault();
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

                                        client2.Expenses.DocumentType = BoSvcEpxDocTypes.edt_Invoice;

                                        client2.Expenses.DocumentNumber = Convert.ToInt32(Factura.DocEntry);
                                        client2.Expenses.DocEntry = Convert.ToInt32(Factura.DocEntry);
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
                                            be.StackTrace = "Insercion de Liga FAC - LL " + client2.DocNum;
                                            be.Fecha = DateTime.Now;

                                            db.BitacoraErrores.Add(be);
                                            db.SaveChanges();
                                            Conexion.Desconectar();


                                        }
                                    }
                                }
                                catch (Exception)
                                {


                                }
                            }
                            
                            

                            //Procesamos el pago
                            var CondicionPago = db.CondicionesPagos.Where(a => a.id == Factura.idCondicionVenta).FirstOrDefault() == null ? db.CondicionesPagos.FirstOrDefault() : db.CondicionesPagos.Where(a => a.id == Factura.idCondicionVenta).FirstOrDefault();
                            //Procesamos el pago
                            if (CondicionPago.Dias == 0)
                            {

                                try
                                {


                                    var Fecha = Factura.Fecha.Date;
                                    var TipoCambio = Factura.TipoCambio;
                                    var MetodosPagos = db.MetodosPagosFacturas.Where(a => a.idEncabezado == Factura.id && a.Monto > 0).FirstOrDefault() == null ? new List<MetodosPagosFacturas>() : db.MetodosPagosFacturas.Where(a => a.idEncabezado == Factura.id && a.Monto > 0).ToList();

                                    var MetodosPagosColones = MetodosPagos.Where(a => a.Moneda == "COL").ToList();
                                    var MetodosPagosDolares = MetodosPagos.Where(a => a.Moneda == "USD").ToList();

                                    bool pagoColonesProcesado = false;
                                    bool pagoDolaresProcesado = false;


                                    var contador = 0;




                                    if (MetodosPagosColones.Count() > 0)
                                    {
                                        try
                                        {


                                            var pagoProcesado = (SAPbobsCOM.Payments)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);
                                            pagoProcesado.DocType = BoRcptTypes.rCustomer;
                                            pagoProcesado.CardCode = Factura.CardCode;
                                            pagoProcesado.DocDate = Factura.Fecha;
                                            pagoProcesado.DueDate = DateTime.Now;
                                            pagoProcesado.TaxDate = DateTime.Now;
                                            pagoProcesado.VatDate = DateTime.Now;
                                            pagoProcesado.Remarks = "Pago procesado por Boletaje";
                                            pagoProcesado.CounterReference = "APP FAC" + Factura.id;
                                            pagoProcesado.DocCurrency = ParametrosFacturacion.MonedaSAPColones;
                                            pagoProcesado.HandWritten = BoYesNoEnum.tNO;
                                            pagoProcesado.Invoices.InvoiceType = BoRcptInvTypes.it_Invoice;
                                            pagoProcesado.Invoices.DocEntry = Convert.ToInt32(Factura.DocEntry);

                                            if (Factura.Moneda != "COL")
                                            {
                                                var SumatoriaPagoColones = MetodosPagosColones.Sum(a => a.Monto) / TipoCambio;
                                                pagoProcesado.Invoices.AppliedFC = Convert.ToDouble(SumatoriaPagoColones);
                                            }
                                            else
                                            {
                                                var SumatoriaPagoColones = MetodosPagosColones.Sum(a => a.Monto);

                                                pagoProcesado.Invoices.SumApplied = Convert.ToDouble(SumatoriaPagoColones);

                                            }
                                            pagoProcesado.Series = ParametrosFacturacion.SeriePago;//154; 161;


                                            var SumatoriaEfectivo = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "Efectivo".ToUpper()).Sum(a => a.Monto);
                                            var PagosTarjetas = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "Tarjeta".ToUpper()).ToList(); //.Sum(a => a.Monto);
                                            var SumatoriaTransferencia = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "Transferencia".ToUpper()).Sum(a => a.Monto);

                                            if (SumatoriaEfectivo > 0)
                                            {
                                                var idcuenta = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "efectivo".ToUpper()).FirstOrDefault().idCuentaBancaria;
                                                var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;


                                                pagoProcesado.CashAccount = Cuenta;
                                                pagoProcesado.CashSum = Convert.ToDouble(SumatoriaEfectivo);

                                            }


                                            foreach (var item in PagosTarjetas)
                                            {

                                                if (item.Monto > 0)
                                                {
                                                    var idcuenta = item.idCuentaBancaria;
                                                    var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;
                                                    if (contador > 0)
                                                    {
                                                        pagoProcesado.CreditCards.Add();
                                                    }
                                                    else
                                                    {
                                                        pagoProcesado.CreditCards.SetCurrentLine(contador);

                                                    }
                                                    pagoProcesado.CreditCards.CardValidUntil = new DateTime(Factura.Fecha.Year, Factura.Fecha.Month, 28); //Fecha en la que se mete el pago 
                                                    pagoProcesado.CreditCards.CreditCard = 1;
                                                    pagoProcesado.CreditCards.CreditType = BoRcptCredTypes.cr_Regular;
                                                    pagoProcesado.CreditCards.PaymentMethodCode = 1; //Quemado
                                                    pagoProcesado.CreditCards.CreditCardNumber = item.BIN; // Ultimos 4 digitos
                                                    pagoProcesado.CreditCards.VoucherNum = item.NumReferencia;// 
                                                    pagoProcesado.CreditCards.CreditAcct = Cuenta;
                                                    pagoProcesado.CreditCards.CreditSum = Convert.ToDouble(item.Monto);
                                                    contador++;

                                                }
                                            }


                                            if (SumatoriaTransferencia > 0)
                                            {
                                                var idcuenta = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "transferencia".ToUpper()).FirstOrDefault().idCuentaBancaria;
                                                var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;

                                                pagoProcesado.TransferAccount = Cuenta;
                                                pagoProcesado.TransferDate = DateTime.Now; //Fecha en la que se mete el pago 
                                                pagoProcesado.TransferReference = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "transferencia".ToUpper()).FirstOrDefault().NumReferencia;
                                                pagoProcesado.TransferSum = Convert.ToDouble(SumatoriaTransferencia);
                                            }

                                            var respuestaPago = pagoProcesado.Add();
                                            if (respuestaPago == 0)
                                            {
                                                pagoColonesProcesado = true;

                                            }
                                            else
                                            {
                                                var error = "Hubo un error en el pago de la factura #" + Factura.id + " -> " + Conexion.Company.GetLastErrorDescription();
                                                BitacoraErrores be = new BitacoraErrores();
                                                be.Descripcion = error;

                                                be.Fecha = DateTime.Now;
                                                db.BitacoraErrores.Add(be);
                                                db.SaveChanges();
                                                Conexion.Desconectar();

                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                            BitacoraErrores be = new BitacoraErrores();
                                            be.Descripcion = ex.Message;
                                            be.Fecha = DateTime.Now;
                                            db.BitacoraErrores.Add(be);
                                            db.SaveChanges();
                                            Conexion.Desconectar();
                                        }
                                    }
                                    else
                                    {
                                        pagoColonesProcesado = true;

                                    }


                                    if (MetodosPagosDolares.Count() > 0)
                                    {
                                        try
                                        {


                                            var pagoProcesado = (SAPbobsCOM.Payments)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);
                                            pagoProcesado.DocType = BoRcptTypes.rCustomer;
                                            pagoProcesado.CardCode = Factura.CardCode;
                                            pagoProcesado.DocDate = Factura.Fecha;
                                            pagoProcesado.DueDate = DateTime.Now;
                                            pagoProcesado.TaxDate = DateTime.Now;
                                            pagoProcesado.VatDate = DateTime.Now;
                                            pagoProcesado.Remarks = "Pago procesado por Boletaje";
                                            pagoProcesado.CounterReference = "APP FAC" + Factura.id;
                                            pagoProcesado.DocCurrency = "USD";
                                            pagoProcesado.HandWritten = BoYesNoEnum.tNO;
                                            pagoProcesado.Invoices.InvoiceType = BoRcptInvTypes.it_Invoice;
                                            pagoProcesado.Invoices.DocEntry = Convert.ToInt32(Factura.DocEntry);


                                            var SumatoriaPagod = MetodosPagosDolares.Sum(a => a.Monto);
                                            pagoProcesado.Invoices.AppliedFC = Convert.ToDouble(SumatoriaPagod);
                                            pagoProcesado.Series = ParametrosFacturacion.SeriePago;//154; 161;


                                            var SumatoriaEfectivo = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "Efectivo".ToUpper()).Sum(a => a.Monto);
                                            var PagosTarjetas = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "Tarjeta".ToUpper()).ToList();//.Sum(a => a.Monto);
                                            var SumatoriaTransferencia = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "Transferencia".ToUpper()).Sum(a => a.Monto);

                                            if (SumatoriaEfectivo > 0)
                                            {
                                                var idcuenta = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "efectivo".ToUpper()).FirstOrDefault().idCuentaBancaria;
                                                var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;


                                                pagoProcesado.CashAccount = Cuenta;
                                                pagoProcesado.CashSum = Convert.ToDouble(SumatoriaEfectivo);

                                            }


                                            foreach (var item in PagosTarjetas)
                                            {

                                                if (item.Monto > 0)
                                                {
                                                    var idcuenta = item.idCuentaBancaria;
                                                    var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;
                                                    if (contador > 0)
                                                    {
                                                        pagoProcesado.CreditCards.Add();
                                                    }
                                                    else
                                                    {
                                                        pagoProcesado.CreditCards.SetCurrentLine(contador);

                                                    }
                                                    pagoProcesado.CreditCards.CardValidUntil = new DateTime(Factura.Fecha.Year, Factura.Fecha.Month, 28); //Fecha en la que se mete el pago 
                                                    pagoProcesado.CreditCards.CreditCard = 1;
                                                    pagoProcesado.CreditCards.CreditType = BoRcptCredTypes.cr_Regular;
                                                    pagoProcesado.CreditCards.PaymentMethodCode = 1; //Quemado
                                                    pagoProcesado.CreditCards.CreditCardNumber = item.BIN; // Ultimos 4 digitos
                                                    pagoProcesado.CreditCards.VoucherNum = item.NumReferencia;// 
                                                    pagoProcesado.CreditCards.CreditAcct = Cuenta;
                                                    pagoProcesado.CreditCards.CreditSum = Convert.ToDouble(item.Monto);

                                                    contador++;
                                                }
                                            }



                                            if (SumatoriaTransferencia > 0)
                                            {
                                                var idcuenta = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "transferencia".ToUpper()).FirstOrDefault().idCuentaBancaria;
                                                var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;


                                                pagoProcesado.TransferAccount = Cuenta;
                                                pagoProcesado.TransferDate = DateTime.Now; //Fecha en la que se mete el pago 
                                                pagoProcesado.TransferReference = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "transferencia".ToUpper()).FirstOrDefault().NumReferencia;
                                                pagoProcesado.TransferSum = Convert.ToDouble(SumatoriaTransferencia);
                                            }
                                            var respuestaPago = pagoProcesado.Add();
                                            if (respuestaPago == 0)
                                            {
                                                pagoDolaresProcesado = true;

                                            }
                                            else
                                            {
                                                var error = "Hubo un error en el pago de la factura # " + Factura.id + " -> " + Conexion.Company.GetLastErrorDescription();
                                                BitacoraErrores be = new BitacoraErrores();
                                                be.Descripcion = error;
                                                be.Fecha = DateTime.Now;
                                                db.BitacoraErrores.Add(be);
                                                db.SaveChanges();
                                                Conexion.Desconectar();

                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                            BitacoraErrores be = new BitacoraErrores();
                                            be.Descripcion = ex.Message;
                                            be.Fecha = DateTime.Now;
                                            db.BitacoraErrores.Add(be);
                                            db.SaveChanges();
                                            Conexion.Desconectar();
                                        }
                                    }
                                    else
                                    {
                                        pagoDolaresProcesado = true;

                                    }

                                    if (pagoColonesProcesado && pagoDolaresProcesado)
                                    {
                                        db.Entry(Factura).State = EntityState.Modified;
                                        Factura.DocEntryPago = Conexion.Company.GetNewObjectKey().ToString();
                                        Factura.ProcesadoSAPPago = true;
                                        Factura.FechaProcesadoPago = DateTime.Now;
                                        db.SaveChanges();
                                    }


                                }
                                catch (Exception ex)
                                {

                                    BitacoraErrores be = new BitacoraErrores();
                                    be.Descripcion = ex.Message;
                                    be.Fecha = DateTime.Now;
                                    db.BitacoraErrores.Add(be);
                                    db.SaveChanges();
                                }



                            }



                            Conexion.Desconectar();

                        }
                        else
                        {
                            var error = "hubo un error en la factura " + Factura.id + " -> " + Conexion.Company.GetLastErrorDescription();
                            BitacoraErrores be = new BitacoraErrores();
                            be.Descripcion = error;

                            be.Fecha = DateTime.Now;
                            be.StackTrace = "Error al mandar a SAP";
                            db.BitacoraErrores.Add(be);
                            db.SaveChanges();
                            Conexion.Desconectar();


                        }
                    }
                    catch (Exception ex)
                    {
                        BitacoraErrores be = new BitacoraErrores();
                        be.Descripcion = ex.Message;

                        be.Fecha = DateTime.Now;
                        be.StackTrace = ex.StackTrace;
                        db.BitacoraErrores.Add(be);
                        db.SaveChanges();
                        Conexion.Desconectar();


                    }

                    //
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
        public async Task<HttpResponseMessage> PostAsync([FromBody] FacturasViewModel factura)
        {
            var t = db.Database.BeginTransaction();
            try
            {

                ParametrosFacturacion parametros = db.ParametrosFacturacion.FirstOrDefault();

                var Factura = db.EncFacturas.Where(a => a.id == factura.id).FirstOrDefault();

                if (Factura == null)
                {
                    if(factura.idEntrega == 0)
                    {
                        var Fecha = factura.Fecha.Date;
                        var VerificaExistencia = db.LlamadasFacturas.Where(a => a.CardCode == factura.CardCode && a.ItemCode == factura.ItemCode && a.Serie == factura.Serie && a.Fecha == Fecha).FirstOrDefault();

                        if(VerificaExistencia != null)
                        {
                            throw new Exception("YA existe una factura igual, favor revisar en el listado de facturas");
                        }
                    }
                    else
                    {
                        var Fecha = factura.Fecha.Date;
                        var Fecha2 = Fecha.AddDays(1);
                        var VerificaExistencia = db.EncFacturas.Where(a => a.CardCode == factura.CardCode && a.idEntrega == factura.idEntrega && a.Fecha >= Fecha && a.Fecha <= Fecha2).FirstOrDefault();

                        if (VerificaExistencia != null)
                        {
                            throw new Exception("YA existe una factura igual, favor revisar en el listado de facturas");
                        }
                    }
                    Factura = new EncFacturas();
                    Factura.idCondicionVenta = factura.idCondicionVenta;
                    Factura.idSucursal = factura.idSucursal;
                    Factura.idPlazoCredito = factura.idPlazoCredito;
                    Factura.idEntrega = factura.idEntrega;
                    Factura.NumLlamada = factura.NumLlamada;
                    Factura.TipoDocumento = factura.TipoDocumento;
                    Factura.CardCode = factura.CardCode;
                    Factura.Cedula = factura.Cedula;
                    Factura.NombreCliente = factura.NombreCliente;
                    Factura.Correo = factura.Correo;
                    Factura.Fecha = DateTime.Now;
                    Factura.Moneda = factura.Moneda;
                    Factura.TipoCambio = factura.TipoCambio;
                    Factura.Comentarios = factura.Comentarios;
                    Factura.ProcesadoSAP = false;
                    Factura.FechaProcesado = DateTime.Now;
                    Factura.Subtotal = factura.Subtotal;
                    Factura.TotalImpuestos = factura.TotalImpuestos;
                    Factura.TotalDescuento = factura.TotalDescuento;
                    Factura.TotalCompra = factura.TotalCompra;
                    Factura.CreadoPor = factura.CreadoPor;
                    Factura.PorDesc = factura.PorDesc;
                    Factura.ProcesadoSAPPago = false;
                    Factura.FechaProcesadoPago = DateTime.Now;
                    db.EncFacturas.Add(Factura);
                    db.SaveChanges();

                    var conexion = g.DevuelveCadena(db);

                    var i = 0;
                    foreach (var item in factura.DetFactura)
                    {

                        var SQL = parametros.SQLProductosBuscar + "'" + item.ItemCode + "'";

                        SqlConnection Cn = new SqlConnection(conexion);
                        SqlCommand Cmd = new SqlCommand(SQL, Cn);
                        SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                        DataSet Ds = new DataSet();
                        Cn.Open();
                        Da.Fill(Ds, "Productos");

                        var DetFactura = new DetFacturas();
                        DetFactura.idEncabezado = Factura.id;
                        DetFactura.idImpuesto = item.idImpuesto;
                        DetFactura.NumLinea = i;
                        DetFactura.ItemCode = item.ItemCode;
                        DetFactura.CodBodega = Ds.Tables["Productos"].Rows[0]["Bodega"].ToString();
                        DetFactura.Cabys = Ds.Tables["Productos"].Rows[0]["Cabys"].ToString();
                        DetFactura.ListaPrecios = Ds.Tables["Productos"].Rows[0]["ListaPrecios"].ToString();
                        DetFactura.Cantidad = item.Cantidad;
                        DetFactura.UnidadMedida = Ds.Tables["Productos"].Rows[0]["UnidadMedida"].ToString();
                        DetFactura.NomPro = item.NomPro;
                        DetFactura.PrecioUnitario = item.PrecioUnitario;
                        DetFactura.PorDescto = item.PorDescto;
                        DetFactura.TotalDescuento = item.TotalDescuento;
                        DetFactura.TotalImpuestos = item.TotalImpuestos;
                        DetFactura.TotalLinea = item.TotalLinea;
                        DetFactura.idDocumentoExoneracion = item.idDocumentoExoneracion;
                        db.DetFacturas.Add(DetFactura);
                        db.SaveChanges();
                        Cn.Close();
                        i++;
                    }

                    foreach (var item in factura.MetodosPagos)
                    {
                        var Metodo = new MetodosPagosFacturas();
                        Metodo.idCuentaBancaria = item.idCuentaBancaria;
                        Metodo.idEncabezado = Factura.id;
                        Metodo.Metodo = item.Metodo;
                        Metodo.BIN = item.BIN;
                        Metodo.Moneda = item.Moneda;
                        Metodo.MonedaVuelto = item.MonedaVuelto;
                        Metodo.Monto = item.Monto;
                        Metodo.NumCheque = item.NumCheque;
                        Metodo.NumReferencia = item.NumReferencia;
                        Metodo.PagadoCon = item.PagadoCon;
                        Metodo.Fecha = DateTime.Now;
                        db.MetodosPagosFacturas.Add(Metodo);
                        db.SaveChanges();

                    }

                    t.Commit();

                    //Empieza la parte de fActuracion
                    HttpClient cliente = new HttpClient();

                    try
                    {

                        var Url = parametros.UrlFacturar.Replace("@DocNumR", Factura.id.ToString()).Replace("@ObjTypeR", (Factura.TipoDocumento != "03" ? "13" : "14")).Replace("@SucursalR", parametros.Sucursal);

                        cliente.Timeout = TimeSpan.FromMinutes(30);
                        HttpResponseMessage response = await cliente.GetAsync(Url);
                        if (response.IsSuccessStatusCode)
                        {
                            response.Content.Headers.ContentType.MediaType = "application/json";
                            var res = await response.Content.ReadAsAsync<RecibidoFacturacion>();

                            db.Entry(Factura).State = EntityState.Modified;
                            Factura.ClaveHacienda = res.ClaveHacienda;
                            Factura.ConsecutivoHacienda = res.ConsecutivoHacienda;

                            factura.ClaveHacienda = res.ClaveHacienda;
                            factura.ConsecutivoHacienda = res.ConsecutivoHacienda;
                            db.SaveChanges();

                            if (Factura.idEntrega > 0)
                            {
                                var Entrega = db.EncMovimiento.Where(a => a.id == Factura.idEntrega).FirstOrDefault();
                                if (Entrega != null && res.code == 1)
                                {
                                    db.Entry(Entrega).State = EntityState.Modified;
                                    Entrega.Facturado = true;
                                    db.SaveChanges();
                                }
                                else
                                {
                                    throw new Exception("Ha ocurrido un error al facturar: " + "el resultado de la factura es " + res.code);
                                }

                            }


                            try
                            {
                                HttpClient cliente2 = new HttpClient();

                                var Url2 = parametros.UrlDocumentos.Replace("@ClaveR", Factura.ClaveHacienda.ToString()).Replace("@SucursalR", parametros.Sucursal);

                                HttpResponseMessage response2 = await cliente2.GetAsync(Url2);
                                if (response2.IsSuccessStatusCode)
                                {
                                    response2.Content.Headers.ContentType.MediaType = "application/json";
                                    var res2 = await response2.Content.ReadAsStringAsync();
                                }

                            }
                            catch (Exception ex)
                            {

                                BitacoraErrores be = new BitacoraErrores();
                                be.Descripcion = ex.Message;
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
                        be.Fecha = DateTime.Now;
                        db.BitacoraErrores.Add(be);
                        db.SaveChanges();
                        throw new Exception(ex.Message);
                    }


                    //Se termina el api de facturacion 

                    //Empieza a mandar a SAP
                    try
                    {
                        var ParametrosFacturacion = db.ParametrosFacturacion.FirstOrDefault();
                        var documentoSAP = (Documents)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices);

                        //Encabezado

                        documentoSAP.DocObjectCode = BoObjectTypes.oInvoices;
                        documentoSAP.CardCode = Factura.CardCode;
                        documentoSAP.DocCurrency = Factura.Moneda == "COL" ? ParametrosFacturacion.MonedaSAPColones : Factura.Moneda;
                        var Dias = db.CondicionesPagos.Where(a => a.id == Factura.idCondicionVenta).FirstOrDefault() == null ? 0 : db.CondicionesPagos.Where(a => a.id == Factura.idCondicionVenta).FirstOrDefault().Dias;

                        documentoSAP.DocDate = Factura.Fecha;
                        documentoSAP.DocDueDate = Factura.Fecha.AddDays(Dias);
                        documentoSAP.DocType = BoDocumentTypes.dDocument_Items;
                        documentoSAP.NumAtCard = "Boletaje FAC:" + " " + Factura.id;
                        documentoSAP.Comments = g.TruncarString(Factura.Comentarios, 200);
                        documentoSAP.PaymentGroupCode = Convert.ToInt32(db.CondicionesPagos.Where(a => a.id == Factura.idCondicionVenta).FirstOrDefault() == null ? "0" : db.CondicionesPagos.Where(a => a.id == Factura.idCondicionVenta).FirstOrDefault().codSAP);
                        var CondPago = db.CondicionesPagos.Where(a => a.id == Factura.idCondicionVenta).FirstOrDefault() == null ? "0" : db.CondicionesPagos.Where(a => a.id == Factura.idCondicionVenta).FirstOrDefault().Nombre;
                        documentoSAP.Series = CondPago.ToLower().Contains("contado") ? ParametrosFacturacion.SerieFECO : ParametrosFacturacion.SerieFECR;  //4;  //param.SerieProforma; //Quemada





                        if (Factura.Moneda == "USD")
                        {
                            documentoSAP.DocTotalFc = Convert.ToDouble(Factura.TotalCompra);
                        }
                        else
                        {
                            documentoSAP.DocTotal = Convert.ToDouble(Factura.TotalCompra);
                        }

                        var EncMovimiento = new EncMovimiento();
                        if (Factura.idEntrega > 0)
                        {
                            EncMovimiento = db.EncMovimiento.Where(a => a.id == Factura.idEntrega).FirstOrDefault();
                            var Llam = Convert.ToInt32(EncMovimiento.NumLlamada);
                            var Llamada2 = db.LlamadasServicios.Where(a => a.DocEntry == Llam).FirstOrDefault();
                            var Tec = Llamada2.Tecnico == null ? "" : Llamada2.Tecnico.ToString();
                            var Tecnico = db.Tecnicos.Where(a => a.idSAP == Tec).FirstOrDefault();



                            if (Tecnico.Letra > 0)
                            {
                                documentoSAP.SalesPersonCode = Tecnico.Letra;
                            }
                        }

                        documentoSAP.UserFields.Fields.Item(ParametrosFacturacion.CampoConsecutivo).Value = Factura.ConsecutivoHacienda; //"U_LDT_NumeroGTI"
                        documentoSAP.UserFields.Fields.Item(ParametrosFacturacion.CampoClave).Value = Factura.ClaveHacienda;       //"U_LDT_FiscalDoc"
                                                                                                                                   //documentoSAP.UserFields.Fields.Item("U_DYD_Estado").Value = "A";

                        //Detalle
                        int z = 0;
                        var Detalle = db.DetFacturas.Where(a => a.idEncabezado == Factura.id).ToList();
                        foreach (var item in Detalle)
                        {

                            documentoSAP.Lines.SetCurrentLine(z);

                            documentoSAP.Lines.Currency = Factura.Moneda == "COL" ? ParametrosFacturacion.MonedaSAPColones : Factura.Moneda;
                            documentoSAP.Lines.Quantity = Convert.ToDouble(item.Cantidad);
                            documentoSAP.Lines.DiscountPercent = Convert.ToDouble(item.PorDescto);
                            documentoSAP.Lines.ItemCode = item.ItemCode;



                            var idImp = item.idImpuesto;




                            documentoSAP.Lines.TaxCode = item.idDocumentoExoneracion > 0 ? (db.Impuestos.Where(a => a.CodSAP.ToLower().Contains("EXE".ToLower())).FirstOrDefault() == null ? "EXE" : db.Impuestos.Where(a => a.CodSAP.ToLower().Contains("EXE".ToLower())).FirstOrDefault().CodSAP) : db.Impuestos.Where(a => a.id == idImp).FirstOrDefault() == null ? "IV" : db.Impuestos.Where(a => a.id == idImp).FirstOrDefault().CodSAP;
                            if (item.idDocumentoExoneracion > 0)
                            {
                                var conexion2 = g.DevuelveCadena(db);
                                var valorAFiltrar = item.idDocumentoExoneracion.ToString();

                                var SQL = ParametrosFacturacion.SQLDocumentoExoneracion + valorAFiltrar;

                                SqlConnection Cn = new SqlConnection(conexion2);
                                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                                DataSet Ds = new DataSet();
                                Cn.Open();
                                Da.Fill(Ds, "DocNum1");
                                documentoSAP.Lines.UserFields.Fields.Item("U_Tipo_Doc").Value = Ds.Tables["DocNum1"].Rows[0]["TipoDocumento"].ToString();
                                documentoSAP.Lines.UserFields.Fields.Item("U_NumDoc").Value = Ds.Tables["DocNum1"].Rows[0]["NumeroDocumento"].ToString();
                                documentoSAP.Lines.UserFields.Fields.Item("U_NomInst").Value = Ds.Tables["DocNum1"].Rows[0]["Emisora"].ToString();
                                documentoSAP.Lines.UserFields.Fields.Item("U_FecEmis").Value = Convert.ToDateTime(Ds.Tables["DocNum1"].Rows[0]["FechaEmision"].ToString());

                                Cn.Close();



                            }

                            documentoSAP.Lines.TaxOnly = BoYesNoEnum.tNO;



                            documentoSAP.Lines.UnitPrice = Convert.ToDouble(item.PrecioUnitario);

                            documentoSAP.Lines.WarehouseCode = item.CodBodega;
                            if(!string.IsNullOrEmpty(ParametrosFacturacion.Norma))
                            {
                                switch (ParametrosFacturacion.Dimension)
                                {
                                    case 1:
                                        {
                                            documentoSAP.Lines.CostingCode = ParametrosFacturacion.Norma;

                                            break;
                                        }
                                    case 2:
                                        {
                                            documentoSAP.Lines.CostingCode2 = ParametrosFacturacion.Norma;
                                            break;
                                        }
                                    case 3:
                                        {
                                            documentoSAP.Lines.CostingCode3 = ParametrosFacturacion.Norma;
                                            break;
                                        }
                                    case 4:
                                        {
                                            documentoSAP.Lines.CostingCode4 = ParametrosFacturacion.Norma;
                                            break;
                                        }
                                    case 5:
                                        {
                                            documentoSAP.Lines.CostingCode5 = ParametrosFacturacion.Norma;
                                            break;
                                        }

                                }
                            }
                           

                            try
                            {
                                if(Factura.idEntrega > 0)
                                {
                                    Documents delivery = (Documents)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDeliveryNotes);
                                    if (delivery.GetByKey(EncMovimiento.DocEntry))
                                    {
                                        for (int j = 0; j < delivery.Lines.Count; j++)
                                        {
                                            delivery.Lines.SetCurrentLine(j);

                                            if (item.ItemCode == delivery.Lines.ItemCode) // Match criteria can be adjusted
                                            {
                                                documentoSAP.Lines.BaseEntry = delivery.DocEntry;
                                                documentoSAP.Lines.BaseType = (int)BoObjectTypes.oDeliveryNotes;
                                                documentoSAP.Lines.BaseLine = delivery.Lines.LineNum;
                                                break;
                                            }
                                        }
                                    }
                                }
                                // Get the delivery
                                
                            }
                            catch (Exception ex)
                            {
                                BitacoraErrores be = new BitacoraErrores();
                                be.Descripcion = ex.Message;

                                be.Fecha = DateTime.Now;

                                db.BitacoraErrores.Add(be);
                                db.SaveChanges();

                            }
                            documentoSAP.Lines.Add();
                            z++;
                        }






                        var respuesta = documentoSAP.Add();
                        if (respuesta == 0) //se creo exitorsamente 
                        {
                            var idEntry = 0;

                            try
                            {
                                idEntry = Convert.ToInt32(Conexion.Company.GetNewObjectKey());
                                throw new Exception("");
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    var Parametros = db.Parametros.FirstOrDefault();
                                    var conexion3 = g.DevuelveCadena(db);
                                    var valorAFiltrar = "Boletaje FAC: " + Factura.id.ToString();
                                    var filtroSQL = "NumAtCard like '%" + valorAFiltrar + "%' order by DocEntry desc";
                                    var SQL = Parametros.SQLDocEntryDocs.Replace("@CampoBuscar", "DocEntry").Replace("@Tabla", "OINV").Replace("@CampoWhere = @reemplazo", filtroSQL);

                                    SqlConnection Cn = new SqlConnection(conexion3);
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


                            db.Entry(Factura).State = EntityState.Modified;
                            Factura.DocEntry = idEntry.ToString();
                            Factura.ProcesadoSAP = true;
                            Factura.FechaProcesado = DateTime.Now;
                            db.SaveChanges();

                            if(Factura.idEntrega > 0)
                            {
                                var count = -1;
                                try
                                {
                                    var client2 = (ServiceCalls)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oServiceCalls);

                                    if (client2.GetByKey(Convert.ToInt32(Factura.NumLlamada)))
                                    {
                                        var idLlamada = Convert.ToInt32(EncMovimiento.NumLlamada);
                                        var Llamada = db.LlamadasServicios.Where(a => a.DocEntry == idLlamada).FirstOrDefault();
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

                                        client2.Expenses.DocumentType = BoSvcEpxDocTypes.edt_Invoice;

                                        client2.Expenses.DocumentNumber = Convert.ToInt32(Factura.DocEntry);
                                        client2.Expenses.DocEntry = Convert.ToInt32(Factura.DocEntry);
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
                                            be.StackTrace = "Insercion de Liga FAC - LL " + client2.DocNum;
                                            be.Fecha = DateTime.Now;

                                            db.BitacoraErrores.Add(be);
                                            db.SaveChanges();
                                            Conexion.Desconectar();


                                        }
                                    }
                                }
                                catch (Exception)
                                {


                                }
                            }
                        

                            //Procesamos el pago
                            var CondicionPago = db.CondicionesPagos.Where(a => a.id == Factura.idCondicionVenta).FirstOrDefault() == null ? db.CondicionesPagos.FirstOrDefault() : db.CondicionesPagos.Where(a => a.id == Factura.idCondicionVenta).FirstOrDefault();
                            //Procesamos el pago
                            if (CondicionPago.Dias == 0)
                            {

                                try
                                {


                                    var Fecha = Factura.Fecha.Date;
                                    var TipoCambio = Factura.TipoCambio;
                                    var MetodosPagos = db.MetodosPagosFacturas.Where(a => a.idEncabezado == Factura.id && a.Monto > 0).FirstOrDefault() == null ? new List<MetodosPagosFacturas>() : db.MetodosPagosFacturas.Where(a => a.idEncabezado == Factura.id && a.Monto > 0).ToList();

                                    var MetodosPagosColones = MetodosPagos.Where(a => a.Moneda == "COL").ToList();
                                    var MetodosPagosDolares = MetodosPagos.Where(a => a.Moneda == "USD").ToList();

                                    bool pagoColonesProcesado = false;
                                    bool pagoDolaresProcesado = false;


                                    var contador = 0;




                                    if (MetodosPagosColones.Count() > 0)
                                    {
                                        try
                                        {


                                            var pagoProcesado = (SAPbobsCOM.Payments)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);
                                            pagoProcesado.DocType = BoRcptTypes.rCustomer;
                                            pagoProcesado.CardCode = Factura.CardCode;
                                            pagoProcesado.DocDate = Factura.Fecha;
                                            pagoProcesado.DueDate = DateTime.Now;
                                            pagoProcesado.TaxDate = DateTime.Now;
                                            pagoProcesado.VatDate = DateTime.Now;
                                            pagoProcesado.Remarks = "Pago procesado por Boletaje";
                                            pagoProcesado.CounterReference = "APP FAC" + Factura.id;
                                            pagoProcesado.DocCurrency = ParametrosFacturacion.MonedaSAPColones;
                                            pagoProcesado.HandWritten = BoYesNoEnum.tNO;
                                            pagoProcesado.Invoices.InvoiceType = BoRcptInvTypes.it_Invoice;
                                            pagoProcesado.Invoices.DocEntry = Convert.ToInt32(Factura.DocEntry);

                                            if (Factura.Moneda != "COL")
                                            {
                                                var SumatoriaPagoColones = MetodosPagosColones.Sum(a => a.Monto) / TipoCambio;
                                                pagoProcesado.Invoices.AppliedFC = Convert.ToDouble(SumatoriaPagoColones);
                                            }
                                            else
                                            {
                                                var SumatoriaPagoColones = MetodosPagosColones.Sum(a => a.Monto);

                                                pagoProcesado.Invoices.SumApplied = Convert.ToDouble(SumatoriaPagoColones);

                                            }
                                            pagoProcesado.Series = ParametrosFacturacion.SeriePago;//154; 161;


                                            var SumatoriaEfectivo = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "Efectivo".ToUpper()).Sum(a => a.Monto);
                                            var PagosTarjetas = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "Tarjeta".ToUpper()).ToList(); //.Sum(a => a.Monto);
                                            var SumatoriaTransferencia = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "Transferencia".ToUpper()).Sum(a => a.Monto);

                                            if (SumatoriaEfectivo > 0)
                                            {
                                                var idcuenta = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "efectivo".ToUpper()).FirstOrDefault().idCuentaBancaria;
                                                var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;


                                                pagoProcesado.CashAccount = Cuenta;
                                                pagoProcesado.CashSum = Convert.ToDouble(SumatoriaEfectivo);

                                            }


                                            foreach (var item in PagosTarjetas)
                                            {

                                                if (item.Monto > 0)
                                                {
                                                    var idcuenta = item.idCuentaBancaria;
                                                    var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;
                                                    if (contador > 0)
                                                    {
                                                        pagoProcesado.CreditCards.Add();
                                                    }
                                                    else
                                                    {
                                                        pagoProcesado.CreditCards.SetCurrentLine(contador);

                                                    }
                                                    pagoProcesado.CreditCards.CardValidUntil = new DateTime(Factura.Fecha.Year, Factura.Fecha.Month, 28); //Fecha en la que se mete el pago 
                                                    pagoProcesado.CreditCards.CreditCard = 1;
                                                    pagoProcesado.CreditCards.CreditType = BoRcptCredTypes.cr_Regular;
                                                    pagoProcesado.CreditCards.PaymentMethodCode = 1; //Quemado
                                                    pagoProcesado.CreditCards.CreditCardNumber = item.BIN; // Ultimos 4 digitos
                                                    pagoProcesado.CreditCards.VoucherNum = item.NumReferencia;// 
                                                    pagoProcesado.CreditCards.CreditAcct = Cuenta;
                                                    pagoProcesado.CreditCards.CreditSum = Convert.ToDouble(item.Monto);
                                                    contador++;

                                                }
                                            }


                                            if (SumatoriaTransferencia > 0)
                                            {
                                                var idcuenta = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "transferencia".ToUpper()).FirstOrDefault().idCuentaBancaria;
                                                var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;

                                                pagoProcesado.TransferAccount = Cuenta;
                                                pagoProcesado.TransferDate = DateTime.Now; //Fecha en la que se mete el pago 
                                                pagoProcesado.TransferReference = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "transferencia".ToUpper()).FirstOrDefault().NumReferencia;
                                                pagoProcesado.TransferSum = Convert.ToDouble(SumatoriaTransferencia);
                                            }

                                            var respuestaPago = pagoProcesado.Add();
                                            if (respuestaPago == 0)
                                            {
                                                pagoColonesProcesado = true;

                                            }
                                            else
                                            {
                                                var error = "Hubo un error en el pago de la factura #" + Factura.id + " -> " + Conexion.Company.GetLastErrorDescription();
                                                BitacoraErrores be = new BitacoraErrores();
                                                be.Descripcion = error;

                                                be.Fecha = DateTime.Now;
                                                db.BitacoraErrores.Add(be);
                                                db.SaveChanges();
                                                Conexion.Desconectar();

                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                            BitacoraErrores be = new BitacoraErrores();
                                            be.Descripcion = ex.Message;
                                            be.Fecha = DateTime.Now;
                                            db.BitacoraErrores.Add(be);
                                            db.SaveChanges();
                                            Conexion.Desconectar();
                                        }
                                    }
                                    else
                                    {
                                        pagoColonesProcesado = true;

                                    }


                                    if (MetodosPagosDolares.Count() > 0)
                                    {
                                        try
                                        {


                                            var pagoProcesado = (SAPbobsCOM.Payments)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);
                                            pagoProcesado.DocType = BoRcptTypes.rCustomer;
                                            pagoProcesado.CardCode = Factura.CardCode;
                                            pagoProcesado.DocDate = Factura.Fecha;
                                            pagoProcesado.DueDate = DateTime.Now;
                                            pagoProcesado.TaxDate = DateTime.Now;
                                            pagoProcesado.VatDate = DateTime.Now;
                                            pagoProcesado.Remarks = "Pago procesado por Boletaje";
                                            pagoProcesado.CounterReference = "APP FAC" + Factura.id;
                                            pagoProcesado.DocCurrency = "USD";
                                            pagoProcesado.HandWritten = BoYesNoEnum.tNO;
                                            pagoProcesado.Invoices.InvoiceType = BoRcptInvTypes.it_Invoice;
                                            pagoProcesado.Invoices.DocEntry = Convert.ToInt32(Factura.DocEntry);


                                            var SumatoriaPagod = MetodosPagosDolares.Sum(a => a.Monto);
                                            pagoProcesado.Invoices.AppliedFC = Convert.ToDouble(SumatoriaPagod);
                                            pagoProcesado.Series = ParametrosFacturacion.SeriePago;//154; 161;


                                            var SumatoriaEfectivo = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "Efectivo".ToUpper()).Sum(a => a.Monto);
                                            var PagosTarjetas = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "Tarjeta".ToUpper()).ToList();//.Sum(a => a.Monto);
                                            var SumatoriaTransferencia = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "Transferencia".ToUpper()).Sum(a => a.Monto);

                                            if (SumatoriaEfectivo > 0)
                                            {
                                                var idcuenta = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "efectivo".ToUpper()).FirstOrDefault().idCuentaBancaria;
                                                var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;


                                                pagoProcesado.CashAccount = Cuenta;
                                                pagoProcesado.CashSum = Convert.ToDouble(SumatoriaEfectivo);

                                            }


                                            foreach (var item in PagosTarjetas)
                                            {

                                                if (item.Monto > 0)
                                                {
                                                    var idcuenta = item.idCuentaBancaria;
                                                    var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;
                                                    if (contador > 0)
                                                    {
                                                        pagoProcesado.CreditCards.Add();
                                                    }
                                                    else
                                                    {
                                                        pagoProcesado.CreditCards.SetCurrentLine(contador);

                                                    }
                                                    pagoProcesado.CreditCards.CardValidUntil = new DateTime(Factura.Fecha.Year, Factura.Fecha.Month, 28); //Fecha en la que se mete el pago 
                                                    pagoProcesado.CreditCards.CreditCard = 1;
                                                    pagoProcesado.CreditCards.CreditType = BoRcptCredTypes.cr_Regular;
                                                    pagoProcesado.CreditCards.PaymentMethodCode = 1; //Quemado
                                                    pagoProcesado.CreditCards.CreditCardNumber = item.BIN; // Ultimos 4 digitos
                                                    pagoProcesado.CreditCards.VoucherNum = item.NumReferencia;// 
                                                    pagoProcesado.CreditCards.CreditAcct = Cuenta;
                                                    pagoProcesado.CreditCards.CreditSum = Convert.ToDouble(item.Monto);

                                                    contador++;
                                                }
                                            }



                                            if (SumatoriaTransferencia > 0)
                                            {
                                                var idcuenta = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "transferencia".ToUpper()).FirstOrDefault().idCuentaBancaria;
                                                var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;


                                                pagoProcesado.TransferAccount = Cuenta;
                                                pagoProcesado.TransferDate = DateTime.Now; //Fecha en la que se mete el pago 
                                                pagoProcesado.TransferReference = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "transferencia".ToUpper()).FirstOrDefault().NumReferencia;
                                                pagoProcesado.TransferSum = Convert.ToDouble(SumatoriaTransferencia);
                                            }
                                            var respuestaPago = pagoProcesado.Add();
                                            if (respuestaPago == 0)
                                            {
                                                pagoDolaresProcesado = true;

                                            }
                                            else
                                            {
                                                var error = "Hubo un error en el pago de la factura # " + Factura.id + " -> " + Conexion.Company.GetLastErrorDescription();
                                                BitacoraErrores be = new BitacoraErrores();
                                                be.Descripcion = error;
                                                be.Fecha = DateTime.Now;
                                                db.BitacoraErrores.Add(be);
                                                db.SaveChanges();
                                                Conexion.Desconectar();

                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                            BitacoraErrores be = new BitacoraErrores();
                                            be.Descripcion = ex.Message;
                                            be.Fecha = DateTime.Now;
                                            db.BitacoraErrores.Add(be);
                                            db.SaveChanges();
                                            Conexion.Desconectar();
                                        }
                                    }
                                    else
                                    {
                                        pagoDolaresProcesado = true;

                                    }

                                    if (pagoColonesProcesado && pagoDolaresProcesado)
                                    {
                                        db.Entry(Factura).State = EntityState.Modified;
                                        Factura.DocEntryPago = Conexion.Company.GetNewObjectKey().ToString();
                                        Factura.ProcesadoSAPPago = true;
                                        Factura.FechaProcesadoPago = DateTime.Now;
                                        db.SaveChanges();
                                    }


                                }
                                catch (Exception ex)
                                {

                                    BitacoraErrores be = new BitacoraErrores();
                                    be.Descripcion = ex.Message;
                                    be.Fecha = DateTime.Now;
                                    db.BitacoraErrores.Add(be);
                                    db.SaveChanges();
                                }



                            }



                            Conexion.Desconectar();

                        }
                        else
                        {
                            var error = "hubo un error " + Conexion.Company.GetLastErrorDescription();
                            BitacoraErrores be = new BitacoraErrores();
                            be.Descripcion = error;

                            be.Fecha = DateTime.Now;
                            be.StackTrace = "Error al mandar a SAP";
                            db.BitacoraErrores.Add(be);
                            db.SaveChanges();
                            Conexion.Desconectar();


                        }
                    }
                    catch (Exception ex)
                    {
                        BitacoraErrores be = new BitacoraErrores();
                        be.Descripcion = ex.Message;

                        be.Fecha = DateTime.Now;
                        be.StackTrace = ex.StackTrace;
                        db.BitacoraErrores.Add(be);
                        db.SaveChanges();
                        Conexion.Desconectar();


                    }

                    //


                }
                else
                {
                    throw new Exception("Esta factura YA existe");
                }


                return Request.CreateResponse(HttpStatusCode.OK, Factura);
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