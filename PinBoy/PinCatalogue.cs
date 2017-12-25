using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace PinBoy
{
    /// <summary>
    /// Respinsible for pinning given messages to the pin channel
    /// </summary>
    public sealed class PinCatalogue
    {
        private readonly IServiceProvider _services;
        public IMessageChannel PinChannel { get; private set; }

        private SocketGuild Guild { get; set; }

        private readonly ConcurrentDictionary<ulong, IMessage> _msgToPin = new ConcurrentDictionary<ulong, IMessage>();
        private readonly DiscordSocketClient _client;

        public PinCatalogue(IServiceProvider services)
        {
            _services = services;
            _client = _services.ThrowOrGet<DiscordSocketClient>();
            _client.Ready += ClientOnReady;
        }

        private async Task ClientOnReady()
        {
            
            var cfg = _services.ThrowOrGet<BotConfig>();
            
            Guild = _client.GetGuild(cfg.GuildId);
            Debug.Assert(Guild != null);
            PinChannel = Guild.GetTextChannel(cfg.PinChannelId);

            Debug.Assert(PinChannel != null);

            _services.ThrowOrGet<PinDispatcher>().IgnoreChannels.Add(PinChannel.Id);

            await ParsePins();
        }

        private async Task<IMessage> ResolveMessageFromPin(IMessage pin)
        {
            // get first line
            var splitData = pin.Content.Split('\n');
            if (splitData.Length == 0)
                return null;

            var raw = splitData[0].Split(';');
            if (raw.Length != 2)
                return null;

            if (!ulong.TryParse(raw[0], out var channelId))
                return null;
            if (!ulong.TryParse(raw[1], out var msgId))
                return null;

            var channel = Guild.GetChannel(channelId) as IMessageChannel;
            if (channel == null)
                return null;

            return await channel.GetMessageAsync(msgId);
        }

        /// <summary>
        /// Parses and stores existing pins
        /// </summary>
        private async Task ParsePins()
        {
            var msgs = PinChannel.GetMessagesAsync(500);
            await msgs.ForEachAsync(async mlist =>
            {
                foreach (var pin in mlist)
                {
                    if (!_client.CurrentUser.Id.Equals(pin.Author.Id))
                        continue;

                    var msg = await ResolveMessageFromPin(pin);
                    if (msg == null)
                        continue;

                    AddMsgToPin(msg, pin);
                }
            });
        }

        private bool CanPostPin(IMessage msg) => !_msgToPin.ContainsKey(msg.Id);

        private void RemoveMsgFromMap(IMessage msg) => _msgToPin.Remove(msg.Id, out var _);
        private void AddMsgToPin(IMessage msg, IMessage pin)
        {
            _msgToPin.AddOrUpdate(msg.Id, pin, (a, b) => pin);
        }

        private IMessage MsgToPin(IMessage msg)
        {
            _msgToPin.TryGetValue(msg.Id, out var pin);
            return pin;
        }

        private string BuildKey(IMessage msg)
        {
            return $"{msg.Channel.Id};{msg.Id}\n";
        }

     
        private async Task PostPin(IMessage msg)
        {
            var builder = new EmbedBuilder();
            builder.WithAuthor(a =>
            {
                a.WithName(msg.Author.Username);
                a.WithIconUrl(msg.Author.GetAvatarUrl());
            });

            builder.WithDescription(msg.Content);

            var links = msg.Content.Split("\t\n ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Where(s => s.StartsWith("http://") || s.StartsWith("www.") || s.StartsWith("https://"));
            foreach (var s in links)
                builder.WithImageUrl(s);

            if (msg.Attachments.Any())
                builder.WithImageUrl(msg.Attachments.Last().Url);

            builder.WithFooter(f => f.WithText($"Posted {msg.CreatedAt.UtcDateTime} UTC"));

            var embed = builder.Build();
            var pin = await PinChannel.SendMessageAsync(BuildKey(msg), false, embed);

            AddMsgToPin(msg, pin);
        }

        public async Task PushMessage(IMessage msg)
        {
            if (msg.Channel.Equals(PinChannel))
                return;

            if (!CanPostPin(msg))
                return;

            await PostPin(msg);
        }

        public async Task RemoveMessage(IMessage msg)
        {
            var pin = MsgToPin(msg) as IDeletable;
            if (pin == null)
                return;

            await pin.DeleteAsync();
            RemoveMsgFromMap(msg);
        }
    }
}