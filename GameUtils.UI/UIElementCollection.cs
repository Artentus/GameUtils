using GameUtils.Collections;

namespace GameUtils.UI
{
    public sealed class UIElementCollection : BufferedList<UIElement>
    {
        readonly ContainerElement parent;

        internal UIElementCollection(ContainerElement parent)
        {
            this.parent = parent;
        }

        protected override void OnItemRemoved(ItemEventArgs<UIElement> e)
        {
            base.OnItemRemoved(e);

            e.Item.Parent = null;
        }

        protected override void OnItemAdded(ItemEventArgs<UIElement> e)
        {
            base.OnItemAdded(e);

            e.Item.Parent = parent;
        }
    }
}
