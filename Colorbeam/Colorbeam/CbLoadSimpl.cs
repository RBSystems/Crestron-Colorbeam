using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace Colorbeam
{
    public class CbLoadSimpl
    {
        public delegate void LoadTypeChange(ushort _loadType);
        public LoadTypeChange newLoadTypeChange { get; set; }
        public delegate void LoadLevelChange(ushort _r, ushort _g, ushort _b, ushort _ww, ushort _cw, ushort _level);
        public LoadLevelChange newLoadLevelChange { get; set; }
        public delegate void FadeCompletionChange();
        public FadeCompletionChange newFadeCompletionChange { get; set; }
        
        private int r = 0, g = 0, b = 0, ww = 0, cw = 0, level = 0;
        private bool sendEnable = false;
        private bool changed;

        private CbProcessor myProc;
        private CbLoad myLoad;

        //Init -------------------------------------------------------
        public void Initialize(ushort _procId, ushort _integrationId)
        {
            myProc = CbCore.AddOrGetProcessorObject(_procId);
            if (myProc == null)
            {
                ErrorLog.Error("Keypad for processor {0} at ID {1} can't be initialized. Make sure processor module is initiated first.");
                return;
            }

            if (myProc.Loads.ContainsKey((int)_integrationId))
            {
                myLoad = myProc.Loads[(int)_integrationId];
            }
            else
            {
                myLoad = new CbLoad();
                myLoad.Initialize(myProc, _integrationId);
                myProc.Loads.Add(_integrationId, myLoad);
            }
            myLoad.CbLoadEvent += new EventHandler<CbLoadEventArgs>(myLoad_CbLoadEvent);
        }

        //Public Functions -------------------------------------------------------
        public void SetSubscribe(ushort _state)
        {
            if (_state == 1)
                myLoad.SetSubscribe(true);
            else
                myLoad.SetSubscribe(false);
        }
        public void SetChannelR(ushort _r)
        {
            r = _r;
            changed = true;
            checkSend();
        }
        public void SetChannelG(ushort _g)
        {
            g = _g;
            changed = true;
            checkSend();
        }
        public void SetChannelB(ushort _b)
        {
            b = _b;
            changed = true;
            checkSend();
        }
        public void SetChannelWW(ushort _ww)
        {
            ww = _ww;
            changed = true;
            checkSend();
        }
        public void SetChannelCW(ushort _cw)
        {
            cw = _cw;
            changed = true;
            checkSend();
        }
        public void SetChannelL(ushort _level)
        {
            level = _level;
            changed = true;
            checkSend();
        }
        public void SetSendEnable(ushort _state)
        {
            if (_state == 1)
            {
                sendEnable = true;
                checkSend();
            }
            else
                sendEnable = false;
        }
        public void SetFade(ushort _fadeTime)
        {
            myLoad.SetFadeTime((int)_fadeTime);
        }


        //Private Functions -------------------------------------------------------
        private void checkSend()
        {
            if (sendEnable && changed)
            {
                int tR = (int)scale(r, 0, 65535, 0, 255);
                int tG = (int)scale(g, 0, 65535, 0, 255);
                int tB = (int)scale(b, 0, 65535, 0, 255);
                int tWW = (int)scale(ww, 0, 65535, 0, 255);
                int tCW = (int)scale(cw, 0, 65535, 0, 255);
                int tL = (int)scale(level, 0, 65535, 0, 100);

                switch (myLoad.GetLoadType)
                {
                    case eLoadType.SingleChannel:
                        myLoad.SendLevelChange(tL);
                        break;
                    case eLoadType.VariableWhite:
                        myLoad.SendColorChange(tL, tWW, tCW);
                        break;
                    case eLoadType.RGB:
                        myLoad.SendColorChange(tL, tR, tG, tB);
                        break;
                    case eLoadType.RGBW:
                        myLoad.SendColorChange(tL, tR, tG, tB, tCW);
                        break;
                }
            }
        }

        //Utilities -------------------------------------------------------
        private double scale(double A, double A1, double A2, double Min, double Max)
        {
            double percentage = (A - A1) / (A1 - A2);
            return (percentage) * (Min - Max) + Min;
        }
        

        //Events -------------------------------------------------------
        void myLoad_CbLoadEvent(object sender, CbLoadEventArgs e)
        {
            switch (e.EventUpdateType)
            {
                case eCbLoadEventUpdateType.LoadType:
                    if (newLoadTypeChange != null)
                        newLoadTypeChange((ushort)e.LoadType);
                    break;
                case eCbLoadEventUpdateType.LevelChange:
                    if (newLoadLevelChange != null)
                    {
                        r = (int)scale(e.Red, 0, 255,0, 65535);
                        g = (int)scale(e.Green, 0, 255, 0, 65535);
                        b = (int)scale(e.Blue, 0, 255, 0, 65535);
                        ww = (int)scale(e.WarmWhite, 0, 255, 0, 65535);
                        cw = (int)scale(e.CoolWhite, 0, 255, 0, 65535);
                        level = (int)scale(e.Level, 0, 100, 0, 65535);
                        newLoadLevelChange((ushort)r, (ushort)g, (ushort)b, (ushort)ww, (ushort)cw, (ushort)level);
                    }
                    break;
                case eCbLoadEventUpdateType.FadeCompletion:
                    newFadeCompletionChange();
                    break;
            }
        }
    }
}