using Domain.Entitites.ApplicationContextDb;
using Infrastructure.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Web.Api.Toolkit.Entity.Infraestructure.Mediators;

namespace Infrastructure.Mediators
{
    public class DatabaseContextMediator<T> : IDatabaseContextMediator<T> where T : class
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
     
        public DatabaseContextMediator(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        } 

        public IQueryable<T> Handle(IQueryable<T> query, DbContext context, bool ignoreUserId)
        {
            if (typeof(BaseUserOwnedEntity).IsAssignableFrom(typeof(T)) && context is ApplicationDbContext appContext && ignoreUserId == false)
            {
                var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var filtered = query.OfType<BaseUserOwnedEntity>().Where(x => x.UserId == userId).Cast<T>();

                return filtered;
            }

            return query;
        }

        public void Handle(T entity, DbContext context)
        {
            if (entity is BaseUserOwnedEntity baseEntity)
            {
                if (context is ApplicationDbContext appContext)
                {
                    var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

                    baseEntity.UserId = userId;
                }
            }
        }
    }
}
