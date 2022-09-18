using Discord;
using Discord.Interactions;
using System;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality;

public sealed class SnowflakeTypeConverter : TypeConverter<Snowflake>
{
    public override ApplicationCommandOptionType GetDiscordType() => ApplicationCommandOptionType.String;

    public override Task<TypeConverterResult> ReadAsync(IInteractionContext context, IApplicationCommandInteractionDataOption option, IServiceProvider services)
    {
        return Task.FromResult(Read(option));
    }
    private static TypeConverterResult Read(IApplicationCommandInteractionDataOption option)
    {
        if (Snowflake.TryParse((string)option.Value, out var result))
            return TypeConverterResult.FromSuccess(result);

        return TypeConverterResult.FromError(InteractionCommandError.ConvertFailed, $"Value {option.Value} cannot be converted to a snowflake.");
    }
}