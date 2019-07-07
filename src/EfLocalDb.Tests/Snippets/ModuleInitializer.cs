﻿using EfLocalDb;

namespace Snippets
{
    #region EfModuleInitializer

    static class ModuleInitializer
    {
        public static void Initialize()
        {
            SqlInstanceService<MyDbContext>.Register(
                builder => new MyDbContext(builder.Options));
        }
    }
    #endregion
}