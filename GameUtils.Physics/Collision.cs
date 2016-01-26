using System.Collections.Generic;

namespace GameUtils.Physics
{
    internal class Collision
    {
        readonly List<CollisionPoint> points;

        internal List<CollisionPoint> Points
        {
            get
            {
                return points;
            }
        }

        internal Collision()
        {
            points = new List<CollisionPoint>();
        }

        internal PhysicsObject Object1 { get; private set; }
        internal PhysicsObject Object2 { get; private set; }

        internal void SetCollision(PhysicsObject obj1, PhysicsObject obj2)
        {
            points.Clear();
            Object1 = obj1;
            Object2 = obj2;
        }
    }
}
