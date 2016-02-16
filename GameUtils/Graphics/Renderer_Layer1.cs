using System;
using GameUtils.Math;
using SharpDX;
using SharpDX.Direct3D11;
using Rectangle = GameUtils.Math.Rectangle;
using Vector2 = GameUtils.Math.Vector2;

namespace GameUtils.Graphics
{
    /// <summary>
    /// The renderer is a singletone component and performs basic drawing operations.
    /// </summary>
    public sealed partial class Renderer
    {
        Matrix2x3 transform = Matrix2x3.Identity;
        InterpolationMode defaultInterpolationMode = InterpolationMode.Linear;
        static readonly BrushBuffer ClipBrush;

        static unsafe Renderer()
        {
            ClipBrush.Type = 1;
            fixed (float* colors = ClipBrush.GradientColors)
            {
                colors[3] = 1;
            }
        }

        /// <summary>
        /// The world transformation matrix.
        /// </summary>
        public Matrix2x3 Transform
        {
            get { return transform; }
            set
            {
                transform = value;
                worldMatrix = transform.ToSharpDXMatrix();
                UpdateMatrixBuffer();
            }
        }

        /// <summary>
        /// Resets the world transformation matrix to the identity matrix.
        /// </summary>
        public void ResetTransform()
        {
            Transform = Matrix2x3.Identity;
        }

        /// <summary>
        /// Sets the amount of vertical blanks the engine waits between presenting the frames.
        /// </summary>
        /// <remarks>A value of 0 means instant presentation of the frames. Too high values can cause a crash.</remarks>
        public int SyncInterval { get; set; }

        /// <summary>
        /// The interpolation mode used by default.
        /// InterpolationMode.Default is not a valid value for this property.
        /// </summary>
        /// <remarks>The default value for this property is 'InterpolationMode.Linear'.</remarks>
        /// <exception cref="System.ArgumentException">InterpolationMode.Default has been passed.</exception>
        public InterpolationMode DefaultInterpolationMode
        {
            get { return defaultInterpolationMode; }
            set
            {
                if (value == InterpolationMode.Default)
                    throw new ArgumentException("The value can not be 'InterpolationMode.Default'.");

                defaultInterpolationMode = value;
            }
        }

        /// <summary>
        /// Retrieves the size of the drawing surface.
        /// </summary>
        public Vector2 SurfaceSize { get; private set; }

        /// <summary>
        /// Retrieves the bounds of the drawing surface.
        /// </summary>
        public Rectangle SurfaceBounds { get; private set; }
        
        /// <summary>
        /// The dpi of the renderers surface.
        /// </summary>
        public float Dpi => surface.Dpi;

        /// <summary>
        /// Draws a colored texture.
        /// </summary>
        /// <param name="texture">The texture.</param>
        /// <param name="color">The color of the texture.</param>
        /// <param name="destinationRectangle">The rectangle the texture will be displayed in.</param>
        /// <param name="sourceRectangle">The rectangle defining the part of the texture that will be displayed.</param>
        /// <param name="opacity">The opacity of the drawn texture.</param>
        /// <param name="interpolationMode">The interpolation mode used to stretch the texture.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">The source rectangle lies not inside the textures bounds.</exception>
        public void DrawTexture(Texture texture, Color4 color, Rectangle destinationRectangle, Rectangle sourceRectangle, float opacity = 1f, InterpolationMode interpolationMode = InterpolationMode.Default)
        {
            if (sourceRectangle.Left < 0 || sourceRectangle.Top < 0 ||
                sourceRectangle.Right > texture.Width || sourceRectangle.Bottom > texture.Height)
                throw new ArgumentOutOfRangeException(nameof(sourceRectangle));

            SetTexture(texture, color, opacity);
            SetSamplerState(currentWrapMode, interpolationMode == InterpolationMode.Default ? defaultInterpolationMode : interpolationMode);

            float textCoordLeft = sourceRectangle.Left / texture.Width;
            float textCoordRight = sourceRectangle.Right / texture.Width;
            float textCoordTop = sourceRectangle.Top / texture.Height;
            float textCoordBottom = sourceRectangle.Bottom / texture.Height;

            Vertex[] vertices = new[]
            {
                new Vertex(destinationRectangle.Left, destinationRectangle.Top, textCoordLeft, textCoordTop),
                new Vertex(destinationRectangle.Right, destinationRectangle.Top, textCoordRight, textCoordTop),
                new Vertex(destinationRectangle.Left, destinationRectangle.Bottom, textCoordLeft, textCoordBottom),
                new Vertex(destinationRectangle.Right, destinationRectangle.Bottom, textCoordRight, textCoordBottom),
            };
            int[] indices = { 0, 1, 2, 1, 2, 3 };

            DrawVertices(indices, vertices, 0);
        }

