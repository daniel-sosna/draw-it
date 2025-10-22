using Draw.it.Server.Models.WordPool;

namespace Draw.it.Server.Services.WordPool
{
    public interface IWordPoolService
    {
        IEnumerable<CategoryModel> GetAllCategories();
        IEnumerable<WordModel> GetAllWordsByCategoryId(long categoryId);
        WordModel GetRandomWordByCategoryId(long categoryId);
    }
}


