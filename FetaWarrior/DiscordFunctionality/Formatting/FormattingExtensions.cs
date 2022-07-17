namespace FetaWarrior.DiscordFunctionality.Formatting;

public static class FormattingExtensions
{
    public static IRichContent Bold(this IRichContent contained) => new BoldRichContent(contained);
    public static IRichContent Italics(this IRichContent contained) => new ItalicsRichContent(contained);
    public static IRichContent Strikethrough(this IRichContent contained) => new StrikethroughRichContent(contained);
    public static IRichContent Underline(this IRichContent contained) => new UnderlineRichContent(contained);
    public static IRichContent Spoiler(this IRichContent contained) => new SpoilerRichContent(contained);
    public static IRichContent ShortCode(this IRichContent contained) => new ShortCodeRichContent(contained);
    public static IRichContent CodeBlock(this IRichContent contained, string language = "") => new CodeBlockRichContent(contained, language);
    public static IRichContent Quote(this IRichContent contained) => new QuoteRichContent(contained);
}
