using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Game3
{
    public static class Input
    {
        public static KeyboardState keystate, lastKeystate;

        public static void Update()
        {
            lastKeystate = keystate;
            keystate = Keyboard.GetState();
        }


        public static bool WasKeyPressed(Keys key)
        {
            return lastKeystate.IsKeyUp(key) && keystate.IsKeyDown(key);
        }

    }
}
