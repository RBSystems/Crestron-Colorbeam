using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace Colorbeam
{
    public class CbLoad
    {
        private int integrationId;
        private bool isRegistered;

        private int red = 0, green = 0, blue = 0, warmWhite = 0, coolWhite = 0, level = 0;
        private int fadeTime = 0;
        private eLoadType loadType = new eLoadType();

        private CbProcessor myProc;


        //Init -------------------------------------------------------
        public void Initialize(CbProcessor _proc, int _integrationId)
        {
            myProc = _proc;
            integrationId = _integrationId;
        }


        //Public Functions -------------------------------------------------------
        public void SendLevelChange(int _level)
        {
            string cmdStr = "";
            level = (int)clamp(_level, 0, 100);
            if (loadType == eLoadType.VariableWhite)
            {
                cmdStr = string.Format("load_id-{0}-{1}-{2}-{3}-{4}", integrationId, level, warmWhite, coolWhite, fadeTime);
            }
            else
            {
                cmdStr = string.Format("load_id-{0}-{1}-XXXXXX-{2}-{3}", integrationId, level, coolWhite, fadeTime);
            }
            myProc.SendDebug(string.Format("Load {0} - SendLevelChange = {1}", integrationId, cmdStr));
            myProc.Enqueue(cmdStr);
        }
        public void SendColorChange(int _level)
        {
            string cmdStr = "";
            level = (int)clamp(_level, 0, 100);
            cmdStr = string.Format("load_id-{0}-{1}-XXXXXX-{2}-{3}", integrationId, level, coolWhite, fadeTime);
            myProc.SendDebug(string.Format("Load {0} - SendLevelChange(w) = {1}", integrationId, cmdStr));
            myProc.Enqueue(cmdStr);
        }
        public void SendColorChange(int _level, int _warmWhite, int _coolWhite)
        {
            string cmdStr = "";
            level = (int)clamp(_level, 0, 100);
            warmWhite = (int)clamp(_warmWhite, 0, 255);
            coolWhite = (int)clamp(_coolWhite, 0, 255);
            cmdStr = string.Format("load_id-{0}-{1}-{2}-{3}-{4}", integrationId, level, warmWhite, coolWhite, fadeTime);
            myProc.SendDebug(string.Format("Load {0} - SendLevelChange(vw) = {1}", integrationId, cmdStr));
            myProc.Enqueue(cmdStr);
        }
        public void SendColorChange(int _level, int _red, int _green, int _blue)
        {
            string cmdStr = "";
            level = (int)clamp(_level, 0, 100);
            red = (int)clamp(_red, 0, 255);
            green = (int)clamp(_green, 0, 255);
            blue = (int)clamp(_blue, 0, 255);
            cmdStr = string.Format("load_id-{0}-{1}-{2:XX}{3:XX}{4:XX}-0-{6}", integrationId, level, red, green, blue, fadeTime);
            myProc.SendDebug(string.Format("Load {0} - SendLevelChange(rgb) = {1}", integrationId, cmdStr));
            myProc.Enqueue(cmdStr);
        }
        public void SendColorChange(int _level, int _red, int _green, int _blue, int _white)
        {
            string cmdStr = "";
            level = (int)clamp(_level, 0, 100);
            red = (int)clamp(_red, 0, 255);
            green = (int)clamp(_green, 0, 255);
            blue = (int)clamp(_blue, 0, 255);
            coolWhite = (int)clamp(_white, 0, 255);
            cmdStr = string.Format("load_id-{0}-{1}-{2:XX}{3:XX}{4:XX}-{5}-{6}", integrationId, level, red, green, blue, coolWhite, fadeTime);
            myProc.SendDebug(string.Format("Load {0} - SendLevelChange(rgbw) = {1}", integrationId, cmdStr));
            myProc.Enqueue(cmdStr);
        }
        public void SetFadeTime(int _fadeTime)
        {
            fadeTime = _fadeTime;
        }
        public eLoadType GetLoadType { get { return loadType; } }

        //Core internal -------------------------------------------------------
        internal void internalSetCurrentStatus(eLoadType _loadType, int _r, int _g, int _b, int _ww, int _cw, int _level)
        {
            if (loadType != _loadType)
            {
                loadType = _loadType;
                myProc.SendDebug(string.Format("Load {0} - internalSetCurrentStatus - Load Type {1}", integrationId, loadType));
                OnCbLoadEvent(eCbLoadEventUpdateType.LoadType, _loadType);
            }
            if (red != _r || green != _g || blue != _b || warmWhite != _ww || coolWhite != _cw || level != _level)
            {
                red = _r;
                green = _g;
                blue = _b;
                warmWhite = _ww;
                coolWhite = _cw;
                level = _level;
                myProc.SendDebug(string.Format("Load {0} - internalSetCurrentStatus - L:{1} Channels:{2},{3},{4},{5},{6}", integrationId, _level, _r, _g, _b, _ww, _cw));
                OnCbLoadEvent(eCbLoadEventUpdateType.LevelChange, red, green, blue, warmWhite, coolWhite, level);
            }
        }
        internal void internalFadeComplete()
        {
            myProc.SendDebug(string.Format("Load {0} - internalFadeComplete", integrationId));
            OnCbLoadEvent(eCbLoadEventUpdateType.FadeCompletion, loadType);
        }


        //Private internal -------------------------------------------------------



        //Events -------------------------------------------------------
        public event EventHandler<CbLoadEventArgs> CbLoadEvent;
        protected virtual void OnCbLoadEvent(eCbLoadEventUpdateType _updateType, eLoadType _loadType)
        {
            if (CbLoadEvent != null)
                CbLoadEvent(this, new CbLoadEventArgs() { EventUpdateType = _updateType, LoadType = _loadType });
        }
        protected virtual void OnCbLoadEvent(eCbLoadEventUpdateType _updateType, int _r, int _g, int _b, int _ww, int _cw, int _level)
        {
            if (CbLoadEvent != null)
                CbLoadEvent(this, new CbLoadEventArgs() { EventUpdateType = _updateType, Red = _r, Green = _g, Blue = _b, WarmWhite = _ww, CoolWhite = _cw, Level = _level });
        }


        //Utilities -------------------------------------------------------
        private double clamp(double _in, double _min, double _max)
        {
            double newVal;
            if (_in > _max)
                newVal = _max;
            else if (_in < _min)
                newVal = _min;
            else
                newVal = _in;
            return newVal;
        }


    }

    //Enum's -------------------------------------------------------
    public enum eLoadType
    {
        SingleChannel = 0,
        VariableWhite = 1,
        RGB = 2,
        RGBW = 3
    }

    //Events -------------------------------------------------------
    public class CbLoadEventArgs : EventArgs
    {
        public eCbLoadEventUpdateType EventUpdateType { get; set; }
        public int Red { get; set; }
        public int Green { get; set; }
        public int Blue { get; set; }
        public int WarmWhite { get; set; }
        public int CoolWhite { get; set; }
        public int Level { get; set; }
        public eLoadType LoadType { get; set; }
    }
    public enum eCbLoadEventUpdateType
    {
        LoadType = 0,
        LevelChange = 1,
        FadeCompletion = 2
    }
}