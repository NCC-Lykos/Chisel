using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Chisel.Editor.Properties;
using System.IO;

namespace Chisel.Editor
{
    public static class ChiselCursors
    {
        static ChiselCursors()
        {
            RotateCursor = new Cursor(new MemoryStream(Resources.Cursor_Rotate));
        }

        public static Cursor RotateCursor;
    }
}
