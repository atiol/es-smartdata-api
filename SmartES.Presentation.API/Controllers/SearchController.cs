using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SmartES.Application.Contracts;
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
        public async Task<IActionResult> Search([FromQuery] string query, int pageIndex = 1, int pageSize = 45)
        {
            var results = await _searchService.Search(query, pageIndex, pageSize);
            return Ok(results);
        }

        [HttpGet("mgmts")]
        public async Task<IActionResult> GetMgmts([FromQuery] string query)
        {
            var result = await _searchService.GetMgmt(query);
            return Ok(result);
        }

        [HttpGet("properties")]
        public async Task<IActionResult> Properties([FromQuery] string query)
        {
            var result = await _searchService.GetMgmt(query);
            return Ok(result);
        }
    }
}
