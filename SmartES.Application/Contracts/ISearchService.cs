using Nest;
using SmartES.Application.Models.Mgmt;
using SmartES.Application.Models.Property;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartES.Application.Contracts
{
    public interface ISearchService
    {
        Task<IEnumerable<PropertyDetailsModel>> GetProperties (string searchPhrase);
        Task<IEnumerable<MgmtDetailsModel>> GetMgmt(string searchPhrase);
        Task<ISearchResponse<object>> Search(string searchPhrase, int pageIndex, int pageSize);
    }
}
