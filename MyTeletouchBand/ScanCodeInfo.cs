using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTeletouchBand
{
    public struct ScanCodeInfo
    {
        public ScanCodeInfo(byte scanCode, byte specialKeys)
        {
            this.ScanCode = scanCode;
            this.SpecialKeys = specialKeys;
        }

        public byte ScanCode;
        public byte SpecialKeys;
    }
}
