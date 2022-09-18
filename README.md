# Feta Warrior

This bot started as a quick solution for mass banning the thousands of accounts that raided a server that I was an admin in. It was then considered to be released for the public since no other bot was found to provide that kind of functionality.

# [Invite Link](https://discord.com/api/oauth2/authorize?client_id=786220671331074109&permissions=76806&scope=bot)

Permissions required:
- Kick Members
- Ban Members
- Manage Guild
- View Channels
- Send Messages
- Manage Messages
- Read Message History

# Usage
## General Information

- All commands have a default prefix of `=`.
- `=help`: Shows all the available commands, or commands that match the given query.
- `=ping`: Gets an estimation of the current latency. It uses Discord.NET's Client.Latency property, which is not updated too frequently. Multiple consecutive commands might yield the same result due to that.
- `=prefix`: Gets, sets or resets the prefix for the server. (Use `=help prefix` for more details).

## Functionality

- **Mass kick/ban members based on system messages**
- **Mass kick/ban members based on join dates**
- **Mass kick/ban members with default avatars**
- **Delete messages in a specified channel within a specified range**
- **Delete all messages sent in a specified channel**
- **Remove bans for deleted users**
- **Delete all announcement crosspost messages where the original announcement message was deleted**
  
## Planned Functionality

- Mass delete messages by user
- [Remove all reactions from banned users](https://github.com/Rekkonnect/FetaWarrior/issues/2)
- [Message and user commands for conveniently selecting a message/user range](https://github.com/Rekkonnect/FetaWarrior/issues/4)
- [Move all channels in a category to another](https://github.com/Rekkonnect/FetaWarrior/issues/6)
- [Move all users from a voice channel to another](https://github.com/Rekkonnect/FetaWarrior/issues/7)

# Libraries Used

- [Discord.NET](https://github.com/discord-net/Discord.Net)
- [Garyon](https://github.com/Rekkonsider/Garyon)

# Contribution

Contributions are welcome, under the following constraints:

- The PR either fixes an issue, or adds functionality that fits the bot. For clarification, just because there is a `FunModule`, it doesn't mean the bot should offer as many fun-related commands as possible; it's a moderation-oriented bot to offer just enough functionality that other bots do not provide.
- The code fits the code style that's applied in the pre-existing code.

## How to Test

¯\\\_(ツ)\_/¯

Ideally, you will want to test your code so that it does not cause issues. The official instance of the bot, including its secrets, are intentionally left unpublished, for obvious reasons.
