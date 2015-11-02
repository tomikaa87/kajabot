using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KajaBot
{
    class StaticResponder
    {
        private readonly Dictionary<string, List<string>> _responses = new Dictionary<string, List<string>>();

        public StaticResponder()
        {
            // Load responses from file
        }

        public void AddResponse(string command, string response)
        {
            
        }

        public bool HasResponse(string command)
        {
            return false;
        }

        /// <summary>
        /// Returns a (random, if more than one answer is associated with the command) response for the given command.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public string GetResponse(string command)
        {
            return "";
        }
    }
}
