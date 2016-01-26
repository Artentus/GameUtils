using System;
using System.Collections.Generic;
using System.Text;
using GameUtils.Math;

namespace GameUtils.Graphics
{
    /// <summary>
    /// Formats a given text and saves the resulting layout.
    /// </summary>
    public class TextLayout : IResource
    {
        internal struct PositionedGlyph
        {
            public Font.Glyph Glyph;
            public Vector2 Location;
        }

        readonly ISurface surface;
        readonly string text;
        readonly Vector2 bounds;
        readonly Font font;
        readonly float fontSize;
        readonly TextFormat format;

        bool dpiChanged;
        internal List<PositionedGlyph> Glyphs;

        public object Tag { get; set; }

        public bool IsReady { get; set; }

        void Layout()
        {
            float scale = (fontSize * surface.Dpi) / (72 * font.UnitsPerEm);

            int currentLineStart = 0;
            int currentLineEnd = 0;
            float currentLineLength = 0;

            int currentWordStart = 0;
            int currentWordEnd = 0;
            float currentWordLength = 0;

            var lines = new List<string>();
            var currentLine = new StringBuilder();

            for (int i = 0; i < text.Length; i++)
            {
                if (char.IsControl(text[i]))
                {

                }
                else
                {
                    Font.Glyph glyph = font.GetGlyph(text[i]);

                    currentWordLength += glyph.AdvanceWidth * scale;
                }
            }
        }

        void OnSurfaceDpiChanged(object obj, EventArgs e)
        {
            dpiChanged = true;
        }

        public TextLayout(string text, Vector2 bounds, Font font, float fontSize, TextFormat format)
        {
            surface = GameEngine.TryQueryComponent<ISurface>();
            if (surface == null) throw new InvalidOperationException("A surface must be registered before a text layout can be created.");
            surface.DpiChanged += OnSurfaceDpiChanged;

            this.text = text;
            this.bounds = bounds;
            this.font = font;
            this.fontSize = fontSize;
            this.format = format;

            dpiChanged = false;
            Glyphs = new List<PositionedGlyph>();
            Layout();
        }

        private bool disposed;

        public void Dispose()
        {
            if (!disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);

                disposed = true;
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Glyphs.Clear();
                Glyphs = null;
            }
            surface.DpiChanged -= OnSurfaceDpiChanged;
        }

        ~TextLayout()
        {
            Dispose(false);
        }

        public UpdateMode UpdateMode => UpdateMode.InitUpdateAsynchronous;
        
        public void ApplyChanges()
        {
            if (dpiChanged)
            {
                Layout();
                dpiChanged = false;
            }
        }
    }
}
