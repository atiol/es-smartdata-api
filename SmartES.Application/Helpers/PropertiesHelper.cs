using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmartES.Application.Extensions;
using SmartES.Application.Models.Property;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SmartES.Application.Helpers
{
    public class PropertiesHelper
    {
        public const string Index = "property";

        private ElasticClient client;

        public PropertiesHelper(ElasticClient client)
        {
            this.client = client;
        }

        public async Task RunAsync()
        {
            var index = await client.Indices.ExistsAsync(Index);

            if (index.Exists)
            {
                // reindex appropriately
                await client.Indices.DeleteAsync(Index);
                //return;
            }

            var createResult =
                await client.Indices.CreateAsync(Index, c => c
                    .Settings(s => s
                        .Analysis(a => a
                            .AddSearchAnalyzer()
                        )
                    )
                .Map<PropertySearchDocument>(m => m.AutoMap())
            );

            var properties = new List<PropertySearchDocument>();
            var propertiesJson = @"D:\learning\elasticsearch\SmartES\SmartES.Presentation.API\Data\properties_formatted.json";

            if (!File.Exists(propertiesJson)) return;

            try
            {
                using StreamReader fileData = File.OpenText(propertiesJson);
                using JsonTextReader reader = new JsonTextReader(fileData);
                while (reader.Read())
                {
                    var obj = (JArray)JToken.ReadFrom(reader);

                    var counter = 0;
                    foreach (var arrData in obj)
                    {
                        var data = JsonConvert.DeserializeObject<PropertyDetailsModel>(arrData["property"].ToString());

                        if (data != null)
                        {
                            properties.Add(new PropertySearchDocument(data));
                            counter += 1;

                            if (counter == 500) Console.Write("#=>");
                        }
                    }
                }

                var bullkResult = await client.BulkAsync(b => b.Index(Index).CreateMany(properties));
            }
            catch (System.Exception ex)
            {
                //
                Console.WriteLine(ex);
            }
        }
    }
}
