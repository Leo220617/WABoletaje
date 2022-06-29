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
    public class ProductosHijosController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();


        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {

                var ProductosHijos = db.ProductosHijos.ToList();

                if(filtro.Codigo1 > 0)
                {
                    var ProdPadre = db.ProductosPadres.Where(a => a.id == filtro.Codigo1).FirstOrDefault();

                    if (ProdPadre != null)
                    {
                        var PH = db.PadresHijosProductos.Where(a => a.idProductoPadre == ProdPadre.id).ToList();
                        var PH2 = db.ProductosHijos.ToList();

                        ProductosHijos = new List<ProductosHijos>();

                        foreach (var item in PH)
                        {
                            var PHC = PH2.Where(a => a.id == item.idProductoHijo).FirstOrDefault();
                            PHC.Cantidad = item.Cantidad;
                            ProductosHijos.Add(PHC);
                        }

                    }
                    else
                    {
                        ProductosHijos = null;
                    }
                }

                if(!string.IsNullOrEmpty(filtro.Texto))
                {
                    var ProdPadre = db.ProductosPadres.Where(a => a.codSAP.ToUpper().Contains(filtro.Texto.ToUpper())).FirstOrDefault();
                    if(ProdPadre != null)
                    {
                        var PH = db.PadresHijosProductos.Where(a => a.idProductoPadre == ProdPadre.id).ToList();
                        var PH2 = db.ProductosHijos.ToList();

                        ProductosHijos = new List<ProductosHijos>();

                        foreach(var item in PH)
                        {
                            var PHC = PH2.Where(a => a.id == item.idProductoHijo).FirstOrDefault();
                            PHC.Cantidad = item.Cantidad;

                            ProductosHijos.Add(PHC);
                        }

                    }
                    else
                    {
                        ProductosHijos = null;
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, ProductosHijos);

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [Route("api/ProductosHijos/Consultar")]
        public HttpResponseMessage GetOne([FromUri]int id)
        {
            try
            {



                var ProductosHijos = db.ProductosHijos.Where(a => a.id == id).FirstOrDefault();


                if (ProductosHijos == null)
                {
                    throw new Exception("Este Producto Hijo no se encuentra registrado");
                }

                return Request.CreateResponse(HttpStatusCode.OK, ProductosHijos);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}