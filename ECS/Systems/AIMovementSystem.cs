using System;
using Arch.Core;
using Arch.Core.Extensions;
using Microsoft.Xna.Framework;
using Game3.ECS.Components;

namespace Game3.ECS.Systems
{
    /// <summary>
    /// Handles AI movement based on current state.
    /// </summary>
    public class AIMovementSystem
    {
        private readonly World world;
        private readonly SharedResources resources;
        private readonly QueryDescription aiQuery;
        private readonly QueryDescription patrolQuery;

        public AIMovementSystem(World world, SharedResources resources)
        {
            this.world = world;
            this.resources = resources;
            this.aiQuery = new QueryDescription()
                .WithAll<Position, Rotation, Velocity, AIAgent, AIMovement>()
                .WithNone<Disabled>();
            this.patrolQuery = new QueryDescription()
                .WithAll<Position, AIAgent, PatrolRoute>()
                .WithNone<Disabled>();
        }

        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update patrol routes first
            UpdatePatrols(deltaTime);

            // Update AI movement
            world.Query(in aiQuery, (Entity entity, ref Position pos, ref Rotation rot, ref Velocity vel, ref AIAgent ai, ref AIMovement movement) =>
            {
                Vector3 targetPosition = pos.ToVector3(); // Default to current position
                bool hasDestination = false;

                // Determine target based on state
                switch (ai.State)
                {
                    case AIState.Idle:
                        // No movement when idle
                        vel.X = 0;
                        vel.Y = 0;
                        movement.IsRunning = false;
                        return;

                    case AIState.Patrolling:
                        // Movement handled by patrol system
                        if (entity.Has<PatrolRoute>())
                        {
                            var patrol = entity.Get<PatrolRoute>();
                            if (patrol.CurrentTarget.HasValue)
                            {
                                targetPosition = patrol.CurrentTarget.Value;
                                hasDestination = true;
                                movement.IsRunning = false;
                            }
                        }
                        else
                        {
                            vel.X = 0;
                            vel.Y = 0;
                            return;
                        }
                        break;

                    case AIState.Alerted:
                        // Move towards last known position
                        targetPosition = ai.LastKnownTargetPosition;
                        hasDestination = true;
                        movement.IsRunning = false;
                        break;

                    case AIState.Pursuing:
                        // Move towards target
                        targetPosition = ai.LastKnownTargetPosition;
                        hasDestination = true;
                        movement.IsRunning = true;
                        break;

                    case AIState.Attacking:
                        // Face target but don't move (or move slightly)
                        if (ai.HasTarget && ai.Target.IsAlive() && ai.Target.Has<Position>())
                        {
                            var targetPos = ai.Target.Get<Position>();
                            FaceTarget(ref rot, pos, targetPos.ToVector3(), deltaTime, movement.RotationSpeed);

                            // Try to attack
                            if (ai.AttackTimer <= 0)
                            {
                                PerformAttack(ref ai, entity);
                            }
                        }
                        vel.X = 0;
                        vel.Y = 0;
                        return;

                    case AIState.Returning:
                        // Return to patrol start or spawn position
                        if (entity.Has<PatrolRoute>())
                        {
                            var patrol = entity.Get<PatrolRoute>();
                            if (patrol.Waypoints != null && patrol.Waypoints.Count > 0)
                            {
                                targetPosition = patrol.Waypoints[0];
                                hasDestination = true;
                            }
                        }
                        movement.IsRunning = false;

                        // Check if reached home
                        float distToHome = Vector3.Distance(pos.ToVector3(), targetPosition);
                        if (distToHome < 1f)
                        {
                            ai.State = entity.Has<PatrolRoute>() ? AIState.Patrolling : AIState.Idle;
                            Program.Log("AI: Returned home");
                        }
                        break;

                    default:
                        vel.X = 0;
                        vel.Y = 0;
                        return;
                }

                if (!hasDestination)
                {
                    vel.X = 0;
                    vel.Y = 0;
                    return;
                }

                // Move towards target
                MoveTowards(ref pos, ref rot, ref vel, targetPosition, deltaTime, movement);
            });
        }

