#nullable enable

namespace FetaWarrior.DiscordFunctionality.Formatting;

public enum SnowflakeEntityType
{
    None,
    
    User,
    Channel,
    Role,
    Guild,
    Message,
    
    // Lesser-known types
    Application,
    ApplicationCommand,
    AuditLogEntry,
    DiscordInteraction,
    Webhook,
}
