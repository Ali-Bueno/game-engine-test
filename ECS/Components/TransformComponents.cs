namespace Game3.ECS.Components
{
    /// <summary>
    /// Position in 3D space.
    /// X = East/West, Y = North/South, Z = Up/Down
    /// </summary>
    public struct Position
    {
        public float X;
        public float Y;
        public float Z;

        public Position(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Microsoft.Xna.Framework.Vector3 ToVector3() =>
            new Microsoft.Xna.Framework.Vector3(X, Y, Z);

        public static Position FromVector3(Microsoft.Xna.Framework.Vector3 v) =>
            new Position(v.X, v.Y, v.Z);
    }

    /// <summary>
    /// Rotation (yaw only for now).
    /// Yaw in degrees: 0 = North (+Y), 90 = East (+X)
    /// </summary>
    public struct Rotation
    {
        public float Yaw;

        public Rotation(float yaw)
        {
            Yaw = yaw;
        }

        /// <summary>
        /// Gets yaw in radians.
        /// </summary>
        public float YawRadians => Yaw * System.MathF.PI / 180f;
    }

    /// <summary>
    /// Scale of an entity (for rendering, collision volumes, etc.)
    /// </summary>
    public struct Scale
    {
        public float X;
        public float Y;
        public float Z;

        public Scale(float uniform)
        {
            X = Y = Z = uniform;
        }

        public Scale(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
