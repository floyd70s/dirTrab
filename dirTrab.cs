﻿using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.Data;
//using Microsoft.Data.Sqlite;
using System.Net;
using System.IO;
using System.Web;
using System.Text;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;

namespace dirTrab
{
    public class dirTrab
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region JSON PROPERTIES
        [JsonProperty("property-value_2302_pid")]
        public string propertyValue_2302_pid { get; set; }

        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        //[JsonProperty("property-value_2504_pvid")]
        //public string propertyValue_2504_pvid { get; set; }

        [JsonProperty("using_cids")]
        public string using_cids { get; set; }

        //[JsonProperty("extended-property-value_pvid")]
        //public string extendedPropertyValue_pvid { get; set; }

        [JsonProperty("cid")]
        public string cid { get; set; }

        [JsonProperty("property-value_2302_pvid")]
        public int propertyValue_2302_pvid { get; set; }

        [JsonProperty("binary_id")]
        public string binary_id { get; set; }

        [JsonProperty("hl1")]
        public string hl1 { get; set; }

        [JsonProperty("hl2")]
        public string hl2 { get; set; }

        [JsonProperty("description")]
        public string description { get; set; }

        [JsonProperty("iid")]
        public string iid { get; set; }

        [JsonProperty("property-value_2302_name")]
        public string documentType { get; set; }

        [JsonProperty("score")]
        public string score { get; set; }

        [JsonProperty("abstract")]
        public string abstrac { get; set; }

        [JsonProperty("keywords")]
        public string keywords { get; set; }

        [JsonProperty("property-value_2294_iso8601")]
        public DateTime sentenceDate { get; set; }

        //[JsonProperty("property-value_2495_pvid")]
        //public string propertyValue_2495_pvid { get; set; }

        [JsonProperty("title")]
        public string title { get; set; }

        [JsonProperty("pid")]
        public string pid { get; set; }

        [JsonProperty("property-value_2495_name")]
        public string propertyValue_2495_name { get; set; }

        [JsonProperty("property-value_2504_pid")]
        public string propertyValue_2504_pid { get; set; }

        [JsonProperty("property-value_2504_name")]
        public string propertyValue_2504_name { get; set; }

        [JsonProperty("aid")]
        public string aid { get; set; }

        [JsonProperty("property-value_2495_pid")]
        public string propertyValue_2495_pidv { get; set; }
        #endregion

        private string conStringSQLite = ConfigurationManager.ConnectionStrings["conStringSQLite"].ConnectionString;
        private DataManager _myDataManager;
        private DataManager myDataManager
        {
            get => _myDataManager;
            set => _myDataManager = new DataManager(conStringSQLite);
        }

        public dirTrab()
        {

        }
        /// <summary>
        /// extract text from "DIRECCION DEL TRABAJO" Web Site
        /// </summary>
        /// <param name="URL">URL DIR TRAB Web</param>
        /// <returns>string with the DirTrab dump</returns>
        public static string extractWeb(string URL)
        {
            try
            {
                 string docImportSrc = string.Empty;
                 int iIndex = URL.IndexOf("=");
                 string sCookieValue = URL.Substring(iIndex);
                
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(new Cookie("search_q", sCookieValue,"/", ".www.dt.gob.cl"));
                request.CookieContainer.Add(new Cookie("search_fullresult", "1", "/", ".www.dt.gob.cl"));
                request.Method = "GET";

                var webResponse = request.GetResponse();
                var webStream = webResponse.GetResponseStream();
                var responseReader = new StreamReader(webStream);
                var response = responseReader.ReadToEnd();
                responseReader.Close();

                return response;

            }
            catch (Exception ex)
            {
                Console.WriteLine("[Fatal Error]\r\n" + ex.Message + "\r\n" + ex.StackTrace + "\r\n" + ex.InnerException + "\r\n" + ex.Source);
                Console.WriteLine("........Fail");
                return "error";
            }
        }

