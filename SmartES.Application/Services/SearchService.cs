using Nest;
using SmartES.Application.Contracts;
using SmartES.Application.Models.BaseModel;
using SmartES.Application.Models.Mgmt;
using SmartES.Application.Models.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartES.Application.Services
{
    public class SearchService : ISearchService
    {
        private readonly ElasticClient _esClient;

        public SearchService(ElasticClient esClient)
        {
            _esClient = esClient;
        }

        public async Task<ISearchResponse<object>> Search(string searchPhrase, int pageIndex, int pageSize)
        {
            if (pageSize > 45)
            {
                pageSize = 45;
            }

            var response = await _esClient.SearchAsync<object>(s => s
                .Index(Indices.Index("mgmt"))
                .Query(q => (q
                    .MatchPhrase(m => m
                        .Field(Infer.Field<PropertySearchDocument>(pf => pf.SearchTexts))
                        .Query(searchPhrase))) 
                        || 
                    (q.MatchPhrase(m => m
                        .Field(Infer.Field<MgmtSearchDocument>(pf => pf.SearchTexts))
                        .Query(searchPhrase))))
                .From(pageIndex - 1 * pageSize)
                .Take(pageSize));

            return response;
        }

        public async Task<IEnumerable<PropertyDetailsModel>> GetProperties(string searchPhrase)
        {
            var response = await _esClient.SearchAsync<PropertySearchDocument>(s => s
                .Index("property")
                .Query(q => q
                    .MultiMatch(mm => mm
                        .Query(searchPhrase)
                        .Type(TextQueryType.BoolPrefix)
                        .Fields(ff => ff
                            .Field(f => f.SearchTexts)
                            .Field("searchTexts._2gram")
                            .Field("searchTexts._3gram"))))
            );

            return response.Documents.Select(p => p.Data);
        }

        public async Task<IEnumerable<MgmtDetailsModel>> GetMgmt(string searchPhrase)
        {
            var response = await _esClient.SearchAsync<MgmtSearchDocument>(s => s
                .Index("mgmt")
                .Query(q => q
                    .MultiMatch(mm => mm
                        .Query(searchPhrase)
                        .Type(TextQueryType.BoolPrefix)
                        .Fields(ff => ff
                            .Field(f => f.SearchTexts)
                            .Field("searchTexts._2gram")
                            .Field("searchTexts._3gram"))))
                
                .Take(10)
            );

            return response.Documents.Select(c => c.Data);
        }
    }
}
