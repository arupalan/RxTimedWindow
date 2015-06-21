using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using log4net;
using log4net.Config;
using Services;

namespace AlanAamy.Net.RxTimedWindow
{
    public sealed class DateTimeHelper
    {
        public  readonly Dictionary<int, string> UtcIndexToLocalHourMap = new Dictionary<int, string>();

        public DateTimeHelper(DateTime runDate,TimeZoneInfo timeZoneInfo)
        {
            DateTime dateTimeStart = new DateTime(runDate.Year, runDate.Month, runDate.Day, 0, 0, 0, DateTimeKind.Unspecified).Date.AddHours(-1.0);
            DateTime dateTimeEnd = dateTimeStart.AddDays(1.0);
            DateTime dateTimeUtcStart = TimeZoneInfo.ConvertTimeToUtc(dateTimeStart, timeZoneInfo);
            DateTime dateTimeUtcEnd = TimeZoneInfo.ConvertTimeToUtc(dateTimeEnd, timeZoneInfo);
            int index = 0;
            for (DateTime dateTime = dateTimeUtcStart; dateTime < dateTimeUtcEnd; dateTime = dateTime.AddHours(1.0))
            {
                UtcIndexToLocalHourMap.Add(++index, TimeZoneInfo.ConvertTimeFromUtc(dateTime, timeZoneInfo).ToString(@"HH:00"));
            }           
        }
    }

    public class IntraDayReporter
    {
        public enum StreamMode
        {
            [Description("Stream result to File")]
            StreamToFile,
            [Description("Stream Result to Memory")]
            StreamToMemory
        }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private IDisposable reporterDisposable;

        public IntraDayReporter()
        {
            XmlConfigurator.Configure();
        }

