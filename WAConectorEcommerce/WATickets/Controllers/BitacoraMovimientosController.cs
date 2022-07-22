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

    public class BitacoraMovimientosController: ApiController
    {
        ModelCliente db = new ModelCliente();
        //Este api es el encargado de llevar el control de los movimientos en cuanto a traslados
        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {
                var time = new DateTime();
                
                if(filtro.FechaFinal != time)
                {
                    filtro.FechaFinal = filtro.FechaFinal.AddDays(1);
                }

                var Bitacora = db.BitacoraMovimientos.Select(a => new {


                    a.id ,
                    idLlamada = db.LlamadasServicios.Where(b => b.id == a.idLlamada).FirstOrDefault() == null ? a.idLlamada : db.LlamadasServicios.Where(b => b.id == a.idLlamada).FirstOrDefault().DocEntry,
                    a.idTecnico,
                    a.idEncabezado ,
                    a.DocEntry ,
                    a.Fecha ,
                    a.TipoMovimiento ,
                    a.BodegaInicial ,
                    a.BodegaFinal ,
                    a.Status ,
                    a.ProcesadaSAP ,
                    Detalle = db.DetBitacoraMovimientos.Where(b => b.idEncabezado == a.id).ToList()

                }).Where(a => (filtro.FechaInicial != time ? a.Fecha >= filtro.FechaInicial : true) && (filtro.FechaFinal != time ? a.Fecha <= filtro.FechaFinal : true)).ToList();

                if(filtro.Codigo1 > 0)
                {
                    Bitacora = Bitacora.Where(a => a.idEncabezado == filtro.Codigo1).ToList();
                }
                else if (filtro.Codigo3 >= 0)
                {
                    string status = filtro.Codigo3.ToString();
                    Bitacora = Bitacora.Where(a => a.Status == status).ToList();
                }


                if (filtro.Codigo2 > 0)
                {
                    Bitacora = Bitacora.Where(a => a.idTecnico == filtro.Codigo2).ToList();
                }

               

                return Request.CreateResponse(HttpStatusCode.OK, Bitacora);

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


        [Route("api/BitacoraMovimientos/Consultar")]
        public HttpResponseMessage GetOne([FromUri]int id)
        {
            try
            {



                var Bitacora = db.BitacoraMovimientos.Select(a => new {


                    a.id,
                    idLlamada = db.LlamadasServicios.Where(b => b.id == a.idLlamada).FirstOrDefault() == null ? a.idLlamada : db.LlamadasServicios.Where(b => b.id == a.idLlamada).FirstOrDefault().DocEntry,
                    a.idTecnico,
                    a.idEncabezado,
                    a.DocEntry,
                    a.Fecha,
                    a.TipoMovimiento,
                    a.BodegaInicial,
                    a.BodegaFinal,
                    a.Status,
                    a.ProcesadaSAP,
                    Detalle = db.DetBitacoraMovimientos.Where(b => b.idEncabezado == a.id).ToList()



                }).Where(a => a.id == id).FirstOrDefault();


                if (Bitacora == null)
                {
                    throw new Exception("Este movimiento no se encuentra registrado");
                }

                return Request.CreateResponse(HttpStatusCode.OK, Bitacora);
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
        public HttpResponseMessage Post([FromBody] BitacoraMovimientos bts)
        {
            try
            {
                var BT = db.BitacoraMovimientos.Where(a => a.id == bts.id).FirstOrDefault();

                if(BT != null)
                {
                    db.Entry(BT).State = EntityState.Modified;
                    BT.Status = bts.Status;
                    db.SaveChanges();

                    if(BT.Status == "1" && !BT.ProcesadaSAP)
                    {
                        try
                        {
                            var Parametro = db.Parametros.FirstOrDefault();
                            var Encabezado = db.EncReparacion.Where(a => a.id == BT.idEncabezado).FirstOrDefault();
                            var Llamada = db.LlamadasServicios.Where(a => a.id == Encabezado.idLlamada).FirstOrDefault();
                            var client = (StockTransfer)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer);
                            client.DocDate = DateTime.Now;
                            if (BT.TipoMovimiento == 1)
                            {
                                client.FromWarehouse = Parametro.BodegaInicial; 
                                client.ToWarehouse = Parametro.BodegaFinal;
                                 
                            }
                            else if (BT.TipoMovimiento == 2)
                            {
                                client.FromWarehouse = Parametro.BodegaFinal; 
                                client.ToWarehouse = Parametro.BodegaInicial;
                            }

                            client.CardCode = Llamada.CardCode;
                            client.Comments = Encabezado.Comentarios;
                            client.UserFields.Fields.Item("U_TiendaDest").Value = BT.TipoMovimiento == 1 ? BT.BodegaInicial : BT.BodegaFinal;
                            client.UserFields.Fields.Item("U_DYD_Boleta").Value = db.LlamadasServicios.Where(a => a.id == Encabezado.idLlamada).FirstOrDefault() == null ? Encabezado.idLlamada.ToString() : db.LlamadasServicios.Where(a => a.id == Encabezado.idLlamada).FirstOrDefault().DocEntry.ToString();

                            client.JournalMemo = "Traslados - " + Llamada.CardCode;

                            var i = 0;
                            var Det = db.DetBitacoraMovimientos.Where(a => a.idEncabezado == BT.id).ToList();
                            foreach (var item in Det)
                            {
                                client.Lines.ItemCode = item.ItemCode.Split('|')[0].Trim();
                                client.Lines.Quantity = item.Cantidad;
                                client.Lines.Add();
                                i++;
                            }

                            var respuesta = client.Add();

                            if (respuesta == 0)
                            {
                                //Ligar el traslado a la llamada de servicio
                                var idEntry = Convert.ToInt32(Conexion.Company.GetNewObjectKey());
                                var count = -1;
                                var client2 = (ServiceCalls)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oServiceCalls);
                                if (client2.GetByKey(Llamada.DocEntry.Value))
                                {
                                    count = db.BitacoraMovimientos.Where(a => a.idLlamada == Encabezado.idLlamada && a.ProcesadaSAP == true).Count();



                                    if (count > 0)
                                    {
                                        client2.Expenses.Add();
                                    }

                                    client2.Expenses.DocumentType = BoSvcEpxDocTypes.edt_StockTransfer;
                                    client2.Expenses.DocumentNumber = idEntry;

                                    client2.Expenses.DocEntry = idEntry;

                                    if (count == 0)
                                    {
                                        client2.Expenses.Add();
                                    }

                                    var respuesta2 = client2.Update();
                                    if (respuesta2 == 0)
                                    {
                                        db.Entry(BT).State = EntityState.Modified;
                                        BT.DocEntry = idEntry;
                                        BT.ProcesadaSAP = true;
                                        db.SaveChanges();

                                        db.Entry(Encabezado).State = EntityState.Modified;
                                        Encabezado.TipoReparacion = 0;
                                        Encabezado.BodegaOrigen = "0";
                                        Encabezado.BodegaFinal = "0";
                                        
                                        db.SaveChanges();

                                        Conexion.Desconectar();
                                    }
                                    else
                                    {
                                        BitacoraErrores be = new BitacoraErrores();
                                        be.DocNum = idEntry.ToString();
                                        be.Descripcion = count + "- " + Conexion.Company.GetLastErrorDescription();
                                        be.StackTrace = "Error al ligar el traslado con la llamada";
                                        be.Fecha = DateTime.Now;

                                        db.BitacoraErrores.Add(be);
                                        db.SaveChanges();
                                        Conexion.Desconectar();


                                    }
                                }
                                Conexion.Desconectar();
                            }
                            else
                            {
                                BitacoraErrores be = new BitacoraErrores();
                                be.DocNum = Encabezado.idLlamada.ToString();
                                be.Descripcion = Conexion.Company.GetLastErrorDescription();
                                be.StackTrace = "Error al hacer el traslado";
                                be.Fecha = DateTime.Now;

                                db.BitacoraErrores.Add(be);
                                db.SaveChanges();
                                Conexion.Desconectar();

                            }
                        }
                        catch (Exception ex1)
                        {

                            BitacoraErrores be = new BitacoraErrores();

                            be.Descripcion = ex1.Message;
                            be.StackTrace = ex1.StackTrace;
                            be.Fecha = DateTime.Now;

                            db.BitacoraErrores.Add(be);
                            db.SaveChanges();

                        }
                    }



                    return Request.CreateResponse(HttpStatusCode.OK, BT);

                }
                else
                {
                    throw new Exception("Este movimiento no se encuentra registrado");

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

    }
}