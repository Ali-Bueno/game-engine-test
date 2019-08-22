using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IrrKlang;
using Microsoft.Xna.Framework;

namespace Game3
{
    public class Doors
    {
        ISoundEngine engine = new ISoundEngine();
        public int x, y, z;
        public double delaytime;
        string location;
        int doorsound;
        string dcs, dos;
         bool isopen;
        public Map Map
        {
            get { return map; }
        }
        Map map;
        
        public Doors(Map map, int dx, int dy, int dz, string snd2="sounds/dooropen.wav", string snd3="sounds/doorclose.wav", bool isOpen=false)
        {
            this.map = map;
            this.x = dx;
            this.y = dy;
            this.z = dz;
            this.dcs = snd2;
            this.dos = snd3;
            this.isopen = isOpen;
        }

        public void Update(GameTime gameTime)
        {
            delaytime += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (Map.Player.me.X == x && Map.Player.me.Y == y && Map.Player.me.Z == z)
            {
                    if (isopen==false)
                    {
                        engine.Play2D("sounds/doorwall.wav");
                        Map.bounce();
                    }
                    else 
                    {
                        return;
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
            ISound door2= map.engine.Play3D(dcs, x, y, z);
        }

        public void close()
        {
            isopen = false;
            map.engine.Play3D(dos, x, y, z);
        }


    }
}
