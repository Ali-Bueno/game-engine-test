using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using DavyKager;
using tfj.exploudEngine;

namespace Game3
{
    public class Object
    {
        public Map Map
        {
            get { return map; }
        }
        Map map;
        public float x, y, z;
        eSound loop;
        eSound breaksound;
        eInstance instancia;
        eInstance breakInstance;
        public string name;
        public bool IsInteractable;
        public bool IsInteracting=true;
        public Object(Map map, float ox, float oy, float oz, string oname, bool interactable = false)
        {
            this.map = map;
            this.x = ox;
            this.y = oy;
            this.z = oz;
            this.name = oname;
            this.IsInteractable = interactable;
            this.loop = Game1.fmodengine.loadSound("sounds/" + oname + "/loop.mp3");
            this.breaksound = Game1.fmodengine.loadSound("sounds/" + oname + "/break.mp3");
            this.instancia = this.loop.play3d(x, y, z, loopMode.simpleLoop);
            this.instancia.minDistance = 1.0f;
            this.instancia.maxDistance = 75;
        }

        public void Update(GameTime gameTime)
        {
        }

        public void interact()
        {
            if (this.IsInteractable == true)
            {
                IsInteracting = true;
                map.activeOverWindow = new DialogBox(this.map, $"hola, soy un lindo {this.name} \n y ahora, adióo. xd");
                if (map.activeOverWindow.finished())
                {
                    IsInteracting = false;
                }
            }
            else if(this.IsInteractable==false)
            {
                return;
            }
        }

        public void breakObject()
            {
            this.breakInstance = breaksound.play3d(this.x, this.y, this.z, loopMode.noLoop);
            this.breakInstance.minDistance = 1.0f;
            this.breakInstance.maxDistance = 75f;
            this.IsInteractable = false;
    }
    }
}
