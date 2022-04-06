﻿using System.Diagnostics;
using System.IO;

namespace BrewLib.Util
{
    public class TraceLogger : TraceListener
    {
        private string path;
        private static object logLock = new object();

        public TraceLogger(string path)
        {
            this.path = path;
            Trace.Listeners.Add(this);
        }

        public override void Write(string message) => log(message);
        public override void WriteLine(string message) => log(message);
        public override void WriteLine(string message, string category) => log(message, category);

        private void log(string message, string category = null)
        {
            var path = this.path;
            if (category != null)
            {

                return;
            }

            try
            {
                lock (logLock)
                    File.AppendAllText(path, message + "\n");
            }
            catch { }
        }
    }
}
