using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using JetBrains.Annotations;

namespace PinBoy
{
    /// <summary>
    /// Responsible for listening, filtering and dispatching data to <see cref="PinCatalogue"/>
    /// </summary>
    public sealed class PinDispatcher
    {
        public IServiceProvider Services { get; }

        /// <summary>
        /// The emote that an message needs to receive in order to be elligible for pinning.
        /// </summary>
        [NotNull]
        public IEmote PinEmote { get; }

        public HashSet<ulong> IgnoreChannels { get; } = new HashSet<ulong>();

        /// <summary>
        /// The number of <see cref="PinEmote"/> a message needs to attain in order for it to be "pinned"
        /// </summary>
        public int EmoteThreshold { get; }

        private readonly PinCatalogue _catalogue;
        private readonly BotConfig _cfg;

        public PinDispatcher(IServiceProvider services)
        {
            Services = services;

            var client = services.ThrowOrGet<DiscordSocketClient>();
            _catalogue = services.ThrowOrGet<PinCatalogue>();
            _cfg = services.ThrowOrGet<BotConfig>();

            client.ReactionAdded += ClientOnReactionAdded;
            client.ReactionRemoved += ClientOnReactionRemoved;
            client.ReactionsCleared += ClientOnReactionsCleared;

            PinEmote = new Emoji("⭐");
            Debug.Assert(PinEmote != null);

            EmoteThreshold = 1;
        }

        public bool IsIgnored(ISocketMessageChannel chnl)
        {
            if (IgnoreChannels.Contains(chnl.Id))
                return true;
            var guildChannel = chnl as IGuildChannel;

            if (guildChannel?.GuildId != _cfg.GuildId)
                return true;

            return false;
        } 

        private async Task ClientOnReactionsCleared(Cacheable<IUserMessage, ulong> cacheMsg, ISocketMessageChannel channel)
        {
            if (IsIgnored(channel))
                return;

            await _catalogue.RemoveMessage(await cacheMsg.GetOrDownloadAsync());
        }

        private async Task ClientOnReactionRemoved(Cacheable<IUserMessage, ulong> cacheMsg, ISocketMessageChannel channel, SocketReaction reacion)
        {

            if (IsIgnored(channel))
                return;
            var msg = await cacheMsg.GetOrDownloadAsync();

            if (msg.Reactions.Count != 0)
            {
                if (!msg.Reactions.TryGetValue(PinEmote, out var data))
                    return;
                if (data.ReactionCount >= EmoteThreshold)
                    return;
            }
                
            
            await _catalogue.RemoveMessage(msg);
        }

        private async Task ClientOnReactionAdded(Cacheable<IUserMessage, ulong> cacheMsg, ISocketMessageChannel channel, SocketReaction reacion)
        {

            if (IsIgnored(channel))
                return;
            var msg = await cacheMsg.GetOrDownloadAsync();

            if (!msg.Reactions.TryGetValue(PinEmote, out var data))
                return;

            if (IsIgnored(channel))
                return;

            if (data.ReactionCount >= EmoteThreshold)
                await _catalogue.PushMessage(msg);
        }
    }
}