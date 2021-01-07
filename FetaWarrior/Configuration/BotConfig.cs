using Discord;
using Garyon.DataStructures;
using Garyon.Extensions;
using System.IO;
using System.Linq;

namespace FetaWarrior.Configuration
{
    // Probably rename to BotPrefixesConfig
    public class BotConfig
    {
        private const string prefixesFilePath = "prefixes.txt";
        public const string DefaultPrefix = "=";

        public static BotConfig Instance { get; }

        static BotConfig()
        {
            Instance = new BotConfig();
        }

        public BotConfig()
        {
            LoadInformation();
        }

        private FlexibleDictionary<ulong, string> prefixes = new();

        public string GetPrefixForGuild(IGuild guild) => GetPrefixForGuild(guild?.Id);
        public string GetPrefixForGuild(ulong? guildID)
        {
            // No need to save the information here because the default prefix is implied in every usage
            // Besides, if the default prefix changes, servers that are using the default prefix have to keep up
            return prefixes[guildID ?? default] ?? DefaultPrefix;
        }
        public void SetPrefixForGuild(IGuild guild, string prefix) => SetPrefixForGuild(guild.Id, prefix);
        public void SetPrefixForGuild(ulong guildID, string prefix)
        {
            prefixes[guildID] = prefix;
            SaveInformation();
        }
        public void ResetPrefixForGuild(IGuild guild) => ResetPrefixForGuild(guild.Id);
        public void ResetPrefixForGuild(ulong guildID)
        {
            prefixes.Remove(guildID);
            SaveInformation();
        }

        public void SaveInformation()
        {
            // Try writing the information to the database; in case multiple servers have their prefixes changed
            try
            {
                File.WriteAllLinesAsync(prefixesFilePath, prefixes.Select(kvp => $"{kvp.Key}|{kvp.Value ?? DefaultPrefix}"));
            }
            catch { }
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
