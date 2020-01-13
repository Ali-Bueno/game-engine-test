using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using IrrKlang;
using Microsoft.Xna.Framework;

namespace Game3
{
    public class Doors
    {
        ISoundEngine engine = new ISoundEngine();
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
           map.engine.Play3D("sounds/doors/"+name+"/loop.mp3", x, y, z, true);
        }

        public void Update(GameTime gameTime)
        {
            if ((int)Map.Player.me.X == x && (int)Map.Player.me.Y == y && (int)Map.Player.me.Z == z)
            {
                    if (isopen==false)
                    {
                    isInpassable = true;
                    map.engine.Play3D("sounds/doors/" + doorname + "/wall.mp3", x, y, z);
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
           map.engine.Play3D("sounds/doors/"+doorname+"/open.mp3", x, y, z);
        }

        public void close()
        {
            isopen = false;
            map.engine.Play3D("sounds/doors/"+doorname+"/close.mp3", x, y, z);
        }


    }
}
