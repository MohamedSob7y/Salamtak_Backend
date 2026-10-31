using Microsoft.EntityFrameworkCore;
using Salamtak.Domain.Models.Common_Entity;
using Salamtak.Domain.Specifications;

namespace Salamtak.Persistance.Specifications
{
    public static class SpecificationEvaluator<TEntity>
        where TEntity : BaseEntity
    {
        public static IQueryable<TEntity> GetQuery(
            IQueryable<TEntity> inputQuery,
            ISpecification<TEntity> specification,
            bool applyPaging = true)
        {
            var query = inputQuery
                .Where(entity => !entity.IsDeleted);

            if (specification.Criteria is not null)
            {
                query = query.Where(specification.Criteria);
            }

            if (!applyPaging)
                return query;

            foreach (var include in specification.Includes)
            {
                query = query.Include(include);
            }

            if (specification.OrderBy is not null)
            {
                query = query.OrderBy(specification.OrderBy);
            }
            else if (specification.OrderByDescending is not null)
            {
                query = query.OrderByDescending(
                    specification.OrderByDescending);
            }

            if (specification.IsPagingEnabled)
            {
                query = query
                    .Skip(specification.Skip)
                    .Take(specification.Take);
            }

            return query;
        }
    }
}