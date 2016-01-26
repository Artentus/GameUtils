using System;
using System.Collections.Generic;

namespace GameUtils
{
    public class GameHandle<T> : IDisposable where T : IBufferedState<T>
    {
        RegistrationTargetBase target;
        LinkedListNode<UpdateContainer> updateableNode;
        RenderContainer renderableItem;

        bool disposed;

        internal GameHandle(RegistrationTargetBase target, LinkedListNode<UpdateContainer> updateableNode, RenderContainer renderableItem)
        {
            this.target = target;
            target.ChangesApplied += ChangesAppliedHandler;

            this.updateableNode = updateableNode;
            this.renderableItem = renderableItem;
        }

        private void ChangesAppliedHandler(object sender, EventArgs e)
        {
            if (disposed)
            {
                updateableNode.Value.Dispose();
                updateableNode = null;

                target.ChangesApplied -= ChangesAppliedHandler;
                target = null;
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                target.Updateables.Remove(updateableNode);
                if (renderableItem != null)
                {
                    target.Renderables.Remove(renderableItem);
                    renderableItem = null;
                }
            }
        }
    }
}
