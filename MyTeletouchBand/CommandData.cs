using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTeletouchBand
{
    public struct CommandData
    {
        public CommandData(byte[] data, bool ensureSuccess)
        {
            Data = data;
            EnsureSuccess = ensureSuccess;
        }

        public byte[] Data { get; set; }

        public bool EnsureSuccess { get; set; }
    }
}
