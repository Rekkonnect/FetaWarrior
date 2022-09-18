namespace FetaWarrior.DiscordFunctionality;

public struct Snowflake
{
    public const ulong MaxValue = long.MaxValue;
    public const ulong LargeValue = MaxValue - 5214;

    public static readonly string LargeSnowflakeString = LargeValue.ToString();

    public static readonly Snowflake MaxSnowflake = new(MaxValue);
    public static readonly Snowflake LargeSnowflake = new(LargeValue);

    public ulong Value { get; }

    public Snowflake(ulong value)
    {
        Value = value;
    }

    public static implicit operator ulong(Snowflake snowflake) => snowflake.Value;
    public static implicit operator Snowflake(ulong l) => new(l);

    public static bool TryParse(string s, out Snowflake snowflake)
    {
        bool success = ulong.TryParse(s, out ulong value);
        snowflake = default;

        if (!success)
            return false;

        snowflake = new(value);
        return true;
    }
}
