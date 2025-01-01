using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DreamCatcher
{
    internal class StringUtils
    {
        public static string RemoveMarkdown(string markdown)
        {
            // 去除标题格式（保留标题内容）
            string noHeaderFormat = Regex.Replace(markdown, @"^#{1,6}\s*", "", RegexOptions.Multiline);

            // 去除粗体和斜体
            string noBoldItalic = Regex.Replace(noHeaderFormat, @"(\*\*|\*|__|_)(.*?)\1", "$2");

            // 去除有序列表和无序列表
            string noLists = Regex.Replace(noBoldItalic, @"^\s*[-\*\+]\s*", "", RegexOptions.Multiline);

            // 去除代码块
            string noCodeBlocks = Regex.Replace(noLists, @"`{3}.*?`{3}", "", RegexOptions.Singleline);

            // 去除行内代码
            string noInlineCode = Regex.Replace(noCodeBlocks, @"`(.*?)`", "$1");

            // 去除链接（保留文本）
            string noLinks = Regex.Replace(noInlineCode, @"\[(.*?)\]\(.*?\)", "$1");

            return noLinks.Trim();
        }
    }
}
