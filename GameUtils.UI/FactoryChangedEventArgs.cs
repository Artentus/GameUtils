using System;
using GameUtils.Graphics.Data;

namespace GameUtils.UI
{
    public class FactoryChangedEventArgs : EventArgs
    {
        public Factory Factory { get; private set; }

        public FactoryChangedEventArgs(Factory factory)
        {
            Factory = factory;
        }
    }
}
