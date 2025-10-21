using Infrastructure.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Packages.Entity.Infraestructure.Mediators;
using Packages.Identity.Domain.Entities;
using System.Security.Claims;

namespace Infrastructure.Mediators
{
    public class DatabaseContextMediator<T> : IDatabaseContextMediator<T> where T : class
    {
        private readonly IHttpContextAccessor _contextHttp;

        public DatabaseContextMediator(IHttpContextAccessor contextHttp)
        {
            _contextHttp = contextHttp;
        }

        public IQueryable<T> Handle(IQueryable<T> query, DbContext context, bool ignoreUserId)
        {
            if (typeof(BaseUserOwnedEntity).IsAssignableFrom(typeof(T)) && context is ApplicationDbContext appContext && ignoreUserId == false)
            {
                var userId = appContext.GetUserId();
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
                    baseEntity.UserId = _contextHttp.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                }
            }
        }

        public void Handle(T entity)
        {
            if (entity is BaseUserOwnedEntity baseEntity)
            {
                if (baseEntity.UserId == _contextHttp.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier))
                {
                    return;
                }
                else
                {
                    throw new MemberAccessException("User dont have permission to apply this action");
                }
            }
        }
    }
}
