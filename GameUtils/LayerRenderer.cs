using System;
using GameUtils.Collections;
using GameUtils.Graphics;

namespace GameUtils
{
    public class LayerRenderer<TState> : IStateRenderer<TState> where TState : LayerState<TState>
    {
        BufferedList<RenderContainer> renderables;
        float depthPosition;

        internal event EventHandler SortingRequested;

        private event EventHandler DepthPositionChanged;

        event EventHandler IStateRenderer<TState>.DepthPositionChanged
        {
            add { DepthPositionChanged += value; }
            remove { DepthPositionChanged -= value; }
        }

        float IStateRenderer<TState>.DepthPosition
        {
            get { return depthPosition; }
        }

        internal float DepthPosition
        {
            get { return depthPosition; }
            set
            {
                depthPosition = value;
                if (DepthPositionChanged != null)
                    DepthPositionChanged(this, EventArgs.Empty);
            }
        }

        internal void SetLayerInfo(BufferedList<RenderContainer> renderables)
        {
            this.renderables = renderables;
        }

        void IStateRenderer<TState>.Render(TState state, Renderer renderer)
        {
            OnRender(state, renderer);
        }

        protected virtual void OnRender(TState state, Renderer renderer)
        {
            renderables.ApplyChanges();
            if (SortingRequested != null)
                SortingRequested(this, EventArgs.Empty);
            
            foreach (RenderContainer renderable in renderables)
                renderable.Render(state.BufferIndex, renderer);
        }
    }

    public sealed class LayerRenderer : LayerRenderer<LayerState>
    { }
}
