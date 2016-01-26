using System;
using System.Runtime.Serialization;

namespace Artentus.GameUtils.Network
{
    [Serializable]
    internal class NotificationMessage : SystemMessage
    {
        public NotificationType NotificationType { get; private set; }

        public NotificationMessage(NotificationType notificationType)
        {
            NotificationType = notificationType;
        }

        protected NotificationMessage(SerializationInfo info, StreamingContext context)
        {
            NotificationType = (NotificationType)info.GetInt32("NotificationType");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("NotificationType", (int)NotificationType);
        }
    }
}
