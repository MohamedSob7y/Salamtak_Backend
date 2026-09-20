using Salamtak.Domain.Models.Common_Entity;
using System.Linq.Expressions;

namespace Salamtak.Domain.Specifications
{
    public interface ISpecification<TEntity>
        where TEntity : BaseEntity
    {
        Expression<Func<TEntity, bool>>? Criteria { get; }

        List<Expression<Func<TEntity, object>>> Includes { get; }

        Expression<Func<TEntity, object>>? OrderBy { get; }

        Expression<Func<TEntity, object>>? OrderByDescending { get; }

        int Skip { get; }

        int Take { get; }

        bool IsPagingEnabled { get; }
    }
}