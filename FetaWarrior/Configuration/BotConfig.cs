using Discord;
using Garyon.DataStructures;
using Garyon.Extensions;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace FetaWarrior.Configuration
{
    // Probably rename to BotPrefixesConfig
    public class BotConfig
    {
        // TODO: Use the database whenever it's setup
        private const string prefixesFilePath = "prefixes.txt";
        public const string DefaultPrefix = "=";

        public static BotConfig Instance { get; }

        static BotConfig()
        {
            Instance = new BotConfig();
        }

        private readonly FlexibleDictionary<ulong, string> prefixes = new();

        private readonly object writer = new();

        public BotConfig()
        {
            LoadInformation();
        }

        public string GetPrefixForChannel(IChannel commandChannel)
        {
            return GetPrefixForEntity(GetRecordedEntity(commandChannel));
        }
        public void SetPrefixForChannel(IChannel commandChannel, string prefix)
        {
            SetPrefixForEntity(GetRecordedEntity(commandChannel), prefix);
        }
        public void ResetPrefixForChannel(IChannel commandChannel)
        {
            ResetPrefixForEntity(GetRecordedEntity(commandChannel));
        }

        public string GetPrefixForGuild(IGuild guild) => GetPrefixForEntity(guild);
        public void SetPrefixForGuild(IGuild guild, string prefix) => SetPrefixForEntity(guild, prefix);
        public void ResetPrefixForGuild(IGuild guild) => ResetPrefixForEntity(guild);

        private static ISnowflakeEntity GetRecordedEntity(IChannel commandChannel)
        {
            return commandChannel switch
            {
                IGuildChannel guildChannel => guildChannel.Guild,
                _ => commandChannel
            };
        }

        private string GetPrefixForEntity(ISnowflakeEntity entity) => GetPrefixForEntity(entity?.Id);
        private string GetPrefixForEntity(ulong? snowflake)
        {
            // No need to save the information here because the default prefix is implied in every usage
            // Besides, if the default prefix changes, servers that are using the default prefix have to keep up
            return prefixes[snowflake ?? default] ?? DefaultPrefix;
        }
        private void SetPrefixForEntity(ISnowflakeEntity entity, string prefix) => SetPrefixForEntity(entity.Id, prefix);
        private void SetPrefixForEntity(ulong snowflake, string prefix)
        {
            prefixes[snowflake] = prefix;
            SaveInformation();
        }
        private void ResetPrefixForEntity(ISnowflakeEntity entity) => ResetPrefixForEntity(entity.Id);
        private void ResetPrefixForEntity(ulong snowflake)
        {
            prefixes.Remove(snowflake);
            SaveInformation();
        }

        public void SaveInformation()
        {
            lock (writer)
            {
                File.WriteAllLinesAsync(prefixesFilePath, prefixes.Select(kvp => $"{kvp.Key}|{kvp.Value ?? DefaultPrefix}"));
            }
        }
        public void LoadInformation()
        {
            try
            {
                var lines = File.ReadAllLines(prefixesFilePath);
                foreach (var l in lines)
                {
                    var split = l.Split('|');
                    ulong id = ulong.Parse(split[0]);
                    var prefix = split.Skip(1).Combine('|');
                    prefixes.Add(id, prefix);
                }
            }
            catch { }
        }
    }
}
