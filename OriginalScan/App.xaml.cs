using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Notification.Wpf;
using ScanApp.Data.Infrastructure.Interface;
using ScanApp.Data.Infrastructure;
using ScanApp.Intergration.ApiClients;
using ScanApp.Intergration.Constracts;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using ScanApp.Data.Entities;
using Microsoft.EntityFrameworkCore;
using ScanApp.Service.Constracts;
using ScanApp.Service.Services;

namespace OriginalScan
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IHost? _host { get; private set; }

        public App()
        {
            try
            {
                _host = Host.CreateDefaultBuilder()
                    .ConfigureServices((context, services) =>
                    {
                        services.AddDbContext<ScanContext>();
                        services.AddSingleton<MainWindow>();
                        services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));
                        services.AddTransient(typeof(IUnitOfWork), typeof(UnitOfWork));
                        services.AddTransient<ITransferApiClient, TransferApiClient>();
                        services.AddSingleton<IBatchService, BatchService>();
                        services.AddSingleton<IDocumentService, DocumentService>();
                        services.AddSingleton<IImageService, ImageService>();
                        services.AddTransient<INotificationManager, NotificationManager>();
                    }).Build();
            }
            catch (Exception ex)
            {
                WriteToFile("App host error at: " + DateTime.Now + ex.ToString());
            }
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host!.StartAsync();
            MainWindow mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            WriteToFile("Stop at: " + DateTime.Now);
            await _host!.StopAsync();
            base.OnExit(e);
        }

        public void WriteToFile(string Message)
        {
            string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string path = Path.Combine(userFolderPath, "Log_TimeCheck");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = Path.Combine(userFolderPath, "Log_TimeCheck", "ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt");
            if (!File.Exists(filepath))
            {
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }
    }
}
