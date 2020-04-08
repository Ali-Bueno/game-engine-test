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
        public int minx, maxx, minz, maxz, y, heigth;
        public float angle;
        public string tile;
        public bool onstaircase;
        public Map Map
        {
        get{return map;}
}
    Map map;
    public Stairs(Map map, int minsx, int maxsx, int minsz, int maxsz, int sy, int sheigth, float Angle, string ssound)
{
            this.map = map;
            this.minx = minsx;
    this.maxx = maxsx;
    this.minz = minsz;
    this.maxz = maxsz;
    this.y = sy;
            this.heigth = sheigth;
            this.angle = Angle;
            this.tile = ssound;
}

        public void Update(GameTime gameTime)
        {
            if (map.Player.me.X >= this.minx && map.Player.me.X <= this.maxx && map.Player.me.Z >= this.minz && map.Player.me.Z <= this.maxz)
            {
                this.onstaircase = true;
                map.Player.angle = this.angle;
            }
            else
            {
                this.onstaircase = false;
            }
            }

    }
}
