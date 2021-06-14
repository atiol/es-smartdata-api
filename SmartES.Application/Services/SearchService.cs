using Nest;
using SmartES.Application.Constants;
using SmartES.Application.Contracts;
using SmartES.Application.Models.BaseModel;
using SmartES.Application.Models.Markets;
using SmartES.Application.Models.Mgmt;
using SmartES.Application.Models.Property;
using SmartES.Application.Models.RequestModels;
using SmartES.Application.Models.ResponseModels;
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

        public async Task<PagedResponseModel<object>> Search(RequestParamsModel model)
        {
            if (model.PageSize > ElasticsearchConstants.MaximumPageSize)
            {
                model.PageSize = ElasticsearchConstants.MaximumPageSize;
            }

            ISearchResponse<object> response;

            if (!string.IsNullOrEmpty(model.Market))
            {
                response = await _esClient.SearchAsync<object>(s => s
                .AllIndices()
                .Index(new[] { ElasticsearchConstants.PropertyIndex, ElasticsearchConstants.MgmtIndex })
                .Query(q => q
                    .Bool(b => b
                        .Must(mu => mu
                            .Match(m => m
                                .Field(Infer.Field<PropertyDetailsModel>(ff => ff.Market))
                                .Query(model.Market)))) 
                         &&
                    
                    q.MultiMatch(m => m
                        .Type(TextQueryType.PhrasePrefix)
                        .Fields(f => f
                            .Field(Infer.Field<PropertyDetailsModel>(pf => pf.Name, 1.3))
                            .Field(Infer.Field<PropertyDetailsModel>(pf => pf.FormerName, 1.3)))
                        .Query(model.Query)) &&
                        q.Term("_index", ElasticsearchConstants.PropertyIndex)

                        ||

                    (q.MatchPhrase(m => m
                            .Field(Infer.Field<MgmtDetailsModel>(pf => pf.Name, 1.2))
                        .Query(model.Query)) &&
                        q.Term("_index", ElasticsearchConstants.MgmtIndex)) && 
                    q.Bool(b => b
                        .Must(mu => mu
                            .Match(m => m
                                .Field(Infer.Field<MgmtDetailsModel>(f => f.Market))
                                .Query(model.Market)))))
                .From((model.PageIndex - 1) * model.PageSize)
                .Take(model.PageSize));
            }
            else
            {
                response = await _esClient.SearchAsync<object>(s => s
                    .AllIndices()
                    .Index(new[] { ElasticsearchConstants.PropertyIndex, ElasticsearchConstants.MgmtIndex })
                    .Query(q => q
                        .MultiMatch(mm => mm
                            .Type(TextQueryType.PhrasePrefix)
                                .Fields(f => f
                                    .Field(Infer.Field<PropertyDetailsModel>(ff => ff.Name))
                                    .Field(Infer.Field<PropertyDetailsModel>(ff => ff.FormerName)))
                                .Query(model.Query)) 
                        
                        ||
                            
                            q.MatchPhrasePrefix(pp => pp
                                .Field(Infer.Field<MgmtDetailsModel>(f => f.Name))
                                .Query(model.Query)
                                .Slop(1)
                                .MaxExpansions(2)))
                    );
            }

            

            return new PagedResponseModel<object>
            {
                TotalItems = (int) response.Total,
                PageIndex = model.PageIndex,
                PageSize = model.PageSize,
                Items = response.Documents
            };
        }

        public async Task<IEnumerable<PropertyDetailsModel>> GetProperties(string searchPhrase, string market)
        {
            ISearchResponse<PropertyDetailsModel> response;

            if (!string.IsNullOrEmpty(searchPhrase))
            {
                response = await _esClient.SearchAsync<PropertyDetailsModel>(s => s
                    .Index(ElasticsearchConstants.PropertyIndex)
                    .Query(q => q
                        .MultiMatch(mm => mm
                            .Query(searchPhrase)
                            .Type(TextQueryType.PhrasePrefix)
                            .Fields(f => f
                                .Field(ff => ff.Name)
                                .Field(ff => ff.FormerName))
                            .Operator(Operator.Or)) && 
                            (!string.IsNullOrEmpty(market) ? 
                                q.Bool(b => b.Filter(m => m.Term(t => t.Market, market))) : 
                                q.Fuzzy(f => f.Fuzziness(Fuzziness.EditDistance(1)))))
                    );
            }
            else
            {
                response = await _esClient.SearchAsync<PropertyDetailsModel>(s => s
                    .Index(ElasticsearchConstants.PropertyIndex)
                    .Query(q => q.Bool(b => b.Filter(m => m.Term(t => t.Market, market))))
                    .From(0)
                    .Take(45));
            }

            return response.Documents;
        }

        public async Task<IEnumerable<MgmtDetailsModel>> GetMgmt(int pageIndex, int pageSize)
        {
            var response = await _esClient.SearchAsync<MgmtDetailsModel>(s => s
                    .Index(ElasticsearchConstants.MgmtIndex)
                    .Query(q => q.MatchAll())
                    .From(pageIndex)
                    .Take(pageSize));

            return response.Documents;
        }

        public async Task<IEnumerable<MarketDetailsModel>> GetMarkets()
        {
            try
            {
                //var response = await _esClient.SearchAsync<PropertyDetailsModel>(s => s
                //.Index(ElasticsearchConstants.PropertyIndex)
                //.Aggregations(a => a
                //    .MultiTerms("markets", m => m
                //        .CollectMode(TermsAggregationCollectMode.BreadthFirst)
                //        .Terms(t => t
                //            .Field(f => f.Market), t => t.Field(f => f.State))
                //        .MinimumDocumentCount(1)
                //        .Size(500) // Assuming current market size is less than 500
                //        .Order(o => o
                //            .KeyAscending()
                //            .CountDescending()))));

                var response = await _esClient.SearchAsync<PropertyDetailsModel>(s => s
                    .Index(ElasticsearchConstants.PropertyIndex)
                    .Size(1000)
                    .Sort(o => o.Ascending(a => a.Market).Ascending(a => a.State))
                    .Aggregations(a => a
                        .Composite("markets", c => c
                            .Sources(s => s
                                .Terms("market", t => t.Field(f => f.Market))
                                .Terms("state", t => t.Field(f => f.State)))))
                    );

                //var mappedResponse = response.Aggregations
                //        .Buckets.Select<MultiTermsBucket<string>, MarketDetailsModel>(s => new MarketDetailsModel
                //        {
                //            Name = s.Key.ToArray()[0],
                //            State = s.Key.ToArray()[1]
                //        });

                var markets = response.Aggregations.Composite("markets").Buckets;

                var mappedResponse = markets.Select(s => new MarketDetailsModel
                {
                    Name = s.Key.Values.ToArray()[0].ToString(),
                    State = s.Key.Values.ToArray()[1].ToString()
                });

                return mappedResponse;
            }
            catch (Exception ex)
            {
                // log error
                Console.WriteLine(ex);
                return null;
            }
        }
    }
}
