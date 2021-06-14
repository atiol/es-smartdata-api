using Microsoft.AspNetCore.Mvc;
using SmartES.Application.Contracts;
using SmartES.Application.Models.RequestModels;
using System.Threading.Tasks;

namespace SmartES.Presentation.API.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;
        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] RequestParamsModel model)
        {
            var results = await _searchService.Search(model);
            return Ok(results);
        }

        [HttpGet("mgmts")]
        public async Task<IActionResult> GetMgmts(int pageIndex, int pageSize)
        {
            var result = await _searchService.GetMgmt(pageIndex, pageSize);
            return Ok(result);
        }

        [HttpGet("properties")]
        public async Task<IActionResult> Properties([FromQuery] string query, string market)
        {
            var result = await _searchService.GetProperties(query, market);
            return Ok(result);
        }

        [HttpGet("markets")]
        public async Task<IActionResult> GetMarkets()
        {
            var result = await _searchService.GetMarkets();
            return Ok(result);
        }
    }
}
