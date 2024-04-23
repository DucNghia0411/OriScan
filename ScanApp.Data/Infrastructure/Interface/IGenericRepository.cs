using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Data.Infrastructure.Interface
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();

        Task<T?> GetByIdAsync(int id);

        Task AddAsync(T entity);

        Task AddRangeAsync(IEnumerable<T> entities);

        void Update(T entity);

        void UpdateRange(IEnumerable<T> entities);

        Task DeleteAsync(int id);

        void DeleteRange(IEnumerable<T> entities);

        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includeProperties);

        Task<T?> LastOrDefaultAsync(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includeProperties);

        Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includeProperties);

        Task<IEnumerable<T>> GetTopAsync(Expression<Func<T, bool>> expression, int total, params Expression<Func<T, object>>[] includeProperties);

        Task<int> CountAsync(Expression<Func<T, bool>> expression);
    }
}
