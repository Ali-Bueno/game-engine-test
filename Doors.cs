using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using IrrKlang;
using Microsoft.Xna.Framework;
using tfj.exploudEngine;

namespace Game3
{
    public class Doors
    {
        ISoundEngine engine = new ISoundEngine();
        eSound loop;
        eSound opensound;
        eSound closesound;
        eSound wallsound;
        eInstance openinstance;
        eInstance closeinstance;
        eInstance wallinstance;
        public int x, y, z;
        public double delaytime;
        string doorname;
        string dcs, dos;
         bool isopen;
bool isInpassable;
        public Map Map
        {
            get { return map; }
        }
        Map map;
        
        public Doors(Map map, int dx, int dy, int dz, string name, bool isOpen=false)
        {
            this.map = map;
            this.x = dx;
            this.y = dy;
            this.z = dz;
            this.isopen = isOpen;
            this.doorname = name;
            this.wallsound = Game1.fmodengine.loadSound("sounds/doors/" + name + "/wall.mp3");
            this.opensound = Game1.fmodengine.loadSound("sounds/doors/" + name + "/open.mp3");
            this.closesound = Game1.fmodengine.loadSound("sounds/doors/" + name + "/close.mp3");
            this.loop = Game1.fmodengine.loadSound("sounds/doors/" + name + "/loop.mp3");
            this.loop.play3d(x, y, z, loopMode.simpleLoop);
        }

        public void Update(GameTime gameTime)
        {
            if ((int)Map.Player.me.X == x && (int)Map.Player.me.Y == y && (int)Map.Player.me.Z == z)
            {
                    if (isopen==false)
                    {
                    isInpassable = true;
                    wallinstance = this.wallsound.play3d(x, y, z, loopMode.noLoop);
                    map.bounce();
                }
                    else 
                    {
                    isInpassable = false;
                }
                }
            }

        public void interact()
        {
            if (isopen)
            {
                close();
            }
            else
            {
                open();
            }
            }

        public void open()
        {
            isopen = true;
            this.openinstance = this.opensound.play3d(x, y, z, loopMode.noLoop);
        }

        public void close()
        {
            isopen = false;
            this.closeinstance = this.closesound.play3d(x, y, z, loopMode.noLoop);
        }

    }
}
