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
        private eLoadType loadType = new eLoadType();


        private CbProcessor myProc;


        //Init -------------------------------------------------------
        public void Initialize(CbProcessor _proc, int _integrationId)
        {
            myProc = _proc;
            integrationId = _integrationId;
        }


        //Public Functions -------------------------------------------------------
        



        //Core internal -------------------------------------------------------
        internal void internalSetCurrentOutputLevels(eLoadType _loadType, int _r, int _g, int _b, int _ww, int _cw, int _level)
        {
            if (loadType == _loadType)
            {
                OnCbLoadEvent(eCbLoadEventUpdateType.LoadType, _loadType);
            }
            if (red == _r || green == _g || blue == _b || warmWhite == _ww || coolWhite == _cw || level == _level)
            {
                red = _r;
                green = _g;
                blue = _b;
                warmWhite = _ww;
                coolWhite = _cw;
                level = _level;
                loadType = _loadType;
                OnCbLoadEvent(eCbLoadEventUpdateType.LevelChange, red, green, blue, warmWhite, coolWhite, level);
            }
        }
        internal void internalFadeComplete()
        {

        }


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
        LevelChange = 1
    }
}