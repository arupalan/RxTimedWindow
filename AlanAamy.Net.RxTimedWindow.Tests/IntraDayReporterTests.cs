using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Concurrency;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using Services;

namespace AlanAamy.Net.RxTimedWindow.Tests
{
    [TestFixture]
    public class IntraDayReporterTests
    {
        private TestScheduler _testScheduler;
        private Mock<IPowerService> _powerService;
        [SetUp]
        public void Setup()
        {
            _testScheduler = new TestScheduler();
            _powerService = new Mock<IPowerService>();
            _powerService.Setup(p => p.GetTradesAsync(It.IsAny<DateTime>()))
                .Returns(
                
                Task.FromResult(CreateMockPowerTrades(It.IsAny<DateTime>(), 2,
                                                new PowerPeriod { Period = 1, Volume = 20 }, 
                                                new PowerPeriod { Period = 2, Volume = 30 }, 
                                                new PowerPeriod { Period = 3, Volume = 40 }
                                                )));
            //_testScheduler.Schedule()
        }

        [Test]
        public void Should_Flatten_Trades_And_Aggregate_Periods_Per_Hour_LocalTime()
        {
            var intradayReporter = new IntraDayReporter();
            DateTime date1 = DateTime.ParseExact( "2011/03/28 10:42:33", "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);

            StringBuilder sb = new StringBuilder();
            TimeZoneInfo gmtTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            intradayReporter.Run(_powerService.Object, _testScheduler, date1, gmtTimeZoneInfo, 1, sb, It.IsAny<String>(), true);
            _testScheduler.Start();
            string output = sb.ToString();

        }

        public static IEnumerable<PowerTrade> CreateMockPowerTrades(DateTime date, int numTrades, params PowerPeriod[] powerPeriods)
        {
            PowerTrade[] powerTrades =
                Enumerable.Range(0, numTrades).Select(_ => PowerTrade.Create(date, powerPeriods.Count())).ToArray();
            foreach (var powerTrade in powerTrades)
                for (var index = 0; index < powerPeriods.Length; index++)
                    powerTrade.Periods[index].Volume = powerPeriods[index].Volume;
            return powerTrades;
        }

    }
}
