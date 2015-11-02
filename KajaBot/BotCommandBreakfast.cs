using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KajaBot
{
    class BotCommandBreakfast : IBotCommand
    {
        public string GetCommand()
        {
            return "reggeli";
        }

        public string RunAction(string[] args)
        {
            return
                "Reggeli: http://opc2.dev1.bloomspin.com/wp-content/uploads/sites/19/2014/02/Tea-Camomiles-Breakfast.jpg";
        }

        public bool Equals(IBotCommand other)
        {
            return other.GetCommand() == GetCommand();
        }
    }
}
