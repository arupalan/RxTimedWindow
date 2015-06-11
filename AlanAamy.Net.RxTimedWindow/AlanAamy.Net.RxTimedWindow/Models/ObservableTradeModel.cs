using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
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
        public IObservable<IEnumerable<PowerTrade>> GetTradePeriodObservable(int periodInMinutes)
        {
            Console.WriteLine("GetTradePeriodObservable Thread id ={0}", Thread.CurrentThread.ManagedThreadId);
            var innerObservable = Observable.Interval(TimeSpan.FromSeconds(periodInMinutes), Scheduler.Default);
            var subscriptions = new CompositeDisposable();
            return Observable.Create<IEnumerable<PowerTrade>>(observer =>
            {

                var innerDisposable = innerObservable.Subscribe(async x =>
                {
                    try
                    {
                        IEnumerable<PowerTrade> trades = await powerService.GetTradesAsync(DateTime.Now.Date);
                        int it = 0;
                        Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                        observer.OnNext(trades);
                        //foreach (var powerTrade in trades)
                        //{
                        //    Console.WriteLine("Trade : {0}",it++);
                        //    foreach (var powerPeriod in powerTrade.Periods)
                        //    {
                        //        observer.OnNext(powerPeriod);
                        //    }
                        //}
                    }
                    catch (PowerServiceException esvc)
                    {
                        Console.WriteLine("PowerServiceException {0}",esvc.Message);
                        //observer.OnError(esvc);
                        innerObservable.Retry();
                    }
                    catch (Exception e)
                    {
                        
                        observer.OnError(e);
                        innerObservable.Retry();
                    }
                });
                subscriptions.Add(innerDisposable);
                return subscriptions;
            });
        }
    }
}
