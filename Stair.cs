using System;
using Microsoft.Xna.Framework;
using Game3.Audio;

namespace Game3
{
    public enum StairDirection
    {
        North,  // Going up as Y increases
        South,  // Going up as Y decreases
        East,   // Going up as X increases
        West    // Going up as X decreases
    }

    public class Stair
    {
        private Map map;
        private Vector3 startPosition;  // Bottom corner of stairs (lowest Z)
        private float width;            // Width perpendicular to climb direction
        private float length;           // Length along climb direction
        private float heightChange;     // Total height change (positive = go up)
        private StairDirection direction;

        public Vector3 StartPosition => startPosition;
        public float Width => width;
        public float Length => length;
        public float HeightChange => heightChange;
        public StairDirection Direction => direction;

        public Stair(Map map, Vector3 startPos, float length, float width, float heightChange, StairDirection direction)
        {
            this.map = map;
            this.startPosition = startPos;
            this.length = length;
            this.width = width;
            this.heightChange = heightChange;
            this.direction = direction;

            // Add stair geometry for raytracing (wooden material)
            AddStairPrimitives();

            Program.Log($"Stair created: {direction} at ({startPos.X:F1}, {startPos.Y:F1}, {startPos.Z:F1}), " +
                       $"length={length}, width={width}, height={heightChange}");
        }

        private void AddStairPrimitives()
        {
            // Create wooden floor for the stair
            Vector3 center = GetCenterPosition();
            Vector3 size = GetSize();

            // Stair floor (wooden)
            var floorPrimitive = new vaudio.PrismPrimitive()
            {
                material = vaudio.MaterialType.Brick, // Using brick as wood-like
                size = new vaudio.Vector3F(size.X, size.Y, 0.2f),
                transform = vaudio.Matrix4F.CreateTranslation(center.X, center.Y, startPosition.Z + heightChange / 2f)
            };
            map.AudioManager.AddPrimitive(floorPrimitive);
        }

        private Vector3 GetCenterPosition()
        {
            float halfWidth = width / 2f;
            float halfLength = length / 2f;

            switch (direction)
            {
                case StairDirection.North:
                    return new Vector3(startPosition.X, startPosition.Y + halfLength, startPosition.Z);
                case StairDirection.South:
                    return new Vector3(startPosition.X, startPosition.Y - halfLength, startPosition.Z);
                case StairDirection.East:
                    return new Vector3(startPosition.X + halfLength, startPosition.Y, startPosition.Z);
                case StairDirection.West:
                    return new Vector3(startPosition.X - halfLength, startPosition.Y, startPosition.Z);
                default:
                    return startPosition;
            }
        }

        private Vector3 GetSize()
        {
            switch (direction)
            {
                case StairDirection.North:
                case StairDirection.South:
                    return new Vector3(width, length, 0.2f);
                default:
                    return new Vector3(length, width, 0.2f);
            }
        }

        public bool IsPlayerOnStair(Vector3 playerPos)
        {
            float minX, maxX, minY, maxY;
            float halfWidth = width / 2f;

            switch (direction)
            {
                case StairDirection.North:
                    minX = startPosition.X - halfWidth;
                    maxX = startPosition.X + halfWidth;
                    minY = startPosition.Y;
                    maxY = startPosition.Y + length;
                    break;
                case StairDirection.South:
                    minX = startPosition.X - halfWidth;
                    maxX = startPosition.X + halfWidth;
                    minY = startPosition.Y - length;
                    maxY = startPosition.Y;
                    break;
                case StairDirection.East:
                    minX = startPosition.X;
                    maxX = startPosition.X + length;
                    minY = startPosition.Y - halfWidth;
                    maxY = startPosition.Y + halfWidth;
                    break;
                case StairDirection.West:
                    minX = startPosition.X - length;
                    maxX = startPosition.X;
                    minY = startPosition.Y - halfWidth;
                    maxY = startPosition.Y + halfWidth;
                    break;
                default:
                    return false;
            }

            return playerPos.X >= minX && playerPos.X <= maxX &&
                   playerPos.Y >= minY && playerPos.Y <= maxY;
        }

        public float GetHeightAtPosition(Vector3 playerPos)
        {
            if (!IsPlayerOnStair(playerPos))
                return -1f; // Not on stair

            // Calculate progress along the stair (0 to 1)
            float progress = 0f;

            switch (direction)
            {
                case StairDirection.North:
                    progress = (playerPos.Y - startPosition.Y) / length;
                    break;
                case StairDirection.South:
                    progress = (startPosition.Y - playerPos.Y) / length;
                    break;
                case StairDirection.East:
                    progress = (playerPos.X - startPosition.X) / length;
                    break;
                case StairDirection.West:
                    progress = (startPosition.X - playerPos.X) / length;
                    break;
            }

            progress = Math.Clamp(progress, 0f, 1f);
            return startPosition.Z + progress * heightChange;
        }
    }
}
