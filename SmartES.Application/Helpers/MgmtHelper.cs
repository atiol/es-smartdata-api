using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmartES.Application.Extensions;
using SmartES.Application.Models.Mgmt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SmartES.Application.Helpers
{
    public class MgmtHelper
    {
        public const string Index = "mgmt";

        private readonly ElasticClient _client;

        public MgmtHelper(ElasticClient client)
        {
            _client = client;
        }

        public async Task RunAsync()
        {
            var index = _client.Indices.Exists(Indices.Index(Index));

            if (index.Exists)
            {
                // reindex appropriately
                //await client.Indices.DeleteAsync(Index);
                return;
            }

            var createResult =
                await _client.Indices.CreateAsync(Index, c => c
                    .Settings(s => s
                        .Analysis(a => a
                            .AddSearchAnalyzer()
                        )
                    )
                .Map<MgmtSearchDocument>(m => m.AutoMap())
            );

            var mgmtList = new List<MgmtSearchDocument>();
            var mgmtJson = @"D:\learning\elasticsearch\SmartES\SmartES.Presentation.API\Data\mgmt.json";
            var counter = 0;

            if (!File.Exists(mgmtJson)) return;

            using StreamReader fileData = File.OpenText(mgmtJson);
            using JsonTextReader reader = new JsonTextReader(fileData);
            while (reader.Read())
            {
                var obj = (JArray)JToken.ReadFrom(reader);
                
                foreach(var arrData in obj)
                {
                    var data = JsonConvert.DeserializeObject<MgmtDetailsModel>(arrData["mgmt"].ToString());
                    mgmtList.Add(new MgmtSearchDocument(data));
                    counter += 1;

                    if (counter == 500) Console.Write("#");
                }
            }

            var bullkResult = await _client.BulkAsync(b => b.Index(Index).CreateMany(mgmtList));
        }
    }
}
