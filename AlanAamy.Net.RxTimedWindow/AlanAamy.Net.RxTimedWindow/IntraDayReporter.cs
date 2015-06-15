using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using Services;

namespace AlanAamy.Net.RxTimedWindow
{
    public sealed class DateTimeHelper
    {
        private static readonly TimeZoneInfo GmtTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        public  readonly Dictionary<int, string> UtcIndexToLocalHourMap = new Dictionary<int, string>();

        public DateTimeHelper(DateTime runDate)
        {
            DateTime dateTimeStart = new DateTime(runDate.Year, runDate.Month, runDate.Day, 0, 0, 0, DateTimeKind.Unspecified).Date.AddHours(-1.0);
            DateTime dateTimeEnd = dateTimeStart.AddDays(1.0);
            DateTime dateTimeUtcStart = TimeZoneInfo.ConvertTimeToUtc(dateTimeStart, GmtTimeZoneInfo);
            DateTime dateTimeUtcEnd = TimeZoneInfo.ConvertTimeToUtc(dateTimeEnd, GmtTimeZoneInfo);
            int index = 0;
            for (DateTime dateTime = dateTimeUtcStart; dateTime < dateTimeUtcEnd; dateTime = dateTime.AddHours(1.0))
            {
                UtcIndexToLocalHourMap.Add(++index, TimeZoneInfo.ConvertTimeFromUtc(dateTime, GmtTimeZoneInfo).ToString(@"HH:00"));
            }           
        }
    }

    public interface ISerializer
    {
        void Clear();
        void AddPeriod(PowerPeriod period);
        Task<IEnumerable<PowerPeriod>> SerializeAsync();
    }

    public sealed class Serializer : ISerializer
    {
        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void AddPeriod(PowerPeriod period)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<PowerPeriod>> SerializeAsync()
        {
            throw new NotImplementedException();
        }
    }
    
    public class IntraDayReporter
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private IDisposable reporterDisposable;

        public IntraDayReporter()
        {
            XmlConfigurator.Configure();
        }


        public void Run(IPowerService svc, IScheduler scheduler, DateTime dtrunDate, int observeIntervalInMinutes, StringBuilder sbpowerpositionLines, string csvFilePath)
        {
            var dateTimeHelper = new DateTimeHelper(dtrunDate);
            reporterDisposable = Observable.Interval(TimeSpan.FromMinutes(observeIntervalInMinutes), scheduler)
                .Select(i => Observable.FromAsync(() => svc.GetTradesAsync(dtrunDate)))
                .Subscribe(m =>
                {
                    sbpowerpositionLines.Clear();
                    sbpowerpositionLines.AppendLine("Local Time,Volume");
                    m.Catch((PowerServiceException ex) =>
                    {
                        
                        Log.Error(string.Format("PowerServiceException  {0}", ex.Message));
                        return Observable.Empty<IEnumerable<PowerTrade>>();
                    })
                    .Retry(5)
                    .SelectMany(t => t.SelectMany(x => x.Periods))
                    .GroupBy(g => g.Period)
                    .Select(p => new { Period = p.Key, Volume = p.Sum(_ => _.Volume) })
                    //.Materialize()
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
                            sbpowerpositionLines.AppendLine(string.Format("{0},{1}", dateTimeHelper.UtcIndexToLocalHourMap[value.Period], volume));
                            Log.Info(string.Format("Period {0}, Volume {1}",
                                value.Period,
                                volume));
                        });

                    }, async () =>
                    {
                        string path = Path.Combine(csvFilePath,
                            "PowerPosition" + dtrunDate.ToString("_yyyyMMdd_") + DateTime.Now.ToString("HHmm") + ".csv");
                        using (var stream = new StreamWriter(path))
                        {
                           await stream.WriteAsync(sbpowerpositionLines.ToString());
                           await stream.FlushAsync();
                        }
                        Log.Debug("Completed " + path + "\n");
                        
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
