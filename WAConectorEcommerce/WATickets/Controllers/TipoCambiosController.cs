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
    public class TipoCambiosController : ApiController
    {
        G g = new G();
        ModelCliente db = new ModelCliente();

        [Route("api/TipoCambios/InsertarSAP")]
        public HttpResponseMessage GetExtraeDatos()
        {
            try
            {
                Parametros parametros = db.Parametros.FirstOrDefault(); //de aqui nos traemos los querys
                var conexion = g.DevuelveCadena(db); //aqui extraemos la informacion de la tabla de sap para hacerle un query a sap

                var SQL = parametros.SQLTipoCambio; //Preparo el query

                SqlConnection Cn = new SqlConnection(conexion);
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open(); //se abre la conexion
                Da.Fill(Ds, "TipoCambios");

                var TipoCambios = db.TipoCambios.ToList();
                foreach (DataRow item in Ds.Tables["TipoCambios"].Rows)
                {
                    var FechaActual = DateTime.Now.Date;

                    var Moneda = item["Moneda"].ToString();

                    var TiposCambio = TipoCambios.Where(a => a.Fecha == FechaActual && a.Moneda == Moneda).FirstOrDefault();

                    if (TiposCambio == null) //Existe ?
                    {
                        try
                        {

                            TiposCambio = new TipoCambios();
                            TiposCambio.TipoCambio = Convert.ToDecimal(item["Precio"]);
                            TiposCambio.Moneda = item["Moneda"].ToString();
                            TiposCambio.Fecha = Convert.ToDateTime(item["Fecha"]);




                            db.TipoCambios.Add(TiposCambio);
                            db.SaveChanges();

                        }
                        catch (Exception ex1)
                        {

                            ModelCliente db2 = new ModelCliente();
                            BitacoraErrores be = new BitacoraErrores();
                            be.Descripcion = ex1.Message;
                            be.StackTrace = ex1.StackTrace;
                            be.Fecha = DateTime.Now; 
                            db2.BitacoraErrores.Add(be);
                            db2.SaveChanges();
                        }
                    }
                    else
                    {
                        try
                        {
                            db.Entry(TiposCambio).State = EntityState.Modified;


                            TiposCambio.TipoCambio = Convert.ToDecimal(item["Precio"]);
                            TiposCambio.Moneda = item["Moneda"].ToString();
                            TiposCambio.Fecha = Convert.ToDateTime(item["Fecha"]);
                            db.SaveChanges();
                        }
                        catch (Exception ex1)
                        {

                            ModelCliente db2 = new ModelCliente();
                            BitacoraErrores be = new BitacoraErrores();
                            be.Descripcion = ex1.Message; 
                            be.Fecha = DateTime.Now; 
                            db2.BitacoraErrores.Add(be);
                            db2.SaveChanges();
                        }

                    }


                }


                Cn.Close(); //se cierra la conexion
                Cn.Dispose();

                return Request.CreateResponse(System.Net.HttpStatusCode.OK, "Procesado con exito");

            }
            catch (Exception ex)
            {

                ModelCliente db2 = new ModelCliente();
                BitacoraErrores be = new BitacoraErrores();
                be.Descripcion = ex.Message;
                be.StackTrace = ex.StackTrace;

                be.Fecha = DateTime.Now; 
                db2.BitacoraErrores.Add(be);
                db2.SaveChanges();

                return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex);
            }
        }

        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {


                var time = new DateTime();

                var Fecha = filtro.FechaInicial.Date;
                var TipoCambios = db.TipoCambios.Where(a => (filtro.FechaInicial != time ? a.Fecha == Fecha : true)).ToList();




                return Request.CreateResponse(HttpStatusCode.OK, TipoCambios);

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