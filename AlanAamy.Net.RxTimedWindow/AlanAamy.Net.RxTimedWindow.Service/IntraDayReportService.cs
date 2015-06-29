using System;
using System.Configuration;
using System.Reactive.Concurrency;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using log4net;
using Services;

namespace AlanAamy.Net.RxTimedWindow.Service
{
    public partial class IntraDayReportService : ServiceBase
    {
        private const string PollIntervalInMinutes = "PollIntervalInMinutes";
        private const string OutputPath = "";
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private IIntraDayReporter _reporter;

        public IntraDayReportService()
        {
            InitializeComponent();
        }

        public void Start()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            Logger.Info("Starting Power IntraDayReport Service");
            var pollIntervalInMinutes = int.Parse(ConfigurationManager.AppSettings[PollIntervalInMinutes]);
            string outputPath = ConfigurationManager.AppSettings[OutputPath];
            _reporter = new IntraDayReporter();
            var sb = new StringBuilder();
            var gmtTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            _reporter.Run(new PowerService(), Scheduler.Default, DateTime.Now, gmtTimeZoneInfo, pollIntervalInMinutes, sb, outputPath);
        }

        protected override void OnStop()
        {
            Logger.Info("Stopping Power IntraDayReport Service");
            _reporter.Stop();
            _reporter = null;
        }
    }
}
