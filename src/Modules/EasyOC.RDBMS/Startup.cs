﻿using EasyOC.RDBMS.Migrations;
using EasyOC.RDBMS.Scripting;
using EasyOC.RDBMS.Services;
using EasyOC.RDBMS.Workflows.Activities;
using EasyOC.RDBMS.Workflows.Drivers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Scripting;
using OrchardCore.Security.Permissions;
using OrchardCore.Workflows.Helpers;
using System;

namespace EasyOC.RDBMS
{
    [Feature("EasyOC.RDBMS")]
    [RequireFeatures("EasyOC.GraphQL")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper(GetType().Assembly);
            services.AddScoped<IContentFieldsValuePathProvider, ContentFieldsValuePathProvider>();
            // services.AddScoped<IQuerySource, FreeSqlQuerySource>();
    
            services.AddSingleton<IGlobalMethodProvider, FreeSqlWorkflowMethodsProvider>();
            services.AddActivity<SQLTask, SQLTaskDisplayDriver>();
            services.AddScoped<IRDBMSAppService, RDBMSAppService>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IDataMigration, RDBMSMappingConfigMigration>();
            services.AddScoped<IDataMigration, DbConnectionConfigMigration>();

        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "Home",
                areaName: "EasyOC.RDBMS",
                pattern: "Home/Index",
                defaults: new { controller = "Home", action = "Index" }
            );
        }
    }
}



