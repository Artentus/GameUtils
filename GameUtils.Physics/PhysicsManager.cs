using System;
using System.Collections.Generic;
using GameUtils.Math;

namespace GameUtils.Physics
{
    public sealed class PhysicsManager : IEngineComponent, IUpdateable
    {
        List<PhysicsObject> objects;
        CollisionCollection collisions;
        double overlapTime;
        double lastUpdateTime;

        bool IGameComponent.IsSynchronized
        {
            get { return false; }
        }

        bool IEngineComponent.IsCompatibleTo(IEngineComponent component)
        {
            return true;
        }

        public object Tag { get; set; }

        public PhysicsManager()
        {
            objects = new List<PhysicsObject>();
            GameEngine.QueryComponent<GameLoop>().Components.Add(this);
        }

        public bool GravityEnabled { get; set; }

        public List<PhysicsObject> Objects
        {
            get
            {
                return objects;
            }
        }

        private Vector2 Edge_GetClosestPoint(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            Vector2 ap = p - a;

            float ab_ab = Vector2.DotProduct(ab, ab);
            float ab_ap = Vector2.DotProduct(ab, ap);

            float t = MathHelper.Clamp(ab_ap / ab_ab, 0.0f, 1.0f);

            return a + t * ab;
        }

        private VectorPair[] GetContactPoints_EdgeEdge(Vector2[] edgeA, Vector2[] edgeB)
        {
            Vector2 dir = edgeA[1] - edgeA[0];
            Vsort[] v = new[] { new Vsort(edgeA[0], dir, 0), new Vsort(edgeA[1], dir, 0), new Vsort(edgeB[0], dir, 1), new Vsort(edgeB[1], dir, 1) };
            VectorPair[] contacts = new VectorPair[2];

            Array.Sort(v);

            for (int i = 0; i < 2; i++)
            {
                if(v[i + 1].bd == 0)
                {
                    contacts[i].ContactA = v[i + 1].p;
                    contacts[i].ContactB = Edge_GetClosestPoint(v[i + 1].p, edgeB[0], edgeB[1]);
                }
                else 
                {
                    contacts[i].ContactA = Edge_GetClosestPoint(v[i + 1].p, edgeA[0], edgeA[1]);
                    contacts[i].ContactB = v[i + 1].p;
                }
            }

            return contacts;
        }

        private VectorPair[] GetContactPoints_VertexVertex(Vector2 pa, Vector2 pb)
        {
            VectorPair[] contacts = new VectorPair[1];
            contacts[0].ContactA = pa;
            contacts[0].ContactB = pb;
            return contacts;
        }

        private VectorPair[] GetContactPoints_VertexEdge(Vector2 pa, Vector2[] edgeB)
        {
            VectorPair[] contacts = new VectorPair[1];
            contacts[0].ContactA = pa;
            contacts[0].ContactB = Edge_GetClosestPoint(pa, edgeB[0], edgeB[1]);
            return contacts;
        }

        private VectorPair[] GetContactPoints_EdgeVertex(Vector2[] edgeA, Vector2 PB)
        {
            VectorPair[] contacts = new VectorPair[1];
            contacts[0].ContactA = Edge_GetClosestPoint(PB, edgeA[0], edgeA[1]);
            contacts[0].ContactB = PB;
            return contacts;
        }

        private VectorPair[] GetContactPoints(Vector2[] supportA, Vector2[] supportB)
        {
            switch(supportA.Length + supportB.Length)
            {
                case 2:
                    return GetContactPoints_VertexVertex(supportA[0], supportB[0]);
                case 3:
                    if(supportA.Length > supportB.Length)
                        return GetContactPoints_EdgeVertex(supportA, supportB[0]);
                    else
                        return GetContactPoints_VertexEdge(supportA[0], supportB);
                case 4:
                    return GetContactPoints_EdgeEdge(supportA, supportB);
                default:
                    return new VectorPair[0];
            }
        }

        private struct Vsort : IComparable<Vsort>
        {
            public Vector2 p;
            public float d;
            public int bd;

            public Vsort(Vector2 pos, Vector2 dir, int body)
            {
                p = pos;
                d = Vector2.DotProduct(pos, dir);
                bd = body;
            }

            public int CompareTo(Vsort other)
            {
 	            return (this.d > other.d) ? 1 : -1;
            }
        }

        private Vector2[] GetSupportPoints(Polygon polygon, Vector2 dir)
        {
            int count = 0;
            const float threshold = 1.0E-8f;
            double mind = 0;
            List<Vector2> support = new List<Vector2>(2);

            for (int i = 0; i < polygon.Points.Length; i++)
            {
                float d = Vector2.DotProduct(polygon.Points[i], dir);

                if (i == 0 || d < mind)
                    mind = d;
            }

            for (int i = 0; i < polygon.Points.Length; i++)
            {
                float d = Vector2.DotProduct(polygon.Points[i], dir);

                if (d < (mind + threshold))
                {
                    support.Add(polygon.Points[i]);
                    count++;
                    if (count >= 2) return support.ToArray();
                }
            }

            return support.ToArray();
        }

