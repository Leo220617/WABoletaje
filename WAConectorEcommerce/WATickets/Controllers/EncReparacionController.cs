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
    //Este api se encarga de llevar el control de las reparaciones realizadas al producto
    [Authorize]
    public class EncReparacionController : ApiController
    {
        ModelCliente db = new ModelCliente();

        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {
                var time = new DateTime();
                if (filtro.FechaFinal != time)
                {
                    filtro.FechaFinal = filtro.FechaFinal.AddDays(1);
                }

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
                //Si me estan filtrando por Status de la llamada
                if(filtro.Codigo3 != 0)
                {
                    var Llamadas = db.LlamadasServicios.Where(a => (filtro.FechaInicial != time ? a.FechaCreacion >= filtro.FechaInicial : true) && (filtro.FechaFinal != time ? a.FechaCreacion <= filtro.FechaFinal : true) && a.Status != filtro.Codigo3 ).ToList();
                    var ListadoReparacionesEnCero = EncReparacion.Where(a => a.idLlamada == 0).ToList() ;

                    foreach(var item in ListadoReparacionesEnCero)
                    {
                        EncReparacion.Remove(item);

                    }


                    foreach (var item in Llamadas)
                    {
                        
                        var EncReparacionSacar = EncReparacion.Where(a => a.idLlamada == item.DocEntry ).FirstOrDefault();
                        if(EncReparacionSacar != null)
                        {
                            EncReparacion.Remove(EncReparacionSacar);
                        }
                    }

                }

                return Request.CreateResponse(HttpStatusCode.OK, EncReparacion);

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
                BitacoraErrores be = new BitacoraErrores();

                be.Descripcion = ex.Message;
                be.StackTrace = ex.StackTrace;
                be.Fecha = DateTime.Now;
                db.BitacoraErrores.Add(be);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
        //Este es una prueba para realizar un traslado no usar
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
                         
                        client.Lines.Quantity = Convert.ToDouble(item.Cantidad);

                        




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

                G g = new G();
                var Parametro = db.Parametros.FirstOrDefault();
                var Encabezado = db.EncReparacion.Where(a => a.id == coleccion.EncReparacion.id).FirstOrDefault();
                var Llamada = db.LlamadasServicios.Where(a => a.id == Encabezado.idLlamada).FirstOrDefault();
                db.Entry(Encabezado).State = EntityState.Modified;
                Encabezado.TipoReparacion = coleccion.EncReparacion.TipoReparacion;
                Encabezado.FechaModificacion = DateTime.Now;
                Encabezado.Status = coleccion.EncReparacion.Status;
                Encabezado.Comentarios = coleccion.EncReparacion.Comentarios;
                Encabezado.BodegaOrigen = coleccion.EncReparacion.BodegaOrigen;
                Encabezado.BodegaFinal = coleccion.EncReparacion.BodegaFinal;
                Encabezado.idDiagnostico = coleccion.EncReparacion.idDiagnostico;
                db.SaveChanges();

                //Para generar una entrega
                if(Encabezado.Status == 2)
                {
                    try
                    {
                        var Existe = true;//db.BackOffice.Where(a => a.idEncabezadoReparacion == Encabezado.id && a.TipoMovimiento == 2).FirstOrDefault() == null;

                        if (Existe)
                        {

                            BackOffice backOffice = new BackOffice();
                            backOffice.idEncabezadoReparacion = Encabezado.id;
                            backOffice.idLlamada = Encabezado.idLlamada;
                            backOffice.TipoMovimiento = 2;
                            backOffice.Fecha = DateTime.Now;
                            db.BackOffice.Add(backOffice);
                            db.SaveChanges();

                            EncMovimiento encMovimiento = new EncMovimiento();
                            encMovimiento.CardCode = Llamada.CardCode;

                            var conexion = g.DevuelveCadena(db);

                            var SQL = Parametro.HtmlLlamada + "'" + Llamada.CardCode + "'";

                            SqlConnection Cn = new SqlConnection(conexion);
                            SqlCommand Cmd = new SqlCommand(SQL, Cn);
                            SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                            DataSet Ds = new DataSet();
                            Cn.Open();
                            Da.Fill(Ds, "Encabezado");
                            encMovimiento.CardName = Ds.Tables["Encabezado"].Rows[0]["CardName"].ToString();
                            Cn.Close();
                            Cn.Dispose();
                            encMovimiento.NumLlamada = Llamada.DocEntry.Value.ToString();
                            encMovimiento.Fecha = DateTime.Now;
                            encMovimiento.TipoMovimiento = 2;
                            encMovimiento.Comentarios = Encabezado.Comentarios;
                            encMovimiento.DocEntry = 0;
                            encMovimiento.CreadoPor = "0";
                            encMovimiento.Subtotal = 0;
                            encMovimiento.PorDescuento = 0;
                            encMovimiento.Descuento = 0;
                            encMovimiento.Impuestos = 0;
                            encMovimiento.TotalComprobante = 0;
                            db.EncMovimiento.Add(encMovimiento);
                            db.SaveChanges();


                            var Bitacoras = db.BitacoraMovimientos.Where(a => a.idEncabezado == Encabezado.id && a.ProcesadaSAP == true).ToList(); //Hago el llamado de las bitacoras de movimiento que tengan el id del encabezado de repracion
                            //Separamos las entradas de las salidas
                            var bitacorasEntradas = Bitacoras.Where(a => a.TipoMovimiento == 1).ToList(); 
                            var bitacorasSalidas = Bitacoras.Where(a => a.TipoMovimiento == 2).ToList();
                           
                            //Recorremos todas las entradas lo que sumaria la cantidad y generaria campos en detmovimientos
                            foreach(var item in bitacorasEntradas)
                            {
                                //Traemos todo el detalle de las entradas
                                var DetallesEntradas = db.DetBitacoraMovimientos.Where(a => a.idEncabezado == item.id).ToList();
                                //Recorremos todos los detalles de las entradas que traen los productos seleccionados
                                foreach(var item2 in DetallesEntradas)
                                   
                                {
                                    var itemCode = item2.ItemCode.Split('|')[0].ToString().Trim();
                                    var itemName = item2.ItemCode.Split('|')[1].ToString().Trim();
                                    var Item = db.DetMovimiento.Where(a => a.idEncabezado == encMovimiento.id && a.ItemCode == itemCode).FirstOrDefault();
                                    var EntregasPrevias = db.EncMovimiento.Where(a => a.NumLlamada == encMovimiento.NumLlamada && a.Comentarios.ToUpper().Contains("entrega de los productos por garantia".ToUpper())).FirstOrDefault();
                                    var DetEntregasprevias = EntregasPrevias == null ? new List<DetMovimiento>() : db.DetMovimiento.Where(a => a.idEncabezado == EntregasPrevias.id).ToList();

                                    if (Item == null) //Si no existe el articulo en el detalle
                                    {
                                        var ExisteEntrega = DetEntregasprevias.Where(a => a.ItemCode == itemCode).FirstOrDefault() == null;
                                        if (ExisteEntrega)
                                        {
                                            DetMovimiento detMovimiento = new DetMovimiento();
                                            detMovimiento.idEncabezado = encMovimiento.id;
                                            detMovimiento.NumLinea = 1;
                                            detMovimiento.ItemCode = itemCode;
                                            detMovimiento.ItemName = itemName;
                                            detMovimiento.PrecioUnitario = db.ProductosHijos.Where(a => a.id == item2.idProducto).FirstOrDefault() == null ? 0 : db.ProductosHijos.Where(a => a.id == item2.idProducto).FirstOrDefault().Precio;
                                            detMovimiento.Cantidad = item2.Cantidad;
                                            detMovimiento.PorDescuento = 0;
                                            detMovimiento.Descuento = 0;
                                            detMovimiento.Impuestos = Convert.ToDecimal((detMovimiento.Cantidad * detMovimiento.PrecioUnitario) * Convert.ToDecimal(0.13));
                                            detMovimiento.TotalLinea = (detMovimiento.Cantidad * detMovimiento.PrecioUnitario) + detMovimiento.Impuestos;
                                            detMovimiento.idError = item2.idError;
                                            detMovimiento.Garantia = false;
                                            db.DetMovimiento.Add(detMovimiento);
                                            db.SaveChanges();
                                        }
                                           
                                    }
                                    else //si si existe
                                    {
                                        db.Entry(Item).State = EntityState.Modified;
                                        Item.Cantidad += item2.Cantidad;
                                        Item.Impuestos = Convert.ToDecimal((Item.Cantidad * Item.PrecioUnitario) * Convert.ToDecimal(0.13));
                                        Item.TotalLinea = (Item.Cantidad * Item.PrecioUnitario) + Item.Impuestos;
                                        if( Item.idError == 0 || Item.idError == null)
                                        {
                                            Item.idError = item2.idError;
                                        }
                                            
                                        db.SaveChanges();
                                    }
                                }
                               
                            }

                            //Recorremos todas las salidas lo que restaria la cantidad
                            foreach (var item in bitacorasSalidas)
                            {
                                //Traemos todo el detalle de las salidas
                                var DetallesSalidas = db.DetBitacoraMovimientos.Where(a => a.idEncabezado == item.id).ToList();
                                //Recorremos todos los detalles de las salidas que traen los productos seleccionados
                                foreach (var item2 in DetallesSalidas)
                                {
                                    var itemCode = item2.ItemCode.Split('|')[0].ToString().Trim();
                                    var itemName = item2.ItemCode.Split('|')[1].ToString().Trim();
                                    var Item = db.DetMovimiento.Where(a => a.idEncabezado == encMovimiento.id && a.ItemCode == itemCode).FirstOrDefault();
                                    if (Item == null) //Si no existe el articulo en el detalle
                                    {
                                         
                                    }
                                    else //si si existe
                                    {
                                        db.Entry(Item).State = EntityState.Modified;
                                        Item.Cantidad -= item2.Cantidad;
                                        Item.Impuestos = Convert.ToDecimal((Item.Cantidad * Item.PrecioUnitario) * Convert.ToDecimal(0.13));
                                        Item.TotalLinea = (Item.Cantidad * Item.PrecioUnitario) + Item.Impuestos;
                                        db.SaveChanges();
                                    }
                                }

                            }


                            var MovimientosEnCero = db.DetMovimiento.Where(a => a.idEncabezado == encMovimiento.id && a.Cantidad <= 0).ToList();
                            foreach(var item in MovimientosEnCero)
                            {
                                db.DetMovimiento.Remove(item);
                                db.SaveChanges();
                            }

                            var CantidadMovimientos = db.DetMovimiento.Where(a => a.idEncabezado == encMovimiento.id).Count();

                            if(CantidadMovimientos == 0)
                            {
                                db.EncMovimiento.Remove(encMovimiento);
                                db.SaveChanges();
                            }
                            else
                            {
                                var MovimientosDetalles = db.DetMovimiento.Where(a => a.idEncabezado == encMovimiento.id).ToList();
                                db.Entry(encMovimiento).State = EntityState.Modified;
                                encMovimiento.Subtotal = MovimientosDetalles.Sum(a => a.Cantidad * a.PrecioUnitario);
                                encMovimiento.Descuento = MovimientosDetalles.Sum(a => a.Descuento);
                                encMovimiento.Impuestos = MovimientosDetalles.Sum(a => a.Impuestos);
                                encMovimiento.TotalComprobante = MovimientosDetalles.Sum(a => a.TotalLinea);
                                db.SaveChanges();
                            }

                           


                        }
                        else
                        {
                            throw new Exception("Ya existe un movimiento de Entrega ");
                        }

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
                //Esto es para Solicitud y Devolucion
                if(Encabezado.TipoReparacion == 1 || Encabezado.TipoReparacion == 2)
                {
                    try
                    {
                        if(coleccion.DetReparacion.Count() > 0)
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
                                dbt.idError = item.idError;
                                db.DetBitacoraMovimientos.Add(dbt);
                                db.SaveChanges();


                            }


                            db.Entry(Encabezado).State = EntityState.Modified;
                            Encabezado.TipoReparacion = 0;
                            Encabezado.BodegaOrigen = "0";
                            Encabezado.BodegaFinal = "0";
                            db.SaveChanges();

                            var DetallesRemover = db.DetReparacion.Where(a => a.idEncabezado == coleccion.EncReparacion.id).ToList();

                            foreach (var item in DetallesRemover)
                            {
                                db.DetReparacion.Remove(item);
                                db.SaveChanges();
                            }
                        }
                        else
                        {
                            throw new Exception("No se puede generar un traslado con ningun repuesto");
                        }
                        

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


                //Para generar Oferta de venta
                if(Encabezado.TipoReparacion == 3)
                {
                    try
                    {
                        var Existe = true;//db.BackOffice.Where(a => a.idEncabezadoReparacion == Encabezado.id && a.TipoMovimiento == 1).FirstOrDefault() == null;

                        if(Existe)
                        {

                            BackOffice backOffice = new BackOffice();
                            backOffice.idEncabezadoReparacion = Encabezado.id;
                            backOffice.idLlamada = Encabezado.idLlamada;
                            backOffice.TipoMovimiento = 1;
                            backOffice.Fecha = DateTime.Now;
                            db.BackOffice.Add(backOffice);
                            db.SaveChanges();

                            EncMovimiento encMovimiento = new EncMovimiento();
                            encMovimiento.CardCode = Llamada.CardCode;

                            var conexion = g.DevuelveCadena(db);

                            var SQL = Parametro.HtmlLlamada + "'" + Llamada.CardCode + "'";

                            SqlConnection Cn = new SqlConnection(conexion);
                            SqlCommand Cmd = new SqlCommand(SQL, Cn);
                            SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                            DataSet Ds = new DataSet();
                            Cn.Open();
                            Da.Fill(Ds, "Encabezado");
                            encMovimiento.CardName = Ds.Tables["Encabezado"].Rows[0]["CardName"].ToString();
                            Cn.Close();
                            Cn.Dispose();
                            encMovimiento.NumLlamada = Llamada.DocEntry.Value.ToString();
                            encMovimiento.Fecha = DateTime.Now;
                            encMovimiento.TipoMovimiento = 1;
                            encMovimiento.Comentarios = Encabezado.Comentarios;
                            encMovimiento.CreadoPor = "0";
                            encMovimiento.Subtotal = 0;
                            encMovimiento.PorDescuento = 0;
                            encMovimiento.Descuento = 0;
                            encMovimiento.Impuestos = 0;
                            encMovimiento.TotalComprobante = 0;
                            encMovimiento.DocEntry = 0;

                            db.EncMovimiento.Add(encMovimiento);
                            db.SaveChanges();


                            var Bitacoras = db.BitacoraMovimientos.Where(a => a.idEncabezado == Encabezado.id && a.ProcesadaSAP == true).ToList(); //Hago el llamado de las bitacoras de movimiento que tengan el id del encabezado de repracion
                            //Separamos las entradas de las salidas
                            var bitacorasEntradas = Bitacoras.Where(a => a.TipoMovimiento == 1).ToList();
                            var bitacorasSalidas = Bitacoras.Where(a => a.TipoMovimiento == 2).ToList();

                            //Recorremos todas las entradas lo que sumaria la cantidad y generaria campos en detmovimientos
                            foreach (var item in bitacorasEntradas)
                            {
                                //Traemos todo el detalle de las entradas
                                var DetallesEntradas = db.DetBitacoraMovimientos.Where(a => a.idEncabezado == item.id).ToList();
                                //Recorremos todos los detalles de las entradas que traen los productos seleccionados
                                foreach (var item2 in DetallesEntradas)

                                {
                                    var itemCode = item2.ItemCode.Split('|')[0].ToString().Trim();
                                    var itemName = item2.ItemCode.Split('|')[1].ToString().Trim();
                                    var Item = db.DetMovimiento.Where(a => a.idEncabezado == encMovimiento.id && a.ItemCode == itemCode).FirstOrDefault();
                                    var EntregasPrevias = db.EncMovimiento.Where(a => a.NumLlamada == encMovimiento.NumLlamada && a.Comentarios.ToUpper().Contains("entrega de los productos por garantia".ToUpper())).FirstOrDefault();
                                    var DetEntregasprevias = EntregasPrevias == null ? new List<DetMovimiento>() : db.DetMovimiento.Where(a => a.idEncabezado == EntregasPrevias.id).ToList();
                                    

                                    if (Item == null) //Si no existe el articulo en el detalle
                                    {
                                        var ExisteEntrega = DetEntregasprevias.Where(a => a.ItemCode == itemCode).FirstOrDefault() == null;
                                        if(ExisteEntrega)
                                        {
                                            DetMovimiento detMovimiento = new DetMovimiento();
                                            detMovimiento.idEncabezado = encMovimiento.id;
                                            detMovimiento.NumLinea = 1;
                                            detMovimiento.ItemCode = itemCode;
                                            detMovimiento.ItemName = itemName;
                                            detMovimiento.PrecioUnitario = db.ProductosHijos.Where(a => a.id == item2.idProducto).FirstOrDefault() == null ? 0 : db.ProductosHijos.Where(a => a.id == item2.idProducto).FirstOrDefault().Precio;
                                            detMovimiento.Cantidad = item2.Cantidad;
                                            detMovimiento.PorDescuento = 0;
                                            detMovimiento.Descuento = 0;
                                            detMovimiento.Impuestos = Convert.ToDecimal((detMovimiento.Cantidad * detMovimiento.PrecioUnitario) * Convert.ToDecimal(0.13));
                                            detMovimiento.TotalLinea = (detMovimiento.Cantidad * detMovimiento.PrecioUnitario) + detMovimiento.Impuestos;
                                            detMovimiento.idError = item2.idError;
                                            detMovimiento.Garantia = false;
                                            db.DetMovimiento.Add(detMovimiento);
                                            db.SaveChanges();
                                        }

                                        
                                    }
                                    else //si si existe
                                    {
                                        db.Entry(Item).State = EntityState.Modified;
                                        Item.Cantidad += item2.Cantidad;
                                        Item.Impuestos = Convert.ToDecimal((Item.Cantidad * Item.PrecioUnitario) * Convert.ToDecimal(0.13));
                                        Item.TotalLinea = (Item.Cantidad * Item.PrecioUnitario) + Item.Impuestos;
                                        if (Item.idError == 0 || Item.idError == null)
                                        {
                                            Item.idError = item2.idError;
                                        }
                                        db.SaveChanges();
                                    }
                                }

                            }

                            //Recorremos todas las salidas lo que restaria la cantidad
                            foreach (var item in bitacorasSalidas)
                            {
                                //Traemos todo el detalle de las salidas
                                var DetallesSalidas = db.DetBitacoraMovimientos.Where(a => a.idEncabezado == item.id).ToList();
                                //Recorremos todos los detalles de las salidas que traen los productos seleccionados
                                foreach (var item2 in DetallesSalidas)
                                {
                                    var itemCode = item2.ItemCode.Split('|')[0].ToString().Trim();
                                    var itemName = item2.ItemCode.Split('|')[1].ToString().Trim();
                                    var Item = db.DetMovimiento.Where(a => a.idEncabezado == encMovimiento.id && a.ItemCode == itemCode).FirstOrDefault();
                                    if (Item == null) //Si no existe el articulo en el detalle
                                    {

                                    }
                                    else //si si existe
                                    {
                                        db.Entry(Item).State = EntityState.Modified;
                                        Item.Cantidad -= item2.Cantidad;
                                        Item.Impuestos = Convert.ToDecimal((Item.Cantidad * Item.PrecioUnitario) * Convert.ToDecimal(0.13));
                                        Item.TotalLinea = (Item.Cantidad * Item.PrecioUnitario) + Item.Impuestos;
                                        db.SaveChanges();
                                    }
                                }

                            }
                            var MovimientosEnCero = db.DetMovimiento.Where(a => a.idEncabezado == encMovimiento.id && a.Cantidad <= 0).ToList();
                            foreach (var item in MovimientosEnCero)
                            {
                                db.DetMovimiento.Remove(item);
                                db.SaveChanges();
                            }

                            var CantidadMovimientos = db.DetMovimiento.Where(a => a.idEncabezado == encMovimiento.id).Count();

                            if (CantidadMovimientos == 0)
                            {
                                db.EncMovimiento.Remove(encMovimiento);
                                db.SaveChanges();
                            }
                            else
                            {
                                var MovimientosDetalles = db.DetMovimiento.Where(a => a.idEncabezado == encMovimiento.id).ToList();
                                db.Entry(encMovimiento).State = EntityState.Modified;
                                encMovimiento.Subtotal = MovimientosDetalles.Sum(a => a.Cantidad * a.PrecioUnitario);
                                encMovimiento.Descuento = MovimientosDetalles.Sum(a => a.Descuento);
                                encMovimiento.Impuestos = MovimientosDetalles.Sum(a => a.Impuestos);
                                encMovimiento.TotalComprobante = MovimientosDetalles.Sum(a => a.TotalLinea);
                                
                                db.SaveChanges();
                            }

                            //Elimina todo lo que estaba pactado para cotizar
                            var DetReparaciones = db.DetReparacion.Where(a => a.idEncabezado == coleccion.EncReparacion.id).ToList();
                            foreach (var item in DetReparaciones)
                            {
                                db.DetReparacion.Remove(item);
                                db.SaveChanges();
                            }
                        }
                        else
                        {
                            throw new Exception("Ya existe un movimiento de Oferta de Venta ");
                        }
                        db.Entry(Encabezado).State = EntityState.Modified;
                        Encabezado.TipoReparacion = 0;
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


                //Para generar una cotizacion BackOffice

                if (Encabezado.TipoReparacion == 4)
                {
                    try
                    {
                        var Existe = true;//db.BackOffice.Where(a => a.idEncabezadoReparacion == Encabezado.id && a.TipoMovimiento == 1).FirstOrDefault() == null;

                        if (Existe)
                        {

                            BackOffice backOffice = new BackOffice();
                            backOffice.idEncabezadoReparacion = Encabezado.id;
                            backOffice.idLlamada = Encabezado.idLlamada;
                            backOffice.TipoMovimiento = 3;
                            backOffice.Fecha = DateTime.Now;
                            db.BackOffice.Add(backOffice);
                            db.SaveChanges();

                            EncMovimiento encMovimiento = new EncMovimiento();
                            encMovimiento.CardCode = Llamada.CardCode;

                            var conexion = g.DevuelveCadena(db);

                            var SQL = Parametro.HtmlLlamada + "'" + Llamada.CardCode + "'";

                            SqlConnection Cn = new SqlConnection(conexion);
                            SqlCommand Cmd = new SqlCommand(SQL, Cn);
                            SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                            DataSet Ds = new DataSet();
                            Cn.Open();
                            Da.Fill(Ds, "Encabezado");
                            encMovimiento.CardName = Ds.Tables["Encabezado"].Rows[0]["CardName"].ToString();
                            Cn.Close();
                            Cn.Dispose();
                            encMovimiento.NumLlamada = Llamada.DocEntry.Value.ToString();
                            encMovimiento.Fecha = DateTime.Now;
                            encMovimiento.TipoMovimiento = 3;
                            encMovimiento.Comentarios = Encabezado.Comentarios;
                            encMovimiento.CreadoPor = "0";
                            encMovimiento.Subtotal = 0;
                            encMovimiento.PorDescuento = 0;
                            encMovimiento.Descuento = 0;
                            encMovimiento.Impuestos = 0;
                            encMovimiento.TotalComprobante = 0;
                            encMovimiento.DocEntry = 0;

                            db.EncMovimiento.Add(encMovimiento);
                            db.SaveChanges();


                             var DetReparaciones = db.DetReparacion.Where(a => a.idEncabezado == coleccion.EncReparacion.id).ToList();

                            foreach(var item in DetReparaciones)
                            {
                                //Recorremos todas las entradas lo que sumaria la cantidad y generaria campos en detmovimientos
                                var itemCode = item.ItemCode.Split('|')[0].ToString().Trim();
                                var itemName = item.ItemCode.Split('|')[1].ToString().Trim();

                                DetMovimiento detMovimiento = new DetMovimiento();
                                detMovimiento.idEncabezado = encMovimiento.id;
                                detMovimiento.NumLinea = 1;
                                detMovimiento.ItemCode = itemCode;
                                detMovimiento.ItemName = itemName;
                                detMovimiento.PrecioUnitario = db.ProductosHijos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? 0 : db.ProductosHijos.Where(a => a.id == item.idProducto).FirstOrDefault().Precio;
                                detMovimiento.Cantidad = item.Cantidad;
                                detMovimiento.PorDescuento = 0;
                                detMovimiento.Descuento = 0;
                                detMovimiento.Impuestos = Convert.ToDecimal((detMovimiento.Cantidad * detMovimiento.PrecioUnitario) * Convert.ToDecimal(0.13));
                                detMovimiento.TotalLinea = (detMovimiento.Cantidad * detMovimiento.PrecioUnitario) + detMovimiento.Impuestos;
                                detMovimiento.idError = item.idError;
                                detMovimiento.Garantia = false;
                                db.DetMovimiento.Add(detMovimiento);
                                db.SaveChanges();
                            }
                                            
                                var MovimientosDetalles = db.DetMovimiento.Where(a => a.idEncabezado == encMovimiento.id).ToList();
                                db.Entry(encMovimiento).State = EntityState.Modified;
                                encMovimiento.Subtotal = MovimientosDetalles.Sum(a => a.Cantidad * a.PrecioUnitario);
                                encMovimiento.Descuento = MovimientosDetalles.Sum(a => a.Descuento);
                                encMovimiento.Impuestos = MovimientosDetalles.Sum(a => a.Impuestos);
                                encMovimiento.TotalComprobante = MovimientosDetalles.Sum(a => a.TotalLinea);

                                db.SaveChanges();

                            
                            //Elimina todo lo que estaba pactado para cotizar
                            foreach (var item in DetReparaciones)
                            {
                                db.DetReparacion.Remove(item);
                                db.SaveChanges();
                            }

                        }
                        else
                        {
                            throw new Exception("Ya existe un movimiento de Oferta de Venta ");
                        }
                        db.Entry(Encabezado).State = EntityState.Modified;
                        Encabezado.TipoReparacion = 0;
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
        [Route("api/EncReparacion/Actualizar")]
        public HttpResponseMessage Put([FromBody] ColeccionRepuestos coleccion)
        {
            try
            {
                
                var encReparacion = db.EncReparacion.Where(a => a.id == coleccion.EncReparacion.id).FirstOrDefault();

                if (encReparacion != null)
                {
                    db.Entry(encReparacion).State = EntityState.Modified;
                    encReparacion.Status = coleccion.EncReparacion.Status;
                    db.SaveChanges();
        
                }
                else
                {
                    throw new Exception("Reparacion no existe");
                }

                return Request.CreateResponse(HttpStatusCode.OK, encReparacion);

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