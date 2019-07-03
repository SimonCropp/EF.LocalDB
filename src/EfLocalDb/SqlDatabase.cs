﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EfLocalDb
{
    public class SqlDatabase<TDbContext>:
        IDisposable
        where TDbContext : DbContext
    {
        Func<DbContextOptionsBuilder<TDbContext>, TDbContext> constructInstance;
        IEnumerable<object> data;

        public SqlDatabase(
            string connection,
            Func<DbContextOptionsBuilder<TDbContext>, TDbContext> constructInstance,
            IEnumerable<object> data)
        {
            this.constructInstance = constructInstance;
            this.data = data;
            Connection = new SqlConnection(connection);
        }

        public SqlConnection Connection { get; }

        public async Task Start()
        {
            await Connection.OpenAsync();
            Context = NewDbContext();
            if (data != null)
            {
                await AddData(data);
            }
        }

        public TDbContext Context { get; private set; }

        public async Task AddData(IEnumerable<object> entities)
        {
            Guard.AgainstNull(nameof(entities), entities);
            Context.AddRange(entities);
            await Context.SaveChangesAsync();
        }

        public Task AddData(params object[] entities)
        {
            return AddData((IEnumerable<object>) entities);
        }

        public TDbContext NewDbContext()
        {
            var builder = DefaultOptionsBuilder.Build<TDbContext>();
            builder.UseSqlServer(Connection);
            return constructInstance(builder);
        }

        public void Dispose()
        {
            Connection.Dispose();
        }
    }
}