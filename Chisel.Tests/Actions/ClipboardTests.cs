using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Chisel.DataStructures.Geometric;
using Chisel.DataStructures.MapObjects;
using Chisel.Editor.Brushes;
using Chisel.Providers.Map;

namespace Chisel.Tests.Actions
{
    [TestClass]
    public class ClipboardTests
    {
        [TestMethod]
        public void CopyEntityTest()
        {
            var idGen = new IDGenerator();
            var box = new Box(Coordinate.One * -100, Coordinate.One * 100);

            // Create an entity with children
            var ent = new Entity(idGen.GetNextObjectID());
            ent.EntityData.Name = "Test";
            ent.EntityData.Properties.Add(new Property { Key = "key1", Value = "value1"});
            ent.EntityData.Properties.Add(new Property { Key = "key2", Value = "value2"});
            ent.EntityData.Flags = 12345;

            var solids = new BlockBrush().Create(idGen, box, null, 0);
            foreach (var mo in solids) mo.SetParent(ent);

            // Copy and reconstruct
            var gs = VmfProvider.CreateCopyStream(new List<MapObject> {ent});
            var pasted = VmfProvider.ExtractCopyStream(gs, idGen).ToList();

            // Test object
            Assert.AreEqual(1, pasted.Count);
            Assert.IsInstanceOfType(pasted[0], typeof(Entity));

            // Test entity
            var pastedEnt = (Entity) pasted[0];
            Assert.AreEqual("Test", pastedEnt.EntityData.Name);
            Assert.AreEqual(12345, pastedEnt.EntityData.Flags);

            // Test properties
            Assert.AreEqual(2, pastedEnt.EntityData.Properties.Count);
            var k1 = pastedEnt.EntityData.Properties.FirstOrDefault(x => x.Key == "key1");
            var k2 = pastedEnt.EntityData.Properties.FirstOrDefault(x => x.Key == "key1");
            Assert.IsNotNull(k1);
            Assert.IsNotNull(k2);
            Assert.AreEqual(k1.Value, "value1");
            Assert.AreEqual(k2.Value, "value1");

            // Test child
            Assert.AreEqual(1, pastedEnt.ChildCount);
            Assert.IsInstanceOfType(pastedEnt.GetChildren().ToList()[0], typeof(Solid));

            // Check number of sides, values of sides not so important
            var pastedSolid = (Solid) pastedEnt.GetChildren().ToList()[0];
            Assert.AreEqual(6, pastedSolid.Faces.Count);
        }
    }
}