using System;
using System.Globalization;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using AlanAamy.Net.RxTimedWindow;
using Services;

namespace ConsoleTest
{



    class Program
    {

        private static void Log(object onNextValue)
        {
            Console.WriteLine("Logging OnNext({0}) @ {1}", onNextValue, DateTime.Now);
        }
        private static void Log(PowerServiceException onErrorValue)
        {
            Console.WriteLine("Logging OnError({0}) @ {1}", onErrorValue, DateTime.Now);
        }
        private static void Log()
        {
            Console.WriteLine("Logging OnCompleted()@ {0}", DateTime.Now);
        }

        static void Main(string[] args)
        {

            Observable.Range(1, 3)
            .Materialize()
            .Dump("Materialize");

            var source = new Subject<int>();
            source.Materialize()
            .Dump("Materialize");
            source.OnNext(1);
            source.OnNext(2);
            source.OnNext(3);
            source.OnError(new Exception("Fail?"));


            DateTime date = DateTime.ParseExact(
           "2011/03/27 10:42:33",
           "yyyy/MM/dd HH:mm:ss",
           CultureInfo.InvariantCulture);

            DateTime date1 = DateTime.ParseExact(
           "2011/03/28 10:42:33",
           "yyyy/MM/dd HH:mm:ss",
           CultureInfo.InvariantCulture);

            DateTime date2 = DateTime.ParseExact(
           "2011/10/29 10:42:33",
           "yyyy/MM/dd HH:mm:ss",
           CultureInfo.InvariantCulture);

            var dateHelper = new DateTimeHelper(date);
            var dateHelper1 = new DateTimeHelper(date1);

            var reporter = new IntraDayReporter();
            reporter.Run(new PowerService(), Scheduler.Default,date1,1,@"C:\Temp");
            Console.ReadKey();
            reporter.Stop();
            /*
            CancellationTokenSource ts = new CancellationTokenSource();
            CancellationDisposable cancel = new CancellationDisposable();

            Console.WriteLine("Main Thread id ={0}", Thread.CurrentThread.ManagedThreadId);

            IPowerService svc = new PowerService();
            var subscription = Observable.Interval(TimeSpan.FromSeconds(10), Scheduler.Default)
                .Select(i => Observable.FromAsync(() => svc.GetTradesAsync(DateTime.Now.Date)))
                .Do(
                    i => i.Count().Subscribe(count => Log(count)),
                    ex => Log(ex.Message),
                    () => Log("Do Completed"))
                .Subscribe(m =>
                {
                        m.Catch((PowerServiceException ex) =>
                        {
                            Console.WriteLine("PowerServiceException  {0}\tThread id ={1}",ex.Message, Thread.CurrentThread.ManagedThreadId);
                            return Observable.Empty<IEnumerable<PowerTrade>>();
                        })
                        .Retry(5)
                        .SelectMany(t => t.SelectMany( x => x.Periods))
                        .GroupBy(g => g.Period)
                        .Select(p => new { Period = p.Key,Volume = p.Sum(_ => _.Volume)})
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
                                Console.WriteLine("Period {0}, Volume {1}\tCurrent Time : {2} Current Thread:{3}",
                                    value.Period,
                                    volume,
                                    DateTime.Now.ToLongTimeString(), Thread.CurrentThread.ManagedThreadId));

                        },()=> Console.WriteLine("Completed\n"));
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
            */
        }
    }
}
