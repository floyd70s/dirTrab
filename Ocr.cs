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

        public string  PdfToText(string sPDFFilePath, string sTIFFFilePath, string sTXTFilePath, string sTIFFPath, string sTXTFPath)
        {
            PdfToTiff(sPDFFilePath, sTIFFFilePath);
            ExecuteMacCommand("tesseract -l eng "+ sTIFFFilePath + " " + sTXTFilePath);
            string sText = FileToText(sTXTFilePath + ".txt");
            cleanFolderMac(sTIFFPath, ".tiff");
            cleanFolderMac(sTXTFPath, ".txt");
            return sText;
        }

        public string cleanFolderMac(string sPath,string sExt)
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
        public static void ExecuteCommand(string command)
        {
            var processInfo = new ProcessStartInfo("terminal", "/c " + command);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            var process = Process.Start(processInfo);

            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                Console.WriteLine("output>>" + e.Data);
            process.BeginOutputReadLine();

            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                Console.WriteLine("error>>" + e.Data);
            process.BeginErrorReadLine();

            process.WaitForExit();

            Console.WriteLine("ExitCode: {0}", process.ExitCode);
            process.Close();

        }
        public static void PdfToTiff(string sPathPdf, string sPathTiff)
        {
            try
            {
                Console.WriteLine("Inicio Conversion a TIFF");
                GcPdfDocument doc = new GcPdfDocument();
                //var fs = new FileStream(Path.Combine("Resources", "c:\\tempdoc\\docs\\Trabajo.pdf"), FileMode.Open, FileAccess.Read);
                var fs = new FileStream(Path.Combine("Resources", sPathPdf), FileMode.Open, FileAccess.Read);
                doc.Load(fs, "");

                SaveAsImageOptions options = new SaveAsImageOptions();
                options.BackColor = Color.White;

                //The tiff file must have high resolution
                options.Resolution = 350;

                //Seteamos el rango de paginas, para suseso eliminamos la primera pagina
                int paginas = doc.Pages.Count;
                List<int> pages = new List<int>();
                for (int i = 0; i < paginas; i++)
                {
                    if (i > 0)
                        pages.Add(i + 1);
                }

                //Para suseso eliminamos la primera pagina, estableciendo un rango desde la pagina 2 hasta el final
                //GrapeCity.Documents.Common.OutputRange rango = new GrapeCity.Documents.Common.OutputRange(pages.ToArray());
                //doc.SaveAsTiff("c:\\tempdoc\\imgs\\suseso.tiff", rango, options);

                //Para la direccion del trabajo empezamos desde la pagina 1
                //doc.SaveAsTiff("c:\\tempdoc\\imgs\\trabajo.tiff", null, options);
                doc.SaveAsTiff(sPathTiff, null, options);
               
            }
            catch
            {
                
            }
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

        public static string FileToText(string sPathTxt) {
            Console.WriteLine("Inicio de lectura de Texto desde TXT");
            String sTextFile="";
            String line="";
            try
            {
                //Pass the file path and file name to the StreamReader constructor
                StreamReader sr = new StreamReader(sPathTxt);
                //Read the first line of text
                line = sr.ReadLine();
                //Continue to read until you reach end of file
                while (line != null)
                {
                    // Concat Text
                    sTextFile += line;

                    //Read the next line
                    line = sr.ReadLine();
                }
                //close the file
                sr.Close();
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
