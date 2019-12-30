using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using TCP_Client;

namespace Colorbeam
{
    public class CbProcessor
    {


        private CrestronQueue<string> commandQueue;
        private CrestronQueue<string> responseQueue;
        private CTimer commandQueueTimer;
        private CTimer responseQueueTimer;
        private TCPClientDevice client;

        public bool debug;
        private int procId;
        public int getProcId { get { return this.procId; } }
        private string procIp = "";
        public string getProcIp { get { return this.procIp; } }
        private int procPort = 0;
        private bool isInitialized = false;
        private bool isConnected = false;
        public bool IsConnected { get { return this.isConnected; } }
        private bool initRun = false;

        internal Dictionary<int, CbKeypad> Keypads = new Dictionary<int, CbKeypad>();
        internal Dictionary<int, CbLoad> Loads = new Dictionary<int, CbLoad>();


        //Initialize
        public void Initialize(int _procId)
        {
            if (this.initRun)
                return;

            procId = _procId;

            //this.SendDebug(string.Format("Added and initialized Panel ", _panelId));

            //for (int i = 1; i <= 8; i++)
            //{
            //    if (!Areas.ContainsKey(i))
            //    {
            //        ElkArea a = new ElkArea();
            //        a.Initialize(this, i);
            //        Areas.Add(i, a);
            //    }
            //}
            //for (int i = 1; i <= 208; i++)
            //{
            //    if (!Zones.ContainsKey(i))
            //    {
            //        ElkZone z = new ElkZone();
            //        z.Initialize(this, i);
            //        Zones.Add(i, z);
            //    }
            //}
            //for (int i = 1; i <= 208; i++)
            //{
            //    if (!Outputs.ContainsKey(i))
            //    {
            //        ElkOutput o = new ElkOutput();
            //        o.Initialize(this, i);
            //        Outputs.Add(i, o);
            //    }
            //}

            commandQueue = new CrestronQueue<string>();
            responseQueue = new CrestronQueue<string>();


            this.initRun = true;
        }

        public void InitialzeConnection(string _host, ushort _port)
        {
            this.procIp = _host;
            this.procPort = _port;

            if (this.procIp.Length > 0)
            {
                this.SendDebug(string.Format("Initializing processor {0} connection @ {1}:{2} & initialize", procId, procIp, procPort));

                if (this.commandQueueTimer == null)
                    this.commandQueueTimer = new CTimer(CommandQueueDequeue, null, 0, 50);

                if (this.responseQueueTimer == null)
                    this.responseQueueTimer = new CTimer(ResponseQueueDequeue, null, 0, 50);

                this.client = new TCPClientDevice();
                this.client.ID = 1;
                this.client.ConnectionStatus += new StatusEventHandler(client_ConnectionStatus);
                this.client.ResponseString += new ResponseEventHandler(client_ResponseString);
                this.client.Connect(this.procIp, (ushort)this.procPort);
            }
        }

        public void InitializePanelParameters()
        {
            //this.Enqueue("as00"); //Arming status request
            this.isInitialized = true;
        }


        //Comms --------------------------------------------------------------
        private void CommandQueueDequeue(object o)
        {
            if (!commandQueue.IsEmpty)
            {
                var data = commandQueue.Dequeue();
                SendDebug(string.Format("Sending from queue: {0}", data));
                client.SendCommand(data + "\x0A");
            }
        }
        StringBuilder RxData = new StringBuilder();
        bool busy = false;
        int Pos = -1;
        private void ResponseQueueDequeue(object o)
        {
            if (!responseQueue.IsEmpty)
            {
                try
                {
                    // removes string from queue, blocks until an item is queued
                    string tmpString = responseQueue.Dequeue();

                    RxData.Append(tmpString); //Append received data to the COM buffer

                    if (!busy)
                    {
                        busy = true;
                        while (RxData.ToString().Contains("*"))
                        {
                            Pos = RxData.ToString().IndexOf("*");
                            var data = RxData.ToString().Substring(0, Pos);
                            var garbage = RxData.Remove(0, Pos + 1); // remove data from COM buffer
                            ParseInternalResponse(data);
                        }

                        busy = false;
                    }
                }
                catch (Exception e)
                {
                    busy = false;
                    ErrorLog.Exception(e.Message, e);
                }
            }
        }
        internal void Enqueue(string data)
        {
            commandQueue.Enqueue(data);
        }
        private void ParseResponse(string data)
        {
            try
            {
                SendDebug(string.Format("Received and adding to queue: {0}", data));
                responseQueue.Enqueue(data);
            }
            catch (Exception e)
            {
                ErrorLog.Error("Colorbeam Processor {0} - Parse error: {1}", procId, e.Message);
            }
        }
        void client_ResponseString(string response, int id)
        {
            ParseResponse(response);
        }
        void client_ConnectionStatus(int status, int id)
        {
            if (status == 2 && !isConnected)
            {
                this.isConnected = true;
                foreach (var item in SimplClients)
                {
                    item.Value.Fire(new SimplEventArgs(eElkSimplEventIds.IsConnected, "true", 1));
                }
                CrestronEnvironment.Sleep(1500);

                foreach (var item in SimplClients)
                {
                    item.Value.Fire(new SimplEventArgs(eElkSimplEventIds.IsRegistered, "true", 1));
                }
                this.InitializePanelParameters();
            }
            else if (isConnected && status != 2)
            {
                this.SendDebug("Colorbeam processor disconnected");
                this.isConnected = false;
                foreach (var item in SimplClients)
                {
                    item.Value.Fire(new SimplEventArgs(eElkSimplEventIds.IsRegistered, "false", 0));
                    item.Value.Fire(new SimplEventArgs(eElkSimplEventIds.IsConnected, "false", 0));
                }
            }
        }

