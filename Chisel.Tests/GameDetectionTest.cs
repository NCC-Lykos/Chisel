﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Chisel.Settings.GameDetection;

namespace Chisel.Tests
{
    [TestClass]
    public class GameDetectionTest
    {
        [TestMethod]
        public void DetectWonInstall()
        {
            var won = new WonDetector();
            var games = won.Detect();
            foreach (var game in games)
            {
                int i = 1;
            }
        }
    }
}
