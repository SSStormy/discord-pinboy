namespace PinBoy
{
    public sealed class BotConfig
    {
        public BotConfig(string token, ulong pinChannelId, ulong guildId, ulong emoteReactUserId)
        {
            Token = token;
            PinChannelId = pinChannelId;
            GuildId = guildId;
            EmoteReactUserId = emoteReactUserId;
        }

        public string Token { get; }
        public ulong PinChannelId { get; }
        public ulong GuildId { get; }
        public ulong EmoteReactUserId { get; }
    }
}