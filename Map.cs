using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using IrrKlang;

namespace Game3
{
    public class Map
    {
 public enum orientation
        {
            front,
            back,
            left,
            right,
up,
down,
        }
        public orientation o;
        Random random = new Random();
        private Rotation rotation = new Rotation();
        public ISoundEngine engine = new ISoundEngine();
        public Player Player
        {
            get { return player; }
        }
        Player player;
        public iOverWindow activeOverWindow;
        public Dictionary<string, string> tiles = new Dictionary<string, string>();
        public Dictionary<string, string> walls = new Dictionary<string, string>();
        public List<Object> obj = new List<Object>();
        public List<Stairs> staircases = new List<Stairs>();
        public List<Doors> door = new List<Doors>();
        public List<Rooms> room = new List<Rooms>();
        int mapName;
        string oldEaredX;
        string oldEaredY;

        public Map(int mapname)
        {
            this.mapName = mapname;
            
        }

        public void Drawmap()
        {
            if (mapName == 1)
            {
                spawn_tile(0, 100, 0, 100, 0, 0, "womanstep");
                spawn_walls(0, 10, 11, 11, 0, 5         , "woodwall");
                spawn_room(0, 100, 0, 100, 0, 20, "rain house");
                spawn_walls(13, 13, 0, 11, 0, 5, "woodwall");
                spawn_door(12, 11, 0, "door1", true);
                spawn_object(0, 20, 0, "window", true);
                spawn_player(0, 19, 0);
            }
        }

        public void Update(KeyboardState keystate, GameTime gameTime)
        {
            if (activeOverWindow != null)
            {
                activeOverWindow.update(gameTime);
                if (activeOverWindow.finished())
                {
                    activeOverWindow = null;
                }
            }
            else
            {
player.Update(keystate, gameTime);
                engine.SetListenerPosition(player.me.X, player.me.Y, player.me.Z, 0, 0, 1);
                engine.SetRolloffFactor(1.0f);
                updateDoors(gameTime);
                updateStairs(gameTime);
                UpdateRooms(gameTime);
                UpdateObjects(gameTime);
            }
        }
        public void UpdateObjects(GameTime gameTime)
        {
            for(int i=0; i<obj.Count(); i++)
            {
                obj[i].Update(gameTime);
            }
        }

        public void UpdateRooms(GameTime gameTime)
        {
for(int i=0; i<room.Count(); i++)
            {
                room[i].Update(gameTime);
            }
        }

        public void updateDoors(GameTime gameTime)
        {
            for(int i=0; i<door.Count(); i++)
            {
                door[i].Update(gameTime);
            }
        }

        public void updateStairs(GameTime gameTime)
        {
            for(int i=0; i<staircases.Count(); i++)
            {
                staircases[i].Update(gameTime);
            }
        }

        public void spawn_player(float x, float y, float z)
        {
            player = new Player(this);
            player.me.X = x;
            player.me.Y = y;
            player.me.Z = z;
        }

        public void spawn_object(int x, int y, int z, string name, bool interactable)
        {
            obj.Add(new Object(this, x, y, z, name, interactable));
            spawn_walls(x, x, y, y, z, z, "doorwall");
        }

        public void spawn_room(int minx, int maxx, int miny, int maxy, int minz, int maxz, string name)
        {
            room.Add(new Rooms(this, minx, maxx, miny, maxy, minz, maxz, name));
        }

        public void spawn_door(int dx, int dy, int dz, string name, bool isopen = false)
        {
            door.Add(new Doors(this, dx, dy, dz, name, isopen));
        }

