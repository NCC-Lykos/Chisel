using System.Collections.Generic;
using System.Windows.Forms;

namespace Chisel.Editor.Menu
{
    public interface IMenuBuilder
    {
        bool ShowInMenu { get; }
        bool ShowInToolStrip { get; }
        IEnumerable<ToolStripItem> Build();
        IEnumerable<ToolStripItem> BuildToolStrip();
    }
}