using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using DavyKager;

namespace Game3
{
    public class Player
    {
        public enum state
        {
            IsCrouching,
            IsStanding,
            IsCrouch,
            IsStand,
        }
        public state playerState;
        public enum walkstile
        {
            IsWalking,
            IsOnStairs,
        }
        public walkstile playerWalkstile;
        private Rotation rotation = new Rotation();
        public bool isMoving;
        public bool canMove = true;
        public float angle = 0;
        public float facing;
        int maxx = 100;
        int runtime;
        int run = 12;
        int speedtime;
        int speed = 30;
        int movetime = 60;
        public Vector3 me;
        public Random random = new Random();
        public double CoolDown;
        public double crouchCoolDown;
        public double turnCoolDown;
        public float sice;
        public float currentsice;
        public float crouchsice;
        public float direction;
        public Map Map
        {
            get { return map; }
        }
        Map map;
        public Player(Map map, float Sice)
        {
            this.map = map;
            this.sice = Sice;
            this.crouchsice = sice/ 2;
            playerState = state.IsStand;
            Script.currentStage = Script.Stages.loop1;
            currentsice = sice;
        }
        public void Update(KeyboardState keystate, GameTime gameTime)
        {
            if (crouchCoolDown > 0)
            {
                crouchCoolDown--;
            }
            if (turnCoolDown > 0)
            {
                turnCoolDown--;
            }
            checkturn(keystate);
            checkcrouch();
            moveUpdate(keystate);
            IsInteracting(keystate);
            CoolDown += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (Input.WasKeyPressed(Keys.K) && playerState == state.IsStand)
            {
                playerState = state.IsCrouching;
            }
            else if (Input.WasKeyPressed(Keys.K) && playerState == state.IsCrouch)
            {
                playerState = state.IsStanding;
            }
        }

        public void moveUpdate(KeyboardState keystate)
        {
            checktiles();
            if (playerWalkstile == walkstile.IsWalking)
            {
                MoveOnGround(keystate);
            }
            else
            {
                moveOnStairs();
            }
        }

        public void checkturn(KeyboardState keystate)
        {
            if (turnCoolDown > 0)
            {
                return;
            }
            if (keystate.IsKeyDown(Keys.Left))
            {
                angle = rotation.turnleft(angle);
                turnCoolDown = 15;
            }
            else if (keystate.IsKeyDown(Keys.Right))
            {
                angle = rotation.turnright(angle);
                turnCoolDown = 15;
            }
        }

        public void tryToInteract()
        {
            for (int i = 0; i < map.door.Count; i++)
            {
                if (checkDistance(me, map.door[i].x, map.door[i].y, map.door[i].z) <= 1.8f)
                {
                    map.door[i].interact();
                }
            }
            for (int j = 0; j < map.obj.Count(); j++)
            {
                if (checkDistance(me, map.obj[j].x, map.obj[j].y, map.obj[j].z) <= 1.8f)
                {
                    map.obj[j].interact();
                }
            }
        }

        public void IsInteracting(KeyboardState keystate)
        {
            if (Input.WasKeyPressed(Keys.E))
            {
                tryToInteract();
            }
        }

        public void checktiles()
        {
            playerWalkstile = walkstile.IsWalking;
for(int i=0; i<map.staircases.Count; i++)
            {
                if (map.staircases[i].onstaircase == true)
                {
                    playerWalkstile = walkstile.IsOnStairs;
                }
            }
        }

        public void MoveOnGround(KeyboardState keystate)
        {
            if (keystate.IsKeyDown(Keys.W) && me.Y < 100)
            {
                if (CoolDown >= 35)
                {
                    isMoving = true;
                    me = rotation.move(me, facing = rotation.north, angle);
                    map.playstep();
                    CoolDown = 0;
                }
            }
            if (keystate.IsKeyDown(Keys.S) && me.Z > 0)
            {
                if (CoolDown >= 35)
                {
                    isMoving = true;
                    me = rotation.move(me, facing = rotation.south, angle);
                    map.playstep();
                    CoolDown = 0;
                }
            }
            if (keystate.IsKeyDown(Keys.A) && me.X > 0)
            {
                if (CoolDown >= 35)
                {
                    isMoving = true;
                    me = rotation.move(me, facing = rotation.west, angle);
                    map.playstep();
                    CoolDown = 0;
                }
            }
            if (keystate.IsKeyDown(Keys.D) && me.X < maxx)
            {
                if (CoolDown >= 35)
                {
                    isMoving = true;
                    me = rotation.move(me, facing = rotation.east, angle);
                    map.playstep();
                    CoolDown = 0;
                }
            }
        }

        public void moveOnStairs()
        {
            for (int i = 0; i < map.staircases.Count(); i++)
            {
                    if (Input.keystate.IsKeyDown(Keys.W) && map.staircases[i].onstaircase == true && CoolDown >= 35 && me.Z <= map.staircases[i].maxz)
                {
                    if (me.Z == map.staircases[i].maxz)
                    {
                        me = rotation.move(me, rotation.north, angle);
                        map.playstep();
                        CoolDown = 0;
                    }
                    else
                    {
                        me = rotation.move(me, rotation.north, angle);
                        map.playstep();
                        me.Y += 0.1f;
                        CoolDown = 0;
                    }
                }
                else if (Input.keystate.IsKeyDown(Keys.S) && map.staircases[i].onstaircase == true && CoolDown >= 35 && me.Z >= map.staircases[i].minz)
                {
                    if (me.Z == map.staircases[i].minz)
                    {
                        me = rotation.move(me, rotation.south, angle);
                        map.playstep();
                        CoolDown = 0;
                        map.staircases[i].onstaircase = false;
                    }
                    else
                    {
                        me = rotation.move(me, rotation.south, angle);
                        map.playstep();
                        me.Y += -0.1f;
                        CoolDown = 0;
                    }
                }
                else if (Input.keystate.IsKeyDown(Keys.A) && CoolDown >= 35)
                {
                    me = rotation.move(me, rotation.west, angle);
                    map.playstep();
                    CoolDown = 0;
                }
                else if (Input.keystate.IsKeyDown(Keys.D) && CoolDown >= 35)
                {
                    me = rotation.move(me, rotation.east, angle);
                    map.playstep();
                    CoolDown = 0;
                }
                }
            }



        public float checkDistance(Vector3 vec, float x, float y, float z)
        {
            float a = x - vec.X;
            float b = y - vec.Y;
            float c = z - vec.Z;
            return (float)Math.Sqrt(a * a + b * b + c * c);
        }

public void checkcrouch()
        {
if(crouchCoolDown>0)
            {
                return;
            }
            switch (playerState)
            {
                case state.IsCrouching:
                    crouching();
                    break;
                case state.IsStanding:
                    standing();
                    break;
            }
        }

        public void standing()
        {
            if(currentsice<sice)
            {
                currentsice += 0.1f;
                crouchCoolDown = 5;
            }
            else
            {
                playerState = state.IsStand;
            }
        }

        public void crouching()
        {
            if(currentsice>crouchsice)
            {
                currentsice -= 0.1f;
                crouchCoolDown = 5;
            }
            else
            {
                playerState = state.IsCrouch;
            }
        }

        
    }
    }
