using System;
using GameUtils.Math;

namespace GameUtils.Graphics
{
    public abstract class Brush : IResource
    {
        public const int MaximumGradientColorCount = 16;

        readonly ResourceHandle handle;

        object currentTag;
        object newTag;

        float currentOpacity;
        float newOpacity;

        Matrix2x3 currentTransform;
        Matrix2x3 newTransform;

        public object Tag
        {
            get { return currentTag; }
            set { newTag = value; }
        }

        public virtual bool IsReady
        {
            get { return true; }
        }

        public float Opacity
        {
            get { return currentOpacity; }
            set { newOpacity = MathHelper.Clamp(value, 0f, 1f); }
        }

        public Matrix2x3 Transform
        {
            get { return currentTransform; }
            set { newTransform = value; }
        }

        internal Brush()
        {
            currentOpacity = 1f;
            newOpacity = 1f;
            currentTransform = Matrix2x3.Identity;
            newTransform = Matrix2x3.Identity;

            handle = GameEngine.RegisterResource(this);
        }

        internal abstract void FillBuffer(ref BrushBuffer buffer);

        internal BrushBuffer CreateBuffer()
        {
            var buffer = new BrushBuffer { Opacity = currentOpacity };
            FillBuffer(ref buffer);
            return buffer;
        }

        internal virtual void UpdateVertices(Vertex[] vertices)
        { }

        void IResource.ApplyChanges()
        {
            ApplyChanges();
        }

        protected virtual void ApplyChanges()
        {
            currentTag = newTag;
            currentOpacity = newOpacity;
            currentTransform = newTransform;
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

        protected virtual void Dispose(bool disposing)
        {
            handle.Dispose();
        }

        ~Brush()
        {
            Dispose(false);
        }

        UpdateMode IResource.UpdateMode
        {
            get { return UpdateMode.Synchronous; }
        }

        bool IResource.IsReady
        {
            get { return IsReady; }
            set { }
        }
    }
}
