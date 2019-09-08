using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Game3
{
    public class Stairs
    {
        public int minx, maxx, miny, maxy, z, heigth;
        public Map.orientation o;
        public string tile;
        public bool onstaircase;
        public Map Map
        {
        get{return map;}
}
    Map map;
    public Stairs(Map map, int minsx, int maxsx, int minsy, int maxsy, int sz, int sheigth, Map.orientation so, string ssound)
{
            this.map = map;
            this.minx = minsx;
    this.maxx = maxsx;
    this.miny = minsy;
    this.maxy = maxsy;
    this.z = sz;
            this.heigth = sheigth;
            this.o = so;
            this.tile = ssound;
}

        public void Update(GameTime gameTime)
        {
            if (map.Player.me.X >= minx && map.Player.me.X <= maxx && map.Player.me.Y >= miny && map.Player.me.Y <= maxy)
            {
                onstaircase = true;
            }
            else
            {
                onstaircase = false;
            }
            }

    }
}
