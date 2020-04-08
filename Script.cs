using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game3
{
    public class Script
    {
public enum Stages
        {
            loop1,
            loop2,
            loop3,
        }
        public static Stages currentStage;
public Map Map
        {
            get { return map; }
        }
        Map map;

        public Script(Map map)
        {
            this.map = map;
        }

        public void DialogEventHandler(object sender, GameEventArgs e, bool parameter)
        {
            Console.WriteLine("diálogo se cerró");
            CheckMissionState();
        }

        public void CheckMissionState()
        {
            if (currentStage == Stages.loop1)
            {
                if (map.obj[0].IsInteracting == false && GameEventArgs.inDialog == false)
                {
                    currentStage = Stages.loop2;
                }
            }
            else if (currentStage == Stages.loop2)
                {
                DialogBox.InDialogEvent += DialogEventHandler;
                map.obj[0].breakObject();
                map.activeOverWindow = new DialogBox(this.map, "como? \n se rompió la ventana!");
                currentStage = Stages.loop3;
                DialogBox.InDialogEvent -= DialogEventHandler;
            }
        }
        }
    }