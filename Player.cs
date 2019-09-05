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

        public Doors Doors
        {
            get { return doors; }
        }
        Doors doors;
         KeyboardState keystate = Keyboard.GetState();
        KeyboardState keystate1;
        public Player(Map map)
        {
            this.map = map;
        }
            public void Update(KeyboardState keystate, GameTime gameTime)
        {
                UpdateMovement(keystate);
            CoolDown += gameTime.ElapsedGameTime.TotalMilliseconds;
        }

public void tryToInteract()
        {
            for (int i= 0; i< map.door.Count; i++)
            {
                if(checkDistance(me, map.door[i].x, map.door[i].y, map.door[i].z)<=1)
                {
                    map.door[i].interact();
                }
            }
        }
        
        public void UpdateMovement(KeyboardState keystate)
            {
            if(keystate.isKeyPress(Keys.E))
            {
                tryToInteract();
            }
            if (keystate.IsKeyDown(Keys.W))
            {
                if (me.Y < 100)
                {
                    for (int i = 0; i < map.staircases.Count; i++)
                    {
                        if (map.staircases[i].onstaircase == true)
                        {
                            if (CoolDown >= 500)
                            { 
                                orientation = playerOrientation.Up;
                            me.Y += 1;
                            map.playstep();
                            CoolDown = 0;
                        }
                        }
                        else
                        {
                            if (CoolDown >= 225)
                            {
                                orientation = playerOrientation.Up;
                                me.Y += 1;
                                map.playstep();
                                CoolDown = 0;
                            }
                            }
                    }
                }
            }
            if (keystate.IsKeyDown(Keys.S))
            {
                if (me.Y > 0)
                {
for(int j=0; j<map.staircases.Count(); j++)
                    {
                        if (map.staircases[j].onstaircase == true)
                        {
                            if (CoolDown >= 500)
                            {
                                orientation = playerOrientation.Down;
                                me.Y += -1;
                                map.playstep();
                                CoolDown = 0;
                            }
                        }
                        else
                        {
                            if (CoolDown >= 225)
                            {
                                orientation = playerOrientation.Down;
                                me.Y += -1;
                                map.playstep();
                                CoolDown = 0;
                            }
                            }
                        }
                    }
            }
            if (keystate.IsKeyDown(Keys.A))
            {
                if (me.X > 0)
                {
                    for (int k = 0; k < map.staircases.Count(); k++)
                    {
                        if (map.staircases[k].onstaircase == true)
                        {
                            if (CoolDown >= 500)
                            {
                                orientation = playerOrientation.Left;
                                me.X += -1;
                                map.playstep();
                                CoolDown = 0;
                            }
                        }
                        else
                        {
                            if (CoolDown >= 225)
                            {
                                orientation = playerOrientation.Left;
                                me.X += -1;
                                map.playstep();
                                CoolDown = 0;
                            }
                        }
                    }
                            }
            }
            if (keystate.IsKeyDown(Keys.D))
            {
                if (me.X < maxx)
                {
                    for (int l = 0; l < map.staircases.Count(); l++)
                    {
                        if (map.staircases[l].onstaircase == true)
                        {
                            if (CoolDown >= 500)
                            {
                                orientation = playerOrientation.Right;
                                me.X += 1;
                                map.playstep();
                                CoolDown = 0;
                            }
                        }
                        else
                        {
                            if (CoolDown >= 225)
                            {
                                orientation = playerOrientation.Right;
                                me.X += 1;
                                map.playstep();
                                CoolDown = 0;
                            }
                        }
                    }
                        }
if(keystate.IsKeyDown(Keys.Up))
{
                    if(me.Z< 100)
                    {
                        for(int u=0; u<map.staircases.Count(); u++)
                        {
                            if(map.staircases[u].onstaircase==true)
                            {
                                if (CoolDown >= 500)
                                {
                                    orientation = playerOrientation.Up;
                                    me.Z += 1;
                                    map.playstep();
                                    CoolDown = 0;
                                }
                                }
                        }
                        }
                }
            }
            if(keystate.IsKeyDown(Keys.Down))
{
                if(me.Z>=0)
                {
                    for(int o=0; o<map.staircases.Count(); o++)
                    {
                        if(map.staircases[o].onstaircase==true)
                        {
                            if(CoolDown>=500)
                            {
                                orientation = playerOrientation.Down;
                                me.Z += -1;
                                map.playstep();
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

