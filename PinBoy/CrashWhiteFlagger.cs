using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace PinBoy
{
    /// <summary>
    /// Purpose: react with the white flag emote every time we find a message from "@Crash" 
    /// </summary>
    public sealed class CrashWhiteFlagger
    {
        private readonly BotConfig _cfg;

        private readonly IEmote _whiteflagEmote = new Emoji("🏳");

        public CrashWhiteFlagger(IServiceProvider services)
        {
            var client = services.ThrowOrGet<DiscordSocketClient>();
            _cfg = services.ThrowOrGet<BotConfig>();

            client.MessageReceived += ClientOnMessageReceived;   
        }

        private async Task ClientOnMessageReceived(SocketMessage msg)
        {
            if (msg.Author.Id != _cfg.CrashUserId)
                return;

            if (msg is IUserMessage userMsg)
            {
                await userMsg.AddReactionAsync(_whiteflagEmote);
            }
        }
    }
}
