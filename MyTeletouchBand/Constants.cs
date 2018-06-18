using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTeletouchBand
{
    public static class Constants
    {
        public const int ResponseDeleyMs = 3;
        public const int MaxResponseDeleyMs = 20;

        public const int MouseDataDeleyMs = 10;
        public const int KyboardDataDeleyMs = 10;
        public const int JoystickDataDeleyMs = 10;

        public const int MoveRange = 124;

        public const double MouseMinDelta = 1;
        public const double JoystickMinDelta = 1;
    }
}
