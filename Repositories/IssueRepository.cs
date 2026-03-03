using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public class IssueRepository : IIssueRepository
{
    private readonly AMSDbContext _dbContext;

    public IssueRepository(AMSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<Issue>> GetIssuesAsync()
    {
        return _dbContext.Issues.AsNoTracking()
            .Include(i => i.Category)
            .Include(i => i.Apartment)
            .Include(i => i.ReportedByUser)
            .Include(i => i.AssignedToUser)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public Task<List<Issue>> GetIssuesByReporterAsync(int reporterUserId)
    {
        return _dbContext.Issues.AsNoTracking()
            .Include(i => i.Category)
            .Include(i => i.Apartment)
            .Include(i => i.AssignedToUser)
            .Where(i => i.ReportedByUserId == reporterUserId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public Task<Issue?> GetIssueByIdAsync(int issueId)
    {
        return _dbContext.Issues
            .Include(i => i.Category)
            .Include(i => i.Apartment)
            .ThenInclude(a => a.Building)
            .Include(i => i.ReportedByUser)
            .Include(i => i.AssignedToUser)
            .FirstOrDefaultAsync(i => i.Id == issueId);
    }

    public async Task<Issue> CreateIssueAsync(Issue issue)
    {
        _dbContext.Issues.Add(issue);
        await _dbContext.SaveChangesAsync();
        return issue;
    }

    public async Task UpdateIssueAsync(Issue issue)
    {
        _dbContext.Issues.Update(issue);
        await _dbContext.SaveChangesAsync();
    }
}
