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
        string location;
        int doorsound;
        string dcs, dos;
        public Map Map
        {
            get { return map; }
        }
        Map map;
        
        public Doors(Map map, int dx, int dy, int dz, string snd="sounds/door1.wav", string snd2="sounds/dooropen.wav", string snd3="doorclose.wav")
        {
            this.map = map;
            this.x = dx;
            this.y = dy;
            this.z = dz;
            this.dos = snd2;
            this.dcs = snd3;
        }
        public void Update()
        {
                if (Map.Player.me.X == x && Map.Player.me.Y == y && Map.Player.me.Z == z)
                {
                    engine.Play2D("sounds/doorwall.wav");
                Map.bounce();
            }
        }

            }
}
