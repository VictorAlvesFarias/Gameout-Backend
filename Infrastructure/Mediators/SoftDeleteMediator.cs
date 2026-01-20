using Domain.Entitites.ApplicationContextDb;
using Infrastructure.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Web.Api.Toolkit.Entity.Domain.Entities;
using Web.Api.Toolkit.Entity.Infraestructure.Mediators;

namespace Infrastructure.Mediators
{
    public class SoftDeleteMediator<T> : IDatabaseContextMediator<T> where T : class
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
     
        public SoftDeleteMediator(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        } 

        public IQueryable<T> Handle(IQueryable<T> query, DbContext context)
        {
            if (typeof(BaseEntity).IsAssignableFrom(typeof(T)) && context is ApplicationDbContext appContext)
            {
                var filtered = query.OfType<BaseEntity>().Where(x => !x.Deleted).Cast<T>();

                return filtered;
            }

            return query;
        }

        public void Handle(T entity, DbContext context)
        {
            return;
        }
    }
}
