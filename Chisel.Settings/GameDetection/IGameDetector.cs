using System.Collections.Generic;
using Chisel.Settings.Models;

namespace Chisel.Settings.GameDetection
{
    public interface IGameDetector
    {
        string Name { get; }

        IEnumerable<Game> Detect();
    }
}
