using Draw.it.Server.Models.WordPool;

namespace Draw.it.Server.Repositories.WordPool
{
    public interface IWordPoolRepository
    {
        CategoryModel GetCategoryById(long categoryId);
        IEnumerable<CategoryModel> GetAllCategories();
        IEnumerable<WordModel> GetAllWords();
        IEnumerable<WordModel> GetWordsByCategoryId(long categoryId);
    }
}


