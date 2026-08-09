namespace SlackClone.Helpers;

using System.Net;
using System.Text;
using System.Text.RegularExpressions;

// Tokenizes the raw text and encodes each token while assembling the HTML.
// Applying regexes to already generated HTML would let them act on each other's
// output, and would also cut a URL at the &amp; of its query string.
internal static partial class MessageFormatter
{
    public static string ToHtml(string content)
    {
        ArgumentNullException.ThrowIfNull(content);

        var builder = new StringBuilder(content.Length + 32);
        var position = 0;

        // Code blocks first: nothing inside them is formatted.
        foreach (Match block in CodeBlockRegex().Matches(content))
        {
            AppendLines(builder, content.AsSpan(position, block.Index - position));
            builder.Append("<pre class=\"msg-codeblock\"><code>")
                .Append(WebUtility.HtmlEncode(block.Groups[1].Value))
                .Append("</code></pre>");
            position = block.Index + block.Length;
        }

        AppendLines(builder, content.AsSpan(position));

        return builder.ToString();
    }

    private static void AppendLines(StringBuilder builder, ReadOnlySpan<char> text)
    {
        var first = true;
        foreach (var line in text.EnumerateLines())
        {
            if (!first)
            {
                builder.Append("<br />");
            }

            first = false;

            var body = line;
            var quoted = body.StartsWith(">", StringComparison.Ordinal);
            if (quoted)
            {
                body = body[1..];
                if (body.StartsWith(" ", StringComparison.Ordinal))
                {
                    body = body[1..];
                }

                builder.Append("<blockquote class=\"msg-blockquote\">");
            }

            AppendInline(builder, body.ToString());

            if (quoted)
            {
                builder.Append("</blockquote>");
            }
        }
    }

    private static void AppendInline(StringBuilder builder, string text)
    {
        var position = 0;
        foreach (Match match in InlineRegex().Matches(text))
        {
            AppendEmphasis(builder, text[position..match.Index]);

            if (match.Groups["code"].Success)
            {
                builder.Append("<code class=\"msg-code\">")
                    .Append(WebUtility.HtmlEncode(match.Groups["code"].Value))
                    .Append("</code>");
            }
            else if (match.Groups["url"].Success)
            {
                var encoded = WebUtility.HtmlEncode(match.Groups["url"].Value);
                builder.Append("<a href=\"").Append(encoded)
                    .Append("\" target=\"_blank\" rel=\"noopener noreferrer\">")
                    .Append(encoded).Append("</a>");
            }
            else
            {
                builder.Append("<span class=\"msg-mention\">")
                    .Append(WebUtility.HtmlEncode(match.Value))
                    .Append("</span>");
            }

            position = match.Index + match.Length;
        }

        AppendEmphasis(builder, text[position..]);
    }

    private static void AppendEmphasis(StringBuilder builder, string text)
    {
        var position = 0;
        foreach (Match match in EmphasisRegex().Matches(text))
        {
            builder.Append(WebUtility.HtmlEncode(text[position..match.Index]));

            var (tag, value) = match.Groups["bold"].Success
                ? ("strong", match.Groups["bold"].Value)
                : match.Groups["italic"].Success
                    ? ("em", match.Groups["italic"].Value)
                    : ("del", match.Groups["strike"].Value);

            builder.Append('<').Append(tag).Append('>')
                .Append(WebUtility.HtmlEncode(value))
                .Append("</").Append(tag).Append('>');

            position = match.Index + match.Length;
        }

        builder.Append(WebUtility.HtmlEncode(text[position..]));
    }

    [GeneratedRegex(@"```([\s\S]*?)```")]
    private static partial Regex CodeBlockRegex();

    [GeneratedRegex("`(?<code>[^`]+)`|(?<url>https?://[^\\s<>\"']+)|(?<mention>@\\w+)")]
    private static partial Regex InlineRegex();

    [GeneratedRegex(@"\*(?<bold>[^\*]+)\*|(?<!\w)_(?<italic>[^_]+)_(?!\w)|~(?<strike>[^~]+)~")]
    private static partial Regex EmphasisRegex();
}
