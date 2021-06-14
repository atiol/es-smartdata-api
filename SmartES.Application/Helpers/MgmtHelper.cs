﻿using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmartES.Application.Constants;
using SmartES.Application.Extensions;
using SmartES.Application.Models.Mgmt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SmartES.Application.Helpers
{
    public class MgmtHelper
    {

        private readonly ElasticClient _client;

        public MgmtHelper(ElasticClient client)
        {
            _client = client;
        }

        public async Task RunAsync()
        {
            Console.WriteLine("[SmartData] INDEXING MANAGEMENT DATA");
            Console.WriteLine("[SmartData] -------------------------------\n");
            var index = await _client.Indices.ExistsAsync(ElasticsearchConstants.MgmtIndex);

            if (index.Exists)
            {
                Console.WriteLine($"[SmartData] [{ElasticsearchConstants.MgmtIndex}] exists! Deleting existing index...");
                // reindex appropriately
                var deleteResult = await _client.Indices.DeleteAsync(ElasticsearchConstants.MgmtIndex);
                if (deleteResult.IsValid)
                {
                    Console.WriteLine($"[SmartData] [{ElasticsearchConstants.MgmtIndex}] index deleted!");
                }
                else
                {
                    Console.WriteLine($"[SmartData] Could not delete [{ElasticsearchConstants.MgmtIndex}] index!");
                    Console.WriteLine(deleteResult.OriginalException.StackTrace);
                    return;
                }
            }
            else
            {
                if (index.ApiCall.HttpStatusCode != 200)
                {
                    Console.WriteLine($"[SmartData] Error while checking whether index exists!\n");
                    Console.WriteLine(index.DebugInformation + "\n");
                }
                else
                {
                    Console.WriteLine($"[SmartData] No existing index found. Proceed to create index [{ElasticsearchConstants.MgmtIndex}]");
                }
            }

            var createResult =
                await _client.Indices.CreateAsync(ElasticsearchConstants.MgmtIndex, c => c
                    .Settings(s => s
                        .Analysis(a => a
                            .ConfigureAnalyzer()))
                    .Map<MgmtDetailsModel>(m => m.AddMgmtMapping())
                );

            if (createResult.IsValid)
            {
                Console.WriteLine($"[SmartData] [{ElasticsearchConstants.MgmtIndex}] index created successfully!!!");
            }
            else
            {
                Console.WriteLine($"[SmartData] Error encountered while trying to create index [{ElasticsearchConstants.MgmtIndex}]!\n");
                Console.WriteLine(createResult.OriginalException.ToString());
                Console.WriteLine();
                return;
            }

            var mgmtList = new List<MgmtDetailsModel>();
            var mgmtDataPath = @"D:\learning\elasticsearch\SmartES\SmartES.Presentation.API\Data\mgmt.json"; 
            Console.WriteLine($"[SmartData] Data filepath: {mgmtDataPath}");
            int counter = 0, dataIndex = 0;

            if (!File.Exists(mgmtDataPath))
            {
                Console.WriteLine($"[SmartData] File not found at path: {mgmtDataPath}");
                return;
            };

            try
            {
                using StreamReader fileData = File.OpenText(mgmtDataPath);
                using JsonTextReader reader = new JsonTextReader(fileData);

                Console.WriteLine("[SmartData] Reading file data...");

                while (reader.Read())
                {
                    var obj = (JArray)JToken.ReadFrom(reader);

                    foreach (var arrData in obj)
                    {
                        var data = JsonConvert.DeserializeObject<MgmtDetailsModel>(arrData["mgmt"].ToString());
                        mgmtList.Add(data);
                        counter += 1;
                        dataIndex += counter;

                        if (counter == 500)
                        {
                            Console.Write("[SmartData] #=>");
                            counter = 0;
                        }
                    }
                }

                var bulkResult = await _client.BulkAsync(b => b.Index(ElasticsearchConstants.MgmtIndex).CreateMany(mgmtList));

                if (bulkResult.IsValid)
                {
                    Console.WriteLine($"\n[SmartData] Successfully indexed {dataIndex} files!!!\n");
                }
                else
                {
                    Console.WriteLine($"[SmartData] Error encountered while bulk indexing data!!!\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace.ToString());
            }
        }
    }
}
