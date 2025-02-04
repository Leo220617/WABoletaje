using System;
using System.Collections.Generic;
using System.Data;
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
    public class FacturaAprobadaController : ApiController
    {
        G g = new G();
        ModelCliente db = new ModelCliente();

        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {
               
                var MovimientoAprobado = db.EncMovimiento.Where(a => a.NumLlamada == filtro.CardCode && a.Aprobada == true).FirstOrDefault();
                var conexion = g.DevuelveCadena(db);
                var valorAFiltrar = MovimientoAprobado == null ? "" : MovimientoAprobado.DocEntry.ToString();
                var filtroSQL = "DocEntry = '" + valorAFiltrar + "' order by DocEntry desc";
                var SQL = db.Parametros.FirstOrDefault().SQLDocEntryDocs.Replace("@CampoBuscar", "DocNum").Replace("@Tabla", "OQUT").Replace("@CampoWhere = @reemplazo", filtroSQL);

                SqlConnection Cn = new SqlConnection(conexion);
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open();
                Da.Fill(Ds, "Oferta");
                 

                Cn.Close();

                return Request.CreateResponse(HttpStatusCode.OK, Ds);

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}