using System;

namespace KajaBot
{
    internal interface IBotCommand : IEquatable<IBotCommand>
    {
        string GetCommand();
        string RunAction(string[] arguments);
    }
}