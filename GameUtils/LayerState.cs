using System;
using System.Threading.Tasks;
using GameUtils.Collections;

namespace GameUtils
{
    public class LayerState<TSelf> : IBufferedState<TSelf> where TSelf : LayerState<TSelf>
    {
        BufferedLinkedList<UpdateContainer> updateables;

        internal int BufferIndex { get; private set; }

        internal void SetLayerInfo(BufferedLinkedList<UpdateContainer> updateables, int bufferIndex)
        {
            this.updateables = updateables;
            BufferIndex = bufferIndex;
        }

        void IBufferedState<TSelf>.Update(TSelf oldState, TimeSpan elapsed)
        {
            OnUpdate(oldState, elapsed);

            updateables.ApplyChanges();
            Parallel.ForEach(updateables, updateable => updateable.Update(BufferIndex, elapsed));
        }

        protected virtual void OnUpdate(TSelf oldState, TimeSpan elapsed)
        { }
    }

    public sealed class LayerState : LayerState<LayerState>
    { }
}
