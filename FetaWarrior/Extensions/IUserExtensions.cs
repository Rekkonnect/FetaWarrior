using Discord;

namespace FetaWarrior.Extensions
{
    public static class IUserExtensions
    {
        /// <summary>Determines whether a <seealso cref="IUser"/> is a normal non-bot user.</summary>
        /// <param name="user">The user to determine whether they are hooman.</param>
        /// <returns><see langword="true"/> if the user is not a bot, nor a webhook and has a non-zero discriminator, otherwise <see langword="false"/>.</returns>
        public static bool IsHuman(this IUser user)
        {
            return !user.IsBot && !user.IsWebhook && user.DiscriminatorValue != 0;
        }
    }
}
