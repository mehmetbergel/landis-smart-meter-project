using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class BackgroundWorker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine("Background task running at: " + DateTime.Now);

            var tasks = new List<Task>
            {
                Task.Run(() => DoWorkAsync("Task 1"), stoppingToken),
                Task.Run(() => DoWorkAsync("Task 2"), stoppingToken),
                Task.Run(() => DoWorkAsync("Task 3"), stoppingToken)
            };

            await Task.WhenAll(tasks);

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    private async Task DoWorkAsync(string taskName)
    {
        Console.WriteLine($"{taskName} is starting.");
        await Task.Delay(3000);
        Console.WriteLine($"{taskName} completed.");
    }
}
