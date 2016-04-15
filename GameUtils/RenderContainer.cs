using System;
using GameUtils.Graphics;

namespace GameUtils
{
    public abstract class RenderContainer : IComparable<RenderContainer>
    {
        public abstract event EventHandler DepthPositionChanged;

        public abstract float DepthPosition { get; }

        public abstract void Render(int bufferIndex, TimeSpan elapsed, Renderer renderer);

        int IComparable<RenderContainer>.CompareTo(RenderContainer other)
        {
            return DepthPosition.CompareTo(other.DepthPosition);
        }
    }

    internal class RenderContainer<TState> : RenderContainer where TState : IBufferedState<TState>
    {
        readonly UpdateContainer<TState> updateContainer;
        readonly TState[] buffers;
        readonly IStateRenderer<TState> renderer;

        public override event EventHandler DepthPositionChanged
        {
            add { renderer.DepthPositionChanged += value; }
            remove { renderer.DepthPositionChanged -= value; }
        }

        public override float DepthPosition => renderer.DepthPosition;

        public RenderContainer(UpdateContainer<TState> updateContainer, TState[] buffers, RegistrationContext<TState> context)
        {
            this.updateContainer = updateContainer;
            this.buffers = buffers;
            renderer = context.Renderer;
        }

        public override void Render(int bufferIndex, TimeSpan elapsed, Renderer renderer)
        {
            if (updateContainer.AvailableForRendering)
                this.renderer.Render(buffers[bufferIndex], elapsed, renderer);
        }
    }
}
