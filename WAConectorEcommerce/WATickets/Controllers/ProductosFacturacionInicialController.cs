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
    public class ProductosFacturacionInicialController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();
        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {

                ParametrosFacturacion parametros = db.ParametrosFacturacion.FirstOrDefault();
                var conexion = G.DevuelveCadena(db);

                var SQL = parametros.SQLProductosFacturar;
                 
                SqlConnection Cn = new SqlConnection(conexion);
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open();
                Da.Fill(Ds, "ProductosFacturar");


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
    }
}