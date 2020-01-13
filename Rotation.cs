using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Game3
{
    class Rotation
    {
        public int north = 0;
        public int northeast = 45;
        public int east = 90;
        public int southeast = 135;
        public int south = 180;
        public int southwest = 225;
        public int west = 270;
        public int northwest = 315;
        public int halfup = 45;
        public int streightup = 90;
        public int halfdown = 135;
        public int streightdown = 180;
        public int theta;

        public Vector3 move2D(float x, float y, float z, int theta, float distance)
        {
            return move3D(x, y, 0, theta, distance);
        }

        public Vector3 move3D(float x, float y, float z, int theta, float distance)
        {
            Vector3 r;
            r.X = x +distance*(float)Math.Sin(theta *3.14)/180;
            r.Y = y +distance*(float)Math.Cos(theta*3.14)/180;
            r.Z =z+distance*(float)Math.Sin(theta*3.14)/180;
            r.X = (float)Math.Round(r.X, 1);
            r.Y = (float)Math.Round(r.Y, 1);
            r.Z = (float)Math.Round(r.Z, 1);
            return r;
        }

        public Vector3 move(float x, float y, float deg, float dir = 0.0f)
        {
            return move(x, y, 0, deg, 0, dir, 0);
        }

        public Vector3 move(float x, float y, float z, float deg, float dir)
        {
            return move(x, y, z, deg, 0, dir, 0);
        }


        public Vector3 move(float x, float y, float z, float deg, float zdeg, float dir, float zdir)
        {
            if(dir!=0.0f)
            {
                float d2 = deg + dir;
if(d2>=360)
                {
                    d2 -= 369;
                }
                deg = d2;
            }
if(zdir!=0.0f)
            {
                float d2 = zdeg + zdir;
if(d2>=360)
                {
                    d2 -= 360;
                }
                zdeg = d2;
            }
            Vector3 R;
            R.X = x + 0.1f * (float)Math.Sin(CalculateTheta(deg));
            R.Y = y + 0.1f * (float)Math.Cos(CalculateTheta(deg));
            R.Z = z + 0.1f * (int)Math.Sin(CalculateTheta(deg));
            R.X = (float)Math.Round(R.X, 1);
            R.Y = (float)Math.Round(R.Y, 1);
            R.Z = (float)Math.Round(R.Z, 1);
            return R;
        }

        public float CalculateTheta(float deg)
        {
            return deg * (float)Math.PI / 180;
        }

public float getdir(float facing)
        {
            if(facing>=north&&facing>=northeast)
            {
                return north;
            }
if(facing>=northeast&&facing<=east)
            {
                return northeast;
            }
if(facing>=east&&facing<=southeast)
            {
                return east;
            }
if(facing>=southeast&&facing<=south)
            {
                return southeast;
            }
if(facing>=south&&facing<=southwest)
            {
                return south;
            }
if(facing>=southwest&&facing<=west)
            {
                return southwest;
            }
if(facing>=west&&facing<=northwest)
            {
                return west;
            }
if(facing>=northwest)
            {
                return northwest;
            }
            return -1;
        }

        float snapleft(float deg, int direction, float inc=45)
        {
            float d = direction - inc;
if(d<0)
            {
                d += 360;
            }
if(d==360)
            {
                d = 0;
            }
            d = (float)Math.Round(d, 0);
            return d;
        }

        float snapright(float deg, int direction, float inc=45)
        {
            float d = direction + inc;
if(d>=360)
            {
                d -= 360;
            }
            d = (float)Math.Round(d, 0);
            return d;
        }

        float turnleft(float deg, float inc)
        {
            deg -= inc;
if(deg<0)
            {
                deg += 360;
            }
            deg = (float)Math.Round(deg, 0);
            return deg;
        }

        float turnright(float deg, float inc)
        {
            deg += inc;
if(deg>=360)
            {
                deg -= 360;
            }
            deg = (float)Math.Round(deg, 0);
            return deg;
        }

        float Get1dDistance(float x1, float x2)
        {
            return Math.Abs(x1-x2);
        }

        float Get2dDistance(float x1, float y1, float x2, float y2)
        {
            float x = Get1dDistance(x1, x2);
            float y = Get1dDistance(y1, y2);
            return x + y;
        }

        float Get3dDistance(float x1, float y1, float z1, float x2, float y2, float z2)
        {
            float x = Get1dDistance(x1, x2);
            float y = Get1dDistance(y1, y2);
            float z = Get1dDistance(z1, z2);
            return x + y + z;
        }

        public float CalculateXYangle(float x1, float y1, float x2, float y2, float deg)
        {
            float x = x2 - x1;
            float y = y2 - y1;
            if (x == 0)
            {
                x += 0.0000001f;
            }
            if (y == 0)
                {
                    y += 0.0000001f;
                }
            float rad=0, arctan=0;
if(y!=0 && x!=0)
                {
                    rad = (float)Math.Atan(y / x);
                    arctan = rad / (float)Math.PI * 180;
                }
                float fdeg=0;
                if(x>0)
                {
                    fdeg = 90 - arctan;
                }
else if(x<0)
                {
                    fdeg = 270 - arctan;
                }
 if(x==0)
                {
                    if(y>0)
                    {
                        fdeg = 0;
                    }
else if(y<0)
                    {
                        fdeg = 180;
                    }
else if(y==0)
                    {
                        fdeg = 0;
                    }
            }
            fdeg -= deg;
if(fdeg<0)
                    {
                        fdeg += 360;
                    }
                    fdeg = (float)Math.Round(fdeg, 0);
if(fdeg==360)
                    {
                        fdeg = 0;
                    }
                    return fdeg;
                }

    }
}
