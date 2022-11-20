using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandManager : MonoBehaviour
{
    private string commandPrefix = "/";
    private Dictionary<string, Action> commandDictionary = new Dictionary<string, Action>();
    private string currentCommand;

    //EVENTS FOR COMMANDS
    public Action<string> ErrorCommand = delegate { };
    public Action<string> PrivateMessageCommand =  delegate { };

    private void Awake()
    {
        commandDictionary.Add("whisper", PrivateMessage);
        commandDictionary.Add("help", HelpList);
    }

    //if it's a command will return true and invoke the command
    public bool IsCommand(string message)
    {
        string[] words = message.Split(' ');
        if (words[0].StartsWith(commandPrefix))
        {
            currentCommand = message;
            var target = words[0].Remove(0, commandPrefix.Length);
            if(commandDictionary.TryGetValue(target, out Action value))
            {
                value();
                return true;
            }

            ErrorCommand($"'{words[0]}' is not a command. Get full list in {commandPrefix}help");
            return true;
        }

        return false;
    }

    private void PrivateMessage()
    {
        PrivateMessageCommand.Invoke(currentCommand);
    }

    private void HelpList()
    {
        //TODO print a list of all commands or show a list of commands somewhere?
        //some commands should only be available while in gameplay maybe?
    }
}
