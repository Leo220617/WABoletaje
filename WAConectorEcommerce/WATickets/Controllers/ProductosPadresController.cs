using Newtonsoft.Json;
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
    public class ProductosPadresController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();

        [Route("api/ProductosPadres/Boleta")]
        public async Task<HttpResponseMessage> GetBoleta()
        {
            try
            {
                List<ProductosBoleta> productos = new List<ProductosBoleta>();
                Parametros parametros = db.Parametros.FirstOrDefault();
                var conexion = G.DevuelveCadena(db);

                var SQL = parametros.SQLProductosBoleta;

                SqlConnection Cn = new SqlConnection(conexion);
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open();
                Da.Fill(Ds, "Productos");
                foreach (DataRow item in Ds.Tables["Productos"].Rows)
                {
                    ProductosBoleta pr = new ProductosBoleta();
                    pr.ItemCode = item["ItemCode"].ToString();
                    pr.ItemName = item["ItemName"].ToString();
                    productos.Add(pr);
                }
                    Cn.Close();
                return Request.CreateResponse(HttpStatusCode.OK, productos);

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

        [Route("api/ProductosPadres/InsertarProductos")]
        public async Task<HttpResponseMessage> Get()
        {
            try
            {

                Parametros parametros = db.Parametros.FirstOrDefault();
                var conexion = G.DevuelveCadena(db);

                var SQL = parametros.SQLItemPadres;

                SqlConnection Cn = new SqlConnection(conexion);
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open();
                Da.Fill(Ds, "Productos");

                foreach (DataRow item in Ds.Tables["Productos"].Rows)
                {
                    var itemcode = item["ItemCode"].ToString();
                    var Prod = db.ProductosPadres.Where(a => a.codSAP == itemcode).FirstOrDefault();
                    if(Prod == null)
                    {
                        try
                        {
                            ProductosPadres productos = new ProductosPadres();
                            productos.codSAP = item["ItemCode"].ToString();
                            productos.Nombre = item["ItemName"].ToString();
                            productos.Stock = Convert.ToDecimal(item["Stock"].ToString());
                            productos.Precio = Convert.ToDecimal(item["Price"].ToString());
                            db.ProductosPadres.Add(productos);
                            db.SaveChanges();
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
                    else
                    {
                        try
                        {
                            db.Entry(Prod).State = EntityState.Modified;
                            Prod.codSAP = item["ItemCode"].ToString();
                            Prod.Nombre = item["ItemName"].ToString();
                            Prod.Stock = Convert.ToDecimal(item["Stock"].ToString());
                            Prod.Precio = Convert.ToDecimal(item["Price"].ToString());

                            db.SaveChanges();
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

                    

                }

                Cn.Close();




                /////
                ///
                 SQL = parametros.SQLItemHijos;

                  Cn = new SqlConnection(conexion);
                 Cmd = new SqlCommand(SQL, Cn);
                 Da = new SqlDataAdapter(Cmd);
                 Ds = new DataSet();
                Cn.Open();
                Da.Fill(Ds, "Productos");

                foreach (DataRow item in Ds.Tables["Productos"].Rows)
                {
                    var itemcode = item["ItemCode"].ToString();
                    var Prod = db.ProductosHijos.Where(a => a.codSAP == itemcode).FirstOrDefault();
                    if (Prod == null)
                    {
                        try
                        {
                            ProductosHijos productos = new ProductosHijos();
                            productos.codSAP = item["ItemCode"].ToString();
                            productos.Nombre = item["ItemName"].ToString();
                            productos.Stock = Convert.ToDecimal(item["Stock"].ToString());
                            productos.Precio = Convert.ToDecimal(item["Precio"].ToString());
                            productos.Cantidad = 0;
                            productos.Localizacion = item["localizacion"].ToString();
                            productos.Costo = Convert.ToDecimal(item["Costo"].ToString());
                            productos.PorMinimo = Convert.ToInt32(item["PorMinimo"].ToString());
                            try
                            {
                            productos.Rate = Convert.ToDecimal(item["Rate"].ToString());

                            }
                            catch (Exception)
                            {
                                productos.Rate = Convert.ToDecimal(item["Rate"].ToString().Replace(".",","));

                            }
                            db.ProductosHijos.Add(productos);
                            db.SaveChanges();
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
                    else
                    {
                        try
                        {
                            db.Entry(Prod).State = EntityState.Modified;
                            Prod.codSAP = item["ItemCode"].ToString();
                            Prod.Nombre = item["ItemName"].ToString();
                            Prod.Stock = Convert.ToDecimal(item["Stock"].ToString());
                            //productos.Cantidad = 0;
                            Prod.Precio = Convert.ToDecimal(item["Precio"].ToString());
                            Prod.Localizacion = item["localizacion"].ToString();

                            Prod.Costo = Convert.ToDecimal(item["Costo"].ToString());
                            Prod.PorMinimo = Convert.ToInt32(item["PorMinimo"].ToString());
                            Prod.Rate = Convert.ToDecimal(item["Rate"].ToString());
                            try
                            {
                                Prod.Rate = Convert.ToDecimal(item["Rate"].ToString());

                            }
                            catch (Exception)
                            {
                                Prod.Rate = Convert.ToDecimal(item["Rate"].ToString().Replace(".", ","));

                            }
                            db.SaveChanges();
                        }
                        catch (Exception ex1)
                        {

                            BitacoraErrores be = new BitacoraErrores();

                            be.Descripcion = ex1.Message + " " + itemcode;
                            be.StackTrace = ex1.StackTrace;
                            be.Fecha = DateTime.Now;

                            db.BitacoraErrores.Add(be);
                            db.SaveChanges();
                        }

                    }



                }

                Cn.Close();





                return Request.CreateResponse(HttpStatusCode.OK, Ds);

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

        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {

                var ProductosPadres = db.ProductosPadres.ToList();





                return Request.CreateResponse(HttpStatusCode.OK, ProductosPadres);

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
        [Route("api/ProductosPadres/Consultar")]
        public HttpResponseMessage GetOne([FromUri]int id)
        {
            try
            {



                var ProductosPadres = db.ProductosPadres.Where(a => a.id == id).FirstOrDefault();


                if (ProductosPadres == null)
                {
                    throw new Exception("Este Producto Padre no se encuentra registrado");
                }

                return Request.CreateResponse(HttpStatusCode.OK, ProductosPadres);
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
        public HttpResponseMessage Post([FromBody] ColeccionProductos coleccion)
        {
            try
            {


                var Prod = db.ProductosPadres.Where(a => a.id == coleccion.ProductosPadres.id).FirstOrDefault();

                var Hijo = db.ProductosHijos.ToList();
                if (Prod != null)
                {
                    //Se borran
                    var HijosAnteriores = Hijo.Where(a => a.idPadre == Prod.id).ToList();
                    var HijAnterios = db.PadresHijosProductos.Where(a => a.idProductoPadre == Prod.id).ToList();
                    for (int i = 0; i < HijosAnteriores.Count(); i++)
                    {
                        var HJa = Hijo.Where(a => a.id == HijosAnteriores[i].id).FirstOrDefault();
                        db.Entry(HJa).State = EntityState.Modified;
                        HJa.idPadre = 0;
                        HJa.Cantidad = 0;
                        db.SaveChanges();
                    }

                    foreach(var item in HijAnterios)
                    {
                        db.PadresHijosProductos.Remove(item);
                        db.SaveChanges();
                    }



                    for (int i = 0; i < coleccion.ProductosHijos.Length; i++)
                    {

                        var HJ = Hijo.Where(a => a.id == coleccion.ProductosHijos[i].id).FirstOrDefault();
                        db.Entry(HJ).State = EntityState.Modified;
                        HJ.idPadre = Prod.id;
                        HJ.Cantidad = coleccion.ProductosHijos[i].Cantidad;
                        db.SaveChanges();

                        var ProductoHijo = new PadresHijosProductos();
                        ProductoHijo.idProductoPadre = Prod.id;
                        ProductoHijo.idProductoHijo = HJ.id;
                        ProductoHijo.Cantidad = coleccion.ProductosHijos[i].Cantidad;
                        db.PadresHijosProductos.Add(ProductoHijo);
                        db.SaveChanges();

                    }

                    

                }
                else
                {
                    throw new Exception("Este Productos Padre  NO existe");
                }


                return Request.CreateResponse(HttpStatusCode.OK, Prod);
            }
            catch (Exception ex)
            {
                BitacoraErrores error = new BitacoraErrores();
                error.DocNum = "";
                error.Descripcion = ex.Message;
                error.StackTrace = ex.StackTrace.ToString() + " " + JsonConvert.SerializeObject(coleccion);
                error.Fecha = DateTime.Now;
                db.BitacoraErrores.Add(error);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }



    }
}