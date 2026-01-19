namespace Game3.ECS.Components
{
    /// <summary>
    /// Velocity in 3D space (units per second).
    /// </summary>
    public struct Velocity
    {
        public float X;
        public float Y;
        public float Z;

        public Velocity(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Velocity Zero => new Velocity(0, 0, 0);
    }

    /// <summary>
    /// Gravity component for entities affected by gravity.
    /// </summary>
    public struct Gravity
    {
        /// <summary>
        /// Current vertical velocity (positive = up).
        /// </summary>
        public float VerticalVelocity;

        /// <summary>
        /// Whether the entity is on the ground.
        /// </summary>
        public bool IsGrounded;

        /// <summary>
        /// Current floor height the entity is standing on.
        /// </summary>
        public float CurrentFloorHeight;

        public Gravity(bool isGrounded)
        {
            VerticalVelocity = 0f;
            IsGrounded = isGrounded;
            CurrentFloorHeight = 0f;
        }
    }

    /// <summary>
    /// Collision radius and height for cylindrical collision shape.
    /// </summary>
    public struct CollisionShape
    {
        /// <summary>
        /// Horizontal radius for collision detection.
        /// </summary>
        public float Radius;

        /// <summary>
        /// Height of the entity (for 3D collision).
        /// </summary>
        public float Height;

        public CollisionShape(float radius, float height)
        {
            Radius = radius;
            Height = height;
        }
    }
}
