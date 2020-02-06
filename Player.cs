using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using IrrKlang;
using DavyKager;

namespace Game3
{
    public class Player
    {
        public enum playerOrientation
        {
            Front,
            Back,
            Left,
            Right,
            Up,
            Down,
        }
        public playerOrientation orientation;
        public enum state
        {
            IsWalking,
            IsOnStairs,
            IsCrouching,
            IsStanding,
IsCrouch,
IsStand,
        }
        public state playerState;
        private Rotation rotation = new Rotation();
        public bool isMoving;
        public bool canMove=true;
        public bool canInteract = true;
        public int angle;
        public bool interaction;
        int maxx = 100;
        public int facing;
        int runtime;
        int run = 12;
        int speedtime;
        int speed = 30;
        int movetime = 60;
        public Vector3 me;
        public Random random = new Random();
        public double CoolDown = 0;
        public double crouchCoolDown;
        public float sice;
        public float currentsice;
        public float crouchsice;
        public Map Map
        {
            get { return map; }
        }
        Map map;
        public Player(Map map, float Sice)
        {
            this.map = map;
            this.sice = Sice;
            sice += me.Y;
            currentsice += sice;
            this.crouchsice += sice / 2;
        }
        public void Update(KeyboardState keystate, GameTime gameTime)
        {
            checkcrouch();
            moveUpdate(keystate);
            IsInteracting(keystate);
            CoolDown += gameTime.ElapsedGameTime.TotalMilliseconds;
crouchCoolDown += gameTime.ElapsedGameTime.TotalMilliseconds;
        }

        public void moveUpdate(KeyboardState keystate)
        {
            checktiles();
            if (playerState==state.IsWalking)
            {
                MoveOnGround(keystate);
            }
else
            {
                moveOnStairs();
            }
if(Input.WasKeyPressed(Keys.K))
            {
                playerState = state.IsCrouching;
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
for(int j=0; j<map.obj.Count(); j++)
            {
                if(checkDistance(me, map.obj[j].x, map.obj[j].y, map.obj[j].z) <= 1.8f)
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
            playerState = state.IsWalking;
            for (int i=0; i<map.staircases.Count(); i++)
            {
                if (map.staircases[i].onstaircase == true)
                {
                    playerState = state.IsOnStairs;
                    break;
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
                     me=rotation.move(me, angle=0);
                    map.playstep();
                    CoolDown = 0;
                }
            }
            if (keystate.IsKeyDown(Keys.S) && me.Z > 0)
            {
                if (CoolDown >= 35)
                {
                    isMoving = true;
                    me = rotation.move(me, angle = 180);
                    map.playstep();
                    CoolDown = 0;
                }
            }
            if (keystate.IsKeyDown(Keys.A) && me.X > 0)
            {
                if (CoolDown >= 35)
                {
                    isMoving = true;
                    me = rotation.move(me, angle = 270);
                    map.playstep();
                    CoolDown = 0;
                }
            }
            if (keystate.IsKeyDown(Keys.D) && me.X < maxx)
            {
                if (CoolDown >= 35)
                {
                    isMoving = true;
                    me = rotation.move(me, angle = 90);
                    map.playstep();
                    CoolDown = 0;
                }
            }
        }

        public void moveOnStairs()
        {
            for (int i = 0; i < map.staircases.Count(); i++)
            {
                if (map.staircases[i].o == Map.orientation.front)
                {
                    if (Input.keystate.IsKeyDown(Keys.W) &&map.staircases[i].onstaircase==true&&CoolDown>=500&& me.Y <= map.staircases[i].maxy)
                    {
                            if (me.Y == map.staircases[i].maxy)
                                {
                            move(4);
                            CoolDown = 0;
                        }
                            else
                            {
                                me.Z += map.staircases[i].heigth;
                                move(4);
                                CoolDown = 0;
                            }
                        }
                     else if (Input.keystate.IsKeyDown(Keys.S) &&map.staircases[i].onstaircase==true&&CoolDown>=500&& me.Y >= map.staircases[i].miny)
                    {
                        if (me.Y == map.staircases[i].miny)
                        {
                            move(3);
                            CoolDown = 0;
                            map.staircases[i].onstaircase = false;
                        }
                        else
                        {
                            me.Z += -map.staircases[i].heigth;
                            move(3);
                            CoolDown = 0;
                        }
                        }
else if(Input.keystate.IsKeyDown(Keys.A)&&CoolDown>=500)
{
                            move(1);
                            CoolDown = 0;
                    }
else if(Input.keystate.IsKeyDown(Keys.D)&&CoolDown>=500)
                    {
                        move(2);
                        CoolDown = 0;
                    }
                }
            }
        }

        public bool move(float dir)
{
            if (dir == 1)
    {
                orientation = playerOrientation.Left;
        me.X += -1;
                map.playstep();
                return (true);
            }
            else if(dir==2)
    {
                orientation = playerOrientation.Right;
        me.X += 1;
                map.playstep();
                return (true);
            }
            else if(dir==3)
            {
                orientation = playerOrientation.Back;
                me.Y += -1;
                map.playstep();
                return (true);
            }
            else if(dir==4)
            {
                orientation = playerOrientation.Front;
                me.Y += 1;
                map.playstep();
                return (true);
            }
            return (false);
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
            switch (playerState)
            {
                case state.IsCrouching:
                    crouching();
                    break;
                case state.IsStanding:
                    standing();
                    break;
                case state.IsCrouch:
                    crouched();
                    break;
            }
        }

        public void standing()
        {
            if(currentsice<sice)
            {
                currentsice += 0.1f;
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
            }
            else
            {
                playerState = state.IsCrouch;
            }
        }

        public void crouched()
        {
            if(Input.WasKeyPressed(Keys.K))
            {
                playerState = state.IsStanding;
            }
        }

    }
    }
