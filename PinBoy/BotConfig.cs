namespace PinBoy
{
    public sealed class BotConfig
    {
        public BotConfig(string token, ulong pinChannelId, ulong guildId, ulong crashUserId)
        {
            Token = token;
            PinChannelId = pinChannelId;
            GuildId = guildId;
            CrashUserId = crashUserId;
        }

        public string Token { get; }
        public ulong PinChannelId { get; }
        public ulong GuildId { get; }
        public ulong CrashUserId { get; }
    }
}