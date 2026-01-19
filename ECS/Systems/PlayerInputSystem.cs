using System;
using Arch.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Game3.ECS.Components;

namespace Game3.ECS.Systems
{
    /// <summary>
    /// Handles player input: rotation and movement direction.
    /// Updates Velocity based on WASD input and Rotation based on arrow keys.
    /// </summary>
    public class PlayerInputSystem
    {
        private readonly World world;
        private readonly SharedResources resources;
        private readonly QueryDescription playerQuery;
        private KeyboardState previousKeyState;

        public PlayerInputSystem(World world, SharedResources resources)
        {
            this.world = world;
            this.resources = resources;
            this.playerQuery = new QueryDescription()
                .WithAll<PlayerControlled, Position, Rotation, Velocity, PlayerMovement, Gravity, CollisionShape>();
        }

        public void Update(GameTime gameTime)
        {
            var keystate = Keyboard.GetState();
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            world.Query(in playerQuery, (ref Position pos, ref Rotation rot, ref Velocity vel,
                ref PlayerMovement movement, ref Gravity gravity, ref CollisionShape shape) =>
            {
                // Rotation with arrow keys
                if (keystate.IsKeyDown(Keys.Left))
                {
                    rot.Yaw -= movement.RotateSpeed * deltaTime;
                    if (rot.Yaw < 0) rot.Yaw += 360;
                }
                if (keystate.IsKeyDown(Keys.Right))
                {
                    rot.Yaw += movement.RotateSpeed * deltaTime;
                    if (rot.Yaw >= 360) rot.Yaw -= 360;
                }

                // Calculate direction based on angle
                float angleRad = rot.YawRadians;
                Vector3 forward = new Vector3(MathF.Sin(angleRad), MathF.Cos(angleRad), 0);
                Vector3 right = new Vector3(MathF.Cos(angleRad), -MathF.Sin(angleRad), 0);

                // Movement with WASD
                Vector3 moveDir = Vector3.Zero;
                if (keystate.IsKeyDown(Keys.W)) moveDir += forward;
                if (keystate.IsKeyDown(Keys.S)) moveDir -= forward;
                if (keystate.IsKeyDown(Keys.A)) moveDir -= right;
                if (keystate.IsKeyDown(Keys.D)) moveDir += right;

                movement.IsMoving = moveDir.LengthSquared() > 0;
                if (movement.IsMoving)
                {
                    moveDir.Normalize();
                    vel.X = moveDir.X * movement.MoveSpeed;
                    vel.Y = moveDir.Y * movement.MoveSpeed;
                }
                else
                {
                    vel.X = 0;
                    vel.Y = 0;
                }

                // Jump (Space key)
                if (keystate.IsKeyDown(Keys.Space) && !previousKeyState.IsKeyDown(Keys.Space) && gravity.IsGrounded)
                {
                    gravity.VerticalVelocity = movement.JumpForce;
                    gravity.IsGrounded = false;

                    // Play jump sound
                    resources.AudioManager.Play3D("sounds/player/jump.wav", pos.X, pos.Y, pos.Z + 0.5f, false, 0.7f);
                }

                // Speak coordinates (C key)
                if (keystate.IsKeyDown(Keys.C) && !previousKeyState.IsKeyDown(Keys.C))
                {
                    SpeakCoordinates(pos);
                }

                // Update collision cooldown
                if (movement.CollisionCooldown > 0)
                    movement.CollisionCooldown -= deltaTime;
            });

            previousKeyState = keystate;
        }

        private void SpeakCoordinates(Position pos)
        {
            var room = resources.GetRoomAt(new Vector3(pos.X, pos.Y, pos.Z));
            string roomName = room != null ? room.Name : "Exterior";
            string msg = $"X {pos.X:F0}, Y {pos.Y:F0}, Z {pos.Z:F0}. {roomName}";
            Program.Log($"Player coordinates: {msg}");

            try
            {
                DavyKager.Tolk.Speak(msg, true);
            }
            catch
            {
                // Tolk not available
            }
        }
    }
}
