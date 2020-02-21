using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chat.Web.Models;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Nest;
using RabbitMQ.Client;
using StackExchange.Redis;

namespace Chat.Web
{
    public class HealthModel : PageModel
    {
        private readonly IConfiguration configuration;

        public HealthModel(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

      
        public IActionResult OnGet()
        {
            return Page();
        }

        public bool GetRedisHealthCheck()
        {
            try
            {
                //redis check
                IConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(configuration["Redis"]);
                IDatabase db = multiplexer.GetDatabase();

                return true;
            }
            catch (Exception ex)
            {
                ViewData.Add("redis_error",ex.Message);
                Response.StatusCode = 500;
                return false;
            }

        }

        public bool GetRabbitMQHealthCheck()
        {
            try
            {
                //rabbitmq check
                var factory = new ConnectionFactory() { HostName = configuration["RabbitMQ"] };
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                }

                return true;
            }
            catch (Exception ex)
            {
                ViewData.Add("rabbitmq_error", ex.Message);
                Response.StatusCode = 500;
                return false;
            }

        }
        public bool GetElasticsearchHealthCheck()
        {
            try
            {
                //elasticsearch check
                if (String.IsNullOrEmpty(configuration["es_username"]))
                {
                    throw new InvalidOperationException("es_username environment variable has not been set!");
                }

                if (String.IsNullOrEmpty(configuration["es_password"]))
                {
                    throw new InvalidOperationException("es_password environment variable has not been set!");
                }

                if (String.IsNullOrEmpty(configuration["es_servername"]))
                {
                    throw new InvalidOperationException("es_servername environment variable has not been set!");
                }

                int es_serverport = 9200;
                if (!String.IsNullOrEmpty(configuration["es_serverport"]))
                {
                    if (!int.TryParse(configuration["es_serverport"], out es_serverport))
                    {
                        throw new InvalidOperationException("es_port environment variable has not been set correctly!");
                    }
                }


                string connectionstring = $"https://{configuration["es_username"]}:{configuration["es_password"]}@{configuration["es_servername"]}:{es_serverport}";


                var settings = new ConnectionSettings(new Uri(connectionstring))
                    .ServerCertificateValidationCallback(CertificateValidations.AllowAll)
                    .DefaultIndex("defaultindex")
                    .DefaultMappingFor<ElasticLogItem>(m => m
                    .IndexName(LogsModel.INDEX_NAME));



                var client = new ElasticClient(settings);


                var scanResults = client.Search<ElasticLogItem>(s => s
                .From(0)
                .Size(100)
               .MatchAll()
               .SearchType(Elasticsearch.Net.SearchType.QueryThenFetch)
               .Scroll("5m"));

                if (!scanResults.IsValid)
                {
                    throw new InvalidOperationException(scanResults.OriginalException.Message);
                }


                return true;
            }
            catch (Exception ex)
            {
                ViewData.Add("elasticsearch_error", ex.Message);
                Response.StatusCode = 500;
                return false;
            }

        }

    }
}