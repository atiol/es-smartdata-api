using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using SmartES.Application.Contracts;
using SmartES.Application.Helpers;
using SmartES.Application.Models.Mgmt;
using SmartES.Application.Models.Property;
using SmartES.Application.Services;
using System;

namespace SmartES.Infrastructure.IoC.Extensions
{
    public static class DependencyInjectionContainer
    {
        internal static void AddElasticsearch(this IServiceCollection services, IConfiguration configuration)
        {
            //var options = configuration.GetAWSOptions();
            //var httpConnection = new AwsHttpConnection(options);
            var uri = configuration["Elasticsearch:EsUri"];
            var username = configuration["Elasticsearch:User"];
            var password = configuration["Elasticsearch:Password"];
            var defaultIndex = configuration["Elasticsearch:defaultIndex"];

            //var pool = new SingleNodeConnectionPool(new Uri(uri));
            var settings = new ConnectionSettings(new Uri(uri))
                .BasicAuthentication(username, password)
                .DefaultMappingFor<MgmtSearchDocument>(m => m.IdProperty(x => x.MgmtId))
                .DefaultMappingFor<PropertySearchDocument>(p => p.IdProperty(x => x.PropertyId));

            var esClient = new ElasticClient(settings);
            services.AddSingleton(settings);
            services.AddScoped(s => 
            {
                var connectionSettings = s.GetRequiredService<ConnectionSettings>();
                var client = new ElasticClient(connectionSettings);
                return client;
            });
        }

        public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApiVersioning(config =>
            {
                config.DefaultApiVersion = new ApiVersion(1, 0);
                config.AssumeDefaultVersionWhenUnspecified = true;
                config.ReportApiVersions = true;
            });

            services.AddElasticsearch(configuration);
            services.AddScoped<PropertiesHelper>();
            services.AddScoped<MgmtHelper>();
            services.AddScoped<ISearchService, SearchService>();
        }
    }
}
