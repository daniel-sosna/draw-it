using Microsoft.AspNetCore.Mvc;

namespace Draw.it.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DrawItemController : ControllerBase
    {
        private static readonly Dictionary<string, string[]> Categories = new Dictionary<string, string[]>
        {
            { "Animals", new[] { "Lion", "Elephant", "Tiger", "Penguin", "Giraffe" } },
            { "Vehicle type", new[] { "Sedan", "Pickup Truck", "Sports Car", "Minivan", "Bus" } },
            { "Games", new[] { "Chess", "Monopoly", "Scrabble", "Jenga", "Pictionary" } }
        };

        private readonly ILogger<DrawItemController> _logger;

        public DrawItemController(ILogger<DrawItemController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetDrawItems")]
        public IEnumerable<DrawItem> Get()
        {
            var random = new Random();
            var items = new List<DrawItem>();

            foreach (var category in Categories)
            {
                var randomIndex = random.Next(category.Value.Length);
                items.Add(new DrawItem
                {
                    Category = category.Key,
                    Example = category.Value[randomIndex]
                });
            }

            return items.ToArray();
        }
    }
}