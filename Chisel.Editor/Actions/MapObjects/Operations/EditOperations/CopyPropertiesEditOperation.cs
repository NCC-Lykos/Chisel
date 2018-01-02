﻿using Chisel.DataStructures.MapObjects;

namespace Chisel.Editor.Actions.MapObjects.Operations.EditOperations
{
    public class CopyPropertiesEditOperation : IEditOperation
    {
        private readonly MapObject _properties;

        public CopyPropertiesEditOperation(MapObject properties)
        {
            _properties = properties;
        }

        public void PerformOperation(MapObject mo)
        {
            mo.Unclone(_properties);
        }
    }
}