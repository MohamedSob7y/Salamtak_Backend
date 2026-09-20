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
            ISpecification<TEntity> specification)
        {
            var query = inputQuery;

            
            query = query.Where(entity => !entity.IsDeleted);

            
            if (specification.Criteria is not null)
            {
                query = query.Where(specification.Criteria);
            }

            
            query = specification.Includes.Aggregate(
                query,
                (current, include) =>
                    current.Include(include));

           
            if (specification.OrderBy is not null)
            {
                query = query.OrderBy(
                    specification.OrderBy);
            }

           
            if (specification.OrderByDescending is not null)
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