using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using IrrKlang;

namespace Game3
{
    public class Rooms
    {
        public double cooldown;
        ISoundEngine engine = new ISoundEngine();

        public Map Map
        {
            get { return map; }
        }
            Map map;
        public int minx, maxx, miny, maxy, minz, maxz;
        public string sound;
        public bool onRoom;
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
            //engine.Play2D("sounds/rooms/"+rsound+".mp3", true);
        }

        public void Update(GameTime gameTime)
        {
/*
            if ((int)map.Player.me.X >= minx && (int)map.Player.me.X <= maxx && (int)map.Player.me.Y >= miny && (int)map.Player.me.Y <= maxy)
            {
                onRoom = true;
                if (engine.IsCurrentlyPlaying("sounds/rooms/"+sound+".mp3")==true)
                {
                    return;
                }
else
                {
                    engine.Play2D("sounds/rooms/"+sound+".mp3", true);
                }
}
            else
            {
                onRoom = false;
                engine.RemoveAllSoundSources();
            }
*/
        }


    }
}
