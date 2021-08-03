using System;
using System.Net.Http;
using System.Threading.Tasks;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Azure.Functions.Authentication.WebAssembly;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace BlazorApp.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            //builder.Services
            //    .AddBlazorise(options =>
            //    {
            //        options.ChangeTextOnKeyPress = true;
            //    })
            //    .AddBootstrapProviders()
            //    .AddFontAwesomeIcons();

            builder.RootComponents.Add<App>("app");

            var baseAddress = builder.Configuration["BaseAddress"] ?? builder.HostEnvironment.BaseAddress;
            builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(baseAddress) });
            builder.Services.AddStaticWebAppsAuthentication();
            //builder.Services.AddOidcAuthentication(options =>
            //{
            //    // Configure your authentication provider options here.
            //    // For more information, see https://aka.ms/blazor-standalone-auth
            //    builder.Configuration.Bind("oidc", options.ProviderOptions);
            //});

            builder.Services.AddMudServices();

            await builder.Build().RunAsync();
        }
    }
}