        private void UpdatePatrols(float deltaTime)
        {
            world.Query(in patrolQuery, (ref Position pos, ref AIAgent ai, ref PatrolRoute patrol) =>
            {
                if (ai.State != AIState.Patrolling)
                    return;

                if (patrol.Waypoints == null || patrol.Waypoints.Count == 0)
                    return;

                // Handle wait timer
                if (patrol.WaitTimer > 0)
                {
                    patrol.WaitTimer -= deltaTime;
                    return;
                }

                // Check if reached current waypoint
                var target = patrol.CurrentTarget;
                if (target.HasValue)
                {
                    float distance = Vector3.Distance(pos.ToVector3(), target.Value);
                    if (distance < 0.5f)
                    {
                        patrol.NextWaypoint();
                    }
                }
            });
        }

        private void MoveTowards(ref Position pos, ref Rotation rot, ref Velocity vel,
            Vector3 target, float deltaTime, AIMovement movement)
        {
            Vector3 currentPos = pos.ToVector3();
            Vector3 direction = target - currentPos;
            direction.Z = 0; // Keep movement horizontal

            float distance = direction.Length();
            if (distance < 0.1f)
            {
                vel.X = 0;
                vel.Y = 0;
                return;
            }

            direction.Normalize();

            // Rotate towards target
            float targetAngle = MathF.Atan2(direction.X, direction.Y);
            float currentAngle = MathHelper.ToRadians(rot.Yaw);
            float angleDiff = NormalizeAngle(targetAngle - currentAngle);

            float maxRotation = MathHelper.ToRadians(movement.RotationSpeed * deltaTime);
            float rotation = MathHelper.Clamp(angleDiff, -maxRotation, maxRotation);
            rot.Yaw = MathHelper.ToDegrees(currentAngle + rotation);

            // Only move if roughly facing target
            if (MathF.Abs(angleDiff) < MathHelper.ToRadians(45f))
            {
                float speed = movement.CurrentSpeed;
                vel.X = direction.X * speed;
                vel.Y = direction.Y * speed;
            }
            else
            {
                vel.X = 0;
                vel.Y = 0;
            }
        }

        private void FaceTarget(ref Rotation rot, Position pos, Vector3 target, float deltaTime, float rotSpeed)
        {
            Vector3 direction = target - pos.ToVector3();
            direction.Z = 0;

            if (direction.LengthSquared() < 0.01f)
                return;

            direction.Normalize();
            float targetAngle = MathF.Atan2(direction.X, direction.Y);
            float currentAngle = MathHelper.ToRadians(rot.Yaw);
            float angleDiff = NormalizeAngle(targetAngle - currentAngle);

            float maxRotation = MathHelper.ToRadians(rotSpeed * deltaTime);
            float rotation = MathHelper.Clamp(angleDiff, -maxRotation, maxRotation);
            rot.Yaw = MathHelper.ToDegrees(currentAngle + rotation);
        }

        private float NormalizeAngle(float angle)
        {
            while (angle > MathF.PI) angle -= 2 * MathF.PI;
            while (angle < -MathF.PI) angle += 2 * MathF.PI;
            return angle;
        }

        private void PerformAttack(ref AIAgent ai, Entity attacker)
        {
            ai.AttackTimer = ai.AttackCooldown;

            if (!ai.HasTarget || !ai.Target.IsAlive())
                return;

            // Add damage event to target
            if (ai.Target.Has<Health>())
            {
                var targetPos = ai.Target.Has<Position>() ? ai.Target.Get<Position>().ToVector3() : Vector3.Zero;

                if (!ai.Target.Has<DamageEvent>())
                {
                    ai.Target.Add(new DamageEvent(ai.AttackDamage, targetPos));
                    Program.Log($"AI attacked player for {ai.AttackDamage} damage");
                }
            }
        }
    }
}
