using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Game3
{
    public static class KeyboardExtender
    {
        public static KeyboardState oldKeystate;

        public static bool isKeyPress(this KeyboardState state, Keys checkKey)
        {
            if (!oldKeystate.IsKeyDown(checkKey) && state.IsKeyDown(checkKey))
            {
                return (true);
            }
            return (false);
        }
    }
}
