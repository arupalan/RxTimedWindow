﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks.Dataflow;
using log4net;
using log4net.Config;
using Services;

namespace AlanAamy.Net.RxTimedWindow
{
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
        private IDisposable _reporterDisposable;

        public IntraDayReporter()
        {
            XmlConfigurator.Configure();
        }

        public void Run0(IPowerService svc, IScheduler scheduler, DateTime dtrunDate, TimeZoneInfo timeZoneInfo,
            int observationIntervalInMinutes, StringBuilder sbpowerpositionLines, string csvFilePath,
            StreamMode streamMode = StreamMode.StreamToFile)
        {
            var dateTimeHelper = new DateTimeHelper(dtrunDate, timeZoneInfo);

            _reporterDisposable = Observable.Interval(TimeSpan.FromMinutes(observationIntervalInMinutes), scheduler)
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

        public void Run(IPowerService svc, IScheduler scheduler, DateTime dtrunDate, TimeZoneInfo timeZoneInfo,
            int observationIntervalInMinutes, StringBuilder sbpowerpositionLines, string csvFilePath, 
            StreamMode streamMode = StreamMode.StreamToFile)
        {
            var dateTimeHelper = new DateTimeHelper(dtrunDate,timeZoneInfo);

            _reporterDisposable = Observable.Interval(TimeSpan.FromMinutes(observationIntervalInMinutes), scheduler)
                .Select(i =>
                {
                    dtrunDate = dtrunDate.AddMinutes((i + 1) * observationIntervalInMinutes);
                    return Observable.FromAsync(() => svc.GetTradesAsync(dtrunDate));
                })
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
                                dateTimeHelper.Reset(dtrunDate, timeZoneInfo).UtcIndexToLocalHourMap[value.Period], volume));
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

        public void Stop()
        {
            if(_reporterDisposable != null)
            _reporterDisposable.Dispose();
        }
    }
}
