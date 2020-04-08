using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using tfj.exploudEngine;

namespace Game3
{
    public class Rooms
    {
        public Map Map
        {
            get { return map; }
        }
            Map map;
        public int minx, maxx, miny, maxy, minz, maxz;
        public string sound;
        public bool onRoom;
        private eMusic RoomSound;
        public Rooms(Map map, int rminx, int rmaxx, int rminy, int rmaxy, int rminz, int rmaxz, string rsound)
        {
            this.map = map;
            this.minx = rminx;
            this.maxx = rmaxx;
            this.miny = rminy;
            this.maxy = rmaxy;
            this.minz = rminz;
            this.maxz = rmaxz;
            this.sound = rsound;
            this.RoomSound=Game1.fmodengine.loadMusic("sounds/rooms/" + rsound + ".mp3");
            RoomSound.play();
        }

        public void Update(GameTime gameTime)
        {
            if ((int)map.Player.me.X >= minx && (int)map.Player.me.X <= maxx && (int)map.Player.me.Y >= miny && (int)map.Player.me.Y <= maxy)
            {
                onRoom = true;
                if (RoomSound.playing)
                {
                    return;
                }
else
                {
                    RoomSound.play();
                }
}
            else
            {
                onRoom = false;
                RoomSound.stop();
            }
        }


    }
}
