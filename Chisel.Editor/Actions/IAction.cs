using System;
using System.Collections.Generic;
using System.Text;
using Chisel.Editor.Documents;

namespace Chisel.Editor.Actions
{
    public interface IAction : IDisposable
    {
        bool SkipInStack { get; }
        bool ModifiesState { get; }
        void Reverse(Document document);
        void Perform(Document document);
    }
}
