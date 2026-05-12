using Salamtak.Domain.Interfaces.Repository;
using Salamtak.Domain.Interfaces.UnitOfWork;
using Salamtak.Domain.Models.Common_Entity;
using Salamtak.Persistance.Context;
using Salamtak.Persistance.Implementation.Repository;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Persistance.Implementation.Unite_Of_Work
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SalamtakDBContext _context;

        private readonly Dictionary<Type, object> _repositories = new();

        public UnitOfWork(SalamtakDBContext context)
        {
            _context = context;
        }

        public IGenericRepository<TEntity> Repository<TEntity>()
            where TEntity : BaseEntity
        {
            var entityType = typeof(TEntity);

            if (_repositories.TryGetValue(entityType, out var repository))
            {
                return (IGenericRepository<TEntity>)repository;
            }

            var newRepository = new GenericRepository<TEntity>(_context);

            _repositories[entityType] = newRepository;

            return newRepository;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
