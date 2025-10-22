using System.Text.Json;
using Draw.it.Server.Models.WordPool;
using Microsoft.Extensions.Hosting;

namespace Draw.it.Server.Repositories.WordPool
{
    public class FileStreamWordPoolRepository : IWordPoolRepository
    {
        private readonly string _dataDir;
        private readonly Lazy<List<CategoryModel>> _categories;

        public FileStreamWordPoolRepository(IHostEnvironment env)
        {
            _dataDir = Path.Combine(env.ContentRootPath, "data", "words");
            _categories = new Lazy<List<CategoryModel>>(() =>
            {
                var path = Path.Combine(_dataDir, "categories.json");
                using var fs = File.OpenRead(path);
                var categories = JsonSerializer.Deserialize<List<CategoryModel>>(fs, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<CategoryModel>();
                return categories;
            });
        }

        public IEnumerable<CategoryModel> GetAllCategories() => _categories.Value;

        public CategoryModel? FindCategoryById(long categoryId)
        {
            return _categories.Value.FirstOrDefault(c => c.Id == categoryId);
        }

        public IEnumerable<WordModel> GetWordsByCategoryId(long categoryId)
        {
            // Validate category exists (repository returns null if not found)
            if (FindCategoryById(categoryId) is null)
            {
                yield break;
            }

            var path = Path.Combine(_dataDir, $"words-{categoryId}.txt");
            if (!File.Exists(path)) yield break;

            // Read file via stream explicitly (meaningful stream usage)
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(stream);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                var value = line.Trim();
                if (!string.IsNullOrEmpty(value))
                {
                    yield return new WordModel { CategoryId = categoryId, Value = value };
                }
            }
        }

        public IEnumerable<WordModel> GetAllWords()
        {
            foreach (var cat in _categories.Value)
            {
                foreach (var w in GetWordsByCategoryId(cat.Id))
                {
                    yield return w;
                }
            }
        }
    }
}


