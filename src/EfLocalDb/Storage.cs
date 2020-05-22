﻿namespace EfLocalDb
{
    public struct Storage
    {
        public static Storage FromSuffix<TDbContext>(string suffix)
        {
            Guard.AgainstWhiteSpace(nameof(suffix), suffix);
            var instanceName = GetInstanceName<TDbContext>(suffix);
            return new Storage(instanceName, DirectoryFinder.Find(instanceName));
        }

        public Storage(string name, string directory)
        {
            Guard.AgainstNullWhiteSpace(nameof(directory), directory);
            Guard.AgainstNullWhiteSpace(nameof(name), name);
            Name = name;
            Directory = directory;
        }

        public string Directory { get; private set; }

        public string Name { get; private set; }

        static string GetInstanceName<TDbContext>(string? scopeSuffix)
        {
            Guard.AgainstWhiteSpace(nameof(scopeSuffix), scopeSuffix);

            #region GetInstanceName

            if (scopeSuffix == null)
            {
                return typeof(TDbContext).Name;
            }

            return $"{typeof(TDbContext).Name}_{scopeSuffix}";

            #endregion
        }
    }
}