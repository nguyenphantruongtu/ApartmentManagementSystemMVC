using BusinessObjects.Models;

namespace Repositories;

public interface IIssueCategoryRepository
{
    Task<List<IssueCategory>> GetCategoriesAsync();
    Task<List<IssueCategory>> GetActiveCategoriesAsync();
    Task<IssueCategory?> GetCategoryByIdAsync(int categoryId);
    Task<IssueCategory> CreateCategoryAsync(IssueCategory category);
    Task UpdateCategoryAsync(IssueCategory category);
    Task DeleteCategoryAsync(IssueCategory category);
}
