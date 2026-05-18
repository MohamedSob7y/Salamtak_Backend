using Salamtak.Domain.Models.Common_Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Domain.Interfaces.Repository
{
    public interface IGenericRepository<TEntity> where TEntity : BaseEntity
    {
        Task AddAsync(TEntity entity);

        Task AddRangeAsync(IEnumerable<TEntity> entities);

        void Update(TEntity entity);

        void Delete(TEntity entity);

        void SoftDelete(TEntity entity);

        Task<IReadOnlyList<TEntity>> GetAllAsync();

        Task<IReadOnlyList<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? condition);

        Task<TEntity?> GetByIdAsync(Guid id);

        Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> condition);

        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> condition);
        Task<TEntity?> FirstOrDefaultWithIncludesAsync(
              Expression<Func<TEntity, bool>> predicate,
              params Expression<Func<TEntity, object>>[] includes);

        Task<IReadOnlyList<TEntity>> GetAllWithIncludesAsync(
            Expression<Func<TEntity, bool>> predicate,
            params Expression<Func<TEntity, object>>[] includes);
    }
}
