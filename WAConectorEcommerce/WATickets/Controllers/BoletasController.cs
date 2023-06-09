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
    public class BoletasController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();


        [HttpPost]
        public HttpResponseMessage Post([FromBody] BoletaViewModel boleta)
        {
            try
            {

                var Parametros = db.Parametros.FirstOrDefault();
                var conexion = G.DevuelveCadena(db); //aqui extraemos la informacion de la tabla de sap para hacerle un query a sap

                var SQL = Parametros.SQLProductos + " where itemCode = '" + boleta.ItemCode + "' and manufSN = '" + boleta.NoSerieFabricante + "' and internalSN = '" + boleta.NoSerie + "'"; //Preparo el query

                SqlConnection Cn = new SqlConnection(conexion);
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open(); //se abre la conexion
                Da.Fill(Ds, "Productos");

                if (Ds.Tables["Productos"].Rows.Count > 0)
                {
                    throw new Exception("El producto " + boleta.ItemCode + " ya contiene la serie indicada (" + boleta.NoSerie + ")");
                }

                Cn.Close();
                Cn.Dispose();


                try
                {
                    //CompanyService companyService = Conexion.Company.GetCompanyService();

                    var client = (CustomerEquipmentCards)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oCustomerEquipmentCards);
                    // var clientw  = (SAPbobsCOM.DepositsService)companyService.GetBusinessService(ServiceTypes.DepositsService);
                    if (!string.IsNullOrEmpty(boleta.NoSerie))
                    {
                        client.InternalSerialNum = boleta.NoSerie;

                    }
                    client.ManufacturerSerialNum = boleta.NoSerieFabricante;
                    client.ItemCode = boleta.ItemCode;
                    client.CustomerCode = boleta.CardCode;





                    var respuesta = client.Add();

                    if (respuesta == 0)
                    {
                        Conexion.Desconectar();
                    }
                    else
                    {
                        BitacoraErrores be = new BitacoraErrores();
                        be.Descripcion = Conexion.Company.GetLastErrorDescription();
                        be.StackTrace = "Tarjeta de Equipo";
                        be.Fecha = DateTime.Now;
                        db.BitacoraErrores.Add(be);
                        db.SaveChanges();
                        Conexion.Desconectar();
                        throw new Exception(be.Descripcion);
                    }


                    return Request.CreateResponse(HttpStatusCode.OK, boleta);

                }
                catch (Exception ex1)
                {

                    Conexion.Desconectar();
                    BitacoraErrores be = new BitacoraErrores();

                    be.Descripcion = ex1.Message;
                    be.StackTrace = ex1.StackTrace;
                    be.Fecha = DateTime.Now;

                    db.BitacoraErrores.Add(be);
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex1.Message);

                }







            }
            catch (Exception ex)
            {
                BitacoraErrores bit = new BitacoraErrores();
                bit.Descripcion = ex.Message;
                bit.StackTrace = ex.StackTrace;
                bit.Fecha = DateTime.Now;
                bit.DocNum = "";

                db.BitacoraErrores.Add(bit);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpPost]

        [Route("api/Boletas/Actualizar")]
        public HttpResponseMessage Put([FromBody] BoletaViewModel boleta)
        {
            try
            {
                var ID = 0;
                var Parametros = db.Parametros.FirstOrDefault();
                var conexion = G.DevuelveCadena(db); //aqui extraemos la informacion de la tabla de sap para hacerle un query a sap

                var SQL = Parametros.SQLProductos + " where manufSN = '" + boleta.NoSerieFabricante + "' and internalSN = '" + boleta.NoSerie + "'"; //Preparo el query

                SqlConnection Cn = new SqlConnection(conexion);
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open(); //se abre la conexion
                Da.Fill(Ds, "Productos");

                if (Ds.Tables["Productos"].Rows.Count < 0)
                {
                    throw new Exception("El producto " + boleta.ItemCode + " no contiene la serie indicada (" + boleta.NoSerie + ")");
                }
                else
                {
                    ID = Convert.ToInt32(Ds.Tables["Productos"].Rows[0]["ID"]);
                }

                Cn.Close();
                Cn.Dispose();


                try
                {
                    //CompanyService companyService = Conexion.Company.GetCompanyService();

                    var client = (CustomerEquipmentCards)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oCustomerEquipmentCards);
                    // var clientw  = (SAPbobsCOM.DepositsService)companyService.GetBusinessService(ServiceTypes.DepositsService);
                    if (client.GetByKey(ID))
                    {
                        if (!string.IsNullOrEmpty(boleta.NoSerie))
                        {
                            client.InternalSerialNum = boleta.NoSerie;

                        }
                        client.ManufacturerSerialNum = boleta.NoSerieFabricante;
                       // client.ItemCode = boleta.ItemCode;
                        client.CustomerCode = boleta.CardCode;





                        var respuesta = client.Update();

                        if (respuesta == 0)
                        {
                            Conexion.Desconectar();
                        }
                        else
                        {
                            BitacoraErrores be = new BitacoraErrores();
                            be.Descripcion = Conexion.Company.GetLastErrorDescription();
                            be.StackTrace = "Tarjeta de Equipo";
                            be.Fecha = DateTime.Now;
                            db.BitacoraErrores.Add(be);
                            db.SaveChanges();
                            Conexion.Desconectar();
                            return Request.CreateResponse(HttpStatusCode.InternalServerError, be.Descripcion);

                        }

                    }


                    return Request.CreateResponse(HttpStatusCode.OK, boleta);

                }
                catch (Exception ex1)
                {

                    Conexion.Desconectar();
                    BitacoraErrores be = new BitacoraErrores();

                    be.Descripcion = ex1.Message;
                    be.StackTrace = ex1.StackTrace;
                    be.Fecha = DateTime.Now;

                    db.BitacoraErrores.Add(be);
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex1.Message);

                }







            }
            catch (Exception ex)
            {
                BitacoraErrores bit = new BitacoraErrores();
                bit.Descripcion = ex.Message;
                bit.StackTrace = ex.StackTrace;
                bit.Fecha = DateTime.Now;
                bit.DocNum = "";

                db.BitacoraErrores.Add(bit);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}