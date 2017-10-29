namespace PinBoy
{
    public sealed class BotConfig
    {
        public BotConfig(string token, ulong pinChannelId, ulong guildId)
        {
            Token = token;
            PinChannelId = pinChannelId;
            GuildId = guildId;
        }

        public string Token { get; }
        public ulong PinChannelId { get; }
        public ulong GuildId { get; }
    }
}