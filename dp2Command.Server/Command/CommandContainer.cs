using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2Command.Service
{
    public class CommandContainer
    {
        public Dictionary<string, BaseCommand> CmdDict = null;

        public CommandContainer()
        {
            CmdDict = new Dictionary<string, BaseCommand>();
        }

        public BaseCommand GetCommand(string cmdName)
        {
            if (CmdDict.ContainsKey(cmdName))
            {
                return CmdDict[cmdName];
            }

            // todo这里改为具体的类型
            BaseCommand command = null;
            if (cmdName == dp2CommandUtility.C_Command_Search)
            {
                command = new SearchCommand();
            }
            else if (cmdName == dp2CommandUtility.C_Command_Binding)
            {
                command = new BindingCommand();
            }
            else
            {
                command = new BaseCommand();
            }
            command.CommandName = cmdName;
            CmdDict[cmdName] = command;
            return command;
        }

    }
}
