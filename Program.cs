using System;
using System.Configuration;
using System.Data;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Collections;
using iText.Layout.Element;
using System.IO;

namespace dirTrab
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("****************************************");
            Console.WriteLine(" LEGAL BOT - DIRECCION DEL TRABAJO ");
            Console.WriteLine(" Version 1.0.0  14-11-2020");
            Console.WriteLine("****************************************");
            int range = Convert.ToInt32(ConfigurationManager.AppSettings["range"]);     // 365 days
            string PDFPath = ConfigurationManager.AppSettings["PDFPath"];               // Path to save PDF
            string TIFFPath = ConfigurationManager.AppSettings["TIFFPath"];               // Path to save PDF
            string TXTPath = ConfigurationManager.AppSettings["TXTPath"];               // Path to save PDF
            string iniDate = DateTime.Now.AddDays(-range).ToString("yyyy/MM/dd");       // initial search date
            string endDate = DateTime.Now.ToString("yyyy/MM/dd");                       // search end date
            string jResult = "-";                                                       // string for save JSON 
            string sResult = "";                                                        // string for print messages  
            int iCountCycle = 0;                                                        // records saved in the current cycle
            int iGeneralCount = 0;                                                      // number of records saved
            int iMainCount = 0;                                                         // total number of records analyzed
            int iNotSaved = 0;                                                          // total number of records not saved
            int iCountJur = 0;                                                          // count register for JUR_ADMIN
            int iCountNoNewsJur = 0;                                                    // unsaved record count for JUR_ADMIN
            JurAdmin miJurAdmin = new JurAdmin();                                       // new instance of JurAdmin Class
            dirTrab miDirTrab = new dirTrab();                                          // new instance of DIRTRAB Class
            Ocr miOCR = new Ocr();                                                      // new instance of OCR Class
            //-----------------------------------------------------------------------------------------------------------------------
            // get info from website SUSESO
            //-----------------------------------------------------------------------------------------------------------------------
            string URL = "https://www.dt.gob.cl/legislacion/1624/w3-search.php?_q=rango1_pnid_2294%3D" + iniDate + "%26rango2_pnid_2294%3D" + endDate;

            //-----------------------------------------------------------------------------------------------------------------------
            // we get all the data
            // we create a class that maps the structure of the json obtained from the suseso website --> suseso.cs
            //-----------------------------------------------------------------------------------------------------------------------
            string siteBase = dirTrab.extractWeb(URL);
            jResult = dirTrab.extractJson(siteBase);

            //-----------------------------------------------------------------------------------------------------------------------
            // we get all the data
            // we create a class that maps the structure of the json obtained from the suseso website --> suseso.cs
            //-----------------------------------------------------------------------------------------------------------------------
            var listElement = JsonConvert.DeserializeObject<List<dirTrab>>(jResult);

            //-----------------------------------------------------------------------------------------------------------------------
            // we get all the data
            //-----------------------------------------------------------------------------------------------------------------------
            foreach (dynamic ElementDirTrab in listElement)
            {
                //-----------------------------------------------------------------------------------------------------------------------
                // get records from SQLite database dirTrab 
                //-----------------------------------------------------------------------------------------------------------------------
                bool bExistAID = ElementDirTrab.validateAID();

                if (bExistAID)
                {
                    sResult = "El registro  \"{0}\" ya fue ingresado anteriormente." + ElementDirTrab.aid;
                }
                else
                {
                    sResult = ElementDirTrab.addElement();
                    if (sResult == "ok")
                    {
                        iGeneralCount++;
                        iCountCycle++;
                    }
                    else
                    {
                        iNotSaved++;
                    }
                }
                iMainCount++;
            }


            DataTable miDataTableSuseso = miDirTrab.getAll();                //get pending records from DIRTRAB - Status=0

            foreach (DataRow dtRow in miDataTableSuseso.Rows)
            {
                string sAID = dtRow[0].ToString();
                miDirTrab.aid = sAID;
                miJurAdmin.rol = dtRow[6].ToString();
                bool bExistAIDJur = miJurAdmin.validateRol();

                if (bExistAIDJur)
                {
                    Console.WriteLine("EL REGISTRO {0} YA EXISTE EN JUR_ADMINISTRATIVA", dtRow[0].ToString());
                    miDirTrab.update();
                    iCountNoNewsJur++;
                }
                else
                {
                    Console.WriteLine("NO  EXISTE EL REGISTRO {0} EN JUR_ADMINISTRATIVA", dtRow[0].ToString());
                    miJurAdmin.sumario = dtRow[2].ToString();
                    miJurAdmin.fechaSentencia = Convert.ToDateTime(dtRow[7]);
                    miJurAdmin.titulo = dtRow[1].ToString();
                    miJurAdmin.rol = dtRow[6].ToString();
                    miJurAdmin.fechaRegistro = Convert.ToDateTime(dtRow[4]);
                    miJurAdmin.linkOrigen = dtRow[0].ToString() + "_archivo_01.pdf";
                    miJurAdmin.tipoDocumento = Convert.ToInt32(ConfigurationManager.AppSettings["DocumentType"]);
                    miJurAdmin.linkOrigen = miDirTrab.savePdf(sAID);


                    string sURLDetail = "https://www.dt.gob.cl/legislacion/1624/w3-article-" + sAID + ".html";

                    // WIN VERSION:
                    //string sLocalPath = PDFPath + "\\" + sAID + "_archivo_01.pdf";
                    string sPDFLocal = PDFPath + sAID + "_archivo_01.pdf";
                    string sTIFFLocal = TIFFPath + sAID + "_archivo_01.tiff";
                    string sTXTLocal = TXTPath + sAID + "_archivo_01";

                    miJurAdmin.textoSentencia = miOCR.PdfToText(sPDFLocal, sTIFFLocal, sTXTLocal);

                    miJurAdmin.addElement();
                    miDirTrab.update();
                    iCountJur++;
                }
            }

            #region COMMENTS
            Console.WriteLine("-----------------------------------------------------------------");
            Console.WriteLine("-- PASO 1                                                        ");
            Console.WriteLine("-- FIN DE LA OBTENCION DE DATOS                                  ");
            Console.WriteLine("-- A LAS " + System.DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss"));
            Console.WriteLine("-- TOTAL DE REGISTROS REVISADOS :" + iMainCount);
            Console.WriteLine("-- TOTAL DE REGISTROS INGRESADOS PARA VALIDAR:" + iGeneralCount);
            Console.WriteLine("-- TOTAL DE REGISTROS NO INGRESADOS:" + iNotSaved);
            Console.WriteLine("-- PAGINAS RECORRIDAS :" + iCountCycle);
            Console.WriteLine("-----------------------------------------------------------------");
            Console.WriteLine("-----------------------------------------------------------------");
            Console.WriteLine("-- PASO 2                                                        ");
            Console.WriteLine("-- SE CRUZAN LOS DATOS ENTRE LA BD LOCAL Y CENTRAL               ");
            Console.WriteLine("-- SE GUARDAN LOS ARCHIVOS PDF                                   ");
            Console.WriteLine("-- SE GUARDA EN TXT EL CONTENIDO                                 ");
            Console.WriteLine("-- TOTAL DE REGISTROS REVISADOS :" + miDataTableSuseso.Rows.Count);
            Console.WriteLine("-- TOTAL DE REGISTROS INGRESADOS:" + iCountJur);
            Console.WriteLine("-- TOTAL DE REGISTROS NO INGRESADOS:" + iCountNoNewsJur);
            Console.WriteLine("-----------------------------------------------------------------");
            #endregion

            Console.ReadLine();
        }
    }
}