        public void Run0(IPowerService svc, IScheduler scheduler, DateTime dtrunDate, TimeZoneInfo timeZoneInfo,
            int observationIntervalInMinutes, StringBuilder sbpowerpositionLines, string csvFilePath,
            StreamMode streamMode = StreamMode.StreamToFile)
        {
            var dateTimeHelper = new DateTimeHelper(dtrunDate, timeZoneInfo);

            reporterDisposable = Observable.Interval(TimeSpan.FromMinutes(observationIntervalInMinutes), scheduler)
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
                    .Retry()
                    .Subscribe(value =>
                    {
                        var powerPeriods = value.SelectMany(t => t.Periods).GroupBy(g => g.Period).Select(
                            s => new PowerPeriod
                            {
                                Period = s.Key,
                                Volume = s.Sum(_ => _.Volume)
                            });
                        foreach (var powerPeriod in powerPeriods)
                        {
                            sbpowerpositionLines.AppendLine(string.Format("{0},{1}",
                                dateTimeHelper.UtcIndexToLocalHourMap[powerPeriod.Period], powerPeriod.Volume));
                            Log.Info(string.Format("Period {0}, Volume {1}",
                                dateTimeHelper.UtcIndexToLocalHourMap[powerPeriod.Period],
                                powerPeriod.Volume));
                        }
                    }, async () =>
                    {
                        if (streamMode == StreamMode.StreamToMemory) return;
                        string path = Path.Combine(csvFilePath,
                            "PowerPosition" + dtrunDate.ToString("_yyyyMMdd_") + DateTime.Now.ToString("HHmm") +
                            ".csv");
                        if (Directory.Exists(csvFilePath))
                        {
                            using (var stream = new StreamWriter(path))
                            {
                                await stream.WriteAsync(sbpowerpositionLines.ToString());
                                await stream.FlushAsync();
                            }

                            Log.Info("Completed " + path + "\n");
                        }
                        else
                        {
                            Log.Error("Completed but Path " + path + " do not exist !!\n");
                        }
                    });
                });
        }

        public void Run1(IPowerService svc, IScheduler scheduler, DateTime dtrunDate, TimeZoneInfo timeZoneInfo,
            int observationIntervalInMinutes, StringBuilder sbpowerpositionLines, string csvFilePath, 
            StreamMode streamMode = StreamMode.StreamToFile)
        {
            var dateTimeHelper = new DateTimeHelper(dtrunDate,timeZoneInfo);

            reporterDisposable = Observable.Interval(TimeSpan.FromMinutes(observationIntervalInMinutes), scheduler)
                .Select(i => Observable.FromAsync(() => svc.GetTradesAsync(dtrunDate)))
                .Subscribe(m =>
                {
                    sbpowerpositionLines.Clear();
                    sbpowerpositionLines.AppendLine("Local Time,Volume");
                    m.Catch((PowerServiceException ex) =>
                    {
                        Log.Error(string.Format("PowerServiceException  {0}", ex.Message));
                        return Observable.FromAsync(() => svc.GetTradesAsync(dtrunDate));
                    })
                    .Retry()
                    .SelectMany(t => t.SelectMany(x => x.Periods))
                    .GroupBy(g => g.Period)
                    .Select(p => new {Period = p.Key, Volume = p.Sum(_ => _.Volume)})
                    .Subscribe(value =>
                    {
                        value.Volume.Subscribe(volume =>
                        {
                            sbpowerpositionLines.AppendLine(string.Format("{0},{1}",
                                dateTimeHelper.UtcIndexToLocalHourMap[value.Period], volume));
                            Log.Info(string.Format("Period {0}, Volume {1}",
                                value.Period,
                                volume));
                        });
                    }, delegate
                    {
                        sbpowerpositionLines.AppendLine("OnError");
                        Log.Error("OnError");
                    }
                        , async () =>
                    {
                        if (streamMode == StreamMode.StreamToMemory) return;
                        string path = Path.Combine(csvFilePath,
                            "PowerPosition" + dtrunDate.ToString("_yyyyMMdd_") + DateTime.Now.ToString("HHmm") +
                            ".csv");
                        if (Directory.Exists(csvFilePath))
                        {
                            using (var stream = new StreamWriter(path))
                            {
                                await stream.WriteAsync(sbpowerpositionLines.ToString());
                                await stream.FlushAsync();
                            }

                            Log.Info("Completed " + path + "\n");
                        }
                        else
                        {
                            Log.Error("Completed but Path " + path + " do not exist !!\n");
                        }
                    });
                });
        }

        public void Run(IPowerService svc, IScheduler scheduler, DateTime dtrunDate, TimeZoneInfo timeZoneInfo,
            int observationIntervalInMinutes, StringBuilder sbpowerpositionLines, string csvFilePath,
            StreamMode streamMode = StreamMode.StreamToFile)
        {
            var dateTimeHelper = new DateTimeHelper(dtrunDate, timeZoneInfo);
            var block = new TransformBlock<long,
                                    IEnumerable<TradeAdapter>>(
                                                    async arg =>
                                                    {
                                                        var trades = await svc.GetTradesAsync(dtrunDate);
                                                        return trades.Select(trade => new TradeAdapter
                                                        {
                                                            Date = trade.Date,
                                                            Periods = trade.Periods
                                                        });
                                                    }
                                                );

            //var fromAsync = (from r in Observable.FromAsync(() => svc.GetTradesAsync(dtrunDate))
            //                 select r).Wait();

            reporterDisposable = Observable.Interval(TimeSpan.FromMinutes(observationIntervalInMinutes), scheduler)
                .FromTdf(block)
                //.FromTdf(() =>
                //new TransformBlock<long, 
                //                    IEnumerable<TradeAdapter>>( x =>
                //                    {
                //                        TaskCompletionSource<IEnumerable<TradeAdapter>> tcs =
                //                                                         new TaskCompletionSource<IEnumerable<TradeAdapter>>();
                //                        var task = svc.GetTradesAsync(dtrunDate);
                //                        return (Task<IEnumerable<TradeAdapter>>) task.ContinueWith(_ =>
                //                        {
                //                            switch (task.Status)
                //                            {
                //                                case TaskStatus.Canceled:
                //                                    tcs.SetCanceled();
                //                                    break;
                //                                case TaskStatus.RanToCompletion:
                //                                    tcs.SetResult(task.Result
                //                                        .Select(trade => new TradeAdapter
                //                                        {
                //                                            Date = trade.Date,
                //                                            Periods = trade.Periods
                //                                        }));
                //                                    break;
                //                                case TaskStatus.Faulted:
                //                                    // SetException will automatically wrap the original AggregateException 
                //                                    // in another one. The new wrapper will be removed in TaskAwaiter, leaving 
                //                                    // the original intact. 
                //                                    if (task.Exception != null) tcs.SetException(task.Exception);
                //                                    break;
                //                                default:
                //                                    tcs.SetException(
                //                                        new InvalidOperationException("Continuation called illegally."));
                //                                    break;
                //                            }
                //                        });
                //                    },
                //        new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }))
                .Catch((PowerServiceException ex) =>
                {
                    Log.Error(string.Format("PowerServiceException  {0}", ex.Message));
                    return Observable.Empty<IEnumerable<TradeAdapter>>();
                })
                .Retry(5)
                .SelectMany(t => t.SelectMany(x => x.Periods))
                .GroupBy(g => g.Period)
                .Select(p => new { Period = p.Key, Volume = p.Sum(_ => _.Volume) })
                .Subscribe(value =>
                {
                    sbpowerpositionLines.Clear();
                    sbpowerpositionLines.AppendLine("Local Time,Volume");
                    
                    value.Volume.Subscribe(volume =>
                    {
                        sbpowerpositionLines.AppendLine(string.Format("{0},{1}",
                            dateTimeHelper.UtcIndexToLocalHourMap[value.Period], volume));
                        Log.Info(string.Format("Period {0}, Volume {1}",
                            value.Period,
                            volume));
                    });
                }, 
                async () =>
                {
                    if (streamMode == StreamMode.StreamToMemory) return;
                    string path = Path.Combine(csvFilePath,
                        "PowerPosition" + dtrunDate.ToString("_yyyyMMdd_") + DateTime.Now.ToString("HHmm") +
                        ".csv");
                    if (Directory.Exists(csvFilePath))
                    {
                        using (var stream = new StreamWriter(path))
                        {
                            await stream.WriteAsync(sbpowerpositionLines.ToString());
                            await stream.FlushAsync();
                        }

                        Log.Info("Completed " + path + "\n");
                    }
                    else
                    {
                        Log.Error("Completed but Path " + path + " do not exist !!\n");
                    }
                });
        }

        public void Stop()
        {
            if(reporterDisposable != null)
            reporterDisposable.Dispose();
        }
    }
}
