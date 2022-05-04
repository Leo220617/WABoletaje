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
    public class EncReparacionController : ApiController
    {
        ModelCliente db = new ModelCliente();

        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {
                var time = new DateTime();

                var EncReparacion = db.EncReparacion.Select(a => new
                {
                    a.id,
                    idLlamada = db.LlamadasServicios.Where(b => b.id == a.idLlamada).FirstOrDefault() == null ? 0 : db.LlamadasServicios.Where(b => b.id == a.idLlamada).FirstOrDefault().DocEntry == null ? 0 : db.LlamadasServicios.Where(b => b.id == a.idLlamada).FirstOrDefault().DocEntry,
                    a.idTecnico,
                    a.idDiagnostico,
                    a.FechaCreacion,
                    a.FechaModificacion,
                    a.TipoReparacion,
                    a.idProductoArreglar,
                    a.Status,
                    a.ProcesadaSAP,
                    a.Comentarios,
                    a.BodegaOrigen,
                    a.BodegaFinal,
                    Detalle = db.DetReparacion.Where(b => b.idEncabezado == a.id).ToList(),
                    Adjuntos = db.Adjuntos.Where(b => b.idEncabezado == a.id).ToList()
                })
                    
                    .Where(a => (filtro.FechaInicial != time ? a.FechaCreacion >= filtro.FechaInicial : true) && (filtro.FechaFinal != time ? a.FechaCreacion <= filtro.FechaFinal : true)).ToList();

                if (!string.IsNullOrEmpty(filtro.Texto))
                {
                    EncReparacion = EncReparacion.Where(a => a.idProductoArreglar.ToUpper().Contains(filtro.Texto.ToUpper())).ToList();
                }

                if(filtro.Codigo1 > 0 )
                {
                    EncReparacion = EncReparacion.Where(a => a.idTecnico == filtro.Codigo1).ToList();
                }

                if (filtro.Codigo2 > 0)
                {
                    filtro.Codigo2--;
                    EncReparacion = EncReparacion.Where(a => a.Status == filtro.Codigo2).ToList();
                }

                return Request.CreateResponse(HttpStatusCode.OK, EncReparacion);

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [Route("api/EncReparacion/Consultar")]
        public HttpResponseMessage GetOne([FromUri]int id)
        {
            try
            {



                var EncReparacion = db.EncReparacion.Select(a => new
                {
                    a.id,
                    idLlamada = db.LlamadasServicios.Where(b => b.id == a.idLlamada).FirstOrDefault() == null ? 0 : db.LlamadasServicios.Where(b => b.id == a.idLlamada).FirstOrDefault().DocEntry == null ? 0 : db.LlamadasServicios.Where(b => b.id == a.idLlamada).FirstOrDefault().DocEntry,

                    a.idTecnico,
                    a.FechaCreacion,
                    a.idDiagnostico,
                    a.FechaModificacion,
                    a.TipoReparacion,
                    a.idProductoArreglar,
                    a.Status,
                    a.ProcesadaSAP,
                    a.Comentarios,
                    a.BodegaOrigen,
                    a.BodegaFinal,
                    Detalle = db.DetReparacion.Where(b => b.idEncabezado == a.id).ToList(),
                    Adjuntos = db.Adjuntos.Where(b => b.idEncabezado == a.id).ToList()
                }).Where(a => a.id == id).FirstOrDefault();


                if (EncReparacion == null)
                {
                    throw new Exception("Este documento no se encuentra registrado");
                }

                return Request.CreateResponse(HttpStatusCode.OK, EncReparacion);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [Route("api/EncReparacion/PruebaTraslado")]
        public HttpResponseMessage GetOneTraslado([FromUri]int id)
        {
            try
            {



                var EncReparacion = db.EncReparacion.Select(a => new
                {
                    a.id,
                    a.idLlamada,
                    a.idTecnico,
                    a.FechaCreacion,
                    a.FechaModificacion,
                    a.TipoReparacion,
                    a.idProductoArreglar,
                    a.Status,
                    a.ProcesadaSAP,
                    a.Comentarios,
                    Detalle = db.DetReparacion.Where(b => b.idEncabezado == a.id).ToList()
                }).Where(a => a.id == id).FirstOrDefault();


                if (EncReparacion == null)
                {
                    throw new Exception("Este documento no se encuentra registrado");
                }


                try
                {
                    var Llamada = db.LlamadasServicios.Where(a => a.id == EncReparacion.idLlamada).FirstOrDefault();
                    var client = (StockTransfer)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer);
                    client.DocDate = DateTime.Now;
                    client.FromWarehouse = "07";
                    client.ToWarehouse = "99";
                    client.CardCode = Llamada.CardCode;
                    client.Comments = "PRUEBA - " + EncReparacion.Comentarios;
                    client.UserFields.Fields.Item("U_TiendaDest").Value = "23";
                    client.JournalMemo = "Traslados - " + Llamada.CardCode;
          
                    var i = 0;
                    foreach(var item in EncReparacion.Detalle)
                    {
                      //  client.Lines.SetCurrentLine(i);
                        client.Lines.ItemCode = item.ItemCode.Split('|')[0].Trim();
                         
                        client.Lines.Quantity = item.Cantidad;

                        




                        client.Lines.Add();
                        i++;
                    }

                    var respuesta = client.Add();

                    if (respuesta == 0)
                    {
                        var idEntry = Convert.ToInt32(Conexion.Company.GetNewObjectKey()); 
                        var client2 = (ServiceCalls)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oServiceCalls);
                        if (client2.GetByKey(Llamada.DocEntry.Value))
                        {
                             if(client2.Expenses.Count > 0)
                            {
                                client2.Expenses.Add();
                            }
                            client2.Expenses.DocumentType = BoSvcEpxDocTypes.edt_StockTransfer;
                            client2.Expenses.DocumentNumber = idEntry;
                            client2.Expenses.DocEntry = idEntry;

                            if(client2.Expenses.Count == 0)
                            {
                                client2.Expenses.Add();
                            }
                            client2.Expenses.Add();
                           var respuesta2 = client2.Update();
                            if(respuesta2 == 0)
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
                    }
                    else
                    {
                        BitacoraErrores be = new BitacoraErrores();

                        be.Descripcion = Conexion.Company.GetLastErrorDescription();
                        be.StackTrace = "Stock Transfer - Actualizar";
                        be.Fecha = DateTime.Now;

                        db.BitacoraErrores.Add(be);
                        db.SaveChanges();
                        Conexion.Desconectar();
                        throw new Exception(be.Descripcion);
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
                    throw new Exception(ex1.Message);
                }

                return Request.CreateResponse(HttpStatusCode.OK, EncReparacion);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }


        [HttpPost]
        public HttpResponseMessage Post([FromBody] ColeccionRepuestos coleccion)
        {
            try
            {


                var Parametro = db.Parametros.FirstOrDefault();
                var Encabezado = db.EncReparacion.Where(a => a.id == coleccion.EncReparacion.id).FirstOrDefault();
                db.Entry(Encabezado).State = EntityState.Modified;
                Encabezado.TipoReparacion = coleccion.EncReparacion.TipoReparacion;
                Encabezado.FechaModificacion = DateTime.Now;
                Encabezado.Status = coleccion.EncReparacion.Status;
                Encabezado.Comentarios = coleccion.EncReparacion.Comentarios;
                Encabezado.BodegaOrigen = coleccion.EncReparacion.BodegaOrigen;
                Encabezado.BodegaFinal = coleccion.EncReparacion.BodegaFinal;
                Encabezado.idDiagnostico = coleccion.EncReparacion.idDiagnostico;
                db.SaveChanges();




                var Detalles = db.DetReparacion.Where(a => a.idEncabezado == coleccion.EncReparacion.id).ToList();

                foreach (var item in Detalles)
                {
                    db.DetReparacion.Remove(item);
                    db.SaveChanges();
                }


                foreach (var item in coleccion.DetReparacion)
                {
                    DetReparacion det = new DetReparacion();
                    det.idEncabezado = item.idEncabezado;
                    det.idProducto = item.idProducto;
                    det.ItemCode = item.ItemCode;
                    det.Cantidad = item.Cantidad;
                    det.idError = item.idError;
                    db.DetReparacion.Add(det);
                    db.SaveChanges();
                }

                foreach(var item in coleccion.Adjuntos)
                {
                    Adjuntos adjunto = new Adjuntos();
                    adjunto.idEncabezado = Encabezado.id;

                    byte[] hex = Convert.FromBase64String(item.base64.Replace("data:image/jpeg;base64,", "").Replace("data:image/png;base64,", ""));
                    adjunto.base64 = hex;
                    db.Adjuntos.Add(adjunto);
                    db.SaveChanges();
                }

                if(Encabezado.TipoReparacion == 1 || Encabezado.TipoReparacion == 2)
                {
                    try
                    {
                        BitacoraMovimientos bts = new BitacoraMovimientos();
                        bts.idLlamada = Encabezado.idLlamada;
                        bts.idEncabezado = Encabezado.id;
                        bts.DocEntry = 0;
                        bts.Fecha = DateTime.Now;
                        bts.TipoMovimiento = Encabezado.TipoReparacion;

                        if (Encabezado.TipoReparacion == 1)
                        {
                            Encabezado.BodegaOrigen = Parametro.BodegaInicial;
                            Encabezado.BodegaFinal = Parametro.BodegaFinal;
                            bts.BodegaInicial = Encabezado.BodegaOrigen;
                            bts.BodegaFinal = Encabezado.BodegaFinal;
                        }
                        else if (Encabezado.TipoReparacion == 2)
                        {
                            Encabezado.BodegaOrigen = Parametro.BodegaFinal;
                            Encabezado.BodegaFinal = Parametro.BodegaInicial;
                            bts.BodegaInicial = Encabezado.BodegaOrigen;
                            bts.BodegaFinal = Encabezado.BodegaFinal;
                        }

                        bts.idTecnico = Encabezado.idTecnico;
                        bts.Status = "0";
                        bts.ProcesadaSAP = false;
                        db.BitacoraMovimientos.Add(bts);
                        db.SaveChanges();

                        foreach (var item in coleccion.DetReparacion)
                        {
                           

                            DetBitacoraMovimientos dbt = new DetBitacoraMovimientos();
                            dbt.idEncabezado = bts.id;
                            dbt.idProducto = item.idProducto;
                            dbt.Cantidad = item.Cantidad;
                            dbt.ItemCode = item.ItemCode;
                            db.DetBitacoraMovimientos.Add(dbt);
                            db.SaveChanges();
                        }


                        db.Entry(Encabezado).State = EntityState.Modified;
                        Encabezado.TipoReparacion = 0;
                        Encabezado.BodegaOrigen = "0";
                        Encabezado.BodegaFinal = "0";
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {

                        BitacoraErrores be = new BitacoraErrores();
                        be.DocNum = Encabezado.id.ToString();
                        be.Descripcion = ex.Message;
                        be.StackTrace = ex.StackTrace;
                        be.Fecha = DateTime.Now;
                        
                        db.BitacoraErrores.Add(be);
                        db.SaveChanges();


                    }
                }



                return Request.CreateResponse(HttpStatusCode.OK, coleccion);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}