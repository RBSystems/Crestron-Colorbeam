using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace Colorbeam
{
    public class CbProcessorSimpl
    {
        private bool isRegistered;
        private bool debug;

        CbProcessor myProc;

        public delegate void IsRegistered(ushort value);
        public delegate void IsConnected(ushort value);
        public IsRegistered onIsRegistered { get; set; }
        public IsConnected onIsConnected { get; set; }


        public void Initialize(ushort _procId, SimplSharpString _host, ushort _port)
        {
            myProc = CbCore.AddOrGetProcessorObject(_procId);
            if (myProc.getProcIp.Length == 0)
            {
                myProc.SetDebug(debug);
                myProc.InitialzeConnection(_host.ToString(), _port);
                myProc.RegisterSimplClient(Convert.ToString(_procId));
                myProc.SimplClients[Convert.ToString(_procId)].OnNewEvent += new EventHandler<SimplEventArgs>(Cb_SimplEvent);
                this.isRegistered = true;
            }
            else
            {
                ErrorLog.Error("Colorbeam processor with ID {0} already exists - please only use one panel instance per panel within project.", _procId);
            }
        }


        public void SetDebug(ushort _value)
        {
            this.debug = Convert.ToBoolean(_value);
            if (isRegistered)
                this.myProc.SetDebug(debug);
        }



        void Cb_SimplEvent(object sender, SimplEventArgs e)
        {
            switch (e.ID)
            {
                case eElkSimplEventIds.IsRegistered:
                    if (onIsRegistered != null)
                        onIsRegistered(e.IntData);
                    break;
                case eElkSimplEventIds.IsConnected:
                    if (onIsConnected != null)
                        onIsConnected(e.IntData);
                    break;
                default:
                    break;
            }
        }

    }
}