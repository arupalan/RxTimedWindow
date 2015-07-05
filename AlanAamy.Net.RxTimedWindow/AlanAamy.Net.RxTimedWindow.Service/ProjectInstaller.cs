using System.ComponentModel;
using System.Configuration.Install;

namespace AlanAamy.Net.RxTimedWindow.Service
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }
    }
}