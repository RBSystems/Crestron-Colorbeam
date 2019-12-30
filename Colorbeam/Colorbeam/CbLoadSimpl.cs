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
        

        private CbProcessor myProc;
        private CbLoad myLoad;

        //Init -------------------------------------------------------
        public void Initialize(ushort _procId, ushort _integrationId)
        {
            myProc = CbCore.AddOrGetCoreObject(_procId);
            if (myProc == null)
                return;

            if (myProc.Loads.ContainsKey((int)_integrationId))
            {
                myLoad = myProc.Loads[(int)_integrationId];
                myLoad.CbLoadEvent += new EventHandler<CbLoadEventArgs>(myLoad_CbLoadEvent);
            }
        }

        //Public Functions -------------------------------------------------------


        //Private Functions -------------------------------------------------------
        private double scale(double A, double A1, double A2, double Min, double Max)
        {
            double percentage = (A - A1) / (A1 - A2);
            return (percentage) * (Min - Max) + Min;
        }
        //private double clamp(double _in, double _min, double _max)
        //{
        //    double newVal;
        //    if (_in > _max)
        //        newVal = _max;
        //    else if (_in < _min)
        //        newVal = _min;
        //    else
        //        newVal = _in;
        //    return newVal;
        //}

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
                        newLoadLevelChange((ushort)scale(e.Red, 0, 255,0,65535),
                            (ushort)scale(e.Green,0,255,0,65535),
                            (ushort)scale(e.Blue,0,255,0,65535),
                            (ushort)scale(e.WarmWhite,0,255,0,65535),
                            (ushort)scale(e.CoolWhite,0,255,0,65535),
                            (ushort)scale(e.Level,0,100,0,65535)
                            );
                    }
                        
                    break;
            }
        }
    }
}