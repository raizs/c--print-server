using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Printing;
using System.IO;
using RawPrint;
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
                while(true)
                {
                    Console.WriteLine("Waiting...");
                    foreach (string p in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
                    {
                        Console.WriteLine(p);
                    }
                    var context = listener.GetContext();
                    string msg = "hello";
                    //context.Response.ContentLength64 = msg;
                    var method = context.Request.HttpMethod;
                    Console.WriteLine(method);
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
                        if (!request.HasEntityBody)
                        {
                            Console.WriteLine("No client data was sent with the request.");
                            return;
                        }
                        Console.WriteLine("Before");
                        try
                        {
                            System.IO.Stream body = request.InputStream;
                            //string s = reader.ReadToEnd();
                            var fileStream = File.OpenWrite("TempFile.pdf");

                            //request.InputStream.Seek(0, SeekOrigin.Begin);
                            body.CopyTo(fileStream);
                            fileStream.Close();
                            body.Close();
                        }
                        finally
                        {
                            string filePath = @"C:\Users\arthu\source\repos\ConsoleApp3\ConsoleApp3\bin\Debug\TempFile.pdf";
                            string filename = "pdf-sample.pdf";
                            string printerName = "Brother HL-L5102DW Printer";
                            Console.WriteLine("Sending to printer...");
                            IPrinter printer = new Printer();
                            printer.PrintRawFile(printerName, filePath, filename);

                            context.Response.StatusCode = (int)HttpStatusCode.OK;
                            using(Stream stream = context.Response.OutputStream)
                            {
                                using (StreamWriter writer = new StreamWriter(stream))
                                {
                                    writer.Write(msg);
                                }
                            }
                        }
                    }
                }
            }
            catch(WebException e)
            {
                Console.WriteLine(e.Status);
            }
            //
            //System.Threading.Thread.Sleep(10000);
        }
    }
}
