using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Services;

namespace AlanAamy.Net.RxTimedWindow.Models
{
    public sealed class ObservableTradeModel
    {
        private readonly IPowerService powerService;
        public ObservableTradeModel(IPowerService powerService)
        {
            this.powerService = powerService;
        }
        public IObservable<PowerPeriod> GetTradePeriodObservable(int periodInMinutes)
        {
            Console.WriteLine("GetTradePeriodObservable Thread id ={0}", Thread.CurrentThread.ManagedThreadId);
            var innerObservable = Observable.Interval(TimeSpan.FromSeconds(periodInMinutes), Scheduler.Default);
            return Observable.Create<PowerPeriod>( async (observer, token) =>
            {
                ManualResetEvent eventResetEvent = new ManualResetEvent(false);

                using (innerObservable.Subscribe(async x =>
                {
                    IEnumerable<PowerTrade> trades = await powerService.GetTradesAsync(DateTime.Now.Date);
                    foreach (var powerTrade in trades)
                    {
                        foreach (var powerPeriod in powerTrade.Periods)
                        {
                            observer.OnNext(powerPeriod);
                        }
                    }
                    if (token.IsCancellationRequested)
                    {
                        Console.WriteLine("Cancellation Requested");
                        eventResetEvent.Set();
                    }
                }))
                {
                    eventResetEvent.WaitOne();
                }
            });
        }
    }
}
