using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmartES.Application.Constants;
using SmartES.Application.Extensions;
using SmartES.Application.Models.Property;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SmartES.Application.Helpers
{
    public class PropertysHelper
    {
        private readonly ElasticClient _client;

        public PropertysHelper(ElasticClient client)
        {
            _client = client;
        }

        public async Task RunAsync()
        {
            Console.WriteLine("[DataUploader] INDEXING PROPERTY DATA");
            Console.WriteLine("[DataUploader] ----------------------\n");
            var index = await _client.Indices.ExistsAsync(ElasticsearchConstants.PropertyIndex);

            if (index.Exists)
            {
                // reindex appropriately
                Console.WriteLine($"[DataUploader] \"{ElasticsearchConstants.PropertyIndex}\" index exists! Deleting existing index...");
                var deleteResult = await _client.Indices.DeleteAsync(ElasticsearchConstants.PropertyIndex);
                
                if (deleteResult.IsValid)
                {
                    Console.WriteLine($"[DataUploader] \"{ElasticsearchConstants.PropertyIndex}\" Index deleted!");
                }
                else
                {
                    Console.WriteLine($"[DataUploader] Failed to delete \"{ElasticsearchConstants.PropertyIndex}\" index!");
                    Console.WriteLine(deleteResult.OriginalException.StackTrace);
                }
                //return;
            }
            else
            {
                Console.WriteLine($"[DataUploader] No existing index found. Proceed to create index \"{ElasticsearchConstants.PropertyIndex}\" ...");
            }

            var createResult =
                await _client.Indices.CreateAsync(ElasticsearchConstants.PropertyIndex, c => c
                    .Settings(s => s
                        .Analysis(a => a
                            .ConfigureAnalyzer()))
                    .Map<PropertyDetailsModel>(m => m.AddPropertyMapping())
                );
            
            if (createResult.IsValid)
            {
                Console.WriteLine($"[DataUploader] \"{ElasticsearchConstants.PropertyIndex}\" index created successfully!!!");
            }
            else
            {
                Console.WriteLine($"[DataUploader] Error encountered while trying to create index [{ElasticsearchConstants.PropertyIndex}]!");
                Console.WriteLine("\n" + createResult.OriginalException.ToString() + "\n");
                return;
            }

            var properties = new List<PropertyDetailsModel>();
            var propertiesJson = @"D:\learning\elasticsearch\SmartES\SmartES.Presentation.API\Data\properties_formatted.json";
            Console.WriteLine($"[DataUploader] Data filepath: {propertiesJson}");

            if (!File.Exists(propertiesJson)) return;

            try
            {
                using StreamReader fileData = File.OpenText(propertiesJson);
                using JsonTextReader reader = new JsonTextReader(fileData);
                int counter = 0, dataIndex = 0;

                Console.WriteLine("[DataUploader] Reading file data...\n");
                
                while (reader.Read())
                {
                    var obj = (JArray)JToken.ReadFrom(reader);

                    foreach (var arrData in obj)
                    {
                        var data = JsonConvert.DeserializeObject<PropertyDetailsModel>(arrData["property"].ToString());

                        if (data != null)
                        {
                            properties.Add(data);
                            counter += 1;
                            dataIndex += counter;

                            if (counter == 500)
                            {
                                Console.Write("# ");
                                counter = 0;
                            }
                        }
                    }
                }

                Console.WriteLine("\n\n[DataUploader] Uploading data...");
                var bulkResult = await _client.BulkAsync(b => b.Index(ElasticsearchConstants.PropertyIndex).CreateMany(properties));

                if (bulkResult.IsValid)
                {
                    Console.WriteLine($"\n[DataUploader] Successfully indexed {dataIndex} files!!!\n");
                }
                else
                {
                    Console.WriteLine($"[DataUploader] Error encountered while bulk indexing data!!!\n");
                    Console.WriteLine($"\n{bulkResult.DebugInformation}\n");
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace.ToString() + "\n");
            }
        }
    }
}
