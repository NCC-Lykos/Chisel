using System;
using System.Reflection;
using System.Windows.Forms;
using Chisel.Common.Mediator;
using Chisel.UI;

namespace Chisel.Editor.UI
{
    public static class Hotkeys
    {
        public static bool HotkeyDown(Keys keyData)
        {
            var keyCombination = KeyboardState.KeysToString(keyData);
            var hotkeyImplementation = Chisel.Settings.Hotkeys.GetHotkeyFor(keyCombination);
            if (hotkeyImplementation != null)
            {
                var def = hotkeyImplementation.Definition;
                Mediator.Publish(def.Action, def.Parameter);
                return true;
            }
            return false;
        }

        public static readonly object SuppressHotkeysTag = new object();
    }
}
