using GameUtils.Math;

namespace GameUtils.Physics
{
    internal class CollisionPoint
    {
        internal Vector2 Normal { get; private set; }
        internal Vector2 Point { get; private set; }
        internal float Penetration { get; private set; }

        internal CollisionPoint(Vector2 normal, Vector2 point, float penetration)
        {
            Normal = normal;
            Point = point;
            Penetration = penetration;
        }
    }
}
