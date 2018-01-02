using System;
using System.Text;
using Chisel.DataStructures.MapObjects;
using Chisel.Editor.Documents;

namespace Chisel.Editor.History
{
    public interface IHistoryItem : IDisposable
    {
        string Name { get; }
        bool SkipInStack { get; }
        bool ModifiesState { get; }
        void Undo(Document document);
        void Redo(Document document);
    }
}