        private CollisionCollection DetectCollisions()
        {
            CollisionCollection collisions = new CollisionCollection();

            for (int i = 0; i < objects.Count; i++)
            {
                for (int j = i + 1; j < objects.Count; j++)
                {
                    PhysicsObject obj1 = objects[i];
                    PhysicsObject obj2 = objects[j];

                    Vector2 mtv;
                    if (obj1.IntersectsWith(obj2, out mtv))
                    {
                        Vector2 normal = mtv.Normalize();
                        float penetration = mtv.Length;

                        Vector2[] support1 = GetSupportPoints(obj1.Polygon, -normal);
                        Vector2[] support2 = GetSupportPoints(obj2.Polygon, normal);

                        VectorPair[] pairs = GetContactPoints(support1, support2);

                        collisions.AddCollisionPair(obj1, obj2, pairs, -normal, penetration, false);
                        collisions.AddCollisionPair(obj2, obj1, pairs, normal, penetration, true);
                    }
                }
            }

            return collisions;
        }

        private void ApplyGravity()
        {
            for (int i = 0; i < objects.Count; i++)
                objects[i].ApplyForceInner(new Vector2(0, 9.81f * objects[i].Mass), objects[i].Center);
        }

        public void Update(TimeSpan elapsed)
        {
            float now = (float)DateTime.Now.TimeOfDay.TotalMilliseconds / 1000;
            if (lastUpdateTime != 0)
                Update(now - lastUpdateTime + overlapTime);
            lastUpdateTime = now;
        }

        private void Update(double dt)
        {
            const float singleStep = 0.001f;

            int steps = (int)(dt / singleStep);
            overlapTime = dt - steps * singleStep;

            for (int i = 0; i < steps; i++)
                Step(singleStep);

            for (int i = 0; i < objects.Count; i++)
                objects[i].ClearForces();
        }

        private void Step(float dt)
        {
            collisions = DetectCollisions();

            if (GravityEnabled) ApplyGravity();

            for (int i = 0; i < objects.Count; i++)
                objects[i].UpdateVelocity(dt);

            //for (int i = 0; i < 7; i++)
                ApplyImpulses(dt);

            for (int i = 0; i < objects.Count; i++)
                objects[i].UpdatePosition(dt);
        }

        private void ApplyImpulses(float dt)
        {
            for (int i = 0; i < collisions.Count; i++)
            {
                Collision col = collisions[i];

                PhysicsObject obj1 = col.Object1;
                PhysicsObject obj2 = col.Object2;

                for (int j = 0; j < col.Points.Count; j++)
                {
                    Vector2 point = col.Points[j].Point;
                    Vector2 normal = col.Points[j].Normal;
                    float penetration = col.Points[j].Penetration;

                    AddImpulseContactPoint(obj1, obj2, point, normal, penetration, col.Points.Count, dt);
                }
            }
        }

        private void AddImpulseContactPoint(PhysicsObject obj1, PhysicsObject obj2, Vector2 contactPoint, Vector2 normal, float penetration, int contactPoints, float dt)
        {
            Vector2 r0 = contactPoint - obj1.Center;
            Vector2 r1 = contactPoint - obj2.Center;

            Vector2 relativeVel = (obj1.LinearVelocity + obj1.AngularVelocity * r0.CrossProduct()) - (obj2.LinearVelocity + obj2.AngularVelocity * r1.CrossProduct());

            float contactVel = Vector2.DotProduct(relativeVel, normal);
            if (contactVel > 0) return;

            float tmp1 = Vector2.VectorProduct(r0, normal);
            tmp1 *= tmp1;
            float tmp2 = Vector2.VectorProduct(r1, normal);
            tmp2 *= tmp2;

            float invMass = obj1.InvertedMass + obj2.InvertedMass + obj1.InvertedMomentOfInertia * tmp1 + obj2.InvertedMomentOfInertia * tmp2;

            const float allowedPenetration = 0.1f;
            const float biasFactor = 0.1f;

            float e = (dt == 0 ? 0 : 1 / dt) * System.Math.Max(0, penetration - allowedPenetration) * biasFactor;
            float j = System.Math.Max(0, (-contactVel + e) / (invMass * contactPoints));

            Vector2 impulse = j * normal;
            obj1.ApplyImpulse(impulse, r0);
        }

        private bool disposed;

        public void Dispose()
        {
            if (!disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);

                disposed = true;
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                objects.Clear();
                objects = null;
                if (collisions != null) collisions.Clear();
                collisions = null;

                GameEngine.QueryComponent<GameLoop>().Components.Remove(this);
            }
        }

        ~PhysicsManager()
        {
            Dispose(false);
        }
    }
}
