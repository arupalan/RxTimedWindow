using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using Services;

namespace AlanAamy.Net.RxTimedWindow
{

    public class IntraDayReporter
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private IDisposable reporterDisposable;

        public IntraDayReporter()
        {
            XmlConfigurator.Configure();
        }
        public void Run(IPowerService svc,IScheduler scheduler,DateTime dtrunDate,string csvFilePath)
        {

            reporterDisposable = Observable.Interval(TimeSpan.FromSeconds(10), scheduler)
                .Select(i => Observable.FromAsync(() => svc.GetTradesAsync(dtrunDate)))
                .Subscribe(m =>
                {
                    StringBuilder sbpowerpositionLines = new StringBuilder();
                    sbpowerpositionLines.AppendLine("Local Time,Volume");
                    m.Catch((PowerServiceException ex) =>
                    {
                        
                        Log.Error(string.Format("PowerServiceException  {0}\tThread id ={1}", ex.Message, 
                            Thread.CurrentThread.ManagedThreadId));
                        return Observable.Empty<IEnumerable<PowerTrade>>();
                    })
                    .Retry(5)
                    .SelectMany(t => t.SelectMany(x => x.Periods))
                    .GroupBy(g => g.Period)
                    .Select(p => new { Period = p.Key, Volume = p.Sum(_ => _.Volume) })
                    .Subscribe(value =>
                    {
                        //var powerPeriods = value.SelectMany(t => t.Periods).GroupBy(g => g.Period).Select(
                        //    s => new PowerPeriod
                        //    {
                        //        Period = s.Key,
                        //        Volume = s.Sum(_ => _.Volume)
                        //    });
                        //foreach (var powerPeriod in powerPeriods)
                        //Console.WriteLine("Period {0}, Volume {1}\tCurrent Time : {2} Current Thread:{3}",
                        //    powerPeriod.Period,
                        //    powerPeriod.Volume,
                        //    DateTime.Now.ToLongTimeString(), Thread.CurrentThread.ManagedThreadId);
                        value.Volume.Subscribe(volume =>
                        {
                            sbpowerpositionLines.AppendLine(string.Format("{0},{1}", value.Period, volume));
                            Log.Info(string.Format("Period {0}, Volume {1}\tCurrent Time : {2} Current Thread:{3}",
                                value.Period,
                                volume,
                                DateTime.Now.ToLongTimeString(), Thread.CurrentThread.ManagedThreadId));
                        });

                    }, async () =>
                    {
                        using (var stream = new StreamWriter(Path.Combine(csvFilePath,"PowerPosition" + dtrunDate.ToString("_yyyyMMdd_HHmm") + ".csv")))
                        {
                           await stream.WriteAsync(sbpowerpositionLines.ToString());
                           await stream.FlushAsync();
                        }
                        Log.Debug("Completed" + " PowerPosition" + dtrunDate.ToString("_yyyyMMdd_HHmm") + ".csv" + "\n");
                        
                    });
                });
        }

        public void Stop()
        {
            if(reporterDisposable != null)
                reporterDisposable.Dispose();
        }
    }
}
