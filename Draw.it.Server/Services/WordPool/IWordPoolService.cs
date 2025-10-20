using Draw.it.Server.Models.WordPool;

namespace Draw.it.Server.Services.WordPool
{
    public interface IWordPoolService
    {
        IEnumerable<CategoryModel> GetAllCategories();
        IEnumerable<WordModel> GetWords(long categoryId);
        WordModel GetRandomWord(long categoryId);
    }
}


