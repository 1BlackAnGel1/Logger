using System;
using System.IO;
using System.Threading.Tasks;

public class Logger
{
    private const string LogFilePath = "log.txt";
    private const string BackupFolderPath = "Backup";
    private const int BackupInterval = 10;

    private int logCount;

    public event EventHandler BackupRequired;

    public async Task Log(string message)
    {
        using (var writer = new StreamWriter(LogFilePath, true))
        {
            await writer.WriteLineAsync(message);
            await writer.FlushAsync();
        }

        logCount++;

        if (logCount % BackupInterval == 0)
        {
            OnBackupRequired();
        }
    }

    protected virtual void OnBackupRequired()
    {
        BackupRequired?.Invoke(this, EventArgs.Empty);
    }
}

public class Starter
{
    private const int LogCount = 50;

    public async Task StartLogging()
    {
        var logger = new Logger();
        logger.BackupRequired += Logger_BackupRequired;

        var task1 = Task.Run(async () =>
        {
            for (int i = 1; i <= LogCount; i++)
            {
                await logger.Log($"Message from Task 1 - Log {i}");
            }
        });

        var task2 = Task.Run(async () =>
        {
            for (int i = 1; i <= LogCount; i++)
            {
                await logger.Log($"Message from Task 2 - Log {i}");
            }
        });

        await Task.WhenAll(task1, task2);
    }

    private void Logger_BackupRequired(object sender, EventArgs e)
    {
        CreateBackup();
    }

    private void CreateBackup()
    {
        string backupFolder = Path.Combine(Directory.GetCurrentDirectory(), "Backup");
        Directory.CreateDirectory(backupFolder);

        string backupFileName = DateTime.Now.ToString("y") + ".txt";
        string backupFilePath = Path.Combine(backupFolder, backupFileName);

        int index = 1;
        while (File.Exists(backupFilePath))
        {
            backupFileName = $"{DateTime.Now.ToString("y")}_{index}.txt";
            backupFilePath = Path.Combine(backupFolder, backupFileName);
            index++;
        }

        File.Move("log.txt", backupFilePath);

        Console.WriteLine($"Backup created: {backupFilePath}");
    }
}

    public class Program
{
    public static async Task Main()
    {
        var starter = new Starter();
        await starter.StartLogging();
    }
}
