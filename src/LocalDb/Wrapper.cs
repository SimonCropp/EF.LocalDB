﻿using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

class Wrapper
{
    string directory;
    string masterConnection;
    string instance;

    public Wrapper(string instance, string directory)
    {
        this.instance = instance;
        masterConnection = $"Data Source=(LocalDb)\\{instance};Database=master";
        this.directory = directory;
        Directory.CreateDirectory(directory);

        ServerName = $@"(LocalDb)\{instance}";
    }

    public static string NonPooled(string connectionString)
    {
        // needs to be pooling=false so that we can immediately detach and use the files
        return connectionString + ";Pooling=false";
    }

    public readonly string ServerName;

    public void Detach(string name)
    {
        var commandText = $"EXEC sp_detach_db '{name}', 'true';";
        try
        {
            using (var connection = new SqlConnection(masterConnection))
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = commandText;
                command.ExecuteNonQuery();
            }
        }
        catch (Exception exception)
        {
            throw new Exception(
                innerException: exception,
                message: $@"Failed to {nameof(Detach)}
{nameof(directory)}: {directory}
{nameof(masterConnection)}: {masterConnection}
{nameof(instance)}: {instance}
{nameof(commandText)}: {commandText}
");
        }
    }

    public void Purge()
    {
            var commandText = @"
declare @command nvarchar(max)
set @command = ''

select @command = @command
+ '

begin try
  alter database [' + [name] + '] set single_user with rollback immediate;
end try
begin catch
end catch;

drop database [' + [name] + '];

'
from [master].[sys].[databases]
where [name] not in ('master', 'model', 'msdb', 'tempdb');
execute sp_executesql @command";
            try
            {
                using (var connection = new SqlConnection(masterConnection))
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = commandText;
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception exception)
            {
                throw new Exception(
                    innerException: exception,
                    message: $@"Failed to {nameof(Purge)}
{nameof(directory)}: {directory}
{nameof(masterConnection)}: {masterConnection}
{nameof(instance)}: {instance}
{nameof(commandText)}: {commandText}
");
            }
    }

    public Task<string> CreateDatabaseFromTemplate(string name, string templateName)
    {
        if (string.Equals(name, "template", StringComparison.OrdinalIgnoreCase))
        {
            throw new Exception("The database name 'template' is reserved.");
        }
        var dataFile = Path.Combine(directory, $"{name}.mdf");
        if (File.Exists(dataFile))
        {
            throw new Exception($"The database name '{name}' has already been used.");
        }

        var templateDataFile = Path.Combine(directory, templateName + ".mdf");

        File.Copy(templateDataFile, dataFile);

        return CreateDatabaseFromFile(name);
    }

    public bool DatabaseFileExists(string name)
    {
        var dataFile = Path.Combine(directory, $"{name}.mdf");
        return File.Exists(dataFile);
    }

    public async Task<string> CreateDatabaseFromFile(string name)
    {
        var dataFile = Path.Combine(directory, $"{name}.mdf");
        var commandText = $@"
create database [{name}] on
(
    name = [{name}],
    filename = '{dataFile}',
    size = 10MB,
    maxSize = 10GB,
    fileGrowth = 5MB
)
for attach;
";
        try
        {
            using (var connection = new SqlConnection(masterConnection))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = commandText;
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }
        catch (Exception exception)
        {
            throw new Exception(
                innerException: exception,
                message: $@"Failed to {nameof(CreateDatabaseFromTemplate)}
{nameof(directory)}: {directory}
{nameof(masterConnection)}: {masterConnection}
{nameof(instance)}: {instance}
{nameof(name)}: {name}
{nameof(dataFile)}: {dataFile}
{nameof(commandText)}: {commandText}
");
        }

        return $"Data Source=(LocalDb)\\{instance};Database={name}";
    }

    public string CreateDatabase(string name)
    {
        var dataFile = Path.Combine(directory, name + ".mdf");
        var commandText = $@"
create database [{name}] on
(
    name = [{name}],
    filename = '{dataFile}',
    size = 10MB,
    maxSize = 10GB,
    fileGrowth = 5MB
);
";
        try
        {
            using (var connection = new SqlConnection(masterConnection))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = commandText;
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
        catch (Exception exception)
        {
            throw new Exception(
                innerException: exception,
                message: $@"Failed to {nameof(CreateDatabase)}
{nameof(directory)}: {directory}
{nameof(masterConnection)}: {masterConnection}
{nameof(instance)}: {instance}
{nameof(name)}: {name}
{nameof(dataFile)}: {dataFile}
{nameof(commandText)}: {commandText}
");
        }

        return $"Data Source=(LocalDb)\\{instance};Database=template";
    }

    public void Start()
    {
        RunLocalDbCommand($"create \"{instance}\"");
        RunLocalDbCommand($"start \"{instance}\"");
    }

    public void DeleteInstance()
    {
        RunLocalDbCommand($"stop \"{instance}\"");
        RunLocalDbCommand($"delete \"{instance}\"");
        DeleteFiles();
    }

    public void DeleteFiles(string exclude = null)
    {
        foreach (var file in Directory.EnumerateFiles(directory))
        {
            if (exclude != null)
            {
                if (Path.GetFileNameWithoutExtension(file) == exclude)
                {
                    continue;
                }
            }
            File.Delete(file);
        }
    }

    void RunLocalDbCommand(string command)
    {
        try
        {
            using (var start = Process.Start("sqllocaldb", command))
            {
                start.WaitForExit();
            }
        }
        catch (Exception exception)
        {
            throw new Exception(
                innerException: exception,
                message: $@"Failed to {nameof(RunLocalDbCommand)}
{nameof(directory)}: {directory}
{nameof(masterConnection)}: {masterConnection}
{nameof(instance)}: {instance}
{nameof(command)}: sqllocaldb {command}
");
        }
    }
}