using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services;

namespace AlanAamy.Net.RxTimedWindow
{
    public interface IEnergyService<T>
    {
        IEnumerable<T> GetTrades(DateTime date);
        Task<IEnumerable<T>> GetTradesAsync(DateTime date);
    }

    public class EnergyService : IEnergyService<PowerTrade>
    {
        private static readonly IPowerService SvcPowerService = new PowerService();


        public IEnumerable<PowerTrade> GetTrades(DateTime date)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<PowerTrade>> GetTradesAsync(DateTime date)
        {
            throw new NotImplementedException();
        }
    }
}
