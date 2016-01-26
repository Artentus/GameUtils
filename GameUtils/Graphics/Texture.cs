using System;
using System.IO;
using SharpDX.Direct3D11;
using TK = SharpDX.Toolkit.Graphics;

namespace GameUtils.Graphics
{
    /// <summary>
    /// Represents a 2D texture.
    /// </summary>
    public sealed class Texture : IResource
    {
        readonly string fileName;
        readonly Stream stream;
        readonly bool isFromFile;
        readonly ResourceHandle handle;
        TK.Texture2D texture;
        Renderer renderer;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public object Tag { get; set; }

        /// <summary>
        /// Indicates whether the texture is loaded. If a texture isn't loaded it cannot be used.
        /// </summary>
        /// <remarks>This property might only be false if the texture is loaded asynchronously.</remarks>
        public bool IsReady { get; private set; }

        internal bool LoadAsync { get; private set; }

        internal ShaderResourceView ResourceView { get; private set; }

        /// <summary>
        /// Creates a texture from a file.
        /// Allowed file types are BMP, PNG, JPG and DDS.
        /// </summary>
        /// <param name="fileName">A path to a valid texture file.</param>
        /// <param name="loadAsync">Optional. Pass true if you want the texture to be loaded asynchronously.
        /// This can reduce frame drops but the texture might not be available immediately (see <see cref="Texture.IsReady"/> property).</param>
        public Texture(string fileName, bool loadAsync = false)
        {
            LoadAsync = loadAsync;

            this.fileName = fileName;
            isFromFile = true;
            handle = GameEngine.RegisterResource(this);
        }

        /// <summary>
        /// Creates a texture from a stream.
        /// The stream can contain data of the types BMP, PNG, JPG and DDS.
        /// </summary>
        /// /// <param name="stream">A stream containing valid texture data.</param>
        /// <param name="loadAsync">Optional. Pass true if you want the texture to be loaded asynchronously.
        /// This can reduce frame drops but the texture might not be available immediately (see <see cref="Texture.IsReady"/> property).</param>
        /// <remarks>If it is possible that the renderer is changed the stream musst be preserved as long as the texture exists.</remarks>
        public Texture(Stream stream, bool loadAsync = false)
        {
            LoadAsync = loadAsync;

            this.stream = stream;
            isFromFile = false;
            handle = GameEngine.RegisterResource(this);
        }

        void IResource.ApplyChanges()
        {
            Renderer newRenderer = GameEngine.QueryComponent<Renderer>();
            if (renderer == null || renderer != newRenderer)
            {
                renderer = newRenderer;

                if (ResourceView != null) ResourceView.Dispose();
                if (texture != null) texture.Dispose();

                if (renderer != null)
                {
                    ResourceView = isFromFile
                        ? renderer.CreateTexture(fileName, out texture)
                        : renderer.CreateTexture(stream, out texture);
                    Width = texture.Width;
                    Height = texture.Height;

                    IsReady = true;
                }
                else
                {
                    ResourceView = null;
                    texture = null;

                    IsReady = false;
                }
            }
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
            if (disposing) Tag = null;

            if (ResourceView != null) ResourceView.Dispose();
            if (texture != null) texture.Dispose();
            handle.Dispose();
        }

        ~Texture()
        {
            Dispose(false);
        }

        UpdateMode IResource.UpdateMode
        {
            get { return UpdateMode.InitUpdateAsynchronous; }
        }

        bool IResource.IsReady
        {
            get { return IsReady; }
            set { }
        }
    }
}
