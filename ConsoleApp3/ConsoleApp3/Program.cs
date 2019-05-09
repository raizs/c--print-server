using System;
using System.IO;
using System.Net;

namespace ConsoleApp3
{
    class Program
    {
        static void Main(string[] args)
        {
            HttpListener listener = null;
            try
            {
                listener = new HttpListener();
                listener.Prefixes.Add("http://localhost:8080/print/");
                listener.Start();
                foreach (string p in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
                {
                    Console.WriteLine(p);
                }
                while (true)
                {
                    Console.WriteLine("Waiting...");
                    var context = listener.GetContext();
                    var method = context.Request.HttpMethod;
                    if (method != "POST")
                    {
                        context.Response.StatusCode = 403;
                        using (Stream stream = context.Response.OutputStream)
                        {

                            using (StreamWriter writer = new StreamWriter(stream))
                            {
                                writer.Write("Wrong Method");
                            }
                        }

                    }
                    else
                    {
                        var request = context.Request;
                        var type = request.QueryString["type"];

                        if (!request.HasEntityBody)
                        {
                            Console.WriteLine("No client data was sent with the request.");
                            return;
                        }
                        Console.WriteLine("Before");
                        try
                        {
                            using (System.IO.FileStream output = new System.IO.FileStream(@"C:\Users\arthu\source\repos\ConsoleApp3\ConsoleApp3\bin\Debug\TempFile.pdf", FileMode.Create))
                            {
                                request.InputStream.CopyTo(output);
                            }
                        }
                        finally
                        {
                            string filePath = @"C:\Users\arthu\source\repos\ConsoleApp3\ConsoleApp3\bin\Debug\TempFile.pdf";
                            string printerName = "";
                            if (type == "paper")
                            {
                                printerName = " \"Brother HL-L5102DW Printer\"";
                            } 
                            else if(type == "label")
                            {
                                printerName = " \"Brother QL-810W\"";
                            }
                            Console.WriteLine("Sending to printer...");
                            System.Diagnostics.Process.Start("PDFtoPrinter.exe", filePath + printerName);

                            context.Response.StatusCode = (int)HttpStatusCode.OK;
                            using (Stream stream = context.Response.OutputStream)
                            {
                                using (StreamWriter writer = new StreamWriter(stream))
                                {
                                    writer.Write("Success");
                                }
                            }
                        }
                    }
                }
            }
            catch (WebException e)
            {
                Console.WriteLine(e.Status);
            }
        }

    }
}