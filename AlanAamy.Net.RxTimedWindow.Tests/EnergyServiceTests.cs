﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlanAamy.Net.RxTimedWindow.Models;
using NUnit.Framework;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Services;

namespace AlanAamy.Net.RxTimedWindow.Tests
{
    [TestFixture]
    public class EnergyServiceTests
    {
        [Test]
        public void Subscribe()
        {
            var client = new ObservableTradeModel(new PowerService());
            //client.GetTradePeriodObservable(1)
            //    .ObserveOn(Scheduler.Default)
            //    .SubscribeOn(TaskPoolScheduler.Default)
            //    .Subscribe(m => Debug.WriteLine("Trades Period,Volume: {0},{1}\tCurrent Time : {2}", m.Period,m.Volume, DateTime.Now.ToLongTimeString()));
            //IEnergyService energyService = new EnergyService();
            //energyService.GetTradesObservable(60).Subscribe( x =>
            //    Console.WriteLine("Trades : {0}\tCurrent Time : {1}", x.Count(), DateTime.Now.ToLongTimeString()));           
        }
    }
}
