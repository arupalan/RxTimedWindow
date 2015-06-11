using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AlanAamy.Net.RxTimedWindow.Models;
using Services;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            CancellationTokenSource ts = new CancellationTokenSource();
            CancellationDisposable cancel = new CancellationDisposable();

            Console.WriteLine("Main Thread id ={0}", Thread.CurrentThread.ManagedThreadId);

            IPowerService svc = new PowerService();
            var subscription = Observable.Interval(TimeSpan.FromSeconds(5), Scheduler.Default)
                .Select(i => Observable.FromAsync(() => svc.GetTradesAsync(DateTime.Now.Date)))
                .Subscribe(m =>
                {
                        m
                        .Catch((PowerServiceException ex) =>
                        {
                            Console.WriteLine("Power catch Thread id ={0}", Thread.CurrentThread.ManagedThreadId);
                            Console.WriteLine(ex.Message);
                            return Observable.Empty<IEnumerable<PowerTrade>>();
                        })
                        .Retry(5)
                        .Subscribe(value =>
                        {
                            var powerPeriods = value.SelectMany(t => t.Periods).GroupBy(g => g.Period).Select(
                                s => new PowerPeriod
                                {
                                    Period = s.Key,
                                    Volume = s.Sum(_ => _.Volume)
                                });
                            foreach (var powerPeriod in powerPeriods)
                                Console.WriteLine("Period {0}, Volume {1}\tCurrent Time : {2} Current Thread:{3}",
                                    powerPeriod.Period,
                                    powerPeriod.Volume,
                                    DateTime.Now.ToLongTimeString(), Thread.CurrentThread.ManagedThreadId);
                            
                        });
                });


        //    var client = new ObservableTradeModel(new PowerService());
        //    var subscription = client.GetTradePeriodObservable(5)
        //        //.ObserveOn(Scheduler.ThreadPool)
        //        //.SubscribeOn(TaskPoolScheduler.Default)
        //        .Catch((PowerServiceException ex) =>
        //        {
        //            Console.WriteLine("Power catch Thread id ={0}", Thread.CurrentThread.ManagedThreadId);
        //            Console.WriteLine(ex.Message);
        //            client.InnObservable.Repeat();
        //            return Observable.Empty<IEnumerable<PowerTrade>>();
        //        })
        //        .Catch((Exception e) =>
        //        {
        //            Console.WriteLine("General catch Thread id ={0}", Thread.CurrentThread.ManagedThreadId);
        //            Console.WriteLine(e.Message);
        //            return Observable.Empty<IEnumerable<PowerTrade>>();
        //        })
        //        .Retry()
        //        .Repeat()
        //        //.OnErrorResumeNext(Observable.Empty<PowerPeriod>())
        //        //.GroupBy(m => m.Period)
        //        //.Sel
        //        //.SelectMany()
        //        //.Subscribe(group =>
        //        //    group.Count().Subscribe(count => Console.WriteLine("Key {0} , Count {1}", group.Key, count)));
        //        //.Subscribe(group =>
        //        //    group.Sum(_ => _.Volume).Subscribe(sum => Console.WriteLine("Key {0} , Sum {1}", group.Key, sum)));
        //        //.SelectMany(grp => 
        //        //            grp.Sum( _ => _.Volume)
        //        //                    .Select( value => new
        //        //                    {
        //        //                       Period = grp.Key, Volume = value
        //        //                    } )
        //        //           )
        //        //.Subscribe(m =>
        //        //{
        //        //    //Console.WriteLine("In Subscribe");
        //        //    Console.WriteLine("Period {0}, Volume {1}\tCurrent Time : {2} Current Thread:{3}", m.Period,
        //        //        m.Volume,
        //        //        DateTime.Now.ToLongTimeString(), Thread.CurrentThread.ManagedThreadId);
        //        //});
        //        .Retry()
        //        .Repeat()
        //        .Subscribe(m =>
        //         {
        //            var powerPeriods = m.SelectMany(t => t.Periods).GroupBy(g => g.Period).Select(
        //                s => new PowerPeriod
        //                {
        //                    Period = s.Key,
        //                    Volume = s.Sum(_ => _.Volume)
        //                });
        //            foreach (var powerPeriod in powerPeriods)
        //                Console.WriteLine("Period {0}, Volume {1}\tCurrent Time : {2} Current Thread:{3}",
        //                    powerPeriod.Period,
        //                    powerPeriod.Volume,
        //                    DateTime.Now.ToLongTimeString(), Thread.CurrentThread.ManagedThreadId);
        //        }, () =>
        //        {
        //            Console.WriteLine("Sequence Completed");
        //            client.InnObservable.Repeat();
        //        }
        //);
            Console.ReadKey();
            subscription.Dispose();
            //IPowerService powerService = new PowerService();
            //IObservable<IEnumerable<PowerTrade>> observable = Observable.Create<IEnumerable<PowerTrade>>(
            //    observer => async delegate
            //    {
            //        while (true)
            //        {
            //            await powerService.GetTradesAsync(DateTime.Now);
            //            await Task.Delay(TimeSpan.FromSeconds(5));
            //        }
            //    }
            //    );

            //IDisposable subscription = observable
            //    //.ObserveOn(Scheduler.ThreadPool)
            //    //.SubscribeOn(TaskPoolScheduler.Default)
            //    .Catch((PowerServiceException ex) =>
            //    {
            //        Console.WriteLine("Power catch Thread id ={0}", Thread.CurrentThread.ManagedThreadId);
            //        Console.WriteLine(ex.Message);
            //        return Observable.Empty<IEnumerable<PowerTrade>>();
            //    })
            //    .Catch((PowerServiceException ex) =>
            //    {
            //        Console.WriteLine("Power catch Thread id ={0}", Thread.CurrentThread.ManagedThreadId);
            //        Console.WriteLine(ex.Message);
            //        return Observable.Empty<IEnumerable<PowerTrade>>();
            //    })
            //    .Catch((Exception e) =>
            //    {
            //        Console.WriteLine("General catch Thread id ={0}", Thread.CurrentThread.ManagedThreadId);
            //        Console.WriteLine(e.Message);
            //        return Observable.Empty<IEnumerable<PowerTrade>>();
            //    })
                //.Subscribe(m =>
                //{
                //    var powerPeriods = m.SelectMany( t => t.Periods).GroupBy(g => g.Period).Select(
                //s => new PowerPeriod
                //{
                //    Period = s.Key,
                //    Volume = s.Sum( _ => _.Volume)
                //});
                    //foreach (var powerPeriod in powerPeriods)
                    //Console.WriteLine("Period {0}, Volume {1}\tCurrent Time : {2} Current Thread:{3}", powerPeriod.Period,
                    //    powerPeriod.Volume,
                    //    DateTime.Now.ToLongTimeString(), Thread.CurrentThread.ManagedThreadId);

            //    });
            //Console.ReadKey();
            //subscription.Dispose();

        }
    }
}
