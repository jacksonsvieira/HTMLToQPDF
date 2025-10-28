using QuestPDF.Infrastructure;

using QuestPDF.Fluent;

namespace HTMLToQPDF.Components
{
    public delegate byte[]? GetImgBySrc(string src);

    internal class HTMLComponentsArgs
    {
        public Dictionary<string, TextStyle> TextStyles { get; }
        public Dictionary<string, Func<IContainer, IContainer>> ContainerStyles { get; }
        public Dictionary<string, TextStyle> ClassTextStyles { get; }
        public Dictionary<string, Func<IContainer, IContainer>> ClassContainerStyles { get; }
        public Dictionary<string, Action<TextDescriptor>> ClassTextAlignments { get; }
        public float ListVerticalPadding { get; }
        public GetImgBySrc GetImgBySrc { get; }

        public HTMLComponentsArgs(
            Dictionary<string, TextStyle> textStyles,
            Dictionary<string, Func<IContainer, IContainer>> containerStyles,
            Dictionary<string, TextStyle> classTextStyles,
            Dictionary<string, Func<IContainer, IContainer>> classContainerStyles,
            Dictionary<string, Action<TextDescriptor>> classTextAlignments,
            float listVerticalPadding,
            GetImgBySrc getImgBySrc)
        {
            TextStyles = textStyles;
            ContainerStyles = containerStyles;
            ClassTextStyles = classTextStyles;
            ClassContainerStyles = classContainerStyles;
            ClassTextAlignments = classTextAlignments;
            ListVerticalPadding = listVerticalPadding;
            GetImgBySrc = getImgBySrc;
        }
    }
}