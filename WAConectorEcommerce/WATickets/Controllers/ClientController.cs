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
    public class ClientController: ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();

        [HttpPost]
        public HttpResponseMessage Post([FromBody] ClientesViewModel cliente)
        {
            try
            {

                var Parametros = db.Parametros.FirstOrDefault();


                try
                {
                    
                    var client = (SAPbobsCOM.BusinessPartners)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oBusinessPartners);


                    client.CardName = cliente.CardName;
                    client.EmailAddress = cliente.Email;
                    client.Series = 48;
                    client.CardForeignName = cliente.Cedula;
                    client.FederalTaxID = cliente.Cedula;
                    client.Currency = "##";
                    client.Phone1 = cliente.Telefono;
                    client.CardType = BoCardTypes.cCustomer;
                    client.Addresses.Add();
                    client.Addresses.SetCurrentLine(0);
                    client.Addresses.AddressName = cliente.Direccion;
                    client.Address = cliente.Direccion;

                    client.ContactPerson = cliente.NombreContacto;
                    client.ContactEmployees.Add();
                    client.ContactEmployees.SetCurrentLine(0);
                    client.ContactEmployees.Name = cliente.NombreContacto;
                    client.ContactEmployees.Phone1 = cliente.NumeroContacto;
             




                    var respuesta = client.Add();

                    if (respuesta == 0)
                    {
                        Conexion.Desconectar();
                    }
                    else
                    {
                        BitacoraErrores be = new BitacoraErrores();

                        be.Descripcion = Conexion.Company.GetLastErrorDescription();
                        be.StackTrace = "Crear Cliente";
                        be.Fecha = DateTime.Now;

                        db.BitacoraErrores.Add(be);
                        db.SaveChanges();
                        Conexion.Desconectar();
                        throw new Exception(be.Descripcion);
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
                    throw new Exception(be.Descripcion);

                }




                return Request.CreateResponse(HttpStatusCode.OK, cliente);
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

        [HttpPost]
        [Route("api/Client/Actualizar")]
        public HttpResponseMessage Put([FromBody] ClientesViewModel cliente)
        {
            try
            {

                var client = (SAPbobsCOM.BusinessPartners)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oBusinessPartners);

                if (client.GetByKey(cliente.CardCode))
                {
                    if (client.ContactEmployees.Count > 0)
                    {
                        client.ContactEmployees.Add();
                    }
                    else
                    {
                        if (client.ContactEmployees.Name != "")
                        {

                            client.ContactEmployees.Add();
                        }
                    }

                    
                    client.ContactEmployees.Name = cliente.NombreContacto;
                    client.ContactEmployees.Phone1 = cliente.NumeroContacto;
                    client.ContactEmployees.E_Mail = cliente.Email;
                    client.ContactEmployees.Add();
                    var respuesta = client.Update();

                    if (respuesta == 0)
                    {
                        Conexion.Desconectar();
                    }
                    else
                    {
                        BitacoraErrores be = new BitacoraErrores();

                        be.Descripcion = Conexion.Company.GetLastErrorDescription();
                        be.StackTrace = "Actualizar Cliente";
                        be.Fecha = DateTime.Now;

                        db.BitacoraErrores.Add(be);
                        db.SaveChanges();
                        Conexion.Desconectar();
                        throw new Exception(be.Descripcion);
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK);
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