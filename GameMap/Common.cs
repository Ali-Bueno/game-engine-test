using System;
using Microsoft.Xna.Framework;

namespace Game3.GameMap
{
    /// <summary>
    /// Lado de una pared en una habitación
    /// </summary>
    public enum WallSide
    {
        North,  // +Y
        South,  // -Y
        East,   // +X
        West    // -X
    }

    /// <summary>
    /// Dirección de una escalera
    /// </summary>
    public enum StairDirection
    {
        North,  // Sube hacia +Y
        South,  // Sube hacia -Y
        East,   // Sube hacia +X
        West    // Sube hacia -X
    }

    /// <summary>
    /// Colisionador de caja para detección de colisiones
    /// </summary>
    public class BoxCollider
    {
        public Vector3 Position;  // Centro
        public Vector3 Size;      // Dimensiones totales

        public Vector3 Min => Position - Size / 2;
        public Vector3 Max => Position + Size / 2;

        public BoxCollider(Vector3 position, Vector3 size)
        {
            Position = position;
            Size = size;
        }

        /// <summary>
        /// Verifica si un punto con radio está dentro del collider (2D en XY, ignora Z)
        /// </summary>
        public bool Intersects(Vector3 point, float radius)
        {
            return point.X + radius > Min.X && point.X - radius < Max.X &&
                   point.Y + radius > Min.Y && point.Y - radius < Max.Y;
        }

        /// <summary>
        /// Verifica si un punto con radio está dentro del collider (3D completo)
        /// </summary>
        public bool Intersects3D(Vector3 point, float radius, float playerHeight = 1.8f)
        {
            // Verificar XY
            if (!(point.X + radius > Min.X && point.X - radius < Max.X &&
                  point.Y + radius > Min.Y && point.Y - radius < Max.Y))
                return false;

            // Verificar Z (el jugador ocupa desde point.Z hasta point.Z + playerHeight)
            float playerBottom = point.Z;
            float playerTop = point.Z + playerHeight;

            return playerTop > Min.Z && playerBottom < Max.Z;
        }

        /// <summary>
        /// Resuelve la colisión empujando el punto fuera del collider
        /// </summary>
        public Vector3 ResolveCollision(Vector3 point, float radius)
        {
            // Calcular la penetración en cada eje
            float overlapLeft = (point.X + radius) - Min.X;
            float overlapRight = Max.X - (point.X - radius);
            float overlapBottom = (point.Y + radius) - Min.Y;
            float overlapTop = Max.Y - (point.Y - radius);

            // Encontrar el menor overlap para resolver
            float minOverlapX = Math.Min(overlapLeft, overlapRight);
            float minOverlapY = Math.Min(overlapBottom, overlapTop);

            if (minOverlapX < minOverlapY)
            {
                // Resolver en X
                if (overlapLeft < overlapRight)
                    point.X = Min.X - radius;
                else
                    point.X = Max.X + radius;
            }
            else
            {
                // Resolver en Y
                if (overlapBottom < overlapTop)
                    point.Y = Min.Y - radius;
                else
                    point.Y = Max.Y + radius;
            }

            return point;
        }
    }

    /// <summary>
    /// Plataforma elevada (área donde el jugador puede caminar a cierta altura)
    /// </summary>
    public class Platform
    {
        public float MinX, MinY, MaxX, MaxY;
        public float Height;

        public Vector2 Min => new Vector2(MinX, MinY);
        public Vector2 Max => new Vector2(MaxX, MaxY);

        public Platform(float minX, float minY, float maxX, float maxY, float height)
        {
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
            Height = height;
        }

        /// <summary>
        /// Verifica si un punto está dentro de la plataforma
        /// </summary>
        public bool Contains(Vector3 point)
        {
            return point.X >= MinX && point.X <= MaxX &&
                   point.Y >= MinY && point.Y <= MaxY;
        }
    }
}
