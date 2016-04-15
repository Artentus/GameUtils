using System;
using System.IO;
using System.Threading.Tasks;
using SharpDX.Direct3D11;
using TK = SharpDX.Toolkit.Graphics;

namespace GameUtils.Graphics
{
    /// <summary>
    /// Represents a 2D texture.
    /// </summary>
    public sealed class Texture : IGraphicsResource
    {
        TK.Texture2D texture;
        volatile bool isReady;

        public int Width { get; private set; }
        public int Height { get; private set; }

        /// <summary>
        /// Indicates whether the texture is loaded. If a texture isn't loaded it cannot be used.
        /// </summary>
        /// <remarks>This property might only be false if the texture is loaded asynchronously.</remarks>
        public bool IsReady => isReady;

        public bool IsAsync { get; }

        internal ShaderResourceView ResourceView { get; private set; }

        /// <summary>
        /// Creates a texture from a file.
        /// Allowed file types are BMP, PNG, JPG and DDS.
        /// </summary>
        /// <param name="fileName">A path to a valid texture file.</param>
        /// <param name="loadAsync">Optional. Pass true if you want the texture to be loaded asynchronously.
        /// This can reduce frame drops but the texture might not be available immediately (see <see cref="Texture.IsReady"/> property).</param>
        /// <exception cref="InvalidOperationException">No renderer is registered.</exception>
        public Texture(string fileName, bool loadAsync = false)
        {
            IsAsync = loadAsync;
            var file = new FileInfo(fileName);

            Renderer renderer = GameEngine.TryQueryComponent<Renderer>();
            if (renderer == null) throw new InvalidOperationException("A renderer must be registered before a texture can be created.");

            if (loadAsync)
            {
                Task.Run(() => renderer.CreateTexture(file.FullName))
                    .ContinueWith((t) =>
                    {
                        var result = t.Result;
                        texture = result.Texture;
                        ResourceView = result.ResourceView;

                        Width = texture.Width;
                        Height = texture.Height;
                        isReady = true;
                    });
            }
            else
            {
                var result = renderer.CreateTexture(file.FullName);
                texture = result.Texture;
                ResourceView = result.ResourceView;

                Width = texture.Width;
                Height = texture.Height;
                isReady = true;
            }
        }

        /// <summary>
        /// Creates a texture from a stream.
        /// The stream can contain data of the types BMP, PNG, JPG and DDS.
        /// </summary>
        /// <param name="stream">A stream containing valid texture data.</param>
        /// <param name="loadAsync">Optional. Pass true if you want the texture to be loaded asynchronously.
        /// This can reduce frame drops but the texture might not be available immediately (see <see cref="Texture.IsReady"/> property).</param>
        /// <exception cref="InvalidOperationException">No renderer is registered.</exception>
        public Texture(Stream stream, bool loadAsync = false)
        {
            IsAsync = loadAsync;

            Renderer renderer = GameEngine.QueryComponent<Renderer>();
            if (renderer == null) throw new InvalidOperationException("A renderer must be registered before a texture can be created.");

            if (loadAsync)
            {
                Task.Run(() => renderer.CreateTexture(stream))
                .ContinueWith((t) =>
                {
                    var result = t.Result;
                    texture = result.Texture;
                    ResourceView = result.ResourceView;

                    Width = texture.Width;
                    Height = texture.Height;
                    isReady = true;
                });
            }
            else
            {
                var result = renderer.CreateTexture(stream);
                texture = result.Texture;
                ResourceView = result.ResourceView;

                Width = texture.Width;
                Height = texture.Height;
                isReady = true;
            }
        }

        private bool disposed;

        public void Dispose()
        {
            if (!disposed)
            {
                DisposeInner();
                GC.SuppressFinalize(this);

                disposed = true;
            }
        }

        private void DisposeInner()
        {
            ResourceView?.Dispose();
            texture?.Dispose();
        }

        ~Texture()
        {
            DisposeInner();
        }
    }
}
