using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using IrrKlang;

namespace Game3
{
    public class Map
    {
        Random random = new Random();
        public ISoundEngine engine = new ISoundEngine();
        public Player Player
        {
            get { return player; }
        }
        Player player;
        public Dictionary<string, string> tiles = new Dictionary<string, string>();
        public Doors Doors
{
        get { return doors; }

}
        Doors doors;
        public List<Doors> door = new List<Doors>();
        int mapName;
        public Map(int mapname)
        {
            this.mapName = mapname;
        }
        public void spawn_door(int dx, int dy, int dz, string s2 = "sounds/dooropen.wav", string s3 = "sounds/doorclose.wav", bool isopen=false)
        {
            door.Add(new Doors(this, dx, dy, dz, s2, s3, isopen));
        }

        public void Drawmap()
        {
            if (mapName == 1)
            {
            spawn_tile(0, 11, 0, 11, 0, 0, "rocks");
            spawn_tile(0, 12, 12, 12, 0, 5, "woodwall");
                spawn_tile(13, 100, 0, 100, 0, 0, "tile");
                spawn_tile(12, 12, 0, 11, 0, 0, "tile");
                spawn_tile(0, 12, 13, 100, 0, 0, "tile");
                spawn_door(5, 5, 0, "sounds/dooropen.wav", "sounds/doorclose.wav", false);
                spawn_door(10, 5, 0, "sounds/dooropen.wav", "sounds/doorclose.wav", true);
                player = new Player(this, Vector3.Zero);
            }
        }

    public void Update(KeyboardState keystate, GameTime gameTime)
        {
            player.Update(keystate, gameTime);
            engine.SetListenerPosition(player.me.X, player.me.Y, player.me.Z, 0, 0, 1);
            updateDoors(gameTime);
            engine.Update();
        }


        public void updateDoors(GameTime gameTime)
        {
            for(int i=0; i<door.Count(); i++)
            {
                door[i].Update(gameTime);
            }
        }
        public void spawn_tile(int minx, int maxx, int miny, int maxy, int minz, int maxz, string tile)
        {
for(int x=minx; x<=maxx; x++)
            {
                for(int y=miny; y<=maxy; y++)
                {
                    for(int z=minz; z<=maxz; z++)
                    {
                        tiles.Add(x+":"+y+":"+z, tile);
                    }
                }
            }
        }

        public string gmt()
        {
            if (tiles.ContainsKey(player.me.X + ":" + player.me.Y + ":" + player.me.Z))
                {
                string outval;
                tiles.TryGetValue(player.me.X + ":" + player.me.Y + ":" + player.me.Z, out outval);
                return outval;
            }
                    return "";
        }


        public string get_tile_at(float x, float y, float z)
        {
            string o;
            tiles.TryGetValue(x + ":" + y+":"+z, out o);
            return o;
        }



        public  void playstep()
        {
            if (gmt().IndexOf("wall",0)>-1)
            {
                engine.Play2D("sounds/" + gmt() + ".wav");
                bounce();
            }
            else
            {
                engine.Play2D("sounds/" + get_tile_at(player.me.X, player.me.Y, player.me.Z) + "step" + random.Next(1, 5) + ".wav");
            }
        }




        public  void bounce()
        {
            if (player.orientation==Player.playerOrientation.Up)
            {
                player.me.Y += -1;
            }
            else if (player.orientation==Player.playerOrientation.Left)
            {
                player.me.X += 1;
            }
            else if (player.orientation==Player.playerOrientation.Right)
            {
                player.me.X += -1;
            }
            else if (player.orientation==Player.playerOrientation.Down)
            {
                player.me.Y += 1;
            }
        }

    }
}
