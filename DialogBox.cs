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
    public  class DialogBox : iOverWindow
    {
public  Map Map
        {
            get { return map; }
        }
         Map map;
        public ISoundEngine engine = new ISoundEngine();
        public bool IsInDialog;
        private string content;
        private bool firstCall;
        KeyboardState keystate = Keyboard.GetState();

        public DialogBox(Map map, string content)
        {
            this.map = map;
            this.content = content;
            this.firstCall = true;
        }

        public bool finished()
        {
            return (!this.IsInDialog);
        }

        public void update(GameTime gt)
        {
            if(this.firstCall)
            {
                this.IsInDialog = true;
                Tolk.Speak(this.content, true);
                engine.Play2D("sounds/UI/menuconfirm.mp3");
                this.firstCall = false;
            }

                if (Input.WasKeyPressed(Keys.E))
                {
                    Tolk.Speak(this.content, true);
                }
                 if (Input.WasKeyPressed(Keys.Enter))
                {
                    Tolk.Speak("dieron enter");
                    engine.Play2D("sounds/UI/menuback.mp3");
                this.IsInDialog = false;
                        }
            
        }


    }
}
