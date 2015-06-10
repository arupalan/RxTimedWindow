using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using AlanAamy.Net.RxTimedWindow.Models;
using Services;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            CancellationToken token = new CancellationToken();
            Console.WriteLine("Main Thread id ={0}",Thread.CurrentThread.ManagedThreadId);
            var client = new ObservableTradeModel(new PowerService());
            client.GetTradePeriodObservable(5)
                //.ObserveOn(Scheduler.ThreadPool)
                //.SubscribeOn(TaskPoolScheduler.Default)
                .Catch((PowerServiceException ex) =>
                {
                    Console.WriteLine("Power catch Thread id ={0}", Thread.CurrentThread.ManagedThreadId);
                    Console.WriteLine(ex.Message);
                    return Observable.Empty <PowerPeriod>();
                } )
                .Catch((Exception e) =>
                {
                    Console.WriteLine("General catch Thread id ={0}", Thread.CurrentThread.ManagedThreadId);
                    Console.WriteLine(e.Message);
                    return Observable.Empty<PowerPeriod>();                    
                })
                .Subscribe(m => 
                    Console.WriteLine("Period {0}, Volume {1}\tCurrent Time : {2} Current Thread:{3}", m.Period, m.Volume, 
                    DateTime.Now.ToLongTimeString(),Thread.CurrentThread.ManagedThreadId));
            Console.ReadKey();
        }
    }
}
