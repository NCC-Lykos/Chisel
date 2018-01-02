using System;

namespace Chisel.Common.Mediator
{
    public interface IMediatorListener
    {
        void Notify(string message, object data);
    }
}