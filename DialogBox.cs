using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IrrKlang;
using DavyKager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Game3
{
    public  class DialogBox
    {
public  Map Map
        {
            get { return map; }
        }
         Map map;
        public ISoundEngine engine = new ISoundEngine();
        public bool IsInDialog;
        KeyboardState keystate = Keyboard.GetState();

        public DialogBox(Map map)
        {
            this.map = map;
        }

        public void dlg(string message)
        {
            Tolk.Speak(message, true);
            engine.Play2D("sounds/UI/menuconfirm.mp3");
            while (true)
            {
                if (Input.WasKeyPressed(Keys.E))
                {
                    Tolk.Speak(message, true);
                }
                 if (Input.WasKeyPressed(Keys.Enter))
                {
                    engine.Play2D("sounds/UI/menuback.mp3");
                    break;
                        }
            }
        }


    }
}
