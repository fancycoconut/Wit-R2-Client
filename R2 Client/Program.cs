using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R2_Client
{
    class Program
    {
        static void Main(string[] args)
        {
            CommandInterpreter interpreter = new CommandInterpreter();
            string command = Console.ReadLine();

            while (command.ToLower() != "exit")
            {
                interpreter.ProcessCommandAsync(command).Wait();

                command = Console.ReadLine();
            }
        }
    }
}
