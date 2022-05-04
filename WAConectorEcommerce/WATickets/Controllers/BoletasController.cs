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
    public class BoletasController: ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();


        [HttpPost]
        public HttpResponseMessage Post([FromBody] BoletaViewModel boleta)
        {
            try
            {

                var Parametros = db.Parametros.FirstOrDefault();
              

                    try
                    {
                        //CompanyService companyService = Conexion.Company.GetCompanyService();

                        var client = (CustomerEquipmentCards)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oCustomerEquipmentCards);
                    // var clientw  = (SAPbobsCOM.DepositsService)companyService.GetBusinessService(ServiceTypes.DepositsService);
                    if(!string.IsNullOrEmpty(boleta.NoSerie))
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
                        }



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
                    }

           
                 

                return Request.CreateResponse(HttpStatusCode.OK, boleta);
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
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }


    }
}