﻿using System;
using System.Collections.Generic;
using EfLocalDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

public class Migrations
{
    Migrations()
    {
        #region Migrations

        var sqlInstance = new SqlInstance<MyDbContext>(
            buildTemplate: (connection, optionsBuilder) =>
            {
                #region IMigrationsSqlGenerator
                optionsBuilder.ReplaceService<IMigrationsSqlGenerator, CustomMigrationsSqlGenerator>();
                #endregion
                #region Migrate
                using (var dbContext = new MyDbContext(optionsBuilder.Options))
                {
                    dbContext.Database.Migrate();
                }
                #endregion
            },
            constructInstance: builder =>
            {
                return new MyDbContext(builder.Options);
            });

        #endregion
    }

    class CustomMigrationsSqlGenerator:
        IMigrationsSqlGenerator
    {
        public IReadOnlyList<MigrationCommand> Generate(IReadOnlyList<MigrationOperation> operations, IModel model = null)
        {
            throw new NotImplementedException();
        }
    }

    class MyDbContext:
        DbContext
    {
        public DbSet<TheEntity> TestEntities { get; set; }

        public MyDbContext(DbContextOptions options) :
            base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder model)
        {
            model.Entity<TheEntity>();
        }
    }
}
