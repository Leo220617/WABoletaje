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

namespace WATickets.Controllers
{
    [Authorize]

    public class BitacoraMovimientosController: ApiController
    {
        ModelCliente db = new ModelCliente();
        G g = new G();
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
                if (string.IsNullOrEmpty(filtro.CardCode))
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
                        StatusLlamada = db.LlamadasServicios.Where(b => b.id == a.idLlamada).FirstOrDefault() == null ? 0 : db.LlamadasServicios.Where(b => b.id == a.idLlamada).FirstOrDefault().Status == null ? 0 : db.LlamadasServicios.Where(b => b.id == a.idLlamada).FirstOrDefault().Status,
                        Detalle = db.DetBitacoraMovimientos.Where(b => b.idEncabezado == a.id).ToList()

                    }).Where(a => (filtro.FechaInicial != time ? a.Fecha >= filtro.FechaInicial : true) && (filtro.FechaFinal != time ? a.Fecha <= filtro.FechaFinal : true)).ToList();

                    if (filtro.Codigo1 > 0)
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
                    if (!string.IsNullOrEmpty(filtro.Texto))
                    {
                        var valores = filtro.Texto.Split('|');
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
                            //if (filtro.FechaInicial != time)
                            //{
                            //    llamadasQuery = llamadasQuery.Where(a => a.FechaCreacion >= filtro.FechaInicial);
                            //}
                            //if (filtro.FechaFinal != time)
                            //{
                            //    llamadasQuery = llamadasQuery.Where(a => a.FechaCreacion <= filtro.FechaFinal);
                            //}

                            // Remover reparaciones con idLlamada == 0 en una sola pasada
                            Bitacora.RemoveAll(a => a.idLlamada == 0);

                            // Remover reparaciones cuyas llamadas coinciden con las llamadas filtradas
                            Bitacora.RemoveAll(a => llamadas.Contains(a.idLlamada));
                        }


                    }
                  


                    return Request.CreateResponse(HttpStatusCode.OK, Bitacora);
                }
                else
                {
                    var DocEntry = 0;
                    try
                    {
                        DocEntry = Convert.ToInt32(filtro.CardCode);
                    }
                    catch (Exception)
                    {

                    }
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
                        StatusLlamada = db.LlamadasServicios.Where(b => b.id == a.idLlamada).FirstOrDefault() == null ? 0 : db.LlamadasServicios.Where(b => b.id == a.idLlamada).FirstOrDefault().Status == null ? 0 : db.LlamadasServicios.Where(b => b.id == a.idLlamada).FirstOrDefault().Status,
                        Detalle = db.DetBitacoraMovimientos.Where(b => b.idEncabezado == a.id).ToList()

                    }).Where(a => (!string.IsNullOrEmpty(filtro.CardCode) ? a.idLlamada.Value == DocEntry : true) ).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, Bitacora);

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
        public HttpResponseMessage Post([FromBody] BitacoraMovimientosViewModel bts)
        {
            try
            {
                var errorSAP = "";
                var BT = db.BitacoraMovimientos.Where(a => a.id == bts.id).FirstOrDefault();

                if(BT != null)
                {
                    db.Entry(BT).State = EntityState.Modified;
                    BT.Status = bts.Status;
                    db.SaveChanges();

                    foreach (var item in bts.Detalle)
                    {
                        var DetBitacoraMovimiento = db.DetBitacoraMovimientos.Where(a => a.idEncabezado == BT.id && a.idProducto == item.idProducto && a.idError == item.idError).FirstOrDefault();
                        db.Entry(DetBitacoraMovimiento).State = EntityState.Modified;
                        DetBitacoraMovimiento.CantidadEnviar = item.CantidadEnviar;
                 //       DetBitacoraMovimiento.CantidadFaltante = DetBitacoraMovimiento.CantidadFaltante - DetBitacoraMovimiento.CantidadEnviar;
                        db.SaveChanges();

                    }

                    if(db.DetBitacoraMovimientos.Where(a => a.idEncabezado == BT.id && a.CantidadEnviar > 0).Count() > 0)
                   // if (BT.Status == "1" && !BT.ProcesadaSAP)
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
                            client.Comments = g.TruncarString( Encabezado.Comentarios,200);
                            client.UserFields.Fields.Item("U_TiendaDest").Value = BT.TipoMovimiento == 1 ? BT.BodegaInicial : BT.BodegaFinal;
                            client.UserFields.Fields.Item("U_DYD_Boleta").Value = db.LlamadasServicios.Where(a => a.id == Encabezado.idLlamada).FirstOrDefault() == null ? Encabezado.idLlamada.ToString() : db.LlamadasServicios.Where(a => a.id == Encabezado.idLlamada).FirstOrDefault().DocEntry.ToString();

                            client.JournalMemo = "Traslados - " + Llamada.CardCode + " - " + BT.id;

                            var i = 0;
                            var Det = db.DetBitacoraMovimientos.Where(a => a.idEncabezado == BT.id && a.CantidadEnviar > 0).ToList();
                            foreach (var item in Det)
                            {
                                client.Lines.ItemCode = item.ItemCode.Split('|')[0].Trim();
                                client.Lines.Quantity = Convert.ToDouble(item.CantidadEnviar);
                                client.Lines.Add();
                                i++;
                            }

                            var respuesta = client.Add();

                            if (respuesta == 0)
                            {
                                //Ligar el traslado a la llamada de servicio

                                var idEntry = 0;
                                try
                                {
                                     idEntry = Convert.ToInt32(Conexion.Company.GetNewObjectKey());
                                   // throw new Exception("");
                                }
                                catch (Exception ex)
                                {
                                    try
                                    {
                                        var Parametros = db.Parametros.FirstOrDefault();
                                        var conexion = g.DevuelveCadena(db); 
                                        var valorAFiltrar = "Traslados - " + Llamada.CardCode + " - " + BT.id;
                                        var filtroSQL = "JrnlMemo like '%" + valorAFiltrar + "%'";
                                        var SQL = Parametros.SQLDocEntryDocs.Replace("@CampoBuscar", "DocEntry").Replace("@Tabla", "OWTR").Replace("@CampoWhere = @reemplazo",filtroSQL);

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

                                        be.Descripcion = "Error en el traslado #" + BT.id + " , al conseguir el docEntry -> " + ex1.Message;
                                        be.StackTrace = ex1.StackTrace;
                                        be.Fecha = DateTime.Now;

                                        db.BitacoraErrores.Add(be);
                                        db.SaveChanges();

                                    }

                                }
                                


                                var count = -1;
                                var client2 = (ServiceCalls)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oServiceCalls);
                                if (client2.GetByKey(Llamada.DocEntry.Value))
                                {
                                   // count = db.BitacoraMovimientos.Where(a => a.idLlamada == Encabezado.idLlamada && a.ProcesadaSAP == true).Count();

                                    count = db.BitacoraMovimientosSAP.Where(a => a.idLlamada == Encabezado.idLlamada && a.ProcesadaSAP == true).Distinct().GroupBy(a => a.DocEntry).Count();

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
                                        if(BT.Status == "0")
                                        {
                                            BT.Status = "3";
                                        }
                                       // BT.ProcesadaSAP = true;
                                        db.SaveChanges();

                                        db.Entry(Encabezado).State = EntityState.Modified;
                                        Encabezado.TipoReparacion = 0;
                                        Encabezado.BodegaOrigen = "0";
                                        Encabezado.BodegaFinal = "0";
                                        
                                        db.SaveChanges();
                                        foreach (var item in bts.Detalle)
                                        {
                                            decimal cant = 0;
                                            var DetBitacoraMovimiento = db.DetBitacoraMovimientos.Where(a => a.idEncabezado == BT.id && a.idProducto == item.idProducto && a.idError == item.idError).FirstOrDefault();
                                            db.Entry(DetBitacoraMovimiento).State = EntityState.Modified;
                                            DetBitacoraMovimiento.CantidadFaltante = DetBitacoraMovimiento.CantidadFaltante - DetBitacoraMovimiento.CantidadEnviar;
                                            cant = DetBitacoraMovimiento.CantidadEnviar;
                                            DetBitacoraMovimiento.CantidadEnviar = 0;
                                            db.SaveChanges();

                                            BitacoraMovimientosSAP btSAP = new BitacoraMovimientosSAP();
                                            btSAP.idEncabezado = BT.id;
                                            btSAP.idDetalle = DetBitacoraMovimiento.id;
                                            btSAP.Cantidad = cant;
                                            btSAP.DocEntry = idEntry.ToString();
                                            btSAP.ProcesadaSAP = true;
                                            btSAP.idLlamada = Encabezado.idLlamada;
                                            db.BitacoraMovimientosSAP.Add(btSAP);
                                            db.SaveChanges();

                                        }
                                        Conexion.Desconectar();
                                    }
                                    else
                                    {
                                        db.Entry(BT).State = EntityState.Modified;
                                        BT.Status = "0"; 
                                        db.SaveChanges();
                                        errorSAP = Conexion.Company.GetLastErrorDescription();
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
                                db.Entry(BT).State = EntityState.Modified;
                                BT.Status = "0";
                                db.SaveChanges();
                                errorSAP = Conexion.Company.GetLastErrorDescription();
                                BitacoraErrores be = new BitacoraErrores();
                                be.DocNum = Encabezado.idLlamada.ToString();
                                be.Descripcion = Conexion.Company.GetLastErrorDescription();
                                be.StackTrace = "Error al hacer el traslado";
                                be.Fecha = DateTime.Now;

                                db.BitacoraErrores.Add(be);
                                db.SaveChanges();
                                Conexion.Desconectar();
                                throw new Exception("Error al generar el traslado en SAP " + be.Descripcion);
                            }
                        }
                        catch (Exception ex1)
                        {
                            db.Entry(BT).State = EntityState.Modified;
                            BT.Status = "0";
                            db.SaveChanges();
                            errorSAP = ex1.Message;
                            BitacoraErrores be = new BitacoraErrores();

                            be.Descripcion = ex1.Message;
                            be.StackTrace = ex1.StackTrace;
                            be.Fecha = DateTime.Now;

                            db.BitacoraErrores.Add(be);
                            db.SaveChanges();
                            throw new Exception("Error al generar el traslado " + be.Descripcion);

                        }
                    }

                    if((BT.Status == "0" && bts.Status != "0")) 
                    {
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, "Ha ocurrido un error al enviar a SAP ("+errorSAP+") , por lo tanto debe volver a intentarlo");

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