using Discord;
using Discord.Rest;
using Discord.WebSocket;
using FetaWarrior.Extensions;
using System;
using System.Text;

#nullable enable

namespace FetaWarrior.DiscordFunctionality.Formatting;

public interface IRichContent
{
    public abstract IRichContent ContainedContent { get; }

    public abstract void Append(StringBuilder builder);
}

public abstract record BaseLiteralContent
    : IRichContent
{
    IRichContent IRichContent.ContainedContent => null!;

    public abstract void Append(StringBuilder builder);
}
public abstract record BaseRichContent(IRichContent ContainedContent)
    : IRichContent
{
    public abstract string FormattingWrapperLeft { get; }
    public abstract string FormattingWrapperRight { get; }

    public virtual void Append(StringBuilder builder)
    {
        builder.Append(FormattingWrapperLeft);
        ContainedContent.Append(builder);
        builder.Append(FormattingWrapperRight);
    }
}
public abstract record CommonlyWrappedRichContent(IRichContent ContainedContent)
    : BaseRichContent(ContainedContent)
{
    public abstract string FormattingWrapper { get; }

    public sealed override string FormattingWrapperLeft => FormattingWrapper;
    public sealed override string FormattingWrapperRight => FormattingWrapper;

    public override void Append(StringBuilder builder)
    {
        var wrapper = FormattingWrapper;
        builder.Append(wrapper);
        ContainedContent.Append(builder);
        builder.Append(wrapper);
    }
}
public abstract record SingleCharacterCommonlyWrappedRichContent(IRichContent ContainedContent)
    : CommonlyWrappedRichContent(ContainedContent)
{
    public abstract char FormattingWrapperChar { get; }
    
    public sealed override string FormattingWrapper => FormattingWrapperChar.ToString();

    public override void Append(StringBuilder builder)
    {
        var wrapper = FormattingWrapperChar;
        builder.Append(wrapper);
        ContainedContent.Append(builder);
        builder.Append(wrapper);
    }
}

public sealed record BoldRichContent(IRichContent ContainedContent)
    : CommonlyWrappedRichContent(ContainedContent)
{
    public override string FormattingWrapper => "**";
}
public sealed record ItalicsRichContent(IRichContent ContainedContent)
    : SingleCharacterCommonlyWrappedRichContent(ContainedContent)
{
    public override char FormattingWrapperChar => '_';
}
public sealed record StrikethroughRichContent(IRichContent ContainedContent)
    : CommonlyWrappedRichContent(ContainedContent)
{
    public override string FormattingWrapper => "~~";
}
public sealed record UnderlineRichContent(IRichContent ContainedContent)
    : CommonlyWrappedRichContent(ContainedContent)
{
    public override string FormattingWrapper => "__";
}
public sealed record SpoilerRichContent(IRichContent ContainedContent)
    : CommonlyWrappedRichContent(ContainedContent)
{
    public override string FormattingWrapper => "||";
}
public sealed record ShortCodeRichContent(IRichContent ContainedContent)
    : SingleCharacterCommonlyWrappedRichContent(ContainedContent)
{
    public override char FormattingWrapperChar => '`';
}
public sealed record CodeBlockRichContent(IRichContent ContainedContent, string Language = "")
    : CommonlyWrappedRichContent(ContainedContent)
{
    public override string FormattingWrapper => "```";

    public override void Append(StringBuilder builder)
    {
        var wrapper = FormattingWrapper;
        builder.Append(wrapper);
        builder.AppendLine(Language);
        ContainedContent.Append(builder);
        builder.AppendLine();
        builder.Append(wrapper);
    }

    public static class LanguageNames
    {
        public const string ASCIIDoc = "asciidoc";
        public const string AutoHotKey = "autohotkey";
        public const string Bash = "bash";
        public const string CoffeeScript = "coffeescript";
        public const string CPlusPlus = "cpp";
        public const string CSharp = "cs";
        public const string CSS = "css";
        public const string Diff = "diff";
        public const string Fix = "fix";
        public const string GLSL = "glsl";
        public const string Ini = "ini";
        public const string JSON = "json";
        public const string Markdown = "md";
        public const string ML = "ml";
        public const string Prolog = "prolog";
        public const string Python = "py";
        public const string LaTeX = "tex";
        public const string XL = "xl";
        public const string XML = "xml";
    }
}
public sealed record QuoteRichContent(IRichContent ContainedContent)
    : BaseRichContent(ContainedContent)
{
    public override string FormattingWrapperLeft => "> ";
    public override string FormattingWrapperRight => "";

    public override void Append(StringBuilder builder)
    {
        builder.Append(FormattingWrapperLeft);
        ContainedContent.Append(builder);
    }
}

