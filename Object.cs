using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using DavyKager;
namespace Game3
{
    public class Object
    {
public Map Map
        {
            get { return map; }
        }
            Map map;
        public int x, y, z;
        public string name;
        public bool IsInteractable;
        public Object(Map map, int ox, int oy, int oz, string oname, bool interactable=false)
        {
            this.map = map;
            this.x = ox;
            this.y = oy;
            this.z = oz;
            this.name = oname;
            this.IsInteractable = interactable;
            map.engine.Play3D("sounds/rooms/" + oname + ".mp3", ox, oy, oz, true);
        }

        public void Update(GameTime gameTime)
        {
        }
        
        public void interact()
        {
            map.activeOverWindow = new DialogBox(this.map, $"hola, soy un lindo {this.name} ");
        }

    }
}
