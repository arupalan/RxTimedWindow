using System;
using System.Globalization;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using AlanAamy.Net.RxTimedWindow;
using Services;

namespace ConsoleTest
{



    class Program
    {

        private static void Log(object onNextValue)
        {
            Console.WriteLine("Logging OnNext({0}) @ {1}", onNextValue, DateTime.Now);
        }
        private static void Log(PowerServiceException onErrorValue)
        {
            Console.WriteLine("Logging OnError({0}) @ {1}", onErrorValue, DateTime.Now);
        }
        private static void Log()
        {
            Console.WriteLine("Logging OnCompleted()@ {0}", DateTime.Now);
        }

        static void Main(string[] args)
        {

           // Observable.Range(1, 3)
           // .Materialize()
           // .Dump("Materialize");

           // var source = new Subject<int>();
           // source.Materialize()
           // .Dump("Materialize");
           // source.OnNext(1);
           // source.OnNext(2);
           // source.OnNext(3);
           // source.OnError(new Exception("Fail?"));


            DateTime date = DateTime.ParseExact( "2011/03/27", "yyyy/MM/dd", CultureInfo.InvariantCulture);
            DateTime date1 = DateTime.ParseExact( "2011/03/28", "yyyy/MM/dd", CultureInfo.InvariantCulture);
            DateTime date2 = DateTime.ParseExact( "2011/10/29", "yyyy/MM/dd", CultureInfo.InvariantCulture);
            DateTime date3 = DateTime.ParseExact( "2015/03/29", "yyyy/MM/dd", CultureInfo.InvariantCulture);
            DateTime date4 = DateTime.ParseExact( "2015/10/25", "yyyy/MM/dd", CultureInfo.InvariantCulture);
            DateTime date5 = DateTime.ParseExact( "2015/10/24", "yyyy/MM/dd", CultureInfo.InvariantCulture);
           // var dateHelper = new DateTimeHelper(date);
           // var dateHelper1 = new DateTimeHelper(date1);

            var reporter = new IntraDayReporter();
            StringBuilder sb = new StringBuilder();
            TimeZoneInfo gmtTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            reporter.Run1(new PowerService(), Scheduler.Default, date5, gmtTimeZoneInfo,1, sb, @"C:\Temp");
            Console.ReadKey();
            reporter.Stop();
        }
    }
}
