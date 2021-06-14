using Elasticsearch.Net;
using Elasticsearch.Net.Aws;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using SmartES.Application.Constants;
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
            var options = configuration.GetAWSOptions();
            //var httpConnection = new AwsHttpConnection(options);
            var uri = configuration["Elasticsearch:AwsUri"];
            var username = configuration["Elasticsearch:AwsUser"];
            var password = configuration["Elasticsearch:AwsPassword"];
            var defaultIndex = configuration["Elasticsearch:defaultIndex"];

            var pool = new SingleNodeConnectionPool(new Uri(uri));
            var config = new ConnectionSettings(pool)
                .BasicAuthentication(username, password)
                .DisableDirectStreaming()
                .DefaultMappingFor<MgmtDetailsModel>(m => m
                    .IndexName(ElasticsearchConstants.MgmtIndex)
                    .IdProperty(x => x.MgmtId)
                        .PropertyName(n => n.MgmtId, "id"))
                .DefaultMappingFor<PropertyDetailsModel>(p => p
                    .IndexName(ElasticsearchConstants.PropertyIndex)
                    .IdProperty(x => x.PropertyId)
                        .PropertyName(n => n.PropertyId, "id"));

            var esClient = new ElasticClient(config);
            services.AddSingleton(esClient);
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
            services.AddScoped<PropertysHelper>();
            services.AddScoped<MgmtHelper>();
            services.AddScoped<ISearchService, SearchService>();
        }
    }
}