        /// <summary>
        /// Draws a colored texture.
        /// </summary>
        /// <param name="texture">The texture.</param>
        /// <param name="color">The color of the texture.</param>
        /// <param name="destinationRectangle">The rectangle the texture will be displayed in.</param>
        /// <param name="opacity">The opacity of the drawn texture.</param>
        /// <param name="interpolationMode">The interpolation mode used to stretch the texture.</param>
        public void DrawTexture(Texture texture, Color4 color, Rectangle destinationRectangle, float opacity = 1f, InterpolationMode interpolationMode = InterpolationMode.Default)
        {
            SetTexture(texture, color, opacity);
            SetSamplerState(currentWrapMode, interpolationMode == InterpolationMode.Default ? defaultInterpolationMode : interpolationMode);

            Vertex[] vertices = new[]
            {
                new Vertex(destinationRectangle.Left, destinationRectangle.Top, 0, 0),
                new Vertex(destinationRectangle.Right, destinationRectangle.Top, 1, 0),
                new Vertex(destinationRectangle.Left, destinationRectangle.Bottom, 0, 1),
                new Vertex(destinationRectangle.Right, destinationRectangle.Bottom, 1, 1),
            };
            int[] indices = { 0, 1, 2, 1, 2, 3 };

            DrawVertices(indices, vertices, 0);
        }

        /// <summary>
        /// Draws a colored texture.
        /// </summary>
        /// <param name="texture">The texture.</param>
        /// <param name="color">The color of the texture.</param>
        /// <param name="location">The loction the texture will be displayed at.</param>
        /// <param name="opacity">The opacity of the drawn texture.</param>
        /// <param name="interpolationMode">The interpolation mode used to stretch the texture.</param>
        public void DrawTexture(Texture texture, Color4 color, Vector2 location, float opacity = 1, InterpolationMode interpolationMode = InterpolationMode.Default)
        {
            DrawTexture(texture, color, new Rectangle(location.X, location.Y, texture.Width, texture.Height), opacity, interpolationMode);
        }

        /// <summary>
        /// Draws a texture.
        /// </summary>
        /// <param name="texture">The texture.</param>
        /// <param name="destinationRectangle">The rectangle the texture will be displayed in.</param>
        /// <param name="sourceRectangle">The rectangle defining the part of the texture that will be displayed.</param>
        /// <param name="opacity">The opacity of the drawn texture.</param>
        /// <param name="interpolationMode">The interpolation mode used to stretch the texture.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">The source rectangle lies not inside the textures bounds.</exception>
        public void DrawTexture(Texture texture, Rectangle destinationRectangle, Rectangle sourceRectangle, float opacity = 1f, InterpolationMode interpolationMode = InterpolationMode.Default)
        {
            DrawTexture(texture, Color4.White, destinationRectangle, sourceRectangle, opacity, interpolationMode);
        }

        /// <summary>
        /// Draws a texture.
        /// </summary>
        /// <param name="texture">The texture.</param>
        /// <param name="destinationRectangle">The rectangle the texture will be displayed in.</param>
        /// <param name="opacity">The opacity of the drawn texture.</param>
        /// <param name="interpolationMode">The interpolation mode used to stretch the texture.</param>
        public void DrawTexture(Texture texture, Rectangle destinationRectangle, float opacity = 1f, InterpolationMode interpolationMode = InterpolationMode.Default)
        {
            DrawTexture(texture, Color4.White, destinationRectangle, opacity, interpolationMode);
        }

        /// <summary>
        /// Draws a texture.
        /// </summary>
        /// <param name="texture">The texture.</param>
        /// <param name="location">The loction the texture will be displayed at.</param>
        /// <param name="opacity">The opacity of the drawn texture.</param>
        /// <param name="interpolationMode">The interpolation mode used to stretch the texture.</param>
        public void DrawTexture(Texture texture, Vector2 location, float opacity = 1, InterpolationMode interpolationMode = InterpolationMode.Default)
        {
            DrawTexture(texture, Color4.White, new Rectangle(location.X, location.Y, texture.Width, texture.Height), opacity, interpolationMode);
        }

        /// <summary>
        /// Fills a rectangle with a brush.
        /// </summary>
        /// <param name="rectangle">The rectangle to fill.</param>
        /// <param name="brush">The brush used to fill the rectangle.</param>
        public void FillRectangle(Rectangle rectangle, Brush brush)
        {
            SetBrush(brush);

            Vertex[] vertices = new[]
            {
                new Vertex(rectangle.Left, rectangle.Top),
                new Vertex(rectangle.Right, rectangle.Top),
                new Vertex(rectangle.Left, rectangle.Bottom),
                new Vertex(rectangle.Right, rectangle.Bottom),
            };
            int[] indices = { 0, 1, 2, 1, 2, 3 };

            brush.UpdateVertices(vertices);
            DrawVertices(indices, vertices, 0);
        }

