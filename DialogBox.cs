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
        private string[] parts;
        private bool firstCall;
        private int index = 0;


        public DialogBox(Map map, string content)
        {
            this.map = map;
            this.content = content;
            parts = content.Split('\n');
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
                Tolk.Speak(this.parts[index], true);
                engine.Play2D("sounds/UI/menuconfirm.mp3");
                this.firstCall = false;
            }

                if (Input.WasKeyPressed(Keys.E))
                {
                    Tolk.Speak(this.parts[index], true);
                }
            if (Input.WasKeyPressed(Keys.Enter))
            {
                if (index < parts.Length-1)
                {
                    index+=1;
                    Tolk.Speak(parts[index], true);
                    engine.Play2D("sounds/UI/menumove.mp3");
                }
                else
                {
                    Tolk.Speak("dieron enter");
                    engine.Play2D("sounds/UI/menuback.mp3");
                    this.IsInDialog = false;
                }
            }           
        }


    }
}
