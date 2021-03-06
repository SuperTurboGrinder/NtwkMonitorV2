﻿using Data.Abstract.Converters;
using Data.Abstract.DataAccessServices;
using Data.Abstract.DbInteraction;
using Data.Abstract.Validation;
using Data.DataServices.Conversion;
using Data.DataServices.DbOperations;
using Data.DataServices.Services;
using Data.DataServices.Validation;
using Data.EFDatabase;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NativeClient.WebAPI.Abstract;
using NativeClient.WebAPI.Services;

namespace NativeClient.WebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<NtwkDBContext>(options =>
                options
                    .UseSqlite(
                        Configuration.GetConnectionString("NtwkDBConnection"),
                        b => b.MigrationsAssembly("NetMonV2.APIServer")
                    )
            );
            //Db data flow services
            services.AddScoped<IEfDbDataSource, EfDataSource>();
            services.AddScoped<IDataRepository, EfDataRepository>();
            //Helper services
            services.AddTransient<IDbErrorLogger, FileErrorLogger>();
            services.AddTransient<IViewModelValidator, ViewModelValidator>();
            services.AddTransient<IEfModelToViewModelConverter, EfModelToViewModelConverter>();
            services.AddTransient<IViewModelToEfModelConverter, ViewModelToEfModelConverter>();
            //Db data access services
            services.AddScoped<INodeTreeDataService, NodeTreeDataService>();
            services.AddScoped<ITagsDataService, TagsDataService>();
            services.AddScoped<ICustomWebServicesDataService, CustomWebServicesDataService>();
            services.AddScoped<ISettingsProfileDataService, SettingsProfilesDataService>();
            services.AddScoped<IMonitoringDataService, MonitoringDataService>();
            services.AddScoped<INodesServicesDataService, NodesServicesDataService>();
            //Special services
            services.AddTransient<IPingService, PingServiceAsync>();
            services.AddTransient<IWebServiceLauncherService, DefaultWebServiceLauncherService>();
            services.AddSingleton<IExecutablesManagerService, ExecutablesManagerService>();
            services.AddSingleton<IErrorReportAssemblerService, ErrorReportAssemblerService>();

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseCors(options => options
                .WithOrigins("http://localhost:4200")
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
            );

            app.UseMvc();
        }
    }
}