        /// <summary>
        /// Fills a set of triangles with a brush.
        /// </summary>
        /// <param name="indices">The index buffer.</param>
        /// <param name="vertices">The vertex buffer.</param>
        /// <param name="brush">The brush used to fill the triangles.</param>
        public void FillTriangleList(int[] indices, Vector2[] vertices, Brush brush)
        {
            SetBrush(brush);

            var realVertices = new Vertex[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
                realVertices[i] = new Vertex(vertices[i]);
            brush.UpdateVertices(realVertices);

            DrawVertices(indices, realVertices);
        }

        /// <summary>
        /// Draws text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="location">The location the text will be displayed at.</param>
        /// <param name="font">The font of the text.</param>
        /// <param name="size">The font size in points.</param>
        /// <param name="brush">The brush used to draw the text.</param>
        public void DrawText(string text, Vector2 location, Font font, float size, Brush brush)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            float scale = (size * Dpi) / (72 * font.UnitsPerEm);
            SetBrush(brush);

            int horizontalPosition = 0;
            int verticalPosition = font.Ascend;
            for (int i = 0; i < text.Length; i++)
            {
                if (char.IsControl(text[i]))
                {
                    switch (text[i])
                    {
                        case '\n':
                            verticalPosition += font.Ascend - font.Descend + font.LineGap;
                            horizontalPosition = 0;
                            break;
                    }
                }
                else
                {
                    Font.Glyph glyph = font.GetGlyph(text[i]);
                    if (glyph.Vertices.Length > 0)
                    {
                        textMatrix = Matrix.Transpose(Matrix.Translation(horizontalPosition + glyph.LeftSideBearing, -verticalPosition, 0)
                            * Matrix.Scaling(scale, -scale, 1)
                            * Matrix.Translation(location.X, location.Y, 0));
                        UpdateMatrixBuffer();

                        brush.UpdateVertices(glyph.Vertices);
                        DrawVertices(glyph.Indices, glyph.Vertices);
                    }

                    horizontalPosition += glyph.AdvanceWidth;
                }
            }

            textMatrix = Matrix.Identity;
            UpdateMatrixBuffer();
        }

        /// <summary>
        /// Draws layouted text.
        /// </summary>
        /// <param name="layout">The precalculated layout.</param>
        /// <param name="location">THe location the layout will be displayed at.</param>
        /// <param name="brush">The brush used to draw the layout.</param>
        public void DrawTextLayout(TextLayout layout, Vector2 location, Brush brush)
        {
            
        }

        /// <summary>
        /// Draws formatted text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="bounds">The bounds the text will be displayed in.</param>
        /// <param name="font">The font of the text.</param>
        /// <param name="size">The font size in points.</param>
        /// <param name="brush">The brush used to draw the text.</param>
        /// <param name="format">The format of the text.</param>
        public void DrawText(string text, Rectangle bounds, Font font, float size, TextFormat format, Brush brush)
        {
            var layout = new TextLayout(text, bounds.Size, font, size, format);
            DrawTextLayout(layout, bounds.Location, brush);
        }

        /// <summary>
        /// Sets the clipping region to the specified triangles.
        /// </summary>
        /// <param name="indices">The index buffer.</param>
        /// <param name="vertices">The vertex buffer.</param>
        /// <param name="reset">Optional. If set to true the old clipping region is discarded, otherweise the new region adds to the old one.</param>
        /// <remarks>The clipping region is reset automatically before every render cycle.</remarks>
        public void SetClip(int[] indices, Vector2[] vertices, bool reset = true)
        {
            SetBrushBuffer(ClipBrush);

            if (reset) ResetClip();
            deviceContext.OutputMerger.SetDepthStencilState(clipDepthStencilState, 1);

            var realVertices = new Vertex[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
                realVertices[i] = new Vertex(vertices[i]);
            DrawVertices(indices, realVertices);

            deviceContext.OutputMerger.SetDepthStencilState(clippingDepthStencilState, 1);
        }

        /// <summary>
        /// Sets the clipping region to the specified rectangle.
        /// </summary>
        /// <param name="region">A rectangle defining the new clipping region.</param>
        /// <param name="reset">Optional. If set to true the old clipping region is discarded, otherweise the new region adds to the old one.</param>
        /// <remarks>The clipping region is reset automatically before every render cycle.</remarks>
        public void SetClip(Rectangle region, bool reset = true)
        {
            SetBrushBuffer(ClipBrush);

            if (reset) ResetClip();
            deviceContext.OutputMerger.SetDepthStencilState(clipDepthStencilState, 1);

            Vertex[] vertices = new[]
            {
                new Vertex(region.Left, region.Top),
                new Vertex(region.Right, region.Top),
                new Vertex(region.Left, region.Bottom),
                new Vertex(region.Right, region.Bottom),
            };
            int[] indices = { 0, 1, 2, 1, 2, 3 };
            DrawVertices(indices, vertices, 0);

            deviceContext.OutputMerger.SetDepthStencilState(clippingDepthStencilState, 1);
        }

        /// <summary>
        /// Resets the clipping region.
        /// </summary>
        /// <remarks>The clipping region is reset automatically before every render cycle.</remarks>
        public void ResetClip()
        {
            deviceContext.OutputMerger.SetDepthStencilState(defaultDepthStencilState);
            deviceContext.ClearDepthStencilView(depthStencilView, DepthStencilClearFlags.Stencil, 1, 0);
        }

        //public void FillEllipse(Vector2 center, float radiusX, float radiusY, Brush brush, float precision = 0.25f)
        //{
        //    SetBrush(brush);


        //}
    }
}
