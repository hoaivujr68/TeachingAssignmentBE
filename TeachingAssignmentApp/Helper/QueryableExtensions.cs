using Microsoft.EntityFrameworkCore;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Helper
{
    public static class QueryableExtensions
    {
        public static async Task<Pagination<T>> GetPagedOrderAsync<T>(this IQueryable<T> query, int currentPage, int pageSize, string sortExpression) where T : class
        {
            if (!string.IsNullOrWhiteSpace(sortExpression))
            {
                query = query.ApplySorting(sortExpression);
            }

            Pagination<T> result = new Pagination<T>(await query.CountAsync(), currentPage, pageSize);
            int count = (currentPage - 1) * pageSize;
            Pagination<T> pagination = result;
            pagination.Content = await query.Skip(count).Take(pageSize).ToListAsync();
            return result;
        }
    }
}
