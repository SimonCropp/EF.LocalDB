﻿using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LocalDb;
using Xunit;

namespace TestBase
{
    #region TestBase

    public class TestBase
    {
        static SqlInstance instance;

        static TestBase()
        {
            instance = new SqlInstance(
                name:"TestBaseUsage",
                buildTemplate: TestDbBuilder.CreateTable);
        }

        public Task<SqlDatabase> LocalDb(
            string databaseSuffix = null,
            [CallerMemberName] string memberName = null)
        {
            return instance.Build(GetType().Name, databaseSuffix, memberName);
        }
    }

    public class Tests:
        TestBase
    {
        [Fact]
        public async Task Test()
        {
            var database = await LocalDb();
            using (var connection = await database.OpenConnection())
            {
                await TestDbBuilder.AddData(connection);
                Assert.Single(await TestDbBuilder.GetData(connection));
            }
        }
    }

    #endregion
}