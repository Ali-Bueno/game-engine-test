using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Game3
{
    public interface iOverWindow
    {
        Map Map
        {
            get;
        }
        void update(GameTime gameTime);
        bool finished();
    }
}
