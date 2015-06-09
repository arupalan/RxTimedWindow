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
            var client = new ObservableTradeModel();
            client.GetTradePeriodObservable(1)
                .ObserveOn(Scheduler.ThreadPool)
                .SubscribeOn(TaskPoolScheduler.Default)
                .Catch<PowerPeriod,PowerServiceException>( ex =>
                {
                    Console.WriteLine("catch Thread id ={0}", Thread.CurrentThread.ManagedThreadId);
                    Console.WriteLine(ex.Message);
                    return Observable.Empty <PowerPeriod>();
                } )
                .Subscribe((m => 
                    Console.WriteLine("Trades Period,Volume: {0},{1}\tCurrent Time : {2} Current Thread:", m.Period, m.Volume, 
                    DateTime.Now.ToLongTimeString()),Thread.CurrentThread.ManagedThreadId);
            Console.ReadKey();
        }
    }
}