public sealed record MentionContent(ISnowflakeEntity Entity)
    : BaseLiteralContent
{
    public override void Append(StringBuilder builder)
    {
        builder.Append('<');
        Append(builder, Entity.GetSnowflakeEntityType());
        builder.Append(Entity.Id);
        builder.Append('>');
    }

    private static void Append(StringBuilder builder, SnowflakeEntityType snowflakeEntityType)
    {
        _ = snowflakeEntityType switch
        {
            SnowflakeEntityType.User => builder.Append('@'),
            SnowflakeEntityType.Channel => builder.Append('#'),
            SnowflakeEntityType.Role => builder.Append("@&"),
            
            _ => builder,
        };
    }

    // Thank you for prohibiting conversions from/to interfaces
    // At least I can narrow the possibilities of using invalid snowflake entities as mentionable
    public static implicit operator MentionContent(SocketUser content) => new(content as ISnowflakeEntity);
    public static implicit operator MentionContent(SocketMessage content) => new(content as ISnowflakeEntity);
    public static implicit operator MentionContent(SocketChannel content) => new(content as ISnowflakeEntity);
    public static implicit operator MentionContent(RestUser content) => new(content as ISnowflakeEntity);
    public static implicit operator MentionContent(RestMessage content) => new(content as ISnowflakeEntity);
    public static implicit operator MentionContent(RestChannel content) => new(content as ISnowflakeEntity);
}
public sealed record EmoteContent(Emote Emote)
    : BaseLiteralContent
{
    public override void Append(StringBuilder builder)
    {
        builder.Append('<');
        if (Emote.Animated)
            builder.Append('a');
        builder.Append(':');
        builder.Append(Emote.Name);
        builder.Append(':');
        builder.Append(Emote.Id);
        builder.Append('>');
    }
    
    public static implicit operator EmoteContent(Emote content) => new(content);
}
public sealed record TimestampContent(DateTimeOffset Timestamp, TimestampContent.Style FormatStyle = default)
    : BaseLiteralContent
{
    public TimestampContent(DateTime timestamp, Style formatStyle = default)
        : this(new DateTimeOffset(timestamp), formatStyle) { }

    public override void Append(StringBuilder builder)
    {
        builder.Append("<t:");
        builder.Append(Timestamp.ToUnixTimeSeconds());
        builder.Append(':');
        builder.Append(FormattingChar(FormatStyle));
        builder.Append('>');
    }

    private static char FormattingChar(Style style) => style switch
    {
        Style.ShortTime => 't',
        Style.LongTime => 'T',
        Style.ShortDate => 'd',
        Style.LongDate => 'D',
        Style.ShortDateTime => 'f',
        Style.LongDateTime => 'F',
        Style.RelativeTime => 'R',
        
        _ => 'f',
    };

    public static implicit operator TimestampContent(DateTimeOffset content) => new(content);
    public static implicit operator TimestampContent(DateTime content) => new(content);

    public enum Style
    {
        Default = default,
        
        ShortDateTime = Default,
        LongDateTime,

        ShortTime,
        LongTime,

        ShortDate,
        LongDate,
        
        RelativeTime,
    }
}

public sealed record RawContent(string RawContentString)
    : BaseLiteralContent
{
    public override void Append(StringBuilder builder)
    {
        builder.Append(RawContentString);
    }

    public static implicit operator RawContent(string content) => new(content);
}
