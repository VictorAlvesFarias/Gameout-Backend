using Infrastructure.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Web.Api.Toolkit.Entity.Infraestructure.Factories;
using Web.Api.Toolkit.Entity.Infraestructure.Mediators;
using Web.Api.Toolkit.Entity.Infraestructure.Repositories;
using Web.Api.Toolkit.Identity.Domain.Entities;
using System;
using System.Security.Claims;

namespace Infrastructure.Mediators
{
    public class DatabaseContextMediator<T> : IDatabaseContextMediator<T> where T : class
    {
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
            if (entity is BaseUserOwnedEntity baseEntity )
            {
                if (context is ApplicationDbContext appContext)
                {
                    baseEntity.UserId = appContext.GetUserId();
                }
            }
        }
    }
}
