﻿using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Chisel.Providers.GameData;

namespace Chisel.Tests
{
    [TestClass]
    public class ParseTest
    {
        [TestMethod]
        public void ParseSourceFGD()
        {
            var hl2 = @"D:\Github\Chisel\_Resources\FGD\portal2.fgd";
            GameDataProvider.Register(new FgdProvider());
            var gd = GameDataProvider.GetGameDataFromFile(hl2);
            Assert.IsTrue(gd.Classes.Count > 0);
        }
        [TestMethod]
        public void ParseTF2FGD()
        {
            var tf2 = @"D:\Github\Chisel\_Resources\FGD\tf2.fgd";
            GameDataProvider.Register(new FgdProvider());
            var gd = GameDataProvider.GetGameDataFromFile(tf2);
            Assert.IsTrue(gd.MaterialExclusions.Count > 0);
            Assert.IsTrue(gd.AutoVisgroups.Count > 0);
        }

        [TestMethod]
        public void ParseGoldsourceFgd()
        {
            var gs = @"D:\Github\Chisel\_Resources\FGD\Half-Life.fgd";
            GameDataProvider.Register(new FgdProvider());
            var gd = GameDataProvider.GetGameDataFromFile(gs);
            var types = gd.Classes.SelectMany(x => x.Properties).Select(x => x.VariableType).Distinct();
            foreach (var variableType in types)
            {
                Console.WriteLine(variableType);
            }
        }
    }
}
