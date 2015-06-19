using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services;

namespace AlanAamy.Net.RxTimedWindow
{
    public interface IEnergyService
    {
        IEnumerable<ITrade> GetTrades(DateTime date);
        Task<IEnumerable<ITrade>> GetTradesAsync(DateTime date);
    }

    public interface IPowerPeriod
    {
        int Period { get; set; }

        double Volume { get; set; }       
    }

    public interface ITrade
    {
        DateTime Date { get; set; }

        IPowerPeriod[] Periods { get; set; }       
    }

    public class EnergyService : IEnergyService
    {
        private static readonly IPowerService SvcPowerService = new PowerService();


        public IEnumerable<ITrade> GetTrades(DateTime date)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ITrade>> GetTradesAsync(DateTime date)
        {
            PowerTrade[] powerTradeArray = Enumerable.ToArray<PowerTrade>(Enumerable.Select<int, PowerTrade>(Enumerable.Range(0, this._random.Next(1, 20)), (Func<int, PowerTrade>)(_ => PowerTrade.Create(date, numberOfPeriods))));
            int index = 0;
            for (DateTime dateTime5 = dateTime3; dateTime5 < dateTime4; dateTime5 = dateTime5.AddHours(1.0))
            {
                foreach (PowerTrade powerTrade in powerTradeArray)
                    powerTrade.Periods[index].Volume = this._random.NextDouble() * 1000.0;
                ++index;
            }
            return (IEnumerable<PowerTrade>)powerTradeArray;
            return SvcPowerService.GetTradesAsync(date);
        }
    }
}
