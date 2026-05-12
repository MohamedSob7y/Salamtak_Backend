using Salamtak.Domain.Interfaces.Repository;
using Salamtak.Domain.Models.Common_Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Domain.Interfaces.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<TEntity> Repository<TEntity>()
           where TEntity : BaseEntity;

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
