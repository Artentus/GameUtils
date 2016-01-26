using System;
using System.Collections.Generic;
using GameUtils.Math;

namespace GameUtils.Physics
{
    public class PhysicsObject
    {
        Vector2 forces;
        float torques;
        readonly float radius;
        readonly Vector2 startCenter;
        readonly Polygon startPolygon;

        public float Mass { get; private set; }

        public float MomentOfInertia { get; private set; }

        internal float InvertedMass { get; private set; }

        internal float InvertedMomentOfInertia { get; private set; }

        public Polygon Polygon { get; private set; }

        public Vector2 Center { get; private set; }

        public Vector2 LinearVelocity { get; private set; }

        public float AngularVelocity { get; private set; }

        public float RotationAngle { get; private set; }

        public PhysicsObject(float mass, Polygon polygon, Vector2 linearVelocity = default(Vector2), float angularVelocity = 0.0f)
        {
            if (mass < 0)
                throw new ArgumentException("mass", "Die Masse muss positiv oder 0 sein.");
            if (!polygon.IsValid)
                throw new ArgumentException("polygon", "Das übergebene Polygon ist nicht gültig.");

            Polygon = polygon;
            startPolygon = polygon;
            Mass = mass;
            forces = new Vector2(0, 0);
            torques = 0;

            radius = 0;
            Center = polygon.Center;
            startCenter = Center;
            for (int i = 0; i < polygon.Points.Length; i++)
            {
                float dist = (Center - polygon.Points[i]).Length;
                if (dist > radius) radius = dist;
            }

            float denom = 0;
            float num = 0;
            for (int i = 0; i < polygon.Points.Length - 1; i++)
            {
                Vector2 a = polygon.Points[i + 1] - Center;
                Vector2 b = polygon.Points[i] - Center;
                float f = System.Math.Abs(a.X * b.Y - a.Y * b.X);
                denom += f * (Vector2.DotProduct(a, a) + Vector2.DotProduct(a, b) + Vector2.DotProduct(b, b));
                num += f;
            }
            MomentOfInertia = (mass * denom) / (6 * num);

            InvertedMass = mass == 0 ? 0 : 1.0f / mass;
            InvertedMomentOfInertia = MomentOfInertia == 0 ? 0 : 1.0f / MomentOfInertia;

            LinearVelocity = linearVelocity;
            AngularVelocity = angularVelocity;
        }

        public PhysicsObject(float mass, IEnumerable<Vector2> polygonPoints, Vector2 linearVelocity = default(Vector2), float angularVelocity = 0.0f)
            : this(mass, new Polygon(polygonPoints), linearVelocity, angularVelocity) { }

        public PhysicsObject(float mass, params Vector2[] polygonPoints)
            : this(mass, new Polygon(polygonPoints)) { }

        internal void ApplyForceInner(Vector2 value, Vector2 attackPoint)
        {
            if (Mass == 0) return;
            forces += value;
            Vector2 v1 = attackPoint - Center;
            float len = value.Length * v1.Length;
            float t = MathHelper.Atan2(value.Y, value.X) - MathHelper.Atan2(v1.Y, v1.X);
            torques += len * MathHelper.Sin(t);
        }

        public void ApplyForce(Force force)
        {
            ApplyForceInner(force.Value, force.AttackPoint);
        }

        internal void ApplyImpulse(Vector2 impulse, Vector2 attackPoint)
        {
            if (Mass == 0) return;
            LinearVelocity += InvertedMass * impulse;
            AngularVelocity += InvertedMomentOfInertia * Vector2.VectorProduct(attackPoint, impulse);
        }

        public bool IntersectsWith(PhysicsObject second, out Vector2 mtv)
        {
            mtv = default(Vector2);
            if ((this.Center - second.Center).Length < this.radius + second.radius)
                return this.Polygon.IntersectsWith(second.Polygon, out mtv);
            else
                return false;
        }

        internal void UpdateVelocity(float dt)
        {
            if (Mass == 0) return;
            AngularVelocity += torques * InvertedMomentOfInertia * dt;
            LinearVelocity += forces * InvertedMass * dt;
        }

        internal void UpdatePosition(float dt)
        {
            if (Mass != 0)
            {
                RotationAngle += AngularVelocity * dt;
                Center += LinearVelocity * dt;
                Polygon = (Matrix2x3.Translation(Center) * Matrix2x3.Rotation(RotationAngle) * Matrix2x3.Translation(-startCenter)).ApplyTo(startPolygon);
            }
        }

        internal void ClearForces()
        {
            forces = new Vector2(0, 0);
            torques = 0;
        }
    }
}
