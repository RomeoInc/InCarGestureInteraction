using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interfaces
{
    class Protocol
    {
        public const String MessageShowMessage = "a";
        public const String MessageRestart = "x";
        public const String MessageEnableUI = "y";
        public const String MessageEnableUIPoint = "y 0";
        public const String MessageEnableUICount = "y 1";
        public const String MessageDisableUI = "z";
        public const String MessageEnterWorkspacePoint = "1";
        public const String MessageEnterWorkspaceCount = "h";
    }
}
