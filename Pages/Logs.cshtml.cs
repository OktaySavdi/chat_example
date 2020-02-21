using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Chat.Web.Models;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;

namespace Chat.Web
{
    public class LogsModel : PageModel
    {
        public List<ElasticLogItem> LogItems = new List<ElasticLogItem>();
        private readonly IConfiguration configuration;
        private readonly ILogger<LogsModel> logger;
        public const string INDEX_NAME = "logstash-*";

        public LogsModel(IConfiguration configuration, ILogger<LogsModel> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
        }


        public IActionResult OnGet()
        {

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
                .IndexName(INDEX_NAME));



            var client = new ElasticClient(settings);


            var scanResults = client.Search<ElasticLogItem>(s => s
            .From(0)
            .Size(100)
           .MatchAll()
           .SearchType(Elasticsearch.Net.SearchType.QueryThenFetch)
           .Scroll("5m"));

            if (!scanResults.IsValid)
            {
                logger.LogError(scanResults.OriginalException.Message);
                throw new InvalidOperationException(scanResults.OriginalException.Message);
            }


            var results = client.Scroll<ElasticLogItem>("10m", scanResults.ScrollId);

            while (results.Documents.Any())
            {
                LogItems.AddRange(results.Documents);
                results = client.Scroll<ElasticLogItem>("10m", results.ScrollId);
            }

            return Page();
        }

        public IActionResult OnPostDeleteLogs()
        {
            var settings = new ConnectionSettings(new Uri(configuration["Elasticsearch"]))
                .DefaultIndex("defaultindex")
                .DefaultMappingFor<ElasticLogItem>(m => m
                .IndexName(INDEX_NAME));



            var client = new ElasticClient(settings);

            client.Indices.Delete(INDEX_NAME);


            return new RedirectToPageResult("Index");
        }

    }
}