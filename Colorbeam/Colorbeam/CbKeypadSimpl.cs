using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace Colorbeam
{
    public class CbKeypadSimpl
    {

        public delegate void ButtonStateChange(ushort _button, ushort _state);
        public ButtonStateChange newButtonStateChange { get; set; }

        private CbProcessor myProc;
        private CbKeypad myKp;

        //Init -------------------------------------------------------
        public void Initialize(ushort _procId, ushort _integrationId)
        {
            myProc = CbCore.AddOrGetProcessorObject(_procId);
            if (myProc == null)
            {
                ErrorLog.Error("Keypad for processor {0} at ID {1} can't be initialized. Make sure processor module is initiated first.");
                return;
            }

            if (myProc.Keypads.ContainsKey((int)_integrationId))
            {
                myKp = myProc.Keypads[(int)_integrationId];
            }
            else
            {
                myKp = new CbKeypad();
                myKp.Initialize(myProc, _integrationId);
                myProc.Keypads.Add(_integrationId, myKp);
            }
            myKp.CbKeypadEvent += new EventHandler<CbKeypadEventArgs>(myKp_CbKeypadEvent);
        }

        

        //Public Functions -------------------------------------------------------
        public void ButtonPress(ushort _button)
        {
            myKp.ButtonPress(_button);
        }
        public void ButtonRelease(ushort _button)
        {
            myKp.ButtonRelease(_button);
        }


        //Events -------------------------------------------------------
        void myKp_CbKeypadEvent(object sender, CbKeypadEventArgs e)
        {
            switch (e.EventUpdateType)
            {
                case eCbKeypadEventUpdateType.ButtonStateChange:
                    if (newButtonStateChange != null)
                        newButtonStateChange((ushort)e.Button, e.State ? (ushort)1 : (ushort)0);
                    break;
            }
        }
    }
}