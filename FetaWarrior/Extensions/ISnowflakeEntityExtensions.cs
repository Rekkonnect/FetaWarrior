using Discord;
using FetaWarrior.DiscordFunctionality.Formatting;

namespace FetaWarrior.Extensions;

public static class ISnowflakeEntityExtensions
{
    public static SnowflakeEntityType GetSnowflakeEntityType(this ISnowflakeEntity entity)
    {
        return entity switch
        {
            IUser => SnowflakeEntityType.User,
            IChannel => SnowflakeEntityType.Channel,
            IGuild => SnowflakeEntityType.Guild,
            IMessage => SnowflakeEntityType.Message,
            IRole => SnowflakeEntityType.Role,
            
            IApplication => SnowflakeEntityType.Application,
            IApplicationCommand => SnowflakeEntityType.ApplicationCommand,
            IAuditLogEntry => SnowflakeEntityType.AuditLogEntry,
            IDiscordInteraction => SnowflakeEntityType.DiscordInteraction,
            IWebhook => SnowflakeEntityType.Webhook,
            
            _ => SnowflakeEntityType.None,
        };
    }
}
