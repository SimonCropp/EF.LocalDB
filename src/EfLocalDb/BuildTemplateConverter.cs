using System;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

static class BuildTemplateConverter
{
    public static Func<DbConnection, DbContextOptionsBuilder<TDbContext>, Task> Convert<TDbContext>(
        Func<DbContextOptionsBuilder<TDbContext>, TDbContext> constructInstance,
        Func<TDbContext, Task>? buildTemplate)
        where TDbContext : DbContext
    {
        return async (connection, builder) =>
        {
            await using var data = constructInstance(builder);
            if (buildTemplate == null)
            {
                await data.Database.EnsureCreatedAsync();
            }
            else
            {
                await buildTemplate(data);
            }
        };
    }
}