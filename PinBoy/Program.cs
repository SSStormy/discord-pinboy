using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Net.Providers.WS4Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace PinBoy
{
    class Program
    {
        static void Main() => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            Console.WriteLine($"Starting, UTC: {DateTime.UtcNow}");
            Console.WriteLine($"Work dir is: {Directory.GetCurrentDirectory()}");

            Console.WriteLine("Files:");
            foreach (var f in Directory.GetFiles(Directory.GetCurrentDirectory()))
            {
                Console.WriteLine(f);
            }

            var builder = new ServiceCollection();
            
            var client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                WebSocketProvider = WS4NetProvider.Instance
            });

            builder.AddSingleton(client);
            builder.AddSingleton(s => new PinDispatcher(s));
            builder.AddSingleton(s => new PinCatalogue(s));
            builder.AddSingleton(s => JsonConvert.DeserializeObject<BotConfig>(File.ReadAllText("config.json")));
            var services = builder.BuildServiceProvider();

            client.Log += Log;

            services.ThrowOrGet<PinDispatcher>();
            services.ThrowOrGet<PinCatalogue>();

            var t = services.ThrowOrGet<BotConfig>().Token;
            await client.LoginAsync(TokenType.Bot, t);
            await client.StartAsync();

            await Task.Delay(-1);
        }

        private Task Log(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }
    }
}