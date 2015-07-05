using System;
using System.Linq;
using System.ServiceProcess;

namespace AlanAamy.Net.RxTimedWindow.Service
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        private static void Main(string[] args)
        {
            if (args.Contains("-console"))
            {
                Console.WriteLine("press 'q' to quit.");
                var app = new IntraDayReportService();
                app.Start();
                while (Console.ReadKey().KeyChar != 'q')
                {
                }
                app.Stop();
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new IntraDayReportService()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}