        /// <summary>
        /// method for extract JSON from website string
        /// </summary>
        /// <param name="siteBase"></param>
        /// <returns></returns>
        public static string extractJson(string siteBase)
        {
            int iniJson = siteBase.IndexOf("\"results\": [");
            int endJson = siteBase.IndexOf("properties");
            return siteBase.Substring(iniJson + 11, endJson - iniJson - 14);
        }

        public static string SaveToFile(string sInfo, string path)
        {
            try
            {
                File.WriteAllText(path, sInfo);
                return "ok";
            }
            catch
            {
                return "error";
            }
        }


        /// <summary>
        ///  validate AID
        ///  if the record exists, then it returns true
        /// </summary>
        /// <returns></returns>
        public bool validateAID()
        {
            DataTable dtTemp;
            this.myDataManager = new DataManager(this.conStringSQLite);

            string rol = this.hl1.Substring(8).Trim();
            string sSQL = "select AID from DIRTRAB where AID=" + this.aid + " LIMIT 1;";
            dtTemp = this.myDataManager.getData(sSQL);
            if (dtTemp.Rows.Count > 0)
            {
                if (dtTemp.Rows[0][0].ToString() != "")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// insert instance of suseso to SQLite Database.
        /// 
        /// status:
        ///     0 pending
        ///     1 ok
        /// </summary>
        /// <returns> OK/Error </returns>
        public string addElement()
        {
            try
            {
                this.myDataManager = new DataManager(this.conStringSQLite);
                string SQL = "INSERT INTO  'DIRTRAB' ('aid', 'title', 'abstract', 'name', 'insertDate', 'status', 'sentenceDate','orden','documentType') VALUES (" +
                            this.aid + "," +
                            "'" + this.hl1 + "'," +
                            "'" + this.abstrac + "'," +
                            "'" + this.name + "'," +
                            "'" + System.DateTime.Now + "'," +
                            "0," +
                            "'" + this.sentenceDate.ToString("yyyy/MM/dd") + "'," +
                            "'" + this.hl1.Trim() + "'," +
                            "'" + this.documentType + "');";

                string sMsg = myDataManager.setData(SQL);
                if (sMsg == "ok")
                {
                    Console.WriteLine("El registro  \"{0}\" ingresado correctamente.", this.aid);
                    return "ok";
                }
                else
                {
                    return "error en el ingreso";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Fatal Error]\r\n" + ex.Message + "\r\n" + ex.StackTrace + "\r\n" + ex.InnerException + "\r\n" + ex.Source);
                Console.WriteLine("........Fail");
                log.Error("[Fatal Error]\r\n" + ex.Message + "\r\n" + ex.StackTrace + "\r\n" + ex.InnerException + "\r\n" + ex.Source);
                return "error";
            }
        }

       

        public DataTable getAll()
        {
            this.myDataManager = new DataManager(this.conStringSQLite);
            string SQL = "select aid,title,abstract,name,insertDate,status,orden,sentenceDate,documentType from DIRTRAB where status=0";
            DataTable miDataTable = myDataManager.getDataTemp(SQL);
            return miDataTable;
        }


        /// <summary>
        /// Obtain  records from DIRTRAB table BY status 
        /// status:
        ///     0 pending
        ///     1 ok
        ///     2 pending pdf
        /// </summary>
        /// <returns>dataset with records from SUSESO with status 0 - pending </returns>
        public DataTable getByStatus(int iStatus)
        {
            this.myDataManager = new DataManager(this.conStringSQLite);
            //string SQL = "select aid,title,abstract,name,status,rol from SUSESO where status=0";
            string SQL = "select aid,title,abstract,name,insertDate,status,orden,sentenceDate from DIRTRAB where status=" + iStatus;
            DataTable miDataTable = myDataManager.getDataTemp(SQL);
            return miDataTable;
        }



        /// <summary>
        /// update DIRTRAB table
        /// status:
        ///     0 pending
        ///     1 ok
        ///     2 pending pdf
        /// </summary>
        /// <param name="iStatus"></param>
        /// <returns></returns>
        public string update(int iStatus)
        {
            try
            {
                this.myDataManager = new DataManager(this.conStringSQLite);
                string SQL = "update DIRTRAB set status=" + iStatus + "  where aid=" + this.aid;

                string sMsg = myDataManager.setData(SQL);
                if (sMsg == "ok")
                {
                    Console.WriteLine("El registro  \"{0}\" actualizado correctamente.", this.aid);
                    return "ok";
                }
                else
                {
                    return "error en la actualizacion.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Fatal Error]\r\n" + ex.Message + "\r\n" + ex.StackTrace + "\r\n" + ex.InnerException + "\r\n" + ex.Source);
                Console.WriteLine("........Fail");
                log.Error("[Fatal Error]\r\n" + ex.Message + "\r\n" + ex.StackTrace + "\r\n" + ex.InnerException + "\r\n" + ex.Source);

                return "error";
            }
        }


        /// <summary>
        /// save PDF from website SUSESO with AID
        /// </summary>
        /// <param name="PDFPath">path for save PDF file</param>
        /// <param name="sAid">unique ID for PDF file</param>
        /// <returns> string with link </returns>
        public string savePdf(string sAid, string PDFPath, string sMode)
        {

            string sUrlPDF = "https://www.dt.gob.cl/legislacion/1624/articles-" + sAid + "_recurso_pdf.pdf";
            string sLocalPDF = "";
            if (sMode == "win")
            {
                sLocalPDF = PDFPath + "\\" + sAid + "_archivo_01.pdf";
            }
            else
            {
                sLocalPDF = PDFPath + sAid + "_archivo_01.pdf";
            }
            Console.WriteLine("salida de sLocalPDF: " + sLocalPDF);

            try
            {
                var request = WebRequest.Create(sUrlPDF);
                using (var response = request.GetResponse())
                using (var responseStream = response.GetResponseStream())
                {
                    // Process the stream
                }
            }
            catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound)
            {
                // handle 404 exceptions
                //Not found
                Console.WriteLine("El archivo {0} no pudo ser encontrado", sUrlPDF);
                sUrlPDF = "no disponible";
                log.Info("archivo no encontrado "+ sUrlPDF);
                return sUrlPDF;
            }
            catch (WebException ex)
            {
                // handle other web exceptions
                Console.WriteLine("No fue posible descargar el archivo {0} Error: {1}", sAid, ex.Message);
            }
            if (sUrlPDF != "no disponible")
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadFile(sUrlPDF, sLocalPDF);
                    return sUrlPDF;
                }
            }
            else
            {
                return sUrlPDF;
            }
        }

        /// <summary>
        /// Extract text from PDFFile
        /// </summary>
        /// <param name="filePath">local file path </param>
        /// <returns>string with the content of PDF file</returns>
        public string extractTextFromPDF(string filePath)
        {
            try
            {
                FileInfo file = new FileInfo(filePath);

                PdfReader pdfReader = new PdfReader(file);
                PdfDocument pdfDoc = new PdfDocument(pdfReader);

                string pageContent = "";
                for (int page = 1; page <= pdfDoc.GetNumberOfPages(); page++)
                {
                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                    pageContent = pageContent + "-" + PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page), strategy);
                }
                pdfDoc.Close();
                pdfReader.Close();
                return pageContent;

            }
            catch (Exception ex)
            {
                Console.WriteLine("[Fatal Error]\r\n" + ex.Message + "\r\n" + ex.StackTrace + "\r\n" + ex.InnerException + "\r\n" + ex.Source);
                Console.WriteLine("........Fail");
                log.Error("[Fatal Error]\r\n" + ex.Message + "\r\n" + ex.StackTrace + "\r\n" + ex.InnerException + "\r\n" + ex.Source);
                return "";
            }
        }

    }
}
