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
using WATickets.Models;
using WATickets.Models.Cliente;

namespace WATickets.Controllers
{
    [Authorize]
    public class SolicitudesComprasController : ApiController
    {
        G g = new G();
        ModelCliente db = new ModelCliente();

        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {
                if (!string.IsNullOrEmpty(filtro.Texto))
                {
                    var DocEntry = Convert.ToInt32(filtro.Texto);
                    var Solicitudes = db.SolicitudCompra.Select(a => new
                    {
                        a.id,
                        a.CardCode,
                        a.idEncabezadoBitacora,
                        a.idOfertaAprobada,
                        a.Fecha,
                        a.GrupoArticulo,
                        a.DocEntry,
                        a.DocNum,
                        a.ProcesadaSAP,
                        BT = db.BitacoraMovimientos.Select(x => new
                        {
                            x.id,
                            x.idLlamada,
                            Llamada = db.LlamadasServicios.Where(c => c.id == x.idLlamada).FirstOrDefault()
                        }

                    ).Where(x => x.id == a.idEncabezadoBitacora).FirstOrDefault(),
                        Detalle = db.DetSolicitudCompra.Where(b => b.idEncabezado == a.id).ToList()
                    }
                    ).Where(a => a.BT.Llamada.DocEntry == DocEntry).ToList();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    return Request.CreateResponse(HttpStatusCode.OK, Solicitudes);

                }
                else
                {
                    var time = new DateTime();
                    if (filtro.FechaFinal != time)
                    {
                        filtro.FechaFinal = filtro.FechaFinal.AddDays(1);
                    }
                    var Solicitudes = db.SolicitudCompra.Select(a => new
                    {
                        a.id,
                        a.CardCode,
                        a.idEncabezadoBitacora,
                        a.idOfertaAprobada,
                        a.Fecha,
                        a.GrupoArticulo,
                        a.DocEntry,
                        a.DocNum,
                        a.ProcesadaSAP,

                        Detalle = db.DetSolicitudCompra.Where(b => b.idEncabezado == a.id).ToList()
                    }
                    ).Where(a => (filtro.FechaInicial != time ? a.Fecha >= filtro.FechaInicial : true)
                    && (filtro.FechaFinal != time ? a.Fecha <= filtro.FechaFinal : true) &&
                    (filtro.Codigo1 > 0 ? a.idEncabezadoBitacora == filtro.Codigo1 : true)

                    ).ToList();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    return Request.CreateResponse(HttpStatusCode.OK, Solicitudes);

                }



            }
            catch (Exception ex)
            {
                BitacoraErrores be = new BitacoraErrores();

                be.Descripcion = ex.Message;
                be.StackTrace = ex.StackTrace;
                be.Fecha = DateTime.Now;
                db.BitacoraErrores.Add(be);
                db.SaveChanges();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [Route("api/SolicitudesCompras/Consultar")]
        public HttpResponseMessage GetOne([FromUri]int id)
        {
            try
            {



                var Solicitudes = db.SolicitudCompra.Select(a => new
                {
                    a.id,
                    a.CardCode,
                    a.idEncabezadoBitacora,
                    a.idOfertaAprobada,
                    a.Fecha,
                    a.GrupoArticulo,
                    a.DocEntry,
                    a.DocNum,
                    a.ProcesadaSAP,

                    Detalle = db.DetSolicitudCompra.Where(b => b.idEncabezado == a.id).ToList()
                }
                   ).Where(a => a.id == id

                   ).FirstOrDefault();


                if (Solicitudes == null)
                {

                    throw new Exception("Esta solicitud no se encuentra registrado");


                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
                return Request.CreateResponse(HttpStatusCode.OK, Solicitudes);
            }
            catch (Exception ex)
            {
                BitacoraErrores be = new BitacoraErrores();

                be.Descripcion = ex.Message;
                be.StackTrace = ex.StackTrace;
                be.Fecha = DateTime.Now;
                db.BitacoraErrores.Add(be);
                db.SaveChanges();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }


        [HttpPost]
        [Route("api/SolicitudesCompras/EnviarSAP")]
        public HttpResponseMessage PostEnviarSAP([FromBody] SolicitudCompra solicitud)
        {
            try
            {
                var Parametros = db.Parametros.FirstOrDefault();
                var Solicitud = db.SolicitudCompra.Where(a => a.id == solicitud.id && a.ProcesadaSAP == false).FirstOrDefault();
                var OfertaAprobada = db.EncMovimiento.Where(a => a.id == Solicitud.idOfertaAprobada).FirstOrDefault();
                if (Solicitud != null)
                {
                    try
                    {
                        var client = (Documents)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseRequest);
                        //client.CardCode = string.IsNullOrEmpty(Solicitud.CardCode) ? throw new Exception("CardCode null") : Solicitud.CardCode.Trim();
                        client.DocDate = DateTime.Now;
                        client.DocDueDate = DateTime.Now.AddDays(7);
                        client.Comments = "Boletaje APP: " + Solicitud.id;
                        client.RequriedDate = DateTime.Now.AddDays(7);
                        var i = 0;
                        int ofertaVentaID = OfertaAprobada != null ? OfertaAprobada.DocEntry : 0; // ID de la oferta de venta en SAP
                        var Detalle = db.DetSolicitudCompra.Where(a => a.idEncabezado == Solicitud.id).ToList();
                        foreach (var item in Detalle)
                        {
                            string itemCodeBuscado = item.ItemCode; // Código del artículo que queremos ligar

                            client.Lines.SetCurrentLine(i);
                            client.Lines.ItemCode = item.ItemCode; // Código del artículo
                            client.Lines.Quantity = Convert.ToDouble(item.Cantidad);
                            client.Lines.RequiredDate = DateTime.Now.AddDays(7); // Ejemplo: 7 días después de hoy

                            // 🔹 Obtener línea de la oferta donde está el artículo
                            int lineaOferta = ObtenerNumeroLineaOferta(Conexion.Company, ofertaVentaID, itemCodeBuscado);
                            if (lineaOferta != -1)
                            {
                                client.Lines.BaseType = 23; // Oferta de Venta
                                client.Lines.BaseEntry = ofertaVentaID; // ID de la oferta
                                client.Lines.BaseLine = lineaOferta; //
                            }
                            client.Lines.Add();
                            i++;
                        }



                        var respuesta = client.Add();

                        if (respuesta == 0)
                        {

                            db.Entry(Solicitud).State = EntityState.Modified;
                            Solicitud.DocEntry = 0;
                            Solicitud.DocNum = 0;



                            try
                            {
                                if (Solicitud.DocEntry == 0)
                                {
                                    Solicitud.DocEntry = Convert.ToInt32(Conexion.Company.GetNewObjectKey());
                                    throw new Exception("");
                                }

                            }
                            catch (Exception)

                            {
                                try
                                {
                                    var conexion = g.DevuelveCadena(db);
                                    var filtroSQL = "Comments like '%Boletaje APP: " + Solicitud.id + "%'";
                                    var SQL = Parametros.SQLDocEntryDocs.Replace("@CampoBuscar", "DocEntry,DocNum").Replace("@Tabla", "OPRQ").Replace("@CampoWhere = @reemplazo", filtroSQL);

                                    SqlConnection Cn = new SqlConnection(conexion);
                                    SqlCommand Cmd = new SqlCommand(SQL, Cn);
                                    SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                                    DataSet Ds = new DataSet();
                                    Cn.Open();
                                    Da.Fill(Ds, "DocNum1");
                                    Solicitud.DocEntry = Convert.ToInt32(Ds.Tables["DocNum1"].Rows[0]["DocEntry"]);
                                    Solicitud.DocNum = Convert.ToInt32(Ds.Tables["DocNum1"].Rows[0]["DocNum"]);
                                    Solicitud.ProcesadaSAP = true;
                                    Cn.Close();
                                    db.SaveChanges();

                                }
                                catch (Exception)
                                {


                                }


                            }


                            Conexion.Desconectar();

                        }
                        else
                        {
                            BitacoraErrores be = new BitacoraErrores();

                            be.Descripcion = "Error en la solicitud #" + Solicitud.id + " -> " + Conexion.Company.GetLastErrorDescription();
                            be.StackTrace = "Solicitud de Compra";
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

                        be.Descripcion = "Error en la solicitud #" + Solicitud.id + " -> " + ex1.Message;
                        be.StackTrace = ex1.StackTrace;
                        be.Fecha = DateTime.Now;

                        db.BitacoraErrores.Add(be);
                        db.SaveChanges();
                    }


                }
                else
                {
                    throw new Exception("Esta Solicitud NO existe");
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
                return Request.CreateResponse(HttpStatusCode.OK, Solicitud);
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
                GC.Collect();
                GC.WaitForPendingFinalizers();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [Route("api/SolicitudesCompras/SincronizarSAP")]
        public HttpResponseMessage GetSincronizarSAPDocumentosMasivos()
        {
            try
            {
                var Parametros = db.Parametros.FirstOrDefault();
                var Solicitudes = db.SolicitudCompra.Where(a => a.ProcesadaSAP == false).OrderByDescending(a => a.id).ToList();

                foreach (var Solicitud in Solicitudes)
                {
                    //Empieza a mandar a SAP
                    try
                    {

                        var OfertaAprobada = db.EncMovimiento.Where(a => a.id == Solicitud.idOfertaAprobada).FirstOrDefault();
                        if (Solicitud != null)
                        {
                            try
                            {
                                var client = (Documents)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseRequest);
                                //client.CardCode = string.IsNullOrEmpty(Solicitud.CardCode) ? throw new Exception("CardCode null") : Solicitud.CardCode.Trim();
                                client.DocDate = DateTime.Now;
                                client.DocDueDate = DateTime.Now.AddDays(7);
                                client.Comments = "Boletaje APP: " + Solicitud.id;
                                client.RequriedDate = DateTime.Now.AddDays(7);
                                var i = 0;
                                int ofertaVentaID = OfertaAprobada != null ? OfertaAprobada.DocEntry : 0; // ID de la oferta de venta en SAP
                                var Detalle = db.DetSolicitudCompra.Where(a => a.idEncabezado == Solicitud.id).ToList();
                                foreach (var item in Detalle)
                                {
                                    string itemCodeBuscado = item.ItemCode; // Código del artículo que queremos ligar

                                    client.Lines.SetCurrentLine(i);
                                    client.Lines.ItemCode = item.ItemCode; // Código del artículo
                                    client.Lines.Quantity = Convert.ToDouble(item.Cantidad);
                                    client.Lines.RequiredDate = DateTime.Now.AddDays(7); // Ejemplo: 7 días después de hoy

                                    // 🔹 Obtener línea de la oferta donde está el artículo
                                    int lineaOferta = ObtenerNumeroLineaOferta(Conexion.Company, ofertaVentaID, itemCodeBuscado);
                                    if (lineaOferta != -1)
                                    {
                                        client.Lines.BaseType = 23; // Oferta de Venta
                                        client.Lines.BaseEntry = ofertaVentaID; // ID de la oferta
                                        client.Lines.BaseLine = lineaOferta; //
                                    }
                                    client.Lines.Add();
                                    i++;
                                }



                                var respuesta = client.Add();

                                if (respuesta == 0)
                                {

                                    db.Entry(Solicitud).State = EntityState.Modified;
                                    Solicitud.DocEntry = 0;
                                    Solicitud.DocNum = 0;



                                    try
                                    {
                                        if (Solicitud.DocEntry == 0)
                                        {
                                            Solicitud.DocEntry = Convert.ToInt32(Conexion.Company.GetNewObjectKey());
                                            throw new Exception("");
                                        }

                                    }
                                    catch (Exception)

                                    {
                                        try
                                        {
                                            var conexion = g.DevuelveCadena(db);
                                            var filtroSQL = "Comments like '%Boletaje APP: " + Solicitud.id + "%'";
                                            var SQL = Parametros.SQLDocEntryDocs.Replace("@CampoBuscar", "DocEntry,DocNum").Replace("@Tabla", "OPRQ").Replace("@CampoWhere = @reemplazo", filtroSQL);

                                            SqlConnection Cn = new SqlConnection(conexion);
                                            SqlCommand Cmd = new SqlCommand(SQL, Cn);
                                            SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                                            DataSet Ds = new DataSet();
                                            Cn.Open();
                                            Da.Fill(Ds, "DocNum1");
                                            Solicitud.DocEntry = Convert.ToInt32(Ds.Tables["DocNum1"].Rows[0]["DocEntry"]);
                                            Solicitud.DocNum = Convert.ToInt32(Ds.Tables["DocNum1"].Rows[0]["DocNum"]);
                                            Solicitud.ProcesadaSAP = true;
                                            Cn.Close();
                                            db.SaveChanges();

                                        }
                                        catch (Exception)
                                        {


                                        }


                                    }


                                    Conexion.Desconectar();

                                }
                                else
                                {
                                    BitacoraErrores be = new BitacoraErrores();

                                    be.Descripcion = "Error en la solicitud #" + Solicitud.id + " -> " + Conexion.Company.GetLastErrorDescription();
                                    be.StackTrace = "Solicitud de Compra";
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

                                be.Descripcion = "Error en la solicitud #" + Solicitud.id + " -> " + ex1.Message;
                                be.StackTrace = ex1.StackTrace;
                                be.Fecha = DateTime.Now;

                                db.BitacoraErrores.Add(be);
                                db.SaveChanges();
                            }


                        }
                        else
                        {
                            throw new Exception("Esta Solicitud NO existe");
                        }
                    }
                    catch (Exception ex)
                    {
                        BitacoraErrores be = new BitacoraErrores();
                        be.Descripcion = ex.Message;

                        be.Fecha = DateTime.Now;
                        be.StackTrace = ex.StackTrace;
                        db.BitacoraErrores.Add(be);
                        db.SaveChanges();
                        Conexion.Desconectar();


                    }

                    //
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
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
                GC.Collect();
                GC.WaitForPendingFinalizers();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
        static int ObtenerNumeroLineaOferta(Company oCompany, int ofertaID, string itemCode)
        {
            try
            {
                G g = new G();
                g.GuardarTxt("BitacoraOferta.txt", ofertaID + " " + itemCode);
                ModelCliente db = new ModelCliente();

                var conexion = g.DevuelveCadena(db);

                var SQL = "SELECT LineNum FROM QUT1 WHERE DocEntry =" + ofertaID + " AND ItemCode = '" + itemCode + "'";

                SqlConnection Cn = new SqlConnection(conexion);
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open();
                Da.Fill(Ds, "DocNum1");

                g.GuardarTxt("BitacoraOferta.txt", Convert.ToInt32(Ds.Tables["DocNum1"].Rows[0]["LineNum"]).ToString());
                Cn.Close();
                return Convert.ToInt32(Ds.Tables["DocNum1"].Rows[0]["LineNum"]);
            }
            catch (Exception)
            {

                return -1;
            }






        }


    }
}