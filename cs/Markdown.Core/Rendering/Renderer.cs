using System.Text;
using Markdown.Core.Parsing.Nodes;

namespace Markdown.Core.Rendering;

public class Renderer : IRenderer
  {
      public string Render(DocumentNode document)
      {
          var result = new StringBuilder();

          foreach (var block in document.Children)
          {
              switch (block)
              {
                  case HeadingNode heading:
                      result.Append(RenderHeading(heading));
                      break;
                  case ParagraphNode paragraph:
                      result.Append(RenderParagraph(paragraph));
                      break;
              }
          }

          return result.ToString();
      }

      private string RenderHeading(HeadingNode heading)
      {
          var content = RenderInlines(heading.Inlines);
          return $"<h{heading.Level}>{content}</h{heading.Level}>";
      }

      private string RenderParagraph(ParagraphNode paragraph)
      {
          var content = RenderInlines(paragraph.Inlines);
          return $"<p>{content}</p>";
      }
      

      private string RenderInlines(IList<InlineNode> inlines)
      {
          var builder = new StringBuilder();

          foreach (var inline in inlines)
          {
              switch (inline)
              {
                  case TextNode text:
                      builder.Append(EscapeHtml(text.Text));
                      break;
                  case EmphasisNode emphasis:
                      builder.Append("<em>");
                      builder.Append(RenderInlines(emphasis.Inlines));
                      builder.Append("</em>");
                      break;
                  case StrongNode strong:
                      builder.Append("<strong>");
                      builder.Append(RenderInlines(strong.Inlines));
                      builder.Append("</strong>");
                      break;
              }
          }

          return builder.ToString();
      }

      private string EscapeHtml(string text) =>
          text.Replace("&", "&amp;")
              .Replace("<", "&lt;")
              .Replace(">", "&gt;")
              .Replace("\"", "&quot;")
              .Replace("'", "&#39;");
  }
