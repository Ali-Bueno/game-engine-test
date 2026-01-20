using System;
using Arch.Core;
using Microsoft.Xna.Framework;
using Game3.ECS.Components;

namespace Game3.ECS.Systems
{
    /// <summary>
    /// Handles AI sight and hearing detection, state transitions.
    /// </summary>
    public class AIDetectionSystem
    {
        private readonly World world;
        private readonly SharedResources resources;
        private readonly QueryDescription aiQuery;
        private readonly QueryDescription playerQuery;

        public AIDetectionSystem(World world, SharedResources resources)
        {
            this.world = world;
            this.resources = resources;
            this.aiQuery = new QueryDescription()
                .WithAll<Position, Rotation, AIAgent, Enemy>()
                .WithNone<Disabled>();
            this.playerQuery = new QueryDescription()
                .WithAll<Position, PlayerControlled>();
        }

        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Get player position and entity
            Vector3 playerPos = Vector3.Zero;
            Entity playerEntity = default;
            bool hasPlayer = false;

            world.Query(in playerQuery, (Entity entity, ref Position pos) =>
            {
                playerPos = pos.ToVector3();
                playerEntity = entity;
                hasPlayer = true;
            });

            if (!hasPlayer) return;

            world.Query(in aiQuery, (Entity entity, ref Position pos, ref Rotation rot, ref AIAgent ai, ref Enemy enemy) =>
            {
                Vector3 aiPos = pos.ToVector3();
                float distanceToPlayer = Vector3.Distance(aiPos, playerPos);

                // Update time since last seen
                ai.TimeSinceLastSeen += deltaTime;

                // Check sight
                bool canSeePlayer = CheckSight(aiPos, rot.Yaw, playerPos, ai.SightRange, ai.FieldOfView);

                // Check hearing (simplified - just distance based for now)
                bool canHearPlayer = distanceToPlayer <= ai.HearingRange;

                // Update target info
                if (canSeePlayer)
                {
                    ai.HasTarget = true;
                    ai.Target = playerEntity;
                    ai.LastKnownTargetPosition = playerPos;
                    ai.TimeSinceLastSeen = 0f;
                }
                else if (canHearPlayer && !ai.HasTarget)
                {
                    // Heard something - investigate
                    ai.LastKnownTargetPosition = playerPos;
                }

                // State machine transitions
                UpdateState(ref ai, canSeePlayer, canHearPlayer, distanceToPlayer);

                // Update attack timer
                if (ai.AttackTimer > 0)
                {
                    ai.AttackTimer -= deltaTime;
                }
            });
        }

        private bool CheckSight(Vector3 aiPos, float aiAngle, Vector3 targetPos, float range, float fov)
        {
            // Check distance
            float distance = Vector3.Distance(aiPos, targetPos);
            if (distance > range)
                return false;

            // Check field of view
            Vector3 toTarget = targetPos - aiPos;
            float angleToTarget = MathF.Atan2(toTarget.X, toTarget.Y);

            // Normalize angles
            float aiAngleRad = MathHelper.ToRadians(aiAngle);
            float angleDiff = MathF.Abs(NormalizeAngle(angleToTarget - aiAngleRad));

            float fovRad = MathHelper.ToRadians(fov);
            if (angleDiff > fovRad)
                return false;

            // TODO: Add raycast for line-of-sight blocking (walls)
            // For now, simple distance + FOV check

            return true;
        }

        private float NormalizeAngle(float angle)
        {
            while (angle > MathF.PI) angle -= 2 * MathF.PI;
            while (angle < -MathF.PI) angle += 2 * MathF.PI;
            return angle;
        }

        private void UpdateState(ref AIAgent ai, bool canSee, bool canHear, float distance)
        {
            switch (ai.State)
            {
                case AIState.Idle:
                    if (canSee)
                    {
                        ai.State = AIState.Pursuing;
                        Program.Log("AI: Idle -> Pursuing (spotted player)");
                    }
                    else if (canHear)
                    {
                        ai.State = AIState.Alerted;
                        Program.Log("AI: Idle -> Alerted (heard something)");
                    }
                    break;

                case AIState.Patrolling:
                    if (canSee)
                    {
                        ai.State = AIState.Pursuing;
                        Program.Log("AI: Patrolling -> Pursuing (spotted player)");
                    }
                    else if (canHear)
                    {
                        ai.State = AIState.Alerted;
                        Program.Log("AI: Patrolling -> Alerted (heard something)");
                    }
                    break;

                case AIState.Alerted:
                    if (canSee)
                    {
                        ai.State = AIState.Pursuing;
                        Program.Log("AI: Alerted -> Pursuing (spotted player)");
                    }
                    else if (ai.TimeSinceLastSeen > ai.GiveUpTime)
                    {
                        ai.State = AIState.Returning;
                        ai.HasTarget = false;
                        Program.Log("AI: Alerted -> Returning (gave up)");
                    }
                    break;

                case AIState.Pursuing:
                    if (distance <= ai.AttackRange && canSee)
                    {
                        ai.State = AIState.Attacking;
                        Program.Log("AI: Pursuing -> Attacking (in range)");
                    }
                    else if (!canSee && ai.TimeSinceLastSeen > ai.GiveUpTime)
                    {
                        ai.State = AIState.Returning;
                        ai.HasTarget = false;
                        Program.Log("AI: Pursuing -> Returning (lost target)");
                    }
                    break;

                case AIState.Attacking:
                    if (distance > ai.AttackRange * 1.5f)
                    {
                        ai.State = AIState.Pursuing;
                        Program.Log("AI: Attacking -> Pursuing (target moved away)");
                    }
                    else if (!canSee && ai.TimeSinceLastSeen > 1f)
                    {
                        ai.State = AIState.Pursuing;
                        Program.Log("AI: Attacking -> Pursuing (lost sight)");
                    }
                    break;

                case AIState.Returning:
                    if (canSee)
                    {
                        ai.State = AIState.Pursuing;
                        Program.Log("AI: Returning -> Pursuing (spotted player)");
                    }
                    // Transition to Patrolling/Idle handled by AIMovementSystem when reaching home
                    break;
            }
        }
    }
}
