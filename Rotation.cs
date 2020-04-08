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
        public float north = 0;
        public float northeast = 45;
        public float east = 90;
        public float southeast = 135;
        public float south = 180;
        public float southwest = 225;
        public float west = 270;
        public float northwest = 315;
        public float halfup = 45;
        public float streightup = 90;
        public float halfdown = 135;
        public float streightdown = 180;
        public float angle;

public Vector3 move(Vector3 coords, float deg, float dir, float pich=0.0f, float factor=0.1f)
        {
            if(dir!=0.0f)
            {
                float d2 = deg + dir;
                if (d2 >= 360)
                {
                    d2 -= 360;
                }
                deg = d2;
            }
            float x = coords.X;
            float y = coords.Y;
            float z = coords.Z;
            float steplength = factor * (float)Math.Cos(CalculateRadians(pich));
            Vector3 r;
            r.X = x + steplength * (float)Math.Sin(CalculateRadians(deg));
            r.Y = y + factor * (float)Math.Sin(CalculateRadians(pich));
            r.Z = z + steplength * (float)Math.Cos(CalculateRadians(deg));
            r.X = (float)Math.Round(r.X, 1);
            r.Y = (float)Math.Round(r.Y, 1);
            r.Z = (float)Math.Round(r.Z, 1);
            return r;
        }

        public float CalculateRadians(float deg)
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
            return d;
        }

        public float turnleft(float deg, float inc=5)
        {
            deg -= inc;
if(deg<0)
            {
                deg += 360;
            }
            return deg;
        }

        public float turnright(float deg, float inc=5)
        {
            deg += inc;
if(deg>=360)
            {
                deg -= 360;
            }
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
