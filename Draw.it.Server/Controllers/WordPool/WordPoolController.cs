using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Draw.it.Server.Services.WordPool;

namespace Draw.it.Server.Controllers.WordPool
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [AllowAnonymous]
    public class WordPoolController : ControllerBase
    {
        private readonly IWordPoolService _wordPoolService;

        public WordPoolController(IWordPoolService wordPoolService)
        {
            _wordPoolService = wordPoolService;
        }

        [HttpGet("categories")]
        public IActionResult GetCategories() => Ok(_wordPoolService.GetAllCategories());

        [HttpGet("categories/{categoryId:long}/words")]
        public IActionResult GetWords(long categoryId) => Ok(_wordPoolService.GetWordsByCategoryId(categoryId));

        [HttpGet("categories/{categoryId:long}/random")]
        public IActionResult GetRandom(long categoryId) => Ok(_wordPoolService.GetRandomWordByCategoryId(categoryId));
    }
}


