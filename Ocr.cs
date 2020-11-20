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

        public string PdfToText(string sMode, string sPDFFilePath, string sTIFFFilePath, string sTXTFilePath, string sTIFFPath, string sTXTFPath)
        {
            string sText = "";
            PdfToTiff(sPDFFilePath, sTIFFFilePath);
            if (sMode == "win")
            {
                ExecuteCommand("tesseract -l eng " + sTIFFFilePath + " " + sTXTFilePath);
                sText = FileToText(sTXTFilePath + ".txt");
                cleanFolderWin(sTIFFPath, ".tiff");
                cleanFolderWin(sTXTFPath, ".txt");
            }
            else
            {
                ExecuteMacCommand("tesseract -l eng " + sTIFFFilePath + " " + sTXTFilePath);
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
                ExecuteCommand(" DEL /F /A " + sPath + sExt);
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
            var processInfo = new ProcessStartInfo("cmd", "/c " + command);
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

        public static string FileToText(string sPathTxt)
        {
            Console.WriteLine("Inicio de lectura de Texto desde TXT");
            String sTextFile = "";
            String line = "";
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
