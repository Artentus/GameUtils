using System.Collections.Generic;
using GameUtils.Math;

namespace GameUtils.Physics
{
    internal class CollisionCollection
    {
        readonly List<Collision> collisions;

        internal Collision this[int index]
        {
            get
            {
                return collisions[index];
            }
        }

        internal int Count
        {
            get
            {
                return collisions.Count;
            }
        }

        internal CollisionCollection()
        {
            collisions = new List<Collision>();
        }

        internal void Clear()
        {
            collisions.Clear();
        }

        internal void AddCollisionPair(PhysicsObject obj1, PhysicsObject obj2, VectorPair[] points, Vector2 normal, float penetration, bool isSecond)
        {
            Collision col = null;

            for (int i = 0; i < collisions.Count; i++)
            {
                if (collisions[i].Object1 == obj1 && collisions[i].Object2 == obj2)
                {
                    col = collisions[i];
                    break;
                }
            }

            if (col == null)
            {
                col = new Collision();
                col.SetCollision(obj1, obj2);
                collisions.Add(col);
            }

            for (int i = 0; i < points.Length; i++)
            {
                if (isSecond)
                    col.Points.Add(new CollisionPoint(normal, points[i].ContactB, penetration));
                else
                    col.Points.Add(new CollisionPoint(normal, points[i].ContactA, penetration));
            }
        }
    }
}
