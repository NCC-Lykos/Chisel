using System;
using Chisel.DataStructures.GameData;

namespace Chisel.Editor.UI.ObjectProperties.SmartEdit
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    internal class SmartEditAttribute : Attribute
    {
        public VariableType VariableType { get; set; }

        public SmartEditAttribute(VariableType variableType)
        {
            VariableType = variableType;
        }
    }
}