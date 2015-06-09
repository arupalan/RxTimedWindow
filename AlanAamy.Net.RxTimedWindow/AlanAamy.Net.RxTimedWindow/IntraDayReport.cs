using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AlanAamy.Net.RxTimedWindow.Services;

namespace AlanAamy.Net.RxTimedWindow
{


    public class IntraDayReport 
    {
        public void Run()
        {
            ComposeParts();

            IEnergyService energyService = new EnergyService();
            energyService.GetTradesObservable(60).Subscribe(x =>
                Console.WriteLine("Trades : {0}\tCurrent Time : {1}", x, DateTime.Now.ToLongTimeString()));
        }

        private void ComposeParts()
        {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add( new DirectoryCatalog(Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location)));
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }
    }
}
