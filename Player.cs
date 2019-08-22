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
        public Player(Map map, Vector3 Me)
        {
            this.map = map;
            this.me = Me;
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
            if(keystate.isKeyPress(Keys.Enter))
            {
                tryToInteract();
                CoolDown = 0;
            }
            if (CoolDown >= 225 && keystate.IsKeyDown(Keys.Up))
            {
                if (me.Y < 100)
                {
                    orientation = playerOrientation.Up;
                    me.Y += 1;
                            map.playstep();
                        CoolDown = 0;
                }
            }
            else if (CoolDown >= 225 && keystate.IsKeyDown(Keys.Down))
            {
                if (me.Y > 0)
                {
                    orientation=playerOrientation.Down;
                    me.Y += -1;
                    map.playstep();
                    CoolDown = 0;
                }
            }
            else if (CoolDown>=225 &&keystate.IsKeyDown(Keys.Left))
                {
                     if (me.X > 0)
                    {
                        orientation= playerOrientation.Left;
                        me.X += -1;
                        map.playstep();
                    CoolDown = 0;
                }
                }
                else if (CoolDown>=225 && keystate.IsKeyDown(Keys.Right))
                {
                    if (me.X < maxx)
                    {
                        orientation=playerOrientation.Right;
                        me.X += 1;
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

    }
    }

