using BusinessObjects.Models;

namespace Repositories;

public interface IIssueRepository
{
    Task<List<Issue>> GetIssuesAsync();
    Task<List<Issue>> GetIssuesByReporterAsync(int reporterUserId);
    Task<Issue?> GetIssueByIdAsync(int issueId);
    Task<Issue> CreateIssueAsync(Issue issue);
    Task UpdateIssueAsync(Issue issue);
}
