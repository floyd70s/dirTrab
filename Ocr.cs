using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using GrapeCity.Documents.Imaging;
using GrapeCity.Documents.Pdf;

namespace dirTrab
{
    public class Ocr
    {
        public Ocr()
        {
        }

        public string PdfToText(string sMode, string sPDFFilePath, string sTIFFFilePath, string sTXTFilePath, string sTIFFPath, string sTXTFPath, string sLanguage)
        {
            string sText = "";
            PdfToTiff(sPDFFilePath, sTIFFFilePath);

            if (sMode == "win")
            {
                Console.WriteLine("proceso antes de tesseract:" + System.Diagnostics.Process.GetCurrentProcess().Id);
                ExecuteCommand("C:\\Program Files\\Tesseract-OCR\\tesseract.exe ","-l " + sLanguage + " " + sTIFFFilePath + " " + sTXTFilePath );
                Console.WriteLine("proceso despues de tesseract" + System.Diagnostics.Process.GetCurrentProcess().Id);
                sText = FileToText(sTXTFilePath+".txt");
                cleanFolderWin(sTIFFPath, ".tiff");
                cleanFolderWin(sTXTFPath, ".txt");
            }
            else
            {
                ExecuteMacCommand("tesseract -l " + sLanguage + " " + sTIFFFilePath + " " + sTXTFilePath);
                sText = FileToText(sTXTFilePath + ".txt");
                cleanFolderMac(sTIFFPath, ".tiff");
                cleanFolderMac(sTXTFPath, ".txt");
            }

            return sText;
        }

        public string cleanFolderMac(string sPath, string sExt)
        {
            try
            {
                ExecuteMacCommand("rm -f " + sPath + sExt);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                return "no fue posible borrar los directorios temporales de conversion OCR.";
            }
            finally
            {
                Console.WriteLine("Error en la limpieza de directorios.");
            }
            return "Borrado de  directorios temporales de conversion OCR OK";
        }

        public string cleanFolderWin(string sPath, string sExt)
        {
            try
            {
                ExecuteCommand("CMD"," DEL /F /A " + sPath + sExt);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                return "no fue posible borrar los directorios temporales de conversion OCR.";
            }
            finally
            {
                Console.WriteLine("Error en la limpieza de directorios.");
            }
            return "Borrado de  directorios temporales de conversion OCR OK";
        }

        public static void ExecuteMacCommand(string command)
        {
            Console.WriteLine("Inicio Conversion a TXT");
            Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = "/bin/bash";
            proc.StartInfo.Arguments = "-c \" " + command + " \"";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();

            while (!proc.StandardOutput.EndOfStream)
            {
                Console.WriteLine(proc.StandardOutput.ReadLine());
            }
        }

        public static void ExecuteCommand(string sProgram, string command)
        {    
            Console.WriteLine("Inicio Conversion OCR a TXT");
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.UseShellExecute = false;
            psi.Arguments = command;
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Normal;
            psi.FileName = @sProgram;
            Process.Start(psi);
        }

       

        public static void PdfToTiff(string sPathPdf, string sPathTiff)
        {
            try
            {
                Console.WriteLine("Inicio Conversion a TIFF");
                GcPdfDocument doc = new GcPdfDocument();

                var fs = new FileStream(Path.Combine("Resources", sPathPdf), FileMode.Open, FileAccess.Read);
                doc.Load(fs, "");

                SaveAsImageOptions options = new SaveAsImageOptions();
                options.BackColor = Color.White;

                //The tiff file must have high resolution
                options.Resolution = 350;

                //Set range for page
                int paginas = doc.Pages.Count;
                List<int> pages = new List<int>();
                for (int i = 0; i < paginas; i++)
                {
                    if (i > 0)
                        pages.Add(i + 1);
                }

                doc.SaveAsTiff(sPathTiff, null, options);
            }
            catch (Exception ex)
            {
                // handle other web exceptions
                Console.WriteLine("No fue posible descargar el archivo a TIFF  Error: {1}",  ex.Message);
            }
        }
       

        public static string FileToText(string sPathTxt)
        {
            Console.WriteLine("Inicio de lectura de Texto desde TXT");
            String sTextFile = "";

            try
            {
                Console.WriteLine("sPathTxt:" + sPathTxt);
                Console.WriteLine("proceso dentro de la funcion FiletoText:"+ System.Diagnostics.Process.GetCurrentProcess().Id);
                sTextFile = System.IO.File.ReadAllText(@sPathTxt.Trim());
                
                return sTextFile;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                return "no disponible";
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }
        }
    }
}
