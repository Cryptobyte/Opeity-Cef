using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opeity.Helpers
{
    public class FileSizeHelper
    {
        //
        // Source: https://stackoverflow.com/a/11124118
        //
        // Returns the human-readable file size for an arbitrary, 64-bit file size 
        // The default format is "0.### XB", e.g. "4.2 KB" or "1.434 GB"
        public static string GetBytesReadable(long i)
        {
            var absoluteI = (i < 0 ? -i : i);

            string suffix;
            double readable;

            if (absoluteI >= 0x1000000000000000)
            {
                suffix = "EBps";
                readable = (i >> 50);
            }
            else if (absoluteI >= 0x4000000000000)
            {
                suffix = "PBps";
                readable = (i >> 40);
            }
            else if (absoluteI >= 0x10000000000)
            {
                suffix = "TBps";
                readable = (i >> 30);
            }
            else if (absoluteI >= 0x40000000)
            {
                suffix = "GBps";
                readable = (i >> 20);
            }
            else if (absoluteI >= 0x100000)
            {
                suffix = "MBps";
                readable = (i >> 10);
            }
            else if (absoluteI >= 0x400)
            {
                suffix = "KBps";
                readable = i;
            }
            else
            {
                return i.ToString("0 Bps");
            }

            readable = readable / 1024;
            return readable.ToString("0.# ") + suffix;
        }
    }
}
