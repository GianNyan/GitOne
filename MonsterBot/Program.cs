using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Telegram.Data;

namespace MonsterBot
{
    class Program
    {
        static void Main(string[] args)
        {
            McBot bot = new McBot("<232076481:AAEWJvrjOqn7E_Rvdo7M1nM00lAe60nmc64>");
            bot.Start();
        }
    }
}
