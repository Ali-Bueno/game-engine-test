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
        public bool ismoving;
        int maxx = 100;
        int runtime;
        int run = 12;
        int speedtime;
        int speed = 30;
        int movetime = 60;
        public Vector3 me;
        public Random random = new Random();
        public double CoolDown = 0;
        ISoundEngine engine = new ISoundEngine();
        public Map Map
        {
            get { return map; }
        }
        Map map;
        KeyboardState keystate = Keyboard.GetState();
        public Player(Map map)
        {
            this.map = map;
        }
        public void Update(KeyboardState keystate, GameTime gameTime)
        {
            for (int i = 0; i < map.staircases.Count(); i++)
            {
                if (map.staircases[i].onstaircase == true)
                {
                    IsOnStairs(keystate);
                }
                else
                {
                    IsWalking(keystate);
                }
            }
                IsInteracting(keystate);
            CoolDown += gameTime.ElapsedGameTime.TotalMilliseconds;
        }

        public void tryToInteract()
        {
            for (int i = 0; i < map.door.Count; i++)
            {
                if (checkDistance(me, map.door[i].x, map.door[i].y, map.door[i].z) <= 1)
                {
                    map.door[i].interact();
                }
            }
        }

        public void IsInteracting(KeyboardState keystate)
        {
            if (keystate.isKeyPress(Keys.E))
            {
                tryToInteract();
            }
        }

        public void IsWalking(KeyboardState keystate)
        {
            if (keystate.IsKeyDown(Keys.W)&&me.Y<100)
            {
                    if (CoolDown >= 225)
                    {
                    ismoving = true;
                    move(4);
                    CoolDown = 0;
                    }
                    }
            else if (keystate.IsKeyDown(Keys.S)&&me.Y>0)
            {
                            if (CoolDown >= 225)
                            {
                    ismoving = true;
                    move(3);
                    CoolDown = 0;
                        }
                    }
            else if (keystate.IsKeyDown(Keys.A)&&me.X>0)
            {
                            if (CoolDown >= 225)
                            {
                    ismoving = true;
                    move(1);
                    CoolDown = 0;
                }
            }
            else if (keystate.IsKeyDown(Keys.D)&&me.X<maxx)
            {
                            if (CoolDown >= 225)
                            {
                    ismoving = true;
                    move(2);
                    CoolDown = 0;
                }
            }
            }

    public void move(float dir)
{
    if (dir == 1)
    {
                orientation = playerOrientation.Left;
        me.X += -1;
                map.playstep();
            }
else if(dir==2)
    {
        orientation = playerOrientation.Right;
        me.X += 1;
                map.playstep();
            }
    else if(dir==3)
            {
                orientation = playerOrientation.Back;
                me.Y += -1;
                map.playstep();
            }
else if(dir==4)
            {
                orientation = playerOrientation.Front;
                me.Y += 1;
                map.playstep();
            }
        }

        public void IsOnStairs(KeyboardState keystate)
        {
            for(int i=0; i<map.staircases.Count(); i++)
            {
                if (map.staircases[i].o == Map.orientation.front)
                { 
                    if (keystate.IsKeyDown(Keys.W)&&me.Y<=map.staircases[i].maxy)
                {
                        if (CoolDown >= 500)
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
                        }
else if(keystate.IsKeyDown(Keys.S)&&me.Y>=map.staircases[i].miny)
                    {
                        if(CoolDown>=500)
                        {
                            if (me.Z == map.staircases[i].z)
                            {
                                move(3);
                                CoolDown = 0;
                            }
                            else
                            {
                                me.Z += -map.staircases[i].heigth;
                                move(3);
                                CoolDown = 0;
                            }
                        }
                    }
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

    }
    }

