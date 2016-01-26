namespace GameUtils.Graphics
{
    /// <summary>
    /// Describes how text is formatted.
    /// </summary>
    public struct TextFormat
    {
        /// <summary>
        /// A default format to align in the top left with word wrapping enabled.
        /// </summary>
        public static TextFormat Default => new TextFormat(VerticalAlignment.Top, HorizontalAlignment.Left, true);

        /// <summary>
        /// Describes how the text is aligned vertically.
        /// </summary>
        public VerticalAlignment VerticalAlignment;

        /// <summary>
        /// Describes how the text is aligned horizontally.
        /// </summary>
        public HorizontalAlignment HorizontalAlignment;

        /// <summary>
        /// If true, line breaks will be inserted between words to avoid overflow.
        /// </summary>
        public bool WordWrap;

        /// <summary>
        /// Creates a new TextFormat.
        /// </summary>
        /// <param name="verticalAlignment">Describes how the text is aligned vertically.</param>
        /// <param name="horizontalAlignment">Describes how the text is aligned horizontally.</param>
        /// <param name="wordWrap">If true, line breaks will be inserted between words to avoid overflow.</param>
        public TextFormat(VerticalAlignment verticalAlignment, HorizontalAlignment horizontalAlignment, bool wordWrap)
        {
            VerticalAlignment = verticalAlignment;
            HorizontalAlignment = horizontalAlignment;
            WordWrap = wordWrap;
        }
    }
}
