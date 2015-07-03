using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        private int _retryCount;

        [SetUp]
        public void Setup()
        {
            _testScheduler = new TestScheduler();
            _powerService = new Mock<IPowerService>();
        }

        [Test]
        public void Should_Flatten_Trades_And_Aggregate_Periods_Per_Hour_LocalTime()
        {
            //Arrange
            var intradayReporter = new IntraDayReporter();
            _powerService.Setup(p => p.GetTradesAsync(It.IsAny<DateTime>()))
                .Returns(

                Task.FromResult(CreateMockPowerTrades(It.IsAny<DateTime>(), 2,new[]{
                                                new PowerPeriod { Period = 1, Volume = 20 },
                                                new PowerPeriod { Period = 2, Volume = 30 },
                                                new PowerPeriod { Period = 3, Volume = 40 }}
                                                )));
            DateTime date = DateTime.ParseExact( "2011/03/28 10:42:33", "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
            StringBuilder sb = new StringBuilder();
            TimeZoneInfo gmtTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            const string expected = "Local Time,Volume\r\n23:00,40\r\n00:00,60\r\n01:00,80\r\n";

            //Act
            intradayReporter.Run(_powerService.Object, _testScheduler, date, gmtTimeZoneInfo, 1, sb, It.IsAny<String>(), IntraDayReporter.StreamMode.StreamToMemory);
            _testScheduler.AdvanceBy(TimeSpan.FromMinutes(1).Ticks);
            var actual = sb.ToString();

            //Assert
            Assert.AreEqual(expected,actual);
        }

        [Test]
        public void Should_RetryOnException_And_Then_Continue_NextSchedule()
        {
            //Arrange
            _retryCount = 0;
            var intradayReporter = new IntraDayReporter();
            _powerService.SetupSequence(p => p.GetTradesAsync(It.IsAny<DateTime>()))
                .Throws(new PowerServiceException("Thrown from Unit Test"))
                .Returns(
                            Task.FromResult(CreateMockPowerTrades(It.IsAny<DateTime>(), 2, new[]{
                                                            new PowerPeriod { Period = 1, Volume = 20 },
                                                            new PowerPeriod { Period = 2, Volume = 30 },
                                                            new PowerPeriod { Period = 3, Volume = 40 }}
                                                            )))
                .Returns(
                            Task.FromResult(CreateMockPowerTrades(It.IsAny<DateTime>(), 3, new[]{
                                                            new PowerPeriod { Period = 1, Volume = 10 },
                                                            new PowerPeriod { Period = 2, Volume = 10 },
                                                            new PowerPeriod { Period = 3, Volume = 10 }}
                                                            )));

            DateTime date = DateTime.ParseExact("2011/03/28 10:42:33", "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
            StringBuilder sb = new StringBuilder();
            TimeZoneInfo gmtTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            const string expectedfirst = "Local Time,Volume\r\n23:00,40\r\n00:00,60\r\n01:00,80\r\n";
            const string expectedsecond = "Local Time,Volume\r\n23:00,30\r\n00:00,30\r\n01:00,30\r\n";

            //Act
            intradayReporter.Run(_powerService.Object, _testScheduler, date, gmtTimeZoneInfo, 1, sb, It.IsAny<String>(), IntraDayReporter.StreamMode.StreamToMemory);
            _testScheduler.AdvanceBy(TimeSpan.FromMinutes(1).Ticks);
            var actual = sb.ToString();

            //Assert
            Assert.AreEqual(expectedfirst, actual);

            //Act
            /*Advance Virtual time to next schedule */
            _testScheduler.AdvanceBy(TimeSpan.FromMinutes(1).Ticks);
            actual = sb.ToString();

            //Assert
            Assert.AreEqual(expectedsecond, actual);
        }

        [Test]
        public void Should_Handle_23HRPreviouDay_to_22HRCurrentDay_LocalTimeOn_DaylightSaving_Start()
        {
            //Arrange
            var intradayReporter = new IntraDayReporter();
            _powerService.Setup(p => p.GetTradesAsync(It.IsAny<DateTime>()))
                .Returns(

                Task.FromResult(CreateMockPowerTrades(It.IsAny<DateTime>(), 2, new[]{
                                                new PowerPeriod { Period = 1, Volume = 10 },
                                                new PowerPeriod { Period = 2, Volume = 10 },
                                                new PowerPeriod { Period = 3, Volume = 10 },
                                                new PowerPeriod { Period = 4, Volume = 10 },
                                                new PowerPeriod { Period = 5, Volume = 10 },
                                                new PowerPeriod { Period = 6, Volume = 10 },
                                                new PowerPeriod { Period = 7, Volume = 10 },
                                                new PowerPeriod { Period = 8, Volume = 10 },
                                                new PowerPeriod { Period = 9, Volume = 10 },
                                                new PowerPeriod { Period = 10, Volume = 10 },
                                                new PowerPeriod { Period = 11, Volume = 10 },
                                                new PowerPeriod { Period = 12, Volume = 10 },
                                                new PowerPeriod { Period = 13, Volume = 10 },
                                                new PowerPeriod { Period = 14, Volume = 10 },
                                                new PowerPeriod { Period = 15, Volume = 10 },
                                                new PowerPeriod { Period = 16, Volume = 10 },
                                                new PowerPeriod { Period = 17, Volume = 10 },
                                                new PowerPeriod { Period = 18, Volume = 10 },
                                                new PowerPeriod { Period = 19, Volume = 10 },
                                                new PowerPeriod { Period = 20, Volume = 10 },
                                                new PowerPeriod { Period = 21, Volume = 10 },
                                                new PowerPeriod { Period = 22, Volume = 10 }}
                                                )));
            DateTime date = DateTime.ParseExact("2015/03/29", "yyyy/MM/dd", CultureInfo.InvariantCulture);
            StringBuilder sb = new StringBuilder();
            TimeZoneInfo gmtTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            const string expected = "Local Time,Volume\r\n23:00,20\r\n00:00,20\r\n02:00,20\r\n03:00,20\r\n04:00,20\r\n05:00,20\r\n06:00,20\r\n07:00,20\r\n08:00,20\r\n09:00,20\r\n10:00,20\r\n11:00,20\r\n12:00,20\r\n13:00,20\r\n14:00,20\r\n15:00,20\r\n16:00,20\r\n17:00,20\r\n18:00,20\r\n19:00,20\r\n20:00,20\r\n21:00,20\r\n";

            //Act
            intradayReporter.Run(_powerService.Object, _testScheduler, date, gmtTimeZoneInfo, 1, sb, It.IsAny<String>(), IntraDayReporter.StreamMode.StreamToMemory);
            _testScheduler.AdvanceBy(TimeSpan.FromMinutes(1).Ticks);
            var actual = sb.ToString();

            //Assert
            Assert.AreEqual(expected, actual);           
        }


        [Test]
        public void Should_Handle_23HRPreviouDay_to_22HRCurrentDay_LocalTimeOn_DaylightSaving_End()
        {
            //Arrange
            var intradayReporter = new IntraDayReporter();
            _powerService.Setup(p => p.GetTradesAsync(It.IsAny<DateTime>()))
                .Returns(
                Task.FromResult(CreateMockPowerTrades(It.IsAny<DateTime>(), 2, new[]{
                                                new PowerPeriod { Period = 1, Volume = 10 },
                                                new PowerPeriod { Period = 2, Volume = 10 },
                                                new PowerPeriod { Period = 3, Volume = 10 },
                                                new PowerPeriod { Period = 4, Volume = 10 },
                                                new PowerPeriod { Period = 5, Volume = 10 },
                                                new PowerPeriod { Period = 6, Volume = 10 },
                                                new PowerPeriod { Period = 7, Volume = 10 },
                                                new PowerPeriod { Period = 8, Volume = 10 },
                                                new PowerPeriod { Period = 9, Volume = 10 },
                                                new PowerPeriod { Period = 10, Volume = 10 },
                                                new PowerPeriod { Period = 11, Volume = 10 },
                                                new PowerPeriod { Period = 12, Volume = 10 },
                                                new PowerPeriod { Period = 13, Volume = 10 },
                                                new PowerPeriod { Period = 14, Volume = 10 },
                                                new PowerPeriod { Period = 15, Volume = 10 },
                                                new PowerPeriod { Period = 16, Volume = 10 },
                                                new PowerPeriod { Period = 17, Volume = 10 },
                                                new PowerPeriod { Period = 18, Volume = 10 },
                                                new PowerPeriod { Period = 19, Volume = 10 },
                                                new PowerPeriod { Period = 20, Volume = 10 },
                                                new PowerPeriod { Period = 21, Volume = 10 },
                                                new PowerPeriod { Period = 22, Volume = 10 },
                                                new PowerPeriod { Period = 23, Volume = 10 },
                                                new PowerPeriod { Period = 24, Volume = 10 },
                                                new PowerPeriod { Period = 25, Volume = 10 }}
                                                )));
            DateTime date = DateTime.ParseExact("2015/10/25", "yyyy/MM/dd", CultureInfo.InvariantCulture);
            StringBuilder sb = new StringBuilder();
            TimeZoneInfo gmtTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            const string expected =
                "Local Time,Volume\r\n23:00,20\r\n00:00,20\r\n01:00,20\r\n01:00,20\r\n02:00,20\r\n03:00,20\r\n04:00,20\r\n05:00,20\r\n06:00,20\r\n07:00,20\r\n08:00,20\r\n09:00,20\r\n10:00,20\r\n11:00,20\r\n12:00,20\r\n13:00,20\r\n14:00,20\r\n15:00,20\r\n16:00,20\r\n17:00,20\r\n18:00,20\r\n19:00,20\r\n20:00,20\r\n21:00,20\r\n22:00,20\r\n";

            //Act
            intradayReporter.Run(_powerService.Object, _testScheduler, date, gmtTimeZoneInfo, 1, sb, It.IsAny<String>(), IntraDayReporter.StreamMode.StreamToMemory);
            _testScheduler.AdvanceBy(TimeSpan.FromMinutes(1).Ticks);
            var actual = sb.ToString();

            //Assert
            Assert.AreEqual(expected, actual);
        }


        public  IEnumerable<PowerTrade> CreateMockPowerTrades(DateTime date, int numTrades, PowerPeriod[] powerPeriods)
        {
            //var powerTrades =
            //    Enumerable.Range(1, numTrades).Select(_ => new TradeAdapter()
            //    {
            //        Date = date,
            //        Periods = powerPeriods
            //    });

            //return powerTrades;

            IEnumerable<PowerTrade> powerTrades =
                Enumerable.Range(1, numTrades).Select(_ =>
                {
                    var trade = PowerTrade.Create(date, powerPeriods.Count());
                    foreach (var powerperiod in trade.Periods)
                    {
                        powerperiod.Volume =
                            powerPeriods.Single(p => p.Period == powerperiod.Period).Volume;
                    }
                    return trade;
                });

            return powerTrades;
        }

    }
}
