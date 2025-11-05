using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Web.Api.Toolkit.Entity.Infraestructure.Factories;

namespace Infrastructure.Factories
{
    public class DbContextFactory : IDatabaseContextFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public DbContextFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public DbContext CreateDbContext<TEntity>() where TEntity : class
        {
            if (typeof(TEntity).Namespace == null)
            {
                throw new ArgumentException($"Entity {typeof(TEntity).Name} not contains namespace.");
            }

            if (typeof(TEntity).Namespace.Contains("Domain.Entitites"))
            {
                return _serviceProvider.GetRequiredService<ApplicationDbContext>();
            }
            else
            {
                return _serviceProvider.GetRequiredService<ApplicationDbContext>();
            }
        }
    }
}
