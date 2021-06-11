using Microsoft.AspNetCore.Mvc;
using SmartES.Application.Contracts;
using System.Threading.Tasks;

namespace SmartES.Presentation.API.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public TestController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet("{searchall}")]
        public async Task<IActionResult> GetPropertiesAndMgmts([FromQuery] string query)
        {
            var results = await _searchService.GetProperties(query);
            return Ok(results);
        }
    }
}
