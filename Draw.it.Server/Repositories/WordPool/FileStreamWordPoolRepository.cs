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

                if (!File.Exists(path))
                {
                    throw new InvalidOperationException($"Categories file not found: {path}");
                }

                try
                {
                    using var fs = File.OpenRead(path);
                    return JsonSerializer.Deserialize<List<CategoryModel>>(fs, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? throw new InvalidOperationException("Categories file is empty or invalid");
                }
                catch (JsonException ex)
                {
                    throw new InvalidOperationException($"Invalid JSON in categories file: {path}", ex);
                }
            });
        }

        public IEnumerable<CategoryModel> GetAllCategories() => _categories.Value;

        public CategoryModel? FindCategoryById(long categoryId)
        {
            return _categories.Value.FirstOrDefault(c => c.Id == categoryId);
        }

        public IEnumerable<WordModel> FindWordsByCategoryId(long categoryId)
        {
            // Repository: just access storage; no domain validation here
            var path = Path.Combine(_dataDir, $"words-{categoryId}.txt");
            if (!File.Exists(path)) yield break;

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
            foreach (var cat in GetAllCategories())
            {
                foreach (var w in FindWordsByCategoryId(cat.Id))
                {
                    yield return w;
                }
            }
        }
    }
}


