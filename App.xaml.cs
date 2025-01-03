using Microsoft.Extensions.DependencyInjection;
using NFSUAuditFilesWizard.Interfaces;
using NFSUAuditFilesWizard.Services;
using System.Windows;
using NFSUAuditFilesWizard.Screens;

namespace NFSUAuditFilesWizard
{
    public partial class App : Application
    {
        public new static App Current => (App)Application.Current;
        public IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IPdfCombinerService, PdfCombinerService>();
            services.AddSingleton<MainWindow>();
        }
    }
}