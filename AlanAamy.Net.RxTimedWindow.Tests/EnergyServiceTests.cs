using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlanAamy.Net.RxTimedWindow.Services;
using NUnit.Framework;
using Services;

namespace AlanAamy.Net.RxTimedWindow.Tests
{
    [TestFixture]
    public class EnergyServiceTests
    {
        [Test]
        public void Subscribe()
        {
            IEnergyService energyService = new EnergyService();
            energyService.GetTradesObservable(60).Subscribe( x =>
                Console.WriteLine("Trades : {0}\tCurrent Time : {1}", x.Count(), DateTime.Now.ToLongTimeString()));           
        }
    }
}
