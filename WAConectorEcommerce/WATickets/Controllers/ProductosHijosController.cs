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
                    ProductosHijos = ProductosHijos.Where(a => a.idPadre == filtro.Codigo1).ToList();
                }

                if(!string.IsNullOrEmpty(filtro.Texto))
                {
                    var ProdPadre = db.ProductosPadres.Where(a => a.codSAP.ToUpper().Contains(filtro.Texto.ToUpper())).FirstOrDefault();
                    if(ProdPadre != null)
                    {
                        ProductosHijos = ProductosHijos.Where(a => a.idPadre == ProdPadre.id).ToList();

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