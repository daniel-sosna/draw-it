using Draw.it.Server.Models.WordPool;
using Draw.it.Server.Repositories.WordPool;
using Draw.it.Server.Exceptions;
using System.Net;

namespace Draw.it.Server.Services.WordPool
{
    public class WordPoolService : IWordPoolService
    {
        private readonly IWordPoolRepository _wordPoolRepository;
        private readonly Random _random = new();

        public WordPoolService(IWordPoolRepository wordPoolRepository)
        {
            _wordPoolRepository = wordPoolRepository;
        }

        public IEnumerable<CategoryModel> GetAllCategories() => _wordPoolRepository.GetAllCategories();

        public IEnumerable<WordModel> GetWordsByCategoryId(long categoryId) => _wordPoolRepository.FindWordsByCategoryId(categoryId);

        public CategoryModel GetCategoryById(long categoryId)
        {
            var category = _wordPoolRepository.FindCategoryById(categoryId);
            if (category is null)
            {
                throw new AppException($"Category with id={categoryId} not found", HttpStatusCode.NotFound);
            }
            return category;
        }

        public WordModel GetRandomWordByCategoryId(long categoryId)
        {
            var category = GetCategoryById(categoryId);

            var words = _wordPoolRepository.FindWordsByCategoryId(categoryId).ToList();
            if (words.Count == 0)
            {
                throw new AppException($"No words for category id={categoryId}", HttpStatusCode.NotFound);
            }

            var idx = _random.Next(words.Count);
            return words[idx];
        }
    }
}


