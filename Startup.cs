using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Teams_Bots.Bots;
using Teams_Bots.Dialogs;
using Teams_Bots.Interfaces;
using Teams_Bots.Repository;

namespace Teams_Bots
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();

            #region Utils

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Create a global hashset for our ConversationReferences
            services.AddSingleton<ConcurrentDictionary<string, ConversationReference>>();

            //Other

            // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
            services.AddSingleton<IStorage, MemoryStorage>();

            //// Create the User state. (Used in this bot's Dialog implementation.)
            //services.AddSingleton<UserState>();

            //// Create the Conversation state. (Used by the Dialog system itself.)
            //services.AddSingleton<ConversationState>();

            #endregion Utils

            #region Repositories

            services.AddScoped<IBaseBotService, BaseBotRepository>();

            #endregion Repositories

            #region Bots

            services.AddTransient<IBot, BaseBot>();

            //// The Dialog that will be run by the bot.
            //services.AddSingleton<ProactiveBotDialog>();

            //// Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            //services.AddTransient<IBot, ProactiveDialogBot>();

            #endregion Bots
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            // app.UseHttpsRedirection();
        }
    }
}