using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DavyKager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using tfj.exploudEngine;

namespace Game3
{
    public class DialogBox : iOverWindow
    {
        public Map Map
        {
            get { return map; }
        }
        Map map;
        public delegate void EventPiloth<TEventArgs>(object sender, TEventArgs e, bool parameter);
        public static event EventPiloth<GameEventArgs> InDialogEvent;

        private eSound InDialogSound;
        private eSound NextDialogSound;
        private eSound OutDialogSound;
        private eInstance InDialogInstance;
        private eInstance NextDialogInstance;
        private eInstance OutDialogInstance;
        public bool IsInDialog;
        private string content;
        private string[] parts;
        private bool firstCall;
        private int index = 0;


        public DialogBox(Map map, string content)
        {
            this.map = map;
            InDialogSound = Game1.fmodengine.loadSound("sounds/UI/menuconfirm.mp3");
            NextDialogSound = Game1.fmodengine.loadSound("sounds/UI/menumove.mp3");
            OutDialogSound = Game1.fmodengine.loadSound("sounds/UI/menuback.mp3");
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
                InDialogInstance = InDialogSound.play(0, loopMode.noLoop);
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
                    NextDialogInstance = NextDialogSound.play(0, loopMode.noLoop);
                }
                else
                {
                    OutDialogInstance = OutDialogSound.play(0, loopMode.noLoop);
                    this.IsInDialog = false;
                    OnInDialog();
                }
            }           
        }

        public void OnInDialog()
        {
            if(this.IsInDialog==false &&InDialogEvent!=null)
            {
                InDialogEvent(this, new GameEventArgs(), GameEventArgs.inDialog=false);
            }
        }

    }
}
