using System;
using Crestron.SimplSharp;                          				// For Basic SIMPL# Classes
using System.Collections.Generic;
using Crestron.SimplSharp.CrestronIO;



namespace Colorbeam
{
    public class CbCore
    {
        internal static Dictionary<int, CbProcessor> Processors = new Dictionary<int, CbProcessor>();
        private static string ProgramID = Directory.GetApplicationDirectory();
        public static string progslot = ProgramID.Substring(ProgramID.Length - 2);
        //private bool isDisposed;

        public static CbProcessor AddOrGetCoreObject(int _procId)
        {
            try
            {
                lock (CbCore.Processors)
                {
                    if (CbCore.Processors.ContainsKey(_procId))
                        return CbCore.Processors[_procId];
                    else
                    {
                        CbProcessor proc = new CbProcessor();
                        proc.Initialize(_procId);
                        CbCore.Processors.Add(_procId, proc);
                        return CbCore.Processors[_procId];
                    }
                }
            }
            catch (Exception e)
            {
                ErrorLog.Error("Program {0} - Colorbeam Core: Couldn't add Processor {1} - {2}", (object)progslot, (object)_procId, (object)e.Message);
                return (CbProcessor)null;
            }
        }
        
    }
}
