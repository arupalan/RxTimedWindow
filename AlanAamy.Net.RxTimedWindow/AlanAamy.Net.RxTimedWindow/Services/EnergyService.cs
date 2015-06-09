using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Services;
using System.Reactive.Concurrency;

namespace AlanAamy.Net.RxTimedWindow.Services
{
    public interface IEnergyService
    {
        IObservable<IEnumerable<PowerTrade>> GetTradesObservable(int periodInMinutes);
    }

    /// <summary>
    /// 
    /// </summary>
    [Export(typeof(IEnergyService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class EnergyService : IEnergyService
    {
        private static readonly IPowerService powerService = new PowerService();

        public IObservable<IEnumerable<PowerTrade>> GetTradesObservable(int periodInMinutes)
        {
            var innerObservable = Observable.Interval(TimeSpan.FromMinutes(periodInMinutes), Scheduler.ThreadPool);
            return Observable.Create<IEnumerable<PowerTrade>>( async (observer,token) =>
            {
                ManualResetEvent eventResetEvent = new ManualResetEvent(false);

                using (innerObservable.Subscribe( async x =>
                {
                    IEnumerable<PowerTrade>  trades = await powerService.GetTradesAsync(DateTime.Now.Date);
                    observer.OnNext(trades);
                    if (token.IsCancellationRequested)
                    {
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
