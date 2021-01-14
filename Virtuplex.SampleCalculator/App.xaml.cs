using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Virtuplex.SampleCalculator.Services;

namespace Virtuplex.SampleCalculator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();

            var mainView = new MainView();
            var keyHandlerService = ServiceProvider.GetRequiredService<KeyHandlerService>();

            keyHandlerService.Attach(mainView);

            mainView.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<KeyHandlerService>();
            services.AddTransient<FileGeneratorService>();
            services.AddTransient<ExpressionParserService>();
            services.AddScoped<ExpressionCalculatorService>();
        }
    }
}
