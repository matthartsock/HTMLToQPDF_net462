using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;
using System.Linq;

namespace HTMLQuestPDF.Utils
{
    internal static class HTMLUtils
    {
        private static readonly string spaceAfterLineElementPattern = @$"\S<\/({string.Join("|", HTMLMapSettings.LineElements)})> ";

        public static string PrepareHTML(string value)
        {
            var result = HttpUtility.HtmlDecode(value);
            result = RemoveExtraSpacesAndBreaks(result);
            result = RemoveSpacesAroundBr(result);
            result = WrapSpacesAfterLineElement(result);
            result = RemoveSpacesBetweenElements(result);
            result = RemoveParagraphsFromTableCells(result);
            return result;
        }

        private static string RemoveParagraphsFromTableCells(string html)
        {
            // Remove <p> tags that are direct children of <td> or <th> elements
            // This handles cases like <td><p>content</p></td> -> <td>content</td>
            // Also handles multiple paragraphs in the same cell
            
            // First handle single paragraph cases
            var pattern1 = @"(<t[dh][^>]*>)\s*<p[^>]*>(.*?)</p>\s*(</t[dh]>)";
            html = Regex.Replace(html, pattern1, "$1$2$3", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            
            // Then handle cases where there might be multiple paragraphs or other content mixed in
            // Remove any remaining <p> and </p> tags within table cells
            var pattern2 = @"(<t[dh][^>]*>(?:[^<]|<(?!/t[dh]))*?)<p[^>]*>";
            var pattern3 = @"</p>((?:[^<]|<(?!/t[dh]))*?</t[dh]>)";
            
            html = Regex.Replace(html, pattern2, "$1", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            html = Regex.Replace(html, pattern3, "$1", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            
            return html;
        }

        private static string RemoveExtraSpacesAndBreaks(string html)
        {
            return Regex.Replace(html, @"[ \r\n]+", " ");
        }

        private static string RemoveSpacesBetweenElements(string html)
        {
            return Regex.Replace(html, @">\s+<", _ => @"><").Replace("<space></space>", "<space> </space>");
        }

        private static string RemoveSpacesAroundBr(string html)
        {
            return Regex.Replace(html, @"\s+<\/?br\s*\/?>\s+", _ => @$"<br>");
        }

        private static string WrapSpacesAfterLineElement(string html)
        {
            return Regex.Replace(html, spaceAfterLineElementPattern, m => $"{m.Value.Substring(0, m.Value.Length - 1)}<space> </space>");
        }

    }
}