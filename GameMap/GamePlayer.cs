using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Game3.Audio;

namespace Game3.GameMap
{
    /// <summary>
    /// Jugador para el sistema GameMap.
    /// Maneja movimiento, colisiones, pisadas e interacción con puertas.
    /// </summary>
    public class GamePlayer
    {
        private GameMap map;
        private Random random = new Random();

        public Vector3 Position;
        public float Angle; // Grados, 0 = Norte (+Y), 90 = Este (+X)

        private const float MoveSpeed = 5f;
        private const float RotateSpeed = 120f;
        private const float PlayerRadius = 0.3f;

        // Pisadas
        private const int FootstepCount = 13;
        private const int WoodFootstepCount = 4;
        private const float FootstepInterval = 0.4f;
        private float footstepTimer = 0f;
        private bool isMoving = false;
        private bool isOnStair = false;

        // Sonido de colisión
        private float collisionCooldown = 0f;
        private const float CollisionCooldownTime = 0.3f;

        // Altura del suelo
        private float currentFloorHeight = 0f;

        // Salto y gravedad
        private float verticalVelocity = 0f;
        private bool isGrounded = true;
        private const float JumpForce = 6f;
        private const float Gravity = 15f;

        // Interacción
        private KeyboardState previousKeyState;

        public GamePlayer(GameMap map)
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

            // Rotación con flechas
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

            // Calcular dirección basada en ángulo
            float angleRad = Angle * MathF.PI / 180f;
            Vector3 forward = new Vector3(MathF.Sin(angleRad), MathF.Cos(angleRad), 0);
            Vector3 right = new Vector3(MathF.Cos(angleRad), -MathF.Sin(angleRad), 0);

            // Movimiento con WASD
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

                // Aplicar colisiones (solo en XY)
                Vector3 resolvedPosition = ApplyCollisions(newPosition);

                // Verificar si chocamos
                float collisionDistance = (resolvedPosition - newPosition).Length();
                if (collisionDistance > 0.01f && collisionCooldown <= 0)
                {
                    PlayCollisionSound();
                    collisionCooldown = CollisionCooldownTime;
                }

                // Verificar escaleras para el flag isOnStair (para sonido de pisadas)
                isOnStair = GetStairHeight(resolvedPosition) >= 0;

                // Mantener la Z actual (la gravedad/salto se maneja en UpdateVerticalMovement)
                resolvedPosition.Z = Position.Z;
                Position = resolvedPosition;
            }

            // Interacción con puertas (tecla E)
            if (keystate.IsKeyDown(Keys.E) && !previousKeyState.IsKeyDown(Keys.E))
            {
                TryInteractWithDoor();
            }

            // Decir coordenadas (tecla C)
            if (keystate.IsKeyDown(Keys.C) && !previousKeyState.IsKeyDown(Keys.C))
            {
                SpeakCoordinates();
            }

            // Salto (tecla Espacio)
            if (keystate.IsKeyDown(Keys.Space) && !previousKeyState.IsKeyDown(Keys.Space) && isGrounded)
            {
                Jump();
            }

            // Aplicar gravedad y actualizar posición vertical
            UpdateVerticalMovement(deltaTime);

            previousKeyState = keystate;

            UpdateFootsteps(deltaTime);
        }

        private int stairLogCounter = 0;

        private float GetStairHeight(Vector3 pos)
        {
            foreach (var stair in map.Stairs)
            {
                float height = stair.GetHeightAtPosition(pos);
                if (height >= 0)
                {
                    // Log periódico cuando está en la escalera
                    stairLogCounter++;
                    if (stairLogCounter >= 60)
                    {
                        stairLogCounter = 0;
                        Program.Log($"OnStair: height={height:F2}, playerZ will be {height:F2}");
                    }
                    return height;
                }
            }
            return -1f;
        }

        private float GetPlatformHeight(Vector3 pos)
        {
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
            var nearestDoor = map.GetNearestDoor(Position, 2f);
            if (nearestDoor != null)
            {
                nearestDoor.Toggle();
            }
        }

        private void SpeakCoordinates()
        {
            var room = map.GetRoomAt(Position);
            string roomName = room != null ? room.Name : "Exterior";
            string msg = $"X {Position.X:F0}, Y {Position.Y:F0}, Z {Position.Z:F0}. {roomName}";
            Program.Log($"Player coordinates: {msg}");

            // Si Tolk está disponible, hablar
            try
            {
                DavyKager.Tolk.Speak(msg, true);
            }
            catch
            {
                // Tolk no disponible
            }
        }

        private void UpdateFootsteps(float deltaTime)
        {
            // Solo reproducir pisadas si está en el suelo y moviéndose
            if (isMoving && isGrounded)
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
            float footX = Position.X;
            float footY = Position.Y;
            float footZ = Position.Z + 0.1f;

            if (isOnStair)
            {
                int stepNumber = random.Next(1, WoodFootstepCount + 1);
                string soundPath = $"sounds/steps/wood/{stepNumber}.wav";
                map.AudioManager.Play3D(soundPath, footX, footY, footZ, false, 0.6f);
            }
            else
            {
                int stepNumber = random.Next(1, FootstepCount + 1);
                string soundPath = $"sounds/steps/womanstep/{stepNumber}.ogg";
                map.AudioManager.Play3D(soundPath, footX, footY, footZ, false, 0.5f);
            }
        }

        private void PlayCollisionSound()
        {
            map.AudioManager.Play3D("sounds/walls/wall.mp3", Position.X, Position.Y, Position.Z + 1.0f, false, 0.7f);
        }

        private void Jump()
        {
            verticalVelocity = JumpForce;
            isGrounded = false;
            PlayJumpSound();
        }

        private void PlayJumpSound()
        {
            map.AudioManager.Play3D("sounds/player/jump.wav", Position.X, Position.Y, Position.Z + 0.5f, false, 0.7f);
        }

        private void UpdateVerticalMovement(float deltaTime)
        {
            // Calcular altura del suelo en la posición actual
            float floorHeight = 0f;

            // Verificar escaleras
            float stairHeight = GetStairHeight(Position);
            if (stairHeight >= 0)
            {
                floorHeight = stairHeight;
            }
            else
            {
                // Verificar plataformas
                float platformHeight = GetPlatformHeight(Position);
                if (platformHeight >= 0)
                {
                    floorHeight = platformHeight;
                }
                else
                {
                    floorHeight = map.GetFloorHeight(Position);
                }
            }

            if (!isGrounded)
            {
                // Aplicar gravedad
                verticalVelocity -= Gravity * deltaTime;
                Position.Z += verticalVelocity * deltaTime;

                // Verificar si tocamos el suelo
                if (Position.Z <= floorHeight)
                {
                    Position.Z = floorHeight;
                    verticalVelocity = 0f;
                    isGrounded = true;
                    currentFloorHeight = floorHeight;
                }
            }
            else
            {
                // En el suelo, seguir la altura del terreno
                currentFloorHeight = floorHeight;
                Position.Z = currentFloorHeight;
            }
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
