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
using System.Windows.Threading;
using FontAwesome5;
using System.Windows.Media;

namespace OriginalScan
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly NotificationManager _notificationManager;

        public static IHost? _host { get; private set; }

        public App()
        {
            TaskScheduler.UnobservedTaskException += App_UnobservedTaskException!;
            _notificationManager = new NotificationManager();

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

        private void NotificationShow(string type, string message)
        {
            switch (type)
            {
                case "error":
                    {
                        var errorNoti = new NotificationContent
                        {
                            Title = "Lỗi!",
                            Message = $"Có lỗi: {message}",
                            Type = NotificationType.Error,
                            Icon = new SvgAwesome()
                            {
                                Icon = EFontAwesomeIcon.Solid_Times,
                                Height = 25,
                                Foreground = new SolidColorBrush(Colors.Black)
                            },
                            Background = new SolidColorBrush(Colors.Red),
                            Foreground = new SolidColorBrush(Colors.White),
                        };
                        _notificationManager.Show(errorNoti);
                        break;
                    }
                case "success":
                    {
                        var successNoti = new NotificationContent
                        {
                            Title = "Thành công!",
                            Message = $"{message}",
                            Type = NotificationType.Success,
                            Icon = new SvgAwesome()
                            {
                                Icon = EFontAwesomeIcon.Solid_Check,
                                Height = 25,
                                Foreground = new SolidColorBrush(Colors.Black)
                            },
                            Background = new SolidColorBrush(Colors.Green),
                            Foreground = new SolidColorBrush(Colors.White),
                        };
                        _notificationManager.Show(successNoti);
                        break;
                    }
                case "warning":
                    {
                        var warningNoti = new NotificationContent
                        {
                            Title = "Thông báo!",
                            Message = $"{message}",
                            Type = NotificationType.Warning,
                            Icon = new SvgAwesome()
                            {
                                Icon = EFontAwesomeIcon.Solid_ExclamationTriangle,
                                Height = 25,
                                Foreground = new SolidColorBrush(Colors.Black)
                            },
                            Background = new SolidColorBrush(Colors.Yellow),
                            Foreground = new SolidColorBrush(Colors.Black),
                        };
                        _notificationManager.Show(warningNoti);
                        break;
                    }
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

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            NotificationShow("error", "Lỗi xử lý luồng " + $"{args.Exception}" + " vào thời gian: " + DateTime.Now);

            WriteToFile("Error DispatcherUnhandled at: " + DateTime.Now + $"{args.Exception}");
            args.Handled = true;
            Environment.Exit(0);
        }

        void App_UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            NotificationShow("error", "Lỗi " + $"{args}" + " vào thời gian: " + DateTime.Now);

            WriteToFile("Error UnhandledException at: " + DateTime.Now + $"{args}");
            Environment.Exit(0);
        }

        void App_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs args)
        {
            NotificationShow("error", "Lỗi tác vụ " + $"{args.Exception}" + " vào thời gian: " + DateTime.Now);

            WriteToFile("Error UnobservedTaskException at: " + DateTime.Now + $"{args.Exception}");
            Environment.Exit(0);
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
