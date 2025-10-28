using HtmlAgilityPack;
using HTMLQuestPDF.Extensions;
using HTMLToQPDF.Components;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace HTMLQuestPDF.Components
{
    internal class ParagraphComponent : IComponent
    {
        private readonly List<HtmlNode> lineNodes;
        private readonly HTMLComponentsArgs args;

        public ParagraphComponent(List<HtmlNode> lineNodes, HTMLComponentsArgs args)
        {
            this.lineNodes = lineNodes;
            this.args = args;
        }

        private HtmlNode? GetParrentBlock(HtmlNode node)
        {
            if (node == null) return null;
            return node.IsBlockNode() ? node : GetParrentBlock(node.ParentNode);
        }

        private HtmlNode? GetListItemNode(HtmlNode node)
        {
            if (node == null || node.IsList()) return null;
            return node.IsListItem() ? node : GetListItemNode(node.ParentNode);
        }

        private void ApplyClassTextAlignments(HtmlNode node, TextDescriptor text)
        {
            if (node == null) return;

            // Get classes from this node and apply alignments
            var classes = node.GetClasses();
            foreach (var className in classes)
            {
                if (args.ClassTextAlignments.TryGetValue(className, out var alignment))
                {
                    alignment(text);
                }
            }

            // Also check parent nodes for alignment classes
            if (node.ParentNode != null)
            {
                ApplyClassTextAlignments(node.ParentNode, text);
            }
        }

        public void Compose(IContainer container)
        {
            var listItemNode = GetListItemNode(lineNodes.First()) ?? GetParrentBlock(lineNodes.First());
            if (listItemNode == null) return;

            var numberInList = listItemNode.GetNumberInList();

            if (numberInList != -1 || listItemNode.GetListNode() != null)
            {
                container.Row(row =>
                {
                    var listPrefix = numberInList == -1 ? "" : numberInList == 0 ? "•  " : $"{numberInList}. ";
                    row.AutoItem().MinWidth(26).AlignCenter().Text(listPrefix);
                    container = row.RelativeItem();
                });
            }

            var first = lineNodes.First();
            var last = lineNodes.First();

            first.InnerHtml = first.InnerHtml.TrimStart();
            last.InnerHtml = last.InnerHtml.TrimEnd();

            container.Text(text =>
            {
                // Apply class-based text alignments from the block parent or list item
                ApplyClassTextAlignments(listItemNode, text);

                // Apply the text content
                GetAction(lineNodes)(text);
            });
        }

        private Action<TextDescriptor> GetAction(List<HtmlNode> nodes)
        {
            return text =>
            {
                lineNodes.ForEach(node => GetAction(node).Invoke(text));
            };
        }

        private Action<TextDescriptor> GetAction(HtmlNode node)
        {
            return text =>
            {
                if (node.NodeType == HtmlNodeType.Text)
                {
                    var span = text.Span(node.InnerText);
                    GetTextSpanAction(node).Invoke(span);
                }
                else if (node.IsBr())
                {
                    var span = text.Span("\n");
                    GetTextSpanAction(node).Invoke(span);
                }
                else
                {
                    foreach (var item in node.ChildNodes)
                    {
                        var action = GetAction(item);
                        action(text);
                    }
                }
            };
        }

        private TextSpanAction GetTextSpanAction(HtmlNode node)
        {
            return spanAction =>
            {
                var action = GetTextStyles(node);
                action(spanAction);
                if (node.ParentNode != null)
                {
                    var parrentAction = GetTextSpanAction(node.ParentNode);
                    parrentAction(spanAction);
                }
            };
        }

        public TextSpanAction GetTextStyles(HtmlNode element)
        {
            return (span) => span.Style(GetTextStyle(element));
        }

        public TextStyle GetTextStyle(HtmlNode element)
        {
            // Start with tag-based style
            var style = args.TextStyles.TryGetValue(element.Name.ToLower(), out TextStyle? tagStyle)
                ? tagStyle
                : TextStyle.Default;

            // Apply class-based styles (classes override tags)
            var classes = element.GetClasses();
            foreach (var className in classes)
            {
                if (args.ClassTextStyles.TryGetValue(className, out var classStyle))
                {
                    style = classStyle;
                }
            }

            return style;
        }
    }
}