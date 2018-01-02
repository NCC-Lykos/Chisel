﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chisel.DataStructures.Geometric;

namespace Chisel.UI
{
    public interface IViewport2DEventListener : IViewportEventListener
    {
        void ZoomChanged(decimal oldZoom, decimal newZoom);
        void PositionChanged(Coordinate oldPosition, Coordinate newPosition);
    }
}
