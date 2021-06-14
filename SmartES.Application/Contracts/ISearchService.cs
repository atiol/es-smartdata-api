using Nest;
using SmartES.Application.Models.Markets;
using SmartES.Application.Models.Mgmt;
using SmartES.Application.Models.Property;
using SmartES.Application.Models.RequestModels;
using SmartES.Application.Models.ResponseModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartES.Application.Contracts
{
    public interface ISearchService
    {
        Task<IEnumerable<PropertyDetailsModel>> GetProperties (string searchPhrase, string market);
        Task<IEnumerable<MgmtDetailsModel>> GetMgmt(int pageIndex, int pageSize);
        Task<PagedResponseModel<object>> Search(RequestParamsModel model);
        Task<IEnumerable<MarketDetailsModel>> GetMarkets();
    }
}
