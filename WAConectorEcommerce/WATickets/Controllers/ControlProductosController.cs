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
    public class ControlProductosController : ApiController
    {
        ModelCliente db = new ModelCliente();

        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {
                List<ControlProductosViewModel> control = new List<ControlProductosViewModel>();

                var Bitacoras = db.BitacoraMovimientos.Where(a => a.idEncabezado == filtro.Codigo1).ToList(); //Hago el llamado de las bitacoras de movimiento que tengan el id del encabezado de repracion
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
                        var Item = control.Where(a =>   a.Item == itemCode).FirstOrDefault();
                        if (Item == null) //Si no existe el articulo en el detalle
                        {
                            var BTS = db.BitacoraMovimientosSAP.Where(a => a.idDetalle == item2.id).FirstOrDefault() == null ? 0 :  db.BitacoraMovimientosSAP.Where(a => a.idDetalle == item2.id).Sum(a => a.Cantidad);
                            ControlProductosViewModel detMovimiento = new ControlProductosViewModel();
                            detMovimiento.idProducto = item2.idProducto;
                            detMovimiento.Item = itemCode;
                            detMovimiento.ItemName = itemName;
                            detMovimiento.CantidadUsado = BTS; //item2.Cantidad;
                           
                            control.Add(detMovimiento);
                            
                        }
                        else //si si existe
                        {
                            control.Remove(Item);
                            var BTS = db.BitacoraMovimientosSAP.Where(a => a.idDetalle == item2.id).FirstOrDefault() == null ? 0 : db.BitacoraMovimientosSAP.Where(a => a.idDetalle == item2.id).Sum(a => a.Cantidad);

                            Item.CantidadUsado += BTS;//item2.Cantidad;
                            control.Add(Item);
                             
                        }
                    }

                }

                //Recorremos todas las salidas lo que restaria la cantidad
                foreach (var item in bitacorasSalidas)
                {
                    //Traemos todo el detalle de las salidas
                    var DetallesSalidas =  db.DetBitacoraMovimientos.Where(a => a.idEncabezado == item.id).ToList();
                    //Recorremos todos los detalles de las salidas que traen los productos seleccionados
                    foreach (var item2 in DetallesSalidas)
                    {
                        var itemCode = item2.ItemCode.Split('|')[0].ToString().Trim();
                        var itemName = item2.ItemCode.Split('|')[1].ToString().Trim();
                        var Item = control.Where(a =>   a.Item == itemCode).FirstOrDefault();
                        if (Item == null) //Si no existe el articulo en el detalle
                        {

                        }
                        else //si si existe
                        {
                            control.Remove(Item);
                            var BTS = db.BitacoraMovimientosSAP.Where(a => a.idDetalle == item2.id).FirstOrDefault() == null ? 0 : db.BitacoraMovimientosSAP.Where(a => a.idDetalle == item2.id).Sum(a => a.Cantidad);
                            Item.CantidadUsado -= BTS;//item2.Cantidad;
                            control.Add(Item);
                            
                        }
                    }

                }

                return Request.CreateResponse(HttpStatusCode.OK, control);

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