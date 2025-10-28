using HTMLToQPDF.Components;
using HTMLToQPDF.Utils;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace HTMLQuestPDF
{
    public class HTMLDescriptor
    {
        internal HTMLComponent PDFPage { get; } = new HTMLComponent();

        public void SetHtml(string html)
        {
            PDFPage.HTML = html;
        }

        public void OverloadImgReceivingFunc(GetImgBySrc getImg)
        {
            PDFPage.GetImgBySrc = getImg;
        }

        public void SetTextStyleForHtmlElement(string tagName, TextStyle style)
        {
            PDFPage.TextStyles[tagName.ToLower()] = style;
        }

        public void SetContainerStyleForHtmlElement(string tagName, Func<IContainer, IContainer> style)
        {
            PDFPage.ContainerStyles[tagName.ToLower()] = style;
        }

        public void SetTextStyleForClass(string className, TextStyle style)
        {
            PDFPage.ClassTextStyles[className] = style;
        }

        public void SetContainerStyleForClass(string className, Func<IContainer, IContainer> style)
        {
            PDFPage.ClassContainerStyles[className] = style;
        }

        public void SetTextAlignmentForClass(string className, Action<TextDescriptor> alignment)
        {
            PDFPage.ClassTextAlignments[className] = alignment;
        }

        public void SetAlignmentForClass(string className, Alignment alignment)
        {
            switch (alignment)
            {
                case Alignment.Center:
                    PDFPage.ClassContainerStyles[className] = c => c.AlignCenter();
                    PDFPage.ClassTextAlignments[className] = t => t.AlignCenter();
                    break;
                case Alignment.Right:
                    PDFPage.ClassContainerStyles[className] = c => c.AlignRight();
                    PDFPage.ClassTextAlignments[className] = t => t.AlignRight();
                    break;
                case Alignment.Left:
                    PDFPage.ClassContainerStyles[className] = c => c.AlignLeft();
                    PDFPage.ClassTextAlignments[className] = t => t.AlignLeft();
                    break;
                case Alignment.Justify:
                    // Note: Justify not available in QuestPDF 2023.12.5
                    // Fallback to left alignment
                    PDFPage.ClassContainerStyles[className] = c => c.AlignLeft();
                    PDFPage.ClassTextAlignments[className] = t => t.AlignLeft();
                    break;
            }
        }

        public void SetListVerticalPadding(float value, Unit unit = Unit.Point)
        {
            PDFPage.ListVerticalPadding = UnitUtils.ToPoints(value, unit);
        }

        public void ClearClassTextStyles()
        {
            PDFPage.ClassTextStyles.Clear();
        }

        public void ClearClassContainerStyles()
        {
            PDFPage.ClassContainerStyles.Clear();
        }

        public void ClearClassTextAlignments()
        {
            PDFPage.ClassTextAlignments.Clear();
        }

        public void ClearDefaultClassStyles()
        {
            ClearClassTextStyles();
            ClearClassContainerStyles();
            ClearClassTextAlignments();
        }
    }

    public enum Alignment
    {
        Left,
        Center,
        Right,
        Justify
    }
}