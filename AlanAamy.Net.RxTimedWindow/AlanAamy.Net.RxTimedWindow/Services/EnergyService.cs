using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Services;

namespace AlanAamy.Net.RxTimedWindow.Services
{
    public interface IEnergyService
    {
        IObservable<IEnumerable<PowerTrade>> GetTradesObservable();
    }

    /// <summary>
    /// 
    /// </summary>
    [Export(typeof(IEnergyService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class EnergyService : IEnergyService
    {
        private static readonly IPowerService powerService = new PowerService();

        public IObservable<IEnumerable<PowerTrade>> GetTradesObservable()
        {
            return Observable.Create<IEnumerable<PowerTrade>>(async (observer,token) =>
            {
                IEnumerable<PowerTrade> trades;
                while ((trades = await powerService.GetTradesAsync(DateTime.Now.Date)) != null)
                 {
                     if (token.IsCancellationRequested) { return; }
                     observer.OnNext(trades);
                 }
                 observer.OnCompleted();
            });          
        }
    }
}
