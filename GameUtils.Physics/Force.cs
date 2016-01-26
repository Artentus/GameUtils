using GameUtils.Math;

namespace GameUtils.Physics
{
    public struct Force
    {
        public Vector2 Value { get; set; }

        public Vector2 AttackPoint { get; set; }

        public Force(Vector2 value, Vector2 attackPoint)
            : this()
        {
            Value = value;
            AttackPoint = attackPoint;
        }
    }
}
