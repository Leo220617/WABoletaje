using SAPbobsCOM;
using SelectPdf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
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

    public class ActividadesController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G g = new G();

        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {


                IQueryable<Actividades> query = db.Actividades;

                if (filtro.Codigo1 != 0)
                {
                    query = query.Where(a => a.idLlamada == filtro.Codigo1);
                }

                var actividades = await query.ToListAsync();

                return Request.CreateResponse(HttpStatusCode.OK, actividades);

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }



        [Route("api/Actividades/Consultar")]
        public HttpResponseMessage GetOne([FromUri]int id)
        {
            try
            {



                var Actividades = db.Actividades.Where(a => a.id == id).FirstOrDefault();


                if (Actividades == null)
                {

                    throw new Exception("Esta Actividad no se encuentra registrado");


                }

                return Request.CreateResponse(HttpStatusCode.OK, Actividades);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        public HttpResponseMessage Post([FromBody] Actividades actividad)
        {
            try
            {
                if (actividad == null)
                {
                    throw new Exception("El objeto actividad viene null");
                }

                var Parametros = db.Parametros.FirstOrDefault();
                var Actividad = db.Actividades.Where(a => a.id == actividad.id).FirstOrDefault();

                if (Actividad == null)
                {
                    Actividad = new Actividades();
                    Actividad.TipoActividad = actividad.TipoActividad;
                    Actividad.idLlamada = actividad.idLlamada;
                    Actividad.FechaCreacion = DateTime.Now;
                    Actividad.Detalle = actividad.Detalle;
                    Actividad.DocEntry = 0;
                    Actividad.ProcesadaSAP = false;
                    Actividad.UsuarioCreador = actividad.UsuarioCreador;
                    Actividad.idLogin = actividad.idLogin;
                    db.Actividades.Add(Actividad);
                    db.SaveChanges();




                    try
                    {

                        var Llamada = db.LlamadasServicios.Where(a => a.id == Actividad.idLlamada).FirstOrDefault();
                        if (Llamada == null)
                        {
                            throw new Exception("No se puede crear actividad sin llamada asignada");
                        }
                        var activity = (Contacts)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oContacts);
                        // var clientw  = (SAPbobsCOM.DepositsService)companyService.GetBusinessService(ServiceTypes.DepositsService);

                        switch (Actividad.TipoActividad)
                        {
                            case 1:
                                {
                                    activity.Activity = BoActivities.cn_Conversation;
                                    break;
                                }
                            case 2:
                                {
                                    activity.Activity = BoActivities.cn_Meeting;
                                    break;
                                }
                            case 3:
                                {
                                    activity.Activity = BoActivities.cn_Task;
                                    break;
                                }
                            case 4:
                                {
                                    activity.Activity = BoActivities.cn_Note;
                                    break;
                                }
                            case 5:
                                {
                                    activity.Activity = BoActivities.cn_Campaign;
                                    break;
                                }
                            case 6:
                                {
                                    activity.Activity = BoActivities.cn_Other;
                                    break;
                                }

                        }

                        activity.Details = Actividad.Detalle.Length > 100 ? Actividad.Detalle.Substring(0, 99) : Actividad.Detalle;
                        activity.Notes = Actividad.Detalle;
                        activity.CardCode = Llamada.CardCode;
                        activity.SalesEmployee = db.Login.Where(a => a.id == Actividad.idLogin).FirstOrDefault() == null ? 0 : db.Login.Where(a => a.id == Actividad.idLogin).FirstOrDefault().EmpleadoSAP;
                        activity.HandledBy = Actividad.UsuarioCreador;


                        var respuesta = activity.Add();

                        if (respuesta == 0)
                        {

                            db.Entry(Actividad).State = EntityState.Modified;
                            Actividad.ProcesadaSAP = true;
                            Actividad.DocEntry = Convert.ToInt32(Conexion.Company.GetNewObjectKey());
                            db.SaveChanges();

                            var client = (ServiceCalls)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oServiceCalls);
                            if (client.GetByKey(Llamada.DocEntry.Value))
                            {
                                var conexion = g.DevuelveCadena(db);

                                var SQL = "select ClgCode from oclg where parentId = '" + Llamada.DocEntry + "' ";

                                SqlConnection Cn = new SqlConnection(conexion);
                                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                                DataSet Ds = new DataSet();
                                Cn.Open();
                                Da.Fill(Ds, "Actividades");

                                var CantidadContactos = Ds.Tables["Actividades"].Rows.Count;

                                Cn.Close();
                                Cn.Dispose();

                                if (CantidadContactos > 0)
                                {
                                    client.Activities.Add();
                                }
                                else
                                {
                                    var Linea = client.Activities.Count - 1;


                                    client.Activities.SetCurrentLine(Linea);
                                }
                                client.Activities.ActivityCode = Convert.ToInt32(Actividad.DocEntry);

                                var respuesta2 = client.Update();
                                if (respuesta2 == 0)
                                {
                                    Conexion.Desconectar();
                                }
                                else
                                {
                                    BitacoraErrores be = new BitacoraErrores();

                                    be.Descripcion = Conexion.Company.GetLastErrorDescription();
                                    be.StackTrace = "Insercion de Liga ACT - LL ";
                                    be.Fecha = DateTime.Now;

                                    db.BitacoraErrores.Add(be);
                                    db.SaveChanges();
                                    Conexion.Desconectar();
                                    throw new Exception(be.Descripcion);

                                }
                            }


                            Conexion.Desconectar();




                        }
                        else
                        {
                            BitacoraErrores be = new BitacoraErrores();

                            be.Descripcion = "Error en la actividad #" + Actividad.id + " -> " + Conexion.Company.GetLastErrorDescription();
                            be.StackTrace = "Actividad";
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

                        be.Descripcion = "Error en la actividad #" + Actividad.id + " -> " + ex1.Message;
                        be.StackTrace = ex1.StackTrace;
                        be.Fecha = DateTime.Now;

                        db.BitacoraErrores.Add(be);
                        db.SaveChanges();
                    }

                }
                else
                {
                    throw new Exception("Esta Actividad YA existe");
                }





                return Request.CreateResponse(HttpStatusCode.OK, Actividad);
            }
            catch (Exception ex)
            {
                BitacoraErrores bit = new BitacoraErrores();
                bit.Descripcion = ex.Message;
                bit.StackTrace = ex.StackTrace + " Caida general";
                bit.Fecha = DateTime.Now;
                bit.DocNum = "";

                db.BitacoraErrores.Add(bit);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [Route("api/Actividades/EnviarSAP")]
        public HttpResponseMessage PostEnviarSAP([FromBody] Actividades actividad)
        {
            try
            {
                var Parametros = db.Parametros.FirstOrDefault();
                var Actividad = db.Actividades.Where(a => a.id == actividad.id && a.ProcesadaSAP == false).FirstOrDefault();

                if (Actividad != null)
                {
                    try
                    {
                        var Llamada = db.LlamadasServicios.Where(a => a.id == Actividad.idLlamada).FirstOrDefault();
                        if (Llamada == null)
                        {
                            throw new Exception("No se puede crear actividad sin llamada asignada");
                        }
                        var activity = (Contacts)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oContacts);
                        // var clientw  = (SAPbobsCOM.DepositsService)companyService.GetBusinessService(ServiceTypes.DepositsService);

                        switch (Actividad.TipoActividad)
                        {
                            case 1:
                                {
                                    activity.Activity = BoActivities.cn_Conversation;
                                    break;
                                }
                            case 2:
                                {
                                    activity.Activity = BoActivities.cn_Meeting;
                                    break;
                                }
                            case 3:
                                {
                                    activity.Activity = BoActivities.cn_Task;
                                    break;
                                }
                            case 4:
                                {
                                    activity.Activity = BoActivities.cn_Note;
                                    break;
                                }
                            case 5:
                                {
                                    activity.Activity = BoActivities.cn_Campaign;
                                    break;
                                }
                            case 6:
                                {
                                    activity.Activity = BoActivities.cn_Other;
                                    break;
                                }

                        }

                        activity.Details = Actividad.Detalle.Length > 100 ? Actividad.Detalle.Substring(0, 99) : Actividad.Detalle;
                        activity.Notes = Actividad.Detalle;
                        activity.CardCode = Llamada.CardCode;
                        activity.SalesEmployee = db.Login.Where(a => a.id == Actividad.idLogin).FirstOrDefault() == null ? 0 : db.Login.Where(a => a.id == Actividad.idLogin).FirstOrDefault().EmpleadoSAP;
                        activity.HandledBy = Actividad.UsuarioCreador;


                        var respuesta = activity.Add();

                        if (respuesta == 0)
                        {

                            db.Entry(Actividad).State = EntityState.Modified;
                            Actividad.ProcesadaSAP = true;
                            Actividad.DocEntry = Convert.ToInt32(Conexion.Company.GetNewObjectKey());
                            db.SaveChanges();

                            var client = (ServiceCalls)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oServiceCalls);
                            if (client.GetByKey(Llamada.DocEntry.Value))
                            {
                                var conexion = g.DevuelveCadena(db);

                                var SQL = "select ClgCode from oclg where parentId = '" + Llamada.DocEntry + "' ";

                                SqlConnection Cn = new SqlConnection(conexion);
                                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                                DataSet Ds = new DataSet();
                                Cn.Open();
                                Da.Fill(Ds, "Actividades");

                                var CantidadContactos = Ds.Tables["Actividades"].Rows.Count;

                                Cn.Close();
                                Cn.Dispose();

                                if (CantidadContactos > 0)
                                {
                                    client.Activities.Add();
                                }
                                else
                                {
                                    var Linea = client.Activities.Count - 1;


                                    client.Activities.SetCurrentLine(Linea);
                                }
                                client.Activities.ActivityCode = Convert.ToInt32(Actividad.DocEntry);
                                var respuesta2 = client.Update();
                                if (respuesta2 == 0)
                                {
                                    Conexion.Desconectar();
                                }
                                else
                                {
                                    BitacoraErrores be = new BitacoraErrores();

                                    be.Descripcion = Conexion.Company.GetLastErrorDescription();
                                    be.StackTrace = "Insercion de Liga ACT - LL " + client.Activities.Count;
                                    be.Fecha = DateTime.Now;

                                    db.BitacoraErrores.Add(be);
                                    db.SaveChanges();
                                    Conexion.Desconectar();
                                    throw new Exception(be.Descripcion);

                                }
                            }


                            Conexion.Desconectar();




                        }
                        else
                        {
                            BitacoraErrores be = new BitacoraErrores();

                            be.Descripcion = "Error en la actividad #" + Actividad.id + " -> " + Conexion.Company.GetLastErrorDescription();
                            be.StackTrace = "Actividad";
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

                        be.Descripcion = "Error en la actividad #" + actividad.id + " -> " + ex1.Message;
                        be.StackTrace = ex1.StackTrace;
                        be.Fecha = DateTime.Now;

                        db.BitacoraErrores.Add(be);
                        db.SaveChanges();
                    }


                }
                else
                {
                    throw new Exception("Esta actividad NO existe");
                }
                return Request.CreateResponse(HttpStatusCode.OK, Actividad);
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


        [HttpGet]
        [Route("api/Actividades/EnviarSAPMasivo")]
        public HttpResponseMessage GetEnviarSAPMasivo()
        {
            try
            {
                var Parametros = db.Parametros.FirstOrDefault();
                var Actividades = db.Actividades.Where(a => a.ProcesadaSAP == false).Take(100).ToList();

                foreach (var item in Actividades)
                {
                    try
                    {
                        var Actividad = db.Actividades.Where(a => a.id == item.id && a.ProcesadaSAP == false).FirstOrDefault();

                        if (Actividad != null)
                        {
                            try
                            {
                                var Llamada = db.LlamadasServicios.Where(a => a.id == Actividad.idLlamada).FirstOrDefault();
                                if (Llamada == null)
                                {
                                    throw new Exception("No se puede crear actividad sin llamada asignada");
                                }
                                var activity = (Contacts)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oContacts);
                                // var clientw  = (SAPbobsCOM.DepositsService)companyService.GetBusinessService(ServiceTypes.DepositsService);

                                switch (Actividad.TipoActividad)
                                {
                                    case 1:
                                        {
                                            activity.Activity = BoActivities.cn_Conversation;
                                            break;
                                        }
                                    case 2:
                                        {
                                            activity.Activity = BoActivities.cn_Meeting;
                                            break;
                                        }
                                    case 3:
                                        {
                                            activity.Activity = BoActivities.cn_Task;
                                            break;
                                        }
                                    case 4:
                                        {
                                            activity.Activity = BoActivities.cn_Note;
                                            break;
                                        }
                                    case 5:
                                        {
                                            activity.Activity = BoActivities.cn_Campaign;
                                            break;
                                        }
                                    case 6:
                                        {
                                            activity.Activity = BoActivities.cn_Other;
                                            break;
                                        }

                                }

                                activity.Details = Actividad.Detalle.Length > 100 ? Actividad.Detalle.Substring(0, 99) : Actividad.Detalle;
                                activity.Notes = Actividad.Detalle;
                                activity.CardCode = Llamada.CardCode;
                                activity.SalesEmployee = db.Login.Where(a => a.id == Actividad.idLogin).FirstOrDefault() == null ? 0 : db.Login.Where(a => a.id == Actividad.idLogin).FirstOrDefault().EmpleadoSAP;
                                activity.HandledBy = Actividad.UsuarioCreador;


                                var respuesta = activity.Add();

                                if (respuesta == 0)
                                {

                                    db.Entry(Actividad).State = EntityState.Modified;
                                    Actividad.ProcesadaSAP = true;
                                    Actividad.DocEntry = Convert.ToInt32(Conexion.Company.GetNewObjectKey());
                                    db.SaveChanges();

                                    var client = (ServiceCalls)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oServiceCalls);
                                    if (client.GetByKey(Llamada.DocEntry.Value))
                                    {
                                        var conexion = g.DevuelveCadena(db);

                                        var SQL = "select ClgCode from oclg where parentId = '" + Llamada.DocEntry + "' ";

                                        SqlConnection Cn = new SqlConnection(conexion);
                                        SqlCommand Cmd = new SqlCommand(SQL, Cn);
                                        SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                                        DataSet Ds = new DataSet();
                                        Cn.Open();
                                        Da.Fill(Ds, "Actividades");

                                        var CantidadContactos = Ds.Tables["Actividades"].Rows.Count;

                                        Cn.Close();
                                        Cn.Dispose();

                                        if (CantidadContactos > 0)
                                        {
                                            client.Activities.Add();
                                        }
                                        else
                                        {
                                            var Linea = client.Activities.Count - 1;


                                            client.Activities.SetCurrentLine(Linea);
                                        }
                                        client.Activities.ActivityCode = Convert.ToInt32(Actividad.DocEntry);
                                        var respuesta2 = client.Update();
                                        if (respuesta2 == 0)
                                        {
                                            Conexion.Desconectar();
                                        }
                                        else
                                        {
                                            BitacoraErrores be = new BitacoraErrores();

                                            be.Descripcion = Conexion.Company.GetLastErrorDescription();
                                            be.StackTrace = "Insercion de Liga ACT - LL " + client.Activities.Count;
                                            be.Fecha = DateTime.Now;

                                            db.BitacoraErrores.Add(be);
                                            db.SaveChanges();
                                            Conexion.Desconectar();
                                            throw new Exception(be.Descripcion);

                                        }
                                    }


                                    Conexion.Desconectar();




                                }
                                else
                                {
                                    BitacoraErrores be = new BitacoraErrores();

                                    be.Descripcion = "Error en la actividad #" + Actividad.id + " -> " + Conexion.Company.GetLastErrorDescription();
                                    be.StackTrace = "Actividad";
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

                                be.Descripcion = "Error en la actividad #" + item.id + " -> " + ex1.Message;
                                be.StackTrace = ex1.StackTrace;
                                be.Fecha = DateTime.Now;

                                db.BitacoraErrores.Add(be);
                                db.SaveChanges();
                            }


                        }
                        else
                        {
                            throw new Exception("Esta actividad NO existe");
                        }
                    }


                    catch (Exception ex )
                    {

                        BitacoraErrores bit = new BitacoraErrores();
                        bit.Descripcion = ex.Message;
                        bit.StackTrace = ex.StackTrace;
                        bit.Fecha = DateTime.Now;
                        bit.DocNum = "";

                        db.BitacoraErrores.Add(bit);
                        db.SaveChanges();
                    }
                }


                return Request.CreateResponse(HttpStatusCode.OK);
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