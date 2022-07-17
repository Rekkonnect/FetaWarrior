using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace FetaWarrior.DiscordFunctionality.TypeReaders;

[MandatoryTypeReader]
public class LazyGuildLoaderUserTypeReader : LazyGuildLoaderUserTypeReader<IUser>
{
}

public class LazyGuildLoaderUserTypeReader<T> : UserTypeReader<T>
    where T : class, IUser
{
    public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
    {
        if (!MentionUtils.TryParseUser(input, out ulong id))
            return await base.ReadAsync(context, input, services);

        var user = await BotClientManager.Instance.RestClient.GetGuildUserAsync(context.Guild.Id, id);

        if (user == null)
            return TypeReaderResult.FromError(CommandError.Unsuccessful, "The user could not be loaded.");

        return TypeReaderResult.FromSuccess(new TypeReaderValue(user, 1));
    }
}
