using System.ComponentModel;

namespace Chisel.Settings
{
    public enum DoubleClick3DAction
    {
        [Description("Do nothing")]
        Nothing,
        [Description("Show object properties")]
        ObjectProperties,
        [Description("Switch to the selection tool")]
        TextureTool
    }
}