using Draw.it.Server.Models.WordPool;
using Draw.it.Server.Repositories.WordPool;

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

        public IEnumerable<WordModel> GetWords(long categoryId) => _wordPoolRepository.GetWordsByCategoryId(categoryId);

        public WordModel GetRandomWord(long categoryId)
        {
            var words = _wordPoolRepository.GetWordsByCategoryId(categoryId).ToList();
            if (words.Count == 0)
            {
                throw new InvalidOperationException($"No words for category {categoryId}");
            }

            var idx = _random.Next(words.Count);
            return words[idx];
        }
    }
}


