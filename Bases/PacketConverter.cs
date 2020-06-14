using System;
using System.Collections.Generic;
using System.Text;

namespace Bases
{
    interface PacketConverter
    {
        public byte[] SeserializeTo();
    }
}
