using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Game3.Audio;

namespace Game3
{
    public class Player
    {
        private Map map;
        private Random random = new Random();

        public Vector3 Position;
        public float Angle; // Degrees, 0 = North (+Y), 90 = East (+X)

        private const float MoveSpeed = 5f;
        private const float RotateSpeed = 120f;
        private const float PlayerRadius = 0.3f;

        // Footstep sounds
        private const int FootstepCount = 13;
        private const int WoodFootstepCount = 4;
        private const float FootstepInterval = 0.4f;
        private float footstepTimer = 0f;
        private bool isMoving = false;
        private bool isOnStair = false;

        // Collision sound
        private float collisionCooldown = 0f;
        private const float CollisionCooldownTime = 0.3f;

        // Floor height tracking
        private float currentFloorHeight = 0f;
        private float targetFloorHeight = 0f;
        private const float FallSpeed = 10f;  // Speed of falling/descending

        // Door interaction
        private KeyboardState previousKeyState;

        public Player(Map map)
        {
            this.map = map;
            Position = Vector3.Zero;
            Angle = 0;
        }

        public void Update(KeyboardState keystate, GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (collisionCooldown > 0)
                collisionCooldown -= deltaTime;

            // Rotation with arrow keys
            if (keystate.IsKeyDown(Keys.Left))
            {
                Angle -= RotateSpeed * deltaTime;
                if (Angle < 0) Angle += 360;
            }
            if (keystate.IsKeyDown(Keys.Right))
            {
                Angle += RotateSpeed * deltaTime;
                if (Angle >= 360) Angle -= 360;
            }

            // Calculate movement direction based on angle
            float angleRad = Angle * MathF.PI / 180f;
            Vector3 forward = new Vector3(MathF.Sin(angleRad), MathF.Cos(angleRad), 0);
            Vector3 right = new Vector3(MathF.Cos(angleRad), -MathF.Sin(angleRad), 0);

            // Movement with WASD
            Vector3 moveDir = Vector3.Zero;

            if (keystate.IsKeyDown(Keys.W))
                moveDir += forward;
            if (keystate.IsKeyDown(Keys.S))
                moveDir -= forward;
            if (keystate.IsKeyDown(Keys.A))
                moveDir -= right;
            if (keystate.IsKeyDown(Keys.D))
                moveDir += right;

            isMoving = moveDir.LengthSquared() > 0;
            if (isMoving)
            {
                moveDir.Normalize();
                Vector3 newPosition = Position + moveDir * MoveSpeed * deltaTime;

                // Apply collision detection (XY only)
                Vector3 resolvedPosition = ApplyCollisions(newPosition);

                // Check if we hit a wall
                float collisionDistance = (resolvedPosition - newPosition).Length();
                if (collisionDistance > 0.01f && collisionCooldown <= 0)
                {
                    PlayCollisionSound();
                    collisionCooldown = CollisionCooldownTime;
                }

                // Check for stairs and update Z position
                isOnStair = false;
                float stairHeight = GetStairHeight(resolvedPosition);
                if (stairHeight >= 0)
                {
                    isOnStair = true;
                    targetFloorHeight = stairHeight;
                    currentFloorHeight = stairHeight;  // Instant on stairs
                }
                else
                {
                    // Check if we're on an upper floor platform
                    float platformHeight = GetPlatformHeight(resolvedPosition);
                    if (platformHeight >= 0 && currentFloorHeight >= platformHeight - 0.5f)
                    {
                        // Stay on platform
                        targetFloorHeight = platformHeight;
                        currentFloorHeight = platformHeight;
                    }
                    else
                    {
                        // Fall towards ground
                        targetFloorHeight = 0f;
                    }
                }

                // Smoothly adjust height when falling
                if (currentFloorHeight > targetFloorHeight)
                {
                    currentFloorHeight -= FallSpeed * deltaTime;
                    if (currentFloorHeight < targetFloorHeight)
                        currentFloorHeight = targetFloorHeight;
                }
                else if (currentFloorHeight < targetFloorHeight)
                {
                    currentFloorHeight = targetFloorHeight;  // Instant rise (stairs)
                }

                resolvedPosition.Z = currentFloorHeight;
                Position = resolvedPosition;
            }

            // Door interaction with E key
            if (keystate.IsKeyDown(Keys.E) && !previousKeyState.IsKeyDown(Keys.E))
            {
                TryInteractWithDoor();
            }
            previousKeyState = keystate;

            UpdateFootsteps(deltaTime);
        }

        private float GetStairHeight(Vector3 pos)
        {
            if (map.Stairs == null) return -1f;

            foreach (var stair in map.Stairs)
            {
                float height = stair.GetHeightAtPosition(pos);
                if (height >= 0)
                {
                    return height;
                }
            }
            return -1f;
        }

        private float GetPlatformHeight(Vector3 pos)
        {
            // Check if player is on any upper floor platform
            foreach (var platform in map.Platforms)
            {
                if (pos.X >= platform.Min.X && pos.X <= platform.Max.X &&
                    pos.Y >= platform.Min.Y && pos.Y <= platform.Max.Y)
                {
                    return platform.Height;
                }
            }
            return -1f;
        }

        private void TryInteractWithDoor()
        {
            if (map.Doors == null) return;

            foreach (var door in map.Doors)
            {
                if (door.IsPlayerNear(Position))
                {
                    door.Toggle();
                    break;
                }
            }
        }

        private void UpdateFootsteps(float deltaTime)
        {
            if (isMoving)
            {
                footstepTimer += deltaTime;
                if (footstepTimer >= FootstepInterval)
                {
                    footstepTimer = 0f;
                    PlayFootstep();
                }
            }
            else
            {
                footstepTimer = FootstepInterval;
            }
        }

        private void PlayFootstep()
        {
            // Play footsteps at player's feet position (Z = floor level)
            // This allows reverb/raytracing to affect the sound
            float footX = Position.X;
            float footY = Position.Y;
            float footZ = Position.Z + 0.1f;  // Slightly above floor

            if (isOnStair)
            {
                // Wood footsteps on stairs
                int stepNumber = random.Next(1, WoodFootstepCount + 1);
                string soundPath = $"sounds/steps/wood/{stepNumber}.wav";
                map.AudioManager.Play3D(soundPath, footX, footY, footZ, false, 0.6f);
            }
            else
            {
                // Normal footsteps
                int stepNumber = random.Next(1, FootstepCount + 1);
                string soundPath = $"sounds/steps/womanstep/{stepNumber}.ogg";
                map.AudioManager.Play3D(soundPath, footX, footY, footZ, false, 0.5f);
            }
        }

        private void PlayCollisionSound()
        {
            // Play collision sound at player position with reverb
            map.AudioManager.Play3D("sounds/walls/wall.mp3", Position.X, Position.Y, Position.Z + 1.0f, false, 0.7f);
        }

        private Vector3 ApplyCollisions(Vector3 newPosition)
        {
            foreach (var collider in map.Colliders)
            {
                if (collider.Intersects(newPosition, PlayerRadius))
                {
                    newPosition = collider.ResolveCollision(newPosition, PlayerRadius);
                }
            }

            if (newPosition.Z < 0)
                newPosition.Z = 0;

            return newPosition;
        }
    }
}