        //Parsing
        private void ParseInternalResponse(string returnString)
        {
            if (returnString.Length <= 2)
                return;

            //Button State
            if (returnString.Contains("B"))
            {
                int bId = 0, bNum = 0;
                if (returnString.Length == 8) //assuming 2 char id, 2 char button
                {
                    bId = int.Parse(returnString.Substring(1, 2));
                    bNum = int.Parse(returnString.Substring(3, 2));
                }
                else if (returnString.Length == 9) //assuming 3 char id, 2 char button
                {
                    bId = int.Parse(returnString.Substring(1, 3));
                    bNum = int.Parse(returnString.Substring(4, 2));
                }
                else //assuming 3 char id, 3 char button
                {
                    bId = int.Parse(returnString.Substring(1, 3));
                    bNum = int.Parse(returnString.Substring(4, 3));
                }
                bool bSt = returnString.Contains("ON") ? true : false;
                if (Keypads.ContainsKey(bId))
                    Keypads[bId].internalSetButtonStatus(bNum, bSt);
            }

            //Loads
            if (returnString.Contains("L"))
            {
                //Tunable White L00001-3-255-000-100*
                //RGB           L00002-4-000-255-000-100*
                //RGBW          L00003-4-000-000-000-022*
                //W             L00004-4-255-000-000-000*
                //Processing Finished *L00001-9*

                string[] sections = returnString.Split('-');
                if (sections.Length > 1)
                {
                    int r = 0, g = 0, b = 0, ww = 0, cw = 0, level = 0;
                    int lId = int.Parse(sections[0]);
                    if (!Loads.ContainsKey(lId))
                        return;
                    switch (int.Parse(sections[1]))
                    {
                        case 9: //transition complete
                            Loads[lId].internalFadeComplete();
                            break;
                        case 1: //single channel
                            if (sections.Length < 2)
                                return;
                            level = int.Parse(sections[2]);
                            Loads[lId].internalSetCurrentOutputLevels(eLoadType.SingleChannel, 0, 0, 0, 0, 0, level);
                            break;
                        case 3: //variable white
                            if (sections.Length < 4)
                                return;
                            ww = int.Parse(sections[2]);
                            cw = int.Parse(sections[3]);
                            level = int.Parse(sections[4]);
                            Loads[lId].internalSetCurrentOutputLevels(eLoadType.VariableWhite, 0, 0, 0, ww, cw, level);
                            break;
                        case 4: //RGB
                            if (sections.Length < 5)
                                return;
                            r = int.Parse(sections[2]);
                            g = int.Parse(sections[3]);
                            b = int.Parse(sections[4]);
                            level = int.Parse(sections[5]);
                            Loads[lId].internalSetCurrentOutputLevels(eLoadType.RGB, r, g, b, 0, 0, level);
                            break;
                        case 5: //RGBW
                            if (sections.Length < 6)
                                return;
                            r = int.Parse(sections[2]);
                            g = int.Parse(sections[3]);
                            b = int.Parse(sections[4]);
                            cw = int.Parse(sections[5]);
                            level = int.Parse(sections[6]);
                            Loads[lId].internalSetCurrentOutputLevels(eLoadType.RGBW, r, g, b, 0, cw, level);
                            break;
                    }
                }
            }
        }

        //Simpl
        internal Dictionary<string, SimplEvents> SimplClients = new Dictionary<string, SimplEvents>();
        public bool RegisterSimplClient(string _id)
        {
            try
            {
                lock (SimplClients)
                {
                    if (!SimplClients.ContainsKey(_id))
                    {
                        this.SimplClients.Add(_id, new SimplEvents());
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                ErrorLog.Error("Colorbeam Processor {0} - Simple client registration error: {1}", procId, e.Message);
                return false;
            }
        }


        //Utility
        public void SetDebug(bool _value)
        {
            this.debug = _value;
            SendDebug("Enabing debug");
        }
        public void SendDebug(string msg)
        {
            if (debug)
                CrestronConsole.PrintLine(String.Format("Colorbeam Processor {0}: {1}", procId, msg));
        }

    }


    //Simpl
    public class SimplEvents
    {
        private event EventHandler<SimplEventArgs> onNewEvent = delegate { };

        public event EventHandler<SimplEventArgs> OnNewEvent
        {
            add
            {
                if (!onNewEvent.GetInvocationList().Contains(value))
                {
                    onNewEvent += value;
                }
            }
            remove
            {
                onNewEvent -= value;
            }
        }

        internal void Fire(SimplEventArgs e)
        {
            onNewEvent(null, e);
        }
    }

    public class SimplEventArgs : EventArgs
    {
        public SimplSharpString StringData;
        public ushort IntData;
        public eElkSimplEventIds ID;

        public SimplEventArgs(eElkSimplEventIds id, SimplSharpString stringData, ushort intData)
        {
            this.StringData = stringData;
            this.IntData = intData;
            this.ID = id;
        }
    }

    public enum eElkSimplEventIds
    {
        IsRegistered = 1,
        IsConnected = 2
    }
}