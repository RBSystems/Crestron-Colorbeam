using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace Colorbeam
{
    public class CbKeypad
    {
        private int integrationId;
        private bool isRegistered;

        private CbProcessor myProc;

        internal Dictionary<int, bool> Buttons = new Dictionary<int, bool>();

        //Init -------------------------------------------------------
        public void Initialize(CbProcessor _proc, int _integrationId)
        {
            myProc = _proc;
            integrationId = _integrationId;
        }


        //Public Functions -------------------------------------------------------
        public void ButtonPress(int _button)
        {
            checkIfButtonExists(_button);
            string cmdStr = string.Format("{0:00}{1:00}P", integrationId, _button);
            myProc.SendDebug(string.Format("Keypad {0} - ButtonPress = {1}", integrationId, cmdStr));
            myProc.Enqueue(cmdStr);
        }
        public void ButtonRelease(int _button)
        {
            checkIfButtonExists(_button);
            string cmdStr = string.Format("{0:00}{1:00}R", integrationId, _button);
            myProc.SendDebug(string.Format("Keypad {0} - ButtonRelease = {1}", integrationId, cmdStr));
            myProc.Enqueue(cmdStr);
        }



        //Core internal -------------------------------------------------------
        internal void internalSetButtonStatus(int _button, bool _state)
        {
            checkIfButtonExists(_button);
            Buttons[_button] = _state;
            myProc.SendDebug(string.Format("Keypad {0} - internalSetButtonStatus = Button:{1} State: {2}", integrationId, _button, _state));
            OnCbKeypadEvent(eCbKeypadEventUpdateType.ButtonStateChange, _button, _state);
        }


        //Private Functions -------------------------------------------------------
        private void checkIfButtonExists(int _button)
        {
            if (!Buttons.ContainsKey(_button))
            {
                bool b = new bool();
                b = false;
                Buttons.Add(_button, b);
            }
        }

        //Events -------------------------------------------------------
        public event EventHandler<CbKeypadEventArgs> CbKeypadEvent;
        protected virtual void OnCbKeypadEvent(eCbKeypadEventUpdateType updateType)
        {
            if (CbKeypadEvent != null)
                CbKeypadEvent(this, new CbKeypadEventArgs() { EventUpdateType = updateType });
        }
        protected virtual void OnCbKeypadEvent(eCbKeypadEventUpdateType updateType, int _button, bool _state)
        {
            if (CbKeypadEvent != null)
                CbKeypadEvent(this, new CbKeypadEventArgs() { EventUpdateType = updateType, Button = _button, State = _state });
        }
    }

    //Events -------------------------------------------------------
    public class CbKeypadEventArgs : EventArgs
    {
        public eCbKeypadEventUpdateType EventUpdateType { get; set; }
        public int Button { get; set; }
        public bool State { get; set; }
    }
    public enum eCbKeypadEventUpdateType
    {
        ButtonStateChange = 0
    }
}