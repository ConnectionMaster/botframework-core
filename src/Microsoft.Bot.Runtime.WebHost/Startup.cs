// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.ApplicationInsights;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.Core;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Integration.AspNet.Core.Skills;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Core;
using Microsoft.Bot.Core.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Bot.Runtime.WebHost
{
    public class Startup
    {
        // Application setting to enable Direct Line App Service Extension
        private string _directLineAppServiceExtensionName = System.Environment.GetEnvironmentVariable("APPSETTING_WEBSITE_SITE_NAME") + ".directline";

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureStorage(IServiceCollection services, BotSettings settings)
        {
            if (BotSettings.ConfigSectionValid(settings?.CosmosDb?.AuthKey))
            {
                services.AddScoped<IStorage>(_ => new CosmosDbPartitionedStorage(settings?.CosmosDb));
            }
            else
            {
                services.AddSingleton<IStorage, MemoryStorage>();
            }
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();

            services.AddSingleton<IConfiguration>(this.Configuration);

            // Load settings
            var settings = new BotSettings();
            Configuration.Bind(settings);

            // register components.
            ComponentRegistration.Add(new DialogsComponentRegistration());
            ComponentRegistration.Add(new DeclarativeComponentRegistration());
            ComponentRegistration.Add(new AdaptiveComponentRegistration());
            ComponentRegistration.Add(new LanguageGenerationComponentRegistration());
            ComponentRegistration.Add(new QnAMakerComponentRegistration());
            ComponentRegistration.Add(new LuisComponentRegistration());

            // This is for custom action component registration.
            //ComponentRegistration.Add(new CustomActionComponentRegistration());

            // Register the skills client and skills request handler.
            services.AddSingleton<SkillConversationIdFactoryBase, SkillConversationIdFactory>();
            services.AddHttpClient<BotFrameworkClient, SkillHttpClient>();
            services.AddSingleton<ChannelServiceHandler, SkillHandler>();

            // Register telemetry client, initializers and middleware
            services.AddApplicationInsightsTelemetry(settings?.ApplicationInsights?.InstrumentationKey ?? string.Empty);

            services.AddSingleton<ITelemetryInitializer, OperationCorrelationTelemetryInitializer>();
            services.AddSingleton<ITelemetryInitializer, TelemetryBotIdInitializer>();
            services.AddSingleton<IBotTelemetryClient, BotTelemetryClient>();
            services.AddSingleton<TelemetryLoggerMiddleware>(sp =>
            {
                var telemetryClient = sp.GetService<IBotTelemetryClient>();
                return new TelemetryLoggerMiddleware(telemetryClient, logPersonalInformation: settings?.Telemetry?.LogPersonalInformation ?? false);
            });
            services.AddSingleton<TelemetryInitializerMiddleware>(sp =>
            {
                var httpContextAccessor = sp.GetService<IHttpContextAccessor>();
                var telemetryLoggerMiddleware = sp.GetService<TelemetryLoggerMiddleware>();
                return new TelemetryInitializerMiddleware(httpContextAccessor, telemetryLoggerMiddleware, settings?.Telemetry?.LogActivities ?? false);
            });

            // Configure Storage and State
            ConfigureStorage(services, settings);
            services.AddSingleton<UserState>();
            services.AddSingleton<ConversationState>();

            // Create the credential provider to be used with the Bot Framework Adapter.
            services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();
            services.AddSingleton<BotAdapter>(sp => (BotFrameworkHttpAdapter)sp.GetService<IBotFrameworkHttpAdapter>());

            // Register AuthConfiguration to enable custom claim validation.
            services.AddSingleton<AuthenticationConfiguration>();

            // Adapter
            services.AddSingleton<IBotFrameworkHttpAdapter, CoreBotAdapter>();

            // ResourceExplorer
            var botDir = settings.Bot;
            var resourceExplorer = new ResourceExplorer().AddFolder(botDir);
            resourceExplorer.RegisterType<OnQnAMatch>("Microsoft.OnQnAMatch");
            services.AddSingleton(resourceExplorer);

            // Bot
            services.AddSingleton<IBot, CoreBot>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
#pragma warning disable CA1801 // Review unused parameters
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
#pragma warning restore CA1801 // Review unused parameters
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseWebSockets();
            app.UseNamedPipes(_directLineAppServiceExtensionName);
            app.UseRouting()
               .UseEndpoints(endpoints =>
               {
                   endpoints.MapControllers();
               });
        }
    }
}
