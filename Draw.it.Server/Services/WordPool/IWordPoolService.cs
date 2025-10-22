using Draw.it.Server.Models.WordPool;

namespace Draw.it.Server.Services.WordPool
{
    public interface IWordPoolService
    {
        CategoryModel GetCategoryById(long categoryId);
        IEnumerable<CategoryModel> GetAllCategories();
        IEnumerable<WordModel> GetAllWordsByCategoryId(long categoryId);
        WordModel GetRandomWordByCategoryId(long categoryId);
    }
}


