# Feta Warrior

This bot started as a quick solution for mass banning the thousands of accounts that raided a server that I'm an admin in. It was then considered to be released for the public since no other bot was found to provide that kind of functionality.

# [Invite Link](https://discord.com/api/oauth2/authorize?client_id=786220671331074109&permissions=8&scope=bot)

Requires Administrator privileges.

# Usage
## General Information

- All commands have a default prefix of `=`.
- `=help`: Shows all the available commands
- `=prefix`: Gets, sets or resets the prefix for the server. (Use `=help prefix` for more details).

## Functionality

- **Mass ban members based on system welcome messages**
  - The command (`=massban sm`) bans all users that were greeted with server messages in the channel that is set to send server welcome messages in. It is currently not adjustable, so if the channel was changed after the users that are intended to be banned, the command will only look for messages in the new channel. You will have to set the setting back to what the channel was when the to-ban users joined.
  - Example: `=massban sm 786063455639044097 786078457812484126`
    - This will ban all users whose system welcome messages' IDs range within [786063455639044097, 786078457812484126], **including** the provided IDs.
- **Mass ban members based on join dates**
  - The command (`=massban jd`) bans all users that joined within a specified timeframe, which is determined by specified users. The start of the timeframe (the least recent) is determined by the join date (and time) of the first user that was provided in the arguments, and the end of the timeframe (the most recent) is determined by the join date of the last user that was provided in the arguments. Note that first and last do not imply a list of users. Only providing 2 user IDs.
  - Example: `=massban jd 786063455639044097 786078457812484126`
    - This will ban all users that joined within the join date of the users 786063455639044097, and 786078457812484126, **including** the users with the provided IDs.
  
## Planned Functionality

- Mass kicking variants of the functions for mass banning

# Libraries Used

- [Discord.NET](https://github.com/discord-net/Discord.Net)
- [Garyon](https://github.com/AlFasGD/Garyon)

# Contribution

Contributions are welcome, under the following constraints:

- The PR either fixes an issue, or adds functionality that fits the bot. For clarification, just because there is a `FunModule`, it doesn't mean the bot should offer as many fun-related commands as possible; it's a moderation-oriented bot to offer just enough functionality that other bots do not provide.
- The code fits the code style that's applied in the pre-existing code.

## How to Test

¯\\\_(ツ)\_/¯

Ideally, you will want to test your code so that it does not cause issues. The official instance of the bot, including its secrets, are intentionally left unpublished, for obvious reasons.
