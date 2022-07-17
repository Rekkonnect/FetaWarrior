using Discord;
using System.Text.RegularExpressions;

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

        private static readonly Regex deletedUserPattern = new(@"Deleted User [\da-f]{8}", RegexOptions.Compiled);

        /// <summary>Determines whether the user appears deleted. This is NOT a 100% reliable measure, since the API does not provide a flag to determine the account's state.</summary>
        /// <param name="user">The user to determine if the account is deleted.</param>
        /// <returns>The determined user deletion state. This will return <see langword="true"/> if the user has no avatar and the user's name matches the deleted user username pattern.</returns>
        public static bool IsDeleted(this IUser user)
        {
            return user.AvatarId is null
                && deletedUserPattern.IsMatch(user.Username);
        }

        // This evades the weird runes around the username of this stupid framework that you cannot disable
        public static string ToNiceString(this IUser user)
        {
            return $"{user.Username}#{user.Discriminator}";
        }
    }
}
