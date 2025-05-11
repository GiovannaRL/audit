using System;
using System.Configuration;
using System.Diagnostics;
using System.Web.Http;
using Azure.Storage.Queues;

namespace xPlannerAPI
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            GlobalConfiguration.Configuration.Formatters.Remove(GlobalConfiguration.Configuration.Formatters.XmlFormatter);

            GlobalConfiguration.Configure(WebApiConfig.Register);

            InitializeStorage();
        }

        private void CreateQueue(string queueName)
        {
            var queueClient = new QueueClient(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ToString(), queueName);
            queueClient.CreateIfNotExists();
        }

        private void InitializeStorage()
        {
            // Open storage account using credentials from .cscfg file.
            try
            {
                Trace.TraceInformation("Creating queues");
                CreateQueue("asset-book");
                CreateQueue("create-cut-sheet");
                CreateQueue("create-all-cut-sheet");
                CreateQueue("budget-summary");
                CreateQueue("asset-status");
                CreateQueue("procurement");
                CreateQueue("asset-by-room");
                CreateQueue("room-by-room");
                CreateQueue("equipment-with-costs");
                CreateQueue("generate-all-podetails-pages");
                CreateQueue("shop-drawing");
                CreateQueue("import-assets");
                CreateQueue("rename-assets-files");
                CreateQueue("import-categories");
                CreateQueue("delete-pos-and-quotes-files");
                CreateQueue("delete-report-files");
                CreateQueue("room-by-room-government");
                CreateQueue("copied-project-inventory-comparison");
                CreateQueue("jsn-rollup");
                CreateQueue("illustration-sheet");
                CreateQueue("government-inventory");
                CreateQueue("comprehensive-interior-design");
                CreateQueue("door-list");
                CreateQueue("room-equipment-list");
                CreateQueue("equipment-dimensional-and-utilities");

                Trace.TraceInformation("Storage initialized");
            }
            catch (Exception)
            {
                Trace.TraceInformation("Fail to initialize storage account");
            }

        }
    }
}