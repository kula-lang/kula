using Kula.Xception;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Kula.Core
{
    class Module
    {
        public static Module Instance { get; } = new Module();

        private Module() { }
    
        public Module SplitModule(StreamReader streamReader, string originPath, out IList<string> modulePath) 
        {
            modulePath = new List<string>();

            while ('$' == (char)streamReader.Peek())
            {
                var line = streamReader.ReadLine();
                if (line.StartsWith("$import"))
                {
                    modulePath.Add(originPath + line.Split('\"')[1]);
                }
                else
                {
                    throw new LexerException($"Wrong Import Format => `{line}`");
                }
            }

            return this;
        }
    }
}
