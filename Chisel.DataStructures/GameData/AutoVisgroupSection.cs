﻿using System.Collections.Generic;

namespace Chisel.DataStructures.GameData
{
    public class AutoVisgroupSection
    {
        public string Name { get; set; }
        public List<AutoVisgroup> Groups { get; private set; }

        public AutoVisgroupSection()
        {
            Groups = new List<AutoVisgroup>();
        }
    }
}