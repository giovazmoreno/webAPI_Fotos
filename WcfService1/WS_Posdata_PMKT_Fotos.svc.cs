using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using WS_Posdata_PMKT_Fotos.Models;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using WS_Posdata_PMKT_Fotos.Models.Object;
//using WS_Posdata_PMKT_Fotos.Models.Response;
using WS_Posdata_PMKT_Fotos.Models.Request;
using WS_Posdata_PMKT_Fotos.Helpers;
using WS_Posdata_PMKT_Fotos.Models.Base;
using System.IO;


namespace WS_Posdata_PMKT_Fotos
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de clase "Service1" en el código, en svc y en el archivo de configuración.
    // NOTE: para iniciar el Cliente de prueba WCF para probar este servicio, seleccione Service1.svc o Service1.svc.cs en el Explorador de soluciones e inicie la depuración.
    public class WS_Posdata_PMKT_Fotos : IWS_Posdata_PMKT_Fotos
    {
        private string dbConnection = ConfigurationManager.ConnectionStrings["dbConnection"].ConnectionString.ToString();

        #region WSMETODS

        public ResponseBase FileReception(FileReceptionRequest request)
        {

            ResponseBase response = new ResponseBase();
            Compression compression = new Compression();

            DataTable dt = new DataTable();
            try
            {

                string idsync = request.IdSync;
                string idstore = request.IdStore;
                string typefile = request.TypeFile;
                List<DataFile> filedatalist = request.Files;



                bool idsyncExist = ValidateIdSync(idsync, idstore);
                bool typefileValid = typefile.Contains("F001"); //evidencia fotografica .jpg o .jpeg
                string pathSaveFile = ConfigurationManager.AppSettings["URLFilesSaved"].ToString();
                string directoryName = DateTime.Now.ToString("yyyy") + @"\" + DateTime.Now.ToString("yyyyMM") + @"\" + idsync;
                pathSaveFile = Path.Combine(pathSaveFile, directoryName);


                if (idsyncExist && typefileValid)
                {

                    try
                    {
                        if (!Directory.Exists(pathSaveFile))
                        {
                            // Try to create the directory.
                            DirectoryInfo di = Directory.CreateDirectory(pathSaveFile);
                        }
                        foreach (DataFile file in filedatalist)
                        {
                            if (file.Name.Contains(".jpg") || file.Name.Contains(".jpeg"))

                            {
                                //decompress , convert byte[] to file(image) and save file, save into databese

                                byte[] decompressFile = compression.Decompress(file.File);
                                byte[] img = default(byte[]);


                                using (FileStream fs = new FileStream(Path.Combine(pathSaveFile, file.Name), FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
                                {

                                    fs.Write(decompressFile, 0, decompressFile.Length);

                                }


                                using (FileStream FS = new FileStream(Path.Combine(pathSaveFile, file.Name), FileMode.Open, FileAccess.Read))
                                {
                                    
                                    img = new byte[FS.Length]; //create a byte array with size of user select file stream length
                                    FS.Read(img, 0, Convert.ToInt32(FS.Length));
                                }

                               /* FileStream FS = new FileStream(Path.Combine(pathSaveFile, file.Name), FileMode.Open, FileAccess.Read); //create a file stream object associate to user selected file 
                                byte[] img = new byte[FS.Length]; //create a byte array with size of user select file stream length
                                FS.Read(img, 0, Convert.ToInt32(FS.Length));}*/

                                SaveDetailIntoDB(idsync, idstore, pathSaveFile, file.Name, file.TypeOfEvidence, file.FileCreationDate, img);
                                //proceso de guardado
                                response.Code = "S001";
                                response.Success = true;
                                response.Message = "Proceso exitoso. ";
                            }
                            else
                            {
                                response.Code = "A002";
                                response.Success = false;
                                response.Message = "Error al determinar el tipo de archivo permitido.";
                                break;
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        response.Code = "A000";
                        response.Success = false;
                        response.Message = "Error en el proceso.  " + ex.Message;
                    }

                }


                else
                {
                    response.Code = "A001";
                    response.Success = false;
                    response.Message = "Clave de sincronización no válida o Error al determinar el tipo de archivo por recibir.";
                }

            }
            catch (Exception ex)
            {
                response.Code = "A000";
                response.Success = false;
                response.Message = "Error en el proceso. " + ex.StackTrace + ". " + ex.Message;
            }

            return response;

        }


        public FileReceptionRequest ObtieneListaImages()
        {
            FileReceptionRequest frr = new FileReceptionRequest();

            List<DataFile> listdatafile = new List<DataFile>();
            Compression compression = new Compression();

            try
            {
                //string directorio = @"C:\Users\gvazquez\Documents\TestDIrectorySaveFile\15396402931_20190131\ImagenesPosdata1\";
                string directorio = @"C:\Users\Giovanna\Pictures\Saved Pictures\";

                //string[] ficheros = Directory.GetFiles(directorio);
                foreach (string file in Directory.GetFiles(directorio))
                {
                    DataFile df = new DataFile();
                    byte[] bytes = compression.Compress(File.ReadAllBytes(file));
                    df.File = bytes;
                    string[] s = file.Split('\\');
                    df.Name = s[s.Length - 1];
                    df.TypeOfEvidence = "E1";
                    df.FileCreationDate = DateTime.Now;
                    listdatafile.Add(df);
                }

                frr.IdStore = "33";
                frr.IdSync = "154916079533";
                frr.TypeFile = "F001";
                frr.Files = listdatafile;
            }
            catch (Exception ex)
            {
                frr.TypeFile = ex.Message;
                frr.IdStore = ex.StackTrace;
            }

            return frr;
        }
        #endregion




        #region HELPERMETODS

        private bool ValidateIdSync(string idsync, string idstore)
        {

            bool valid = false;
            object id = string.Empty;

            try
            {
                using (SqlConnection connection = new SqlConnection(dbConnection))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("spWS_PosData_PMKTData_Fotos", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("Action", "ValidateidSync"));
                    command.Parameters.Add(new SqlParameter("nSIN_clave", idsync));
                    command.Parameters.Add(new SqlParameter("cTIE_clave", idstore));

                    try
                    {
                        id = command.ExecuteScalar();
                    }
                    catch (Exception exception)
                    {
                        throw new Exception("Sincronizacion:" + idsync + "Tienda:" + idstore);
                    }
                };
                if (id != null)
                {
                    if (!string.IsNullOrEmpty(id.ToString()))
                    {
                        valid = true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Sincronizacion:" + idsync + "Tienda:" + idstore );
            }
            
            return valid;
        }

        private void SaveDetailIntoDB(string idsync, string idstore, string rutafisica, string nombrefoto, string TypeOfEvidence, DateTime fechacaptura, byte[] imagen)
        {
            try
            {
                string url = ConfigurationManager.AppSettings["URLWebSite"].ToString().TrimEnd('/')  + "/" + DateTime.Now.ToString("yyyy") + "/" + DateTime.Now.ToString("yyyyMM") + "/" + idsync + "/" + nombrefoto; 
                using (SqlConnection connection = new SqlConnection(dbConnection))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("spWS_PosData_PMKTData_Fotos", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("Action", "SavePhotographicEvidence"));
                    command.Parameters.Add(new SqlParameter("nSIN_clave", idsync));
                    command.Parameters.Add(new SqlParameter("cTIE_clave", idstore));
                    command.Parameters.Add(new SqlParameter("cEVF_nombrefoto", nombrefoto));
                    command.Parameters.Add(new SqlParameter("cEVF_rutafisica", Path.Combine(rutafisica,nombrefoto)));
                    command.Parameters.Add(new SqlParameter("cEVF_url", url));
                    command.Parameters.Add(new SqlParameter("dEVF_fechatoma", fechacaptura));
                    command.Parameters.Add(new SqlParameter("nTEF_clave", TypeOfEvidence));
                    command.Parameters.Add(new SqlParameter("nEVF_foto", imagen));
                    command.ExecuteNonQuery();
                };


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #endregion
    }
}
