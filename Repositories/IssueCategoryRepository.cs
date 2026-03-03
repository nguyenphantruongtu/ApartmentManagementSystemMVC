using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public class IssueCategoryRepository : IIssueCategoryRepository
{
    private readonly AMSDbContext _dbContext;

    public IssueCategoryRepository(AMSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<IssueCategory>> GetCategoriesAsync()
    {
        return _dbContext.IssueCategories.AsNoTracking()
            .Include(c => c.Issues)
            .OrderBy(c => c.CategoryName)
            .ToListAsync();
    }

    public Task<List<IssueCategory>> GetActiveCategoriesAsync()
    {
        return _dbContext.IssueCategories.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.CategoryName)
            .ToListAsync();
    }

    public Task<IssueCategory?> GetCategoryByIdAsync(int categoryId)
    {
        return _dbContext.IssueCategories
            .Include(c => c.Issues)
            .FirstOrDefaultAsync(c => c.Id == categoryId);
    }

    public async Task<IssueCategory> CreateCategoryAsync(IssueCategory category)
    {
        _dbContext.IssueCategories.Add(category);
        await _dbContext.SaveChangesAsync();
        return category;
    }

    public async Task UpdateCategoryAsync(IssueCategory category)
    {
        _dbContext.IssueCategories.Update(category);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteCategoryAsync(IssueCategory category)
    {
        _dbContext.IssueCategories.Remove(category);
        await _dbContext.SaveChangesAsync();
    }
}
