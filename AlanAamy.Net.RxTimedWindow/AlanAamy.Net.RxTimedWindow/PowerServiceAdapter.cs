using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Services;

namespace AlanAamy.Net.RxTimedWindow
{


    public interface IPowerServiceAdapter
    {
        IEnumerable<TradeAdapter> GetTrades(DateTime date);
        Task<IEnumerable<TradeAdapter>> GetTradesAsync(DateTime date);
    }


    public class TradeAdapter
    {
        public DateTime Date { get; set; }
        public PowerPeriod[] Periods { get; set; }       
    }

    public class PowerServiceAdapter : IPowerServiceAdapter
    {


        private static readonly IPowerService SvcPowerService = new PowerService();

        public IEnumerable<TradeAdapter> GetTrades(DateTime date)
        {
            return SvcPowerService.GetTrades(date).Select(
                trade => new TradeAdapter()
                {
                    Date = trade.Date,
                    Periods = trade.Periods
                });
        }

        public Task<IEnumerable<TradeAdapter>> GetTradesAsync(DateTime date)
        {

            TaskCompletionSource<IEnumerable<TradeAdapter>> tcs =
                                                                         new TaskCompletionSource<IEnumerable<TradeAdapter>>();
            var task = SvcPowerService.GetTradesAsync(date);

            return (Task<IEnumerable<TradeAdapter>>) task.ContinueWith(_ =>
            {
                switch (task.Status)
                {
                    case TaskStatus.Canceled:
                        tcs.SetCanceled();
                        break;
                    case TaskStatus.RanToCompletion:
                        tcs.SetResult(task.Result
                            .Select(trade => new TradeAdapter
                            {
                                Date = trade.Date,
                                Periods = trade.Periods
                            }));
                        break;
                    case TaskStatus.Faulted:
                        // SetException will automatically wrap the original AggregateException 
                        // in another one. The new wrapper will be removed in TaskAwaiter, leaving 
                        // the original intact. 
                        if (task.Exception != null) tcs.SetException(task.Exception);
                        break;
                    default:
                        tcs.SetException(new InvalidOperationException("Continuation called illegally."));
                        break;
                }
            });
        }
    }
}