        public void spawn_staircases(int minsx, int maxsx, int minsy, int maxsy, int sz, int sheigth, orientation o, string stile)
        {
            staircases.Add(new Stairs(this, minsx, maxsx, minsy, maxsy, sz, sheigth, o, stile));
            spawn_tileWithSlope(minsx, maxsx, minsy, maxsy, sz, sheigth, o, stile);
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

        public void spawn_walls(int minx, int maxx, int miny, int maxy, int minz, int maxz, string tile)
        {
            for(int x=minx; x<=maxx; x++)
            {
                for(int y=miny; y<=maxy; y++)
                {
                    for(int z=minz; z<=maxz; z++)
                    {
                        walls.Add(x + ":" + y + ":" + z, tile);
                    }
                }
            }
        }

        public void remove_walls(int minx, int maxx, int miny, int maxy, int minz, int maxz)
        {
            for(int x=minx; x<=maxx; x++)
            {
                for(int y=miny; y<=maxy; y++)
                {
                    for(int z=minz; z<=maxz; z++)
                    {
                        walls.Remove(x + ":" + y + ":" + z);
                    }
                }
            }
        }

public void spawn_tileWithSlope(int minx, int maxx, int miny, int maxy, int minz, int stepHeight, orientation o, string tile)
        {
            int maxz = 0;
            int basez = minz;
if(o==orientation.left||o==orientation.right)
            {
                maxz = maxx - minx;
            }
            else
            {
                maxz = maxy - miny;
            }
            maxz = Math.Abs(maxz * stepHeight);
            int factor = stepHeight;
if(o==orientation.left||o==orientation.back)
            {
                factor = -1 * factor;
                basez = maxz;
            }
if(o==orientation.left || o==orientation.right)
            {
                for(int x=minx; x<=maxx; x++)
                {
                    for(int y=miny; y<=maxy; y++)
                    {
                        for(int z=basez; z<=maxz; z++)
                        {
                            tiles.Add(x + ":" + y + ":" + z, tile);
                        }
                    }
                    basez += factor;
                }
            }
            else
            {
for(int y=miny; y<=maxy; y++)
                {
                    for(int x=minx; x<=maxx; x++)
                    {
                        for(int z=basez; z<=maxz; z++)
                        {
                            tiles.Add(x + ":" + y + ":" + z, tile);
                        }
                    }
                    basez += factor;
                }
            }
        }


        public string gmw()
        {
            if (walls.ContainsKey((int)player.me.X + ":" + (int)player.me.Y + ":" + (int)player.me.Z))
                {
                string outval;
                walls.TryGetValue((int)player.me.X + ":" + (int)player.me.Y + ":" + (int)player.me.Z, out outval);
                return outval;
            }
                    return "";
        }


        public string get_tile_at(int x, int y, int z)
        {
            string o;
            tiles.TryGetValue(x + ":" + y+":"+z, out o);
            return o;
        }

        public  void playstep()
        {
            string stx = player.me.X.ToString("R");
            string sty = player.me.Y.ToString("R");
            if (gmw().IndexOf("wall", 0) > -1)
            {
                engine.Play2D("sounds/walls/" + gmw() + ".wav");
                bounce();
            }
            if (stx.Contains(",1") && stx!=oldEaredX)
            {
                engine.Play2D("sounds/steps/" + get_tile_at((int)player.me.X, (int)player.me.Y, (int)player.me.Z) + "/" + random.Next(1, Directory.GetFiles("sounds/steps/"+get_tile_at((int)player.me.X,  (int)player.me.Y, (int)player.me.Z)).Length+1) + ".ogg");
                oldEaredX = stx;
            }
            else if(stx.Contains(",5")&&stx!=oldEaredX)
                {
                engine.Play2D("sounds/Movement/stepback" + random.Next(1, 7) + ".wav");
                oldEaredX = stx;
            }
                    else if (sty.Contains(",1")&&sty!=oldEaredY)
            {
                engine.Play2D("sounds/steps/" + get_tile_at((int)player.me.X, (int)player.me.Y, (int)player.me.Z) + "/" + random.Next(1, Directory.GetFiles("sounds/steps/"+get_tile_at((int)player.me.X, (int)player.me.Y, (int)player.me.Z)+"/").Length+1) + ".ogg");
                oldEaredY = sty;
            }
                    else if(sty.Contains(",5")&&sty!=oldEaredY)
{
                engine.Play2D("sounds/Movement/move" + random.Next(1, 12) + ".ogg");
                oldEaredY = sty;
            }
        }




        public  void bounce()
        {
if(player.angle==rotation.north)
            {
                player.me.Y += -1;
            }
else if(player.angle==rotation.south)
            {
                player.me.Y += 1;
            }
else if(player.angle==rotation.east)
            {
                player.me.X += -1;
            }
else if(player.angle==rotation.west)
            {
                player.me.X += 1;
            }
        }

    }
}
