using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Chisel.DataStructures.MapObjects;
using System.Globalization;
using System.IO;
using Chisel.Common;
using System.Diagnostics;
using Chisel.DataStructures.Geometric;
using Chisel.Providers;


namespace Chisel.Providers.Map
{
    public class ThreeDtProvider : MapProvider
    {
        protected override IEnumerable<MapFeature> GetFormatFeatures()
        {
            return new[]
            {
                MapFeature.Worldspawn,
                MapFeature.Solids,
                MapFeature.Entities,

                MapFeature.Motions,

                MapFeature.Colours,
                MapFeature.SingleVisgroups,
            };
        }

        private float MapVersion { get; set; }

        protected override bool IsValidForFileName(string filename)
        {
            return filename.EndsWith(".3dt", true, CultureInfo.InvariantCulture);
        }
        private void Assert(bool b, string message = "Malformed file")
        {
            if (!b) throw new Exception(message);
        }

        private string FormatCoordinate(Coordinate c)
        {
            return c.X.ToString("0.000000", CultureInfo.InvariantCulture)
                    + " " + c.Z.ToString("0.000000", CultureInfo.InvariantCulture)
                    + " " + (-c.Y).ToString("0.000000", CultureInfo.InvariantCulture);
        }
        private string FormatCoordinateNonSwitch(Coordinate c)
        {
            return c.X.ToString("0.000000", CultureInfo.InvariantCulture)
                    + " " + c.Y.ToString("0.000000", CultureInfo.InvariantCulture)
                    + " " + c.Z.ToString("0.000000", CultureInfo.InvariantCulture);
        }
        private string FormatIntCoordinate(Coordinate c)
        {
            return c.X.ToString("0", CultureInfo.InvariantCulture)
                    + " " + c.Z.ToString("0", CultureInfo.InvariantCulture)
                    + " " + (-c.Y).ToString("0", CultureInfo.InvariantCulture);
        }
        private string FormatColor(System.Drawing.Color c)
        {
            return c.R.ToString("0", CultureInfo.InvariantCulture)
                   + " " + c.G.ToString("0", CultureInfo.InvariantCulture)
                   + " " + c.B.ToString("0", CultureInfo.InvariantCulture);
        }

        //private Chisel.DataStructures.GameData.GameData gamedata;
        private List<DataStructures.GameData.GameDataObject> ClassList;
        Dictionary<string, UInt32> EntityCounts;

        private static System.Drawing.Color GetGenesisBrushColor(int Flags)
        {
            //Determine type of brush and color it
            if (Flags == 72)
                return Color.FromArgb(255, 186, 85, 211);
            return Colour.GetRandomBrushColour();
        }

        private static List<string> FaceProperties = new List<string> { "NumPoints", "Flags", "Light", "MipMapBias", "Translucency", "Reflectivity" };
        private static List<string> SolidProperties = new List<string> { "Flags", "ModelId", "GroupId", "HullSize", "Type", "BrushFaces" };
        private static List<string> EntityProperties = new List<string> { "eStyle", "eOrigin", "eFlags", "eGroup", "ePairCount" };
        private static List<string> EntityListProperties = new List<string> { "EntCount", "CurEnt" };

        private Face ReadFace(Solid parent, StreamReader rdr, IDGenerator generator)
        {
            const NumberStyles ns = NumberStyles.Float;
            var properties = new Dictionary<string, string>();

            foreach (var prop in FaceProperties)
            {
                (string name, string value) = ReadProperty(rdr);
                Assert(name == prop);
               
                properties[name] = value;
            }

            var numPoints = int.Parse(properties["NumPoints"]);
            var coords = new List<Coordinate>(numPoints);
            for (int i = 0; i < numPoints; ++i)
            {
                var line = rdr.ReadLine().Trim();
                Assert(line.StartsWith("Vec3d"));

                var split = line.Split(' ');
                Assert(split.Length == 4);
                
                var coord = Coordinate.Parse(split[1], split[2], split[3]);
                var tmp = coord.Y;
                coord.Y = -coord.Z;
                coord.Z = tmp;

                coords.Add(coord);
            }
            var poly = new Polygon(coords);

            // Parse texture
            var texSplit = rdr.ReadLine().Trim().Split(' ');
            Assert(texSplit.Length == 11);

            var face = new Face(generator.GetNextFaceID())
            {
                Plane = poly.GetPlane(),
                Parent = parent,
                Texture = { Name = texSplit[10].Trim('"') },
                Light = int.Parse(properties["Light"]),
            };

            face.Vertices.AddRange(poly.Vertices.Select(x => new Vertex(x, face)));
            
            face.Texture.Flags = (FaceFlags)int.Parse(properties["Flags"]);
            face.Texture.Translucency = decimal.Parse(properties["Translucency"]);
            if(face.Texture.Flags.HasFlag(FaceFlags.Transparent) && !face.Texture.Flags.HasFlag(FaceFlags.Mirror))
            {
                face.Texture.Opacity = (decimal.Parse(properties["Translucency"]) / (decimal)255.0f);
            }
            else
            {
                face.Texture.Opacity = 1;
            }
                
            face.Texture.XShift = decimal.Parse(texSplit[4], ns, CultureInfo.InvariantCulture);
            face.Texture.YShift = decimal.Parse(texSplit[5], ns, CultureInfo.InvariantCulture);
            face.Texture.Rotation = decimal.Parse(texSplit[2], ns, CultureInfo.InvariantCulture);
            face.Texture.XScale = decimal.Parse(texSplit[7], ns, CultureInfo.InvariantCulture);
            face.Texture.YScale = decimal.Parse(texSplit[8], ns, CultureInfo.InvariantCulture);
            
            face.SetHighlights();
            face.UpdateBoundingBox();

            // Parse light scale
            var ln = rdr.ReadLine();
            var scale = ln.Split(' ');
            face.LightScale = Coordinate.Parse(scale[1], scale[2], "0");

            // Skip currently unknown values Transform and Pos
            if (MapVersion > 1.31)
            {
                texSplit = rdr.ReadLine().Trim().Split(' ','\t');
                Assert(texSplit.Length == 13);
                face.Texture.TransformAngleRF = new Matrix();

                /*
                NOTE(SVK): Keep RF YZ
                Chisel
                0-AX  1-AY  2-AZ  3-TX
                4-BX  5-BY  6-BZ  7-TY
                8-CX  9-CY 10-CZ 11-TZ

                RF
                1-AX  2-AY  3-AZ 10-TX
                4-BX  5-BY  6-BZ 11-TY
                7-CX  8-CY  9-CZ 12-TZ
                */
                //A
                face.Texture.TransformAngleRF.Values[0] = decimal.Parse(texSplit[1], ns, CultureInfo.InvariantCulture);
                face.Texture.TransformAngleRF.Values[1] = decimal.Parse(texSplit[2], ns, CultureInfo.InvariantCulture);
                face.Texture.TransformAngleRF.Values[2] = decimal.Parse(texSplit[3], ns, CultureInfo.InvariantCulture);

                //B
                face.Texture.TransformAngleRF.Values[4] = decimal.Parse(texSplit[4], ns, CultureInfo.InvariantCulture);
                face.Texture.TransformAngleRF.Values[5] = decimal.Parse(texSplit[5], ns, CultureInfo.InvariantCulture);
                face.Texture.TransformAngleRF.Values[6] = decimal.Parse(texSplit[6], ns, CultureInfo.InvariantCulture);

                //C
                face.Texture.TransformAngleRF.Values[8] = decimal.Parse(texSplit[7], ns, CultureInfo.InvariantCulture);
                face.Texture.TransformAngleRF.Values[9] = decimal.Parse(texSplit[8], ns, CultureInfo.InvariantCulture);
                face.Texture.TransformAngleRF.Values[10] = decimal.Parse(texSplit[9], ns, CultureInfo.InvariantCulture);

                //T (transform)
                face.Texture.TransformAngleRF.Values[3] = decimal.Parse(texSplit[10], ns, CultureInfo.InvariantCulture);
                face.Texture.TransformAngleRF.Values[7] = decimal.Parse(texSplit[11], ns, CultureInfo.InvariantCulture);
                face.Texture.TransformAngleRF.Values[11] = decimal.Parse(texSplit[12], ns, CultureInfo.InvariantCulture);

                texSplit = rdr.ReadLine().Trim().Split(' ','\t');
                face.Texture.PositionRF = Coordinate.Parse(texSplit[1], texSplit[3], texSplit[2]);
                face.Texture.PositionRF.Y *= -1;
            } else
            {
                face.InitFaceAngle();
            }

            return face;
        }
        private Solid ReadSolid(StreamReader rdr, IDGenerator generator, string brushName = "NoName")
        {
            var properties = new Dictionary<string, string>();

            foreach (var prop in SolidProperties)
            {
                (string name, string value) = ReadProperty(rdr);
                Assert(name == prop);
                properties[name] = value;
            }

            var numFaces = int.Parse(properties["BrushFaces"]);
            var faces = new List<Face>(numFaces);
            var ret = new Solid(generator.GetNextObjectID()) { ClassName = brushName };
            for (int i = 0; i < numFaces; ++i)
            {
                faces.Add(ReadFace(ret, rdr, generator));
            }

            ret.Faces.AddRange(faces);

            // Ensure all the faces point outwards
            var origin = ret.GetOrigin();
            foreach (var face in ret.Faces)
            {
                if (face.Plane.OnPlane(origin) >= 0) face.Flip();
            }

            ret.UpdateBoundingBox();

            //var ret = Solid.CreateFromIntersectingPlanes(faces.Select(x => x.Plane), generator);
            ret.Colour = GetGenesisBrushColor(int.Parse(properties["Flags"]));
            //ret.MetaData.Set("Flags", properties["Flags"]);
            ret.Flags = (UInt32)int.Parse(properties["Flags"]);

            //Fix for any windows created without the detail flag. This was before we were smart...
            if (((ret.Flags & (UInt32)SolidFlags.window) != 0) && ((ret.Flags & (UInt32)SolidFlags.detail) == 0))
                    ret.Flags = ret.Flags | (UInt32)SolidFlags.detail;

            ret.MetaData.Set("ModelId", properties["ModelId"]);
            ret.MetaData.Set("HullSize", properties["HullSize"]);
            ret.MetaData.Set("Type", properties["Type"]);

            int group = int.Parse(properties["GroupId"]);
            if (group > 0)
                ret.Visgroups.Add(group);

            return ret;
        }
        private static void ReadKeyValue(Entity ent, string line)
        {
            var split = line.Split(' ');
            var key = split[1].Trim();
            var value = string.Join(" ", split.Skip(3)).Trim('"');

            if (key == "classname")
                ent.ClassName = value;
            else if (key.Equals("origin", StringComparison.OrdinalIgnoreCase))
            {
                var osp = value.Split(' ');
                ent.Origin = Coordinate.Parse(osp[0], osp[2], osp[1]);
                ent.Origin.Y = -ent.Origin.Y;
            }
            else if (key == "%name%")
            {
                ent.EntityData.Name = value;
                ent.EntityData.SetPropertyValue(key, value);
            }
            else
            {
                if (key == "color")
                {
                    var csp = value.Split(' ');
                    var r = int.Parse(csp[0]);
                    var g = int.Parse(csp[1]);
                    var b = int.Parse(csp[2]);

                    ent.Colour = System.Drawing.Color.FromArgb(r, g, b);
                }

                ent.EntityData.SetPropertyValue(key, value);
            }
        }
        private static (string name, string value) ReadProperty(StreamReader rdr)
        {
            var line = rdr.ReadLine();
            return ReadProperty(line);
        }
        private static (string name, string value) ReadEntProperty(StreamReader rdr)
        {
            var line = rdr.ReadLine().Replace("Key ","").Replace("Value ","");

            //TODO(SVK): Fix this?
            line = line.Replace("Origin", "origin");
            return ReadProperty(line);
        }
        private static (string name, string value) ReadProperty(string line)
        {
            var split = line.Split(' ');
            return (split[0].Trim(), string.Join(" ", split.Skip(1)).Trim('"'));
        }
        private Entity ReadWorldEntity(StreamReader rdr, IDGenerator generator)
        {
            var ent = new Entity(generator.GetNextObjectID()) { EntityData = new EntityData(), Colour = Colour.GetRandomBrushColour() };
            ent.EntityData.Name = "worldspawn";

            string line;
            while ((line = rdr.ReadLine()).StartsWith("Brush"))
            {
                var match = System.Text.RegularExpressions.Regex.Match(line, "Brush \"(?<BrushName>.*)\"");
                string brushName = "NoName";
                if (match.Success)
                    brushName = match.Groups["BrushName"].Value;

                var s = ReadSolid(rdr, generator, brushName);
                if (s != null) s.SetParent(ent, false);
                if (s != null) s.SetHighlights();
            }

            ent.UpdateBoundingBox();
            return ent;
        }

        private int FindClassIndex(List<Chisel.DataStructures.GameData.GameDataObject> c, string name)
        {
            int ret = -1;
            for (int x = 0; x < c.Count; x++)
            {
                if (c[x].Name.ToLower() == name.ToLower()) ret = x;
            }
            return ret;
        }
        private int FindEntityPropertyIndex(List<Property> list, string p)
        {
            int ret = -1;
            for(int x = 0; x < list.Count(); x++)
            {
                if (list[x].Key.ToLower() == p.ToLower()) ret = x;
            }
            return ret;
        }
        private int FindEntityPropertyIndex(List<DataStructures.GameData.Property> list, string p)
        {
            int ret = -1;
            for (int x = 0; x < list.Count(); x++)
            {
                if (list[x].Name.ToLower() == p.ToLower()) ret = x;
            }
            return ret;
        }

        private Color GetEntityColor(Dictionary<string, string> props)
        {
            Color ret = new Color();

            var Keys = props.Keys.ToArray();
            for (int x = 0; x < props.Count; x++)
            {
                if (Keys[x] == "color")
                {
                    var color = props[Keys[x]].Split(' ');
                    ret = Color.FromArgb(255, int.Parse(color[0]), int.Parse(color[1]), int.Parse(color[2]));
                }

            }
            if (ret.IsEmpty) ret = Color.FromArgb(255, 255, 255, 255); //White

            return ret;
        }

        private Entity ReadEntity(StreamReader rdr, IDGenerator generator)
        {
            var properties = new Dictionary<string, string>();
            var entprops = new Dictionary<string, string>();
            string line = rdr.ReadLine();
            Assert(line.Trim() == "CEntity");
            
            foreach (var prop in EntityProperties)
            {
                (string name, string value) = ReadProperty(rdr);
                Assert(name == prop);
                properties[name] = value;
            }

            for (int x = 0; x < int.Parse(properties["ePairCount"]); x++)
            {
                (string name, string value) = ReadEntProperty(rdr);
                Assert(name != "End CEntity");
                entprops[name] = value;
            }

            int index = FindClassIndex(ClassList, entprops["classname"]);
            Assert(index > -1);
            var origin = properties["eOrigin"].ToString().Split(' ');
            DataStructures.GameData.GameDataObject gdo = ClassList[index];

            entprops.Remove("classname");
            entprops.Remove("origin");

            var ent = new Entity(generator.GetNextObjectID())
            {
                EntityData = new EntityData(ClassList[index]),
                ClassName = ClassList[index].Name,
                Origin = Coordinate.Parse(origin[0], origin[2], origin[1]),
                Colour = GetEntityColor(entprops)
            };
            ent.Origin.Y = -ent.Origin.Y;
            ent.EntityData.Flags = int.Parse(properties["eFlags"]);


            Assert(ent.EntityData.Properties.Count == entprops.Count());

            var Keys = entprops.Keys.ToArray();
            for(int x = 0; x < entprops.Count; x++)
            {
                string val = entprops[Keys[x]].ToString();
                string key = Keys[x];
                if (key.ToLower() == "angle") key = "angles";
                int PropIndex = FindEntityPropertyIndex(ent.EntityData.Properties, key);

                var type = ClassList[index].Properties[PropIndex].VariableType;
                switch (type)
                {
                    case DataStructures.GameData.VariableType.Color255:
                        //val = val.Replace(".0", "");
                        val += " 255"; //Add Alpha
                        break;
                    case DataStructures.GameData.VariableType.Origin:
                        var tval = val.Split(' ');
                        int tint = 0;
                        tint = -int.Parse(tval[2]);
                        val = tval[0] + ' ' + tint.ToString() + ' ' + tval[1]; //Add Alpha
                        break;
                    case DataStructures.GameData.VariableType.Angle:
                        {
                            var tval2 = val.Split(' ');
                            float a, b, c;
                            a = float.Parse(tval2[0]); if (a < 0) a = 360 + a;
                            b = float.Parse(tval2[1]); if (b < 0) b = 360 + b;
                            c = float.Parse(tval2[2]); if (c < 0) c = 360 + c; if (c != 0) c = 360 - c;
                            val = c.ToString() + ' ' + b.ToString() + ' ' + a.ToString();
                        }
                        break;
                }
                if (key == "%name%")
                {
                    UInt32 y = UInt32.Parse(val.ToLower().Replace(ent.ClassName, ""));
                    if(y > EntityCounts[ClassList[index].Name]) EntityCounts[ClassList[index].Name] = y;
                }
                ent.EntityData.Properties[PropIndex].Value = val;
            }
            // Swallow end
            line = rdr.ReadLine();
            Assert(line == "End CEntity");

            ent.UpdateBoundingBox();
            return ent;
        }
        private List<Entity> ReadAllEntities(StreamReader rdr, IDGenerator generator)
        {
            var properties = new Dictionary<string, string>();
            var list = new List<Entity>();
            
            list.Add(ReadWorldEntity(rdr, generator));

            foreach (var prop in EntityListProperties)
            {
                (string name, string value) = ReadProperty(rdr);
                Assert(name == prop);
                properties[name] = value;
            }

            var numEntites = int.Parse(properties["EntCount"]);
            for (int i = 0; i < numEntites; ++i)
            {
                Entity ent = ReadEntity(rdr, generator);
                if (ent != null)
                    list.Add(ent);
            }

            return list;
        }
        
        private Dictionary<string, string> ReadMapStats(StreamReader rdr)
        {
            var ret = new Dictionary<string, string>();
            (string name, string value) = ReadProperty(rdr);
            var _3dtVersion = float.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
            var numStats = 0;

            //Compatibility with what we've done already
            if (_3dtVersion == 1.35f)
                _3dtVersion = 1.31f;

            if (_3dtVersion >= 1.33f)
                numStats = 8;
            else
                numStats = 6;
            //ret["3dtVersion"] = value;
            MapVersion = _3dtVersion;

            for (int i = 0; i < numStats; ++i)
            {
                (name, value) = ReadProperty(rdr);
                ret[name] = value;
            }
            return ret;
        }

        private List<Motion> ReadMotions(int numMotions, StreamReader rdr)
        {
            const NumberStyles ns = NumberStyles.Float;
            FileStream fs = (FileStream)rdr.BaseStream;
            List<Motion> models = new List<Motion>();
            
            
            for (int i = 0; i < numMotions; ++i)
            {
                string line = null;
                var start = rdr.GetPosition();
                if (line == null) line = rdr.ReadLine();

                Assert(line.StartsWith("Model "));
                string nameline = line;

                var split = line.Split('"');
                var name = split[1].ToString();

                line = rdr.ReadLine();
                string idline = line;
                split = line.Trim().Split(' ');
                var model = new Motion(Convert.ToInt32(split[1]));

                model.Name = name;
                model.RawModelLines.Add(nameline);
                model.RawModelLines.Add(idline);
                
                for(int x = 0; x < 16; x++)
                {
                    line = rdr.ReadLine();
                    model.RawModelLines.Add(line);
                    char tab = '\u0009';
                    line = line.Replace(tab.ToString(),"").Trim();
                    if (!line.StartsWith("Transform"))
                    {
                        split = line.Split(' ');
                        switch (split[0])
                        {
                            case "CurrentKeyTime":
                                model.CurrentKeyTime = Convert.ToDouble(split[1]);
                                break;
                            case "Motion":
                                Assert(split[1] == "1");
                                break;
                            case "MOTN":
                                Assert(split[1] == "0.F0");
                                break;
                            case "NameID":
                                //Assert(split[1] == null);
                                break;
                            case "MaintainNames":
                                Assert(split[1] == "1");
                                break;
                            case "PathCount":
                                Assert(split[1] == "1");
                                break;
                            case "NameChecksum":
                                Assert(split[1] == "2379");
                                break;
                            case "Events":
                                Assert(split[1] == "0");
                                break;
                            case "NameArray":
                                Assert(split[1] == "1");
                                break;
                            case "SBLK":
                                Assert(split[1] == "0.F0");
                                break;
                            case "Strings":
                                Assert(split[1] == "1");
                                break;
                            case "PathInfo":
                                //Assert(split[1] == null);
                                break;
                            case "PathArray":
                                Assert(split[1] == "1");
                                break;
                            case "PATH":
                                Assert(split[1] == "0.F2");
                                break;
                            case "Rotation":
                                Assert(split[1] == "1");
                                Assert(split[2] == "4");
                                break;
                        }
                    }
                    else
                    {
                        //Transform
                        line = rdr.ReadLine();
                        model.RawModelLines.Add(line);
                        split = line.Split(' ');
                        Matrix m = new Matrix();
                        m.Values[0] = Convert.ToDecimal(split[0]);
                        m.Values[1] = Convert.ToDecimal(split[1]);
                        m.Values[2] = Convert.ToDecimal(split[2]);
                        m.Values[4] = Convert.ToDecimal(split[3]);
                        m.Values[5] = Convert.ToDecimal(split[4]);
                        m.Values[6] = Convert.ToDecimal(split[5]);
                        m.Values[8] = Convert.ToDecimal(split[6]);
                        m.Values[9] = Convert.ToDecimal(split[7]);
                        m.Values[10] = Convert.ToDecimal(split[8]);
                        m.Values[3] = Convert.ToDecimal(split[9]);
                        m.Values[7] = Convert.ToDecimal(split[11]);
                        m.Values[11] = -Convert.ToDecimal(split[10]);
                        model.Transform = m;
                    }

                }
                
                //At Rotation  TODO
                Assert(line.StartsWith("Rotation"));

                line = rdr.ReadLine();
                model.RawModelLines.Add(line);
                split = line.Split(' ');

                int KeyCount = Convert.ToInt32(split[1]);
                for (int x = 0; x < KeyCount; x++)
                {
                    line = rdr.ReadLine();
                    model.RawModelLines.Add(line);
                    split = line.Split(' ');
                    MotionKeyFrames k = new MotionKeyFrames((float)Convert.ToDouble(split[0]), model);
                    Coordinate c = new Coordinate(Convert.ToDecimal(split[1]),
                                                  Convert.ToDecimal(split[2]),
                                                  Convert.ToDecimal(split[3]));
                    k.SetRotation(c);
                    model.KeyFrames.Add(k);
                }

                for(int x = 0; x < 2; x++)
                {
                    line = rdr.ReadLine();
                    model.RawModelLines.Add(line);
                }

                Assert(line.StartsWith("Keys"));
                split = line.Split(' ');
                Assert(Convert.ToInt32(split[1]) == KeyCount);

                for (int x = 0; x < KeyCount; x++)
                {
                    line = rdr.ReadLine();
                    model.RawModelLines.Add(line);
                    split = line.Split(' ');
                    Coordinate c = new Coordinate( Convert.ToDecimal(split[1]),
                                                  -Convert.ToDecimal(split[3]),
                                                   Convert.ToDecimal(split[2]));
                    model.KeyFrames[x].SetTranslation(c);
                }

                models.Add(model);
            }
            
            //rdr.SetPosition(start);
            return models;
        }
        private List<Visgroup> ReadGroups(int numGroups, StreamReader rdr)
        {
            var ret = new List<Visgroup>();
            for (int i = 0; i < numGroups; ++i)
            {
                var group = new Visgroup();

                (string name, string value) = ReadProperty(rdr);
                group.Name = value;

                (name, value) = ReadProperty(rdr);
                group.ID = int.Parse(value);

                (name, value) = ReadProperty(rdr);
                group.Visible = value == "1";

                (name, value) = ReadProperty(rdr);
                (name, value) = ReadProperty(rdr);
                var colors = value.Split(' ');
                var r = int.Parse(colors[0], NumberStyles.Float, CultureInfo.InvariantCulture);
                var g = int.Parse(colors[1], NumberStyles.Float, CultureInfo.InvariantCulture);
                var b = int.Parse(colors[2], NumberStyles.Float, CultureInfo.InvariantCulture);

                group.Colour = System.Drawing.Color.FromArgb(r, g, b);

                ret.Add(group);
            }

            return ret;
        }

        private void WriteFace(Face face, StreamWriter wr)
        {
            WriteProperty("NumPoints", face.Vertices.Count().ToString(), wr, false, 2);
            WriteProperty("Flags", ((int)face.Texture.Flags).ToString(), wr, false, 2);
            WriteProperty("Light", face.Light.ToString(), wr, false, 2);
            WriteProperty("MipMapBias", "1.000000", wr, false, 2);
            WriteProperty("Translucency", (face.Texture.Translucency).ToString(), wr, false, 2);
            WriteProperty("Reflectivity", "1.000000", wr, false, 2);

            foreach (var vert in face.Vertices)
            {
                WriteProperty("Vec3d", FormatCoordinate(vert.Location), wr, false, 3);
            }

            var texInfo = string.Format("Rotate {0} Shift {1} {2} Scale {3} {4} Name \"{5}\"",
                face.Texture.Rotation.ToString("0", CultureInfo.InvariantCulture),
                face.Texture.XShift.ToString("0", CultureInfo.InvariantCulture),
                face.Texture.YShift.ToString("0", CultureInfo.InvariantCulture),
                face.Texture.XScale.ToString("0.000000", CultureInfo.InvariantCulture),
                face.Texture.YScale.ToString("0.000000", CultureInfo.InvariantCulture),
                face.Texture.Name);
            WriteProperty("TexInfo", texInfo, wr, false, 3);

            if (face.LightScale != null)
                WriteProperty("LightScale",
                face.LightScale.X.ToString("0.000000", CultureInfo.InvariantCulture) + " "
                + face.LightScale.Y.ToString("0.000000", CultureInfo.InvariantCulture), wr, false, 2);
            else
                WriteProperty("LightScale", "1.000000 1.000000", wr, false, 2);

            /*
                NOTE(SVK): Keep G3D YZ
                Chisel
                0-AX  1-AY  2-AZ  3-TX
                4-BX  5-BY  6-BZ  7-TY
                8-CX  9-CY 10-CZ 11-TZ

                RF
                1-AX  2-AY  3-AZ 10-TX
                4-BX  5-BY  6-BZ 11-TY
                7-CX  8-CY  9-CZ 12-TZ
            */
            WriteProperty("Transform",
                face.Texture.TransformAngleRF.Values[0].ToString("0.000000", CultureInfo.InvariantCulture) + " " +
                face.Texture.TransformAngleRF.Values[1].ToString("0.000000", CultureInfo.InvariantCulture) + " " +
                face.Texture.TransformAngleRF.Values[2].ToString("0.000000", CultureInfo.InvariantCulture) + " " +

                face.Texture.TransformAngleRF.Values[4].ToString("0.000000", CultureInfo.InvariantCulture) + " " +
                face.Texture.TransformAngleRF.Values[5].ToString("0.000000", CultureInfo.InvariantCulture) + " " +
                face.Texture.TransformAngleRF.Values[6].ToString("0.000000", CultureInfo.InvariantCulture) + " " +

                face.Texture.TransformAngleRF.Values[8].ToString("0.000000", CultureInfo.InvariantCulture) + " " +
                face.Texture.TransformAngleRF.Values[9].ToString("0.000000", CultureInfo.InvariantCulture) + " " +
                face.Texture.TransformAngleRF.Values[10].ToString("0.000000", CultureInfo.InvariantCulture) + " " +

                face.Texture.TransformAngleRF.Values[3].ToString("0.000000", CultureInfo.InvariantCulture) + " " +
                face.Texture.TransformAngleRF.Values[7].ToString("0.000000", CultureInfo.InvariantCulture) + " " +
                face.Texture.TransformAngleRF.Values[11].ToString("0.000000", CultureInfo.InvariantCulture), wr, false, 1);
            //X,-Z,Y
            // ->
            //X,Y,-Z
            decimal Temp = -face.Texture.PositionRF.Y;
            WriteProperty("Pos",
                face.Texture.PositionRF.X.ToString("0.000000", CultureInfo.InvariantCulture) + " " +
                face.Texture.PositionRF.Z.ToString("0.000000", CultureInfo.InvariantCulture) + " " +
                Temp.ToString("0.000000", CultureInfo.InvariantCulture), wr, false, 1);
        }
        private void WriteKeyValue(string key, string value, StreamWriter wr)
        {
            string line = string.Format("Key {0} Value \"{1}\"", key, value);
            wr.WriteLine(line);
        }
        private static void WriteProperty(string name, string value, StreamWriter wr, bool quote = false, int numTabs = 0, bool newlineValue = false, string defaultValue = null)
        {

            var bld = new StringBuilder();

            for (int i = 0; i < numTabs; ++i)
                bld.Append('\t');

            if (quote)
                bld.AppendFormat("{0} \"{1}\"", name, value);
            else if (!newlineValue)
                bld.AppendFormat("{0} {1}", name, value);
            else // This is because we need to newline the Transform in motions.
                bld.AppendFormat("{0}\r{1}", name, value);

            wr.WriteLine(bld.ToString());
        }

        private int GBSPGroup(List<int> l)
        {
            int ret = 0;

            foreach(int x in l)
            {
                if (x > 0) ret = x;
            }
            
            return ret;
        }

        private void WriteWorldEntity(List<Solid> solids, StreamWriter wr)
        {
            foreach(var solid in solids)
            {
                int vis = GBSPGroup(solid.Visgroups);

                WriteProperty("Brush", solid.ClassName ?? "NoName", wr, true);
                WriteProperty("Flags", ((int)solid.Flags).ToString(), wr, false, 1);
                WriteProperty("ModelId", solid.MetaData.Get<string>("ModelId") ?? "0", wr, false, 1);
                WriteProperty("GroupId", (vis < 0 ? 0 : vis).ToString(), wr, false, 1);
                WriteProperty("HullSize", solid.MetaData.Get<string>("HullSize") ?? "1.000000", wr, false, 1);
                WriteProperty("Type", solid.MetaData.Get<string>("Type") ?? "2", wr, false, 1);
                WriteProperty("BrushFaces", solid.Faces.Count().ToString(), wr, false, 1);
                foreach (var face in solid.Faces)
                    WriteFace(face, wr);
            }
        }


        private void WriteEntity(Entity entity, StreamWriter wr)
        {
            WriteProperty("CEntity", "", wr);
            WriteProperty("eStyle", "0", wr);
            WriteProperty("eOrigin", FormatIntCoordinate(entity.Origin) + " 0", wr);
            WriteProperty("eFlags", entity.EntityData.Flags.ToString(), wr);
            WriteProperty("eGroup", "0", wr);
            WriteProperty("ePairCount", (entity.EntityData.Properties.Count() + 2).ToString(), wr);

            //Case of class matters
            var cls = ClassList[FindClassIndex(ClassList, entity.ClassName)];
            WriteKeyValue("classname", cls.Description, wr);
            WriteKeyValue("origin", FormatIntCoordinate(entity.Origin), wr);
            //Order does not need to be consistant.
            foreach (var prop in entity.EntityData.Properties)
            {
                int PropIndex = FindEntityPropertyIndex(cls.Properties, prop.Key);

                if (prop.Key == "color")
                {
                    //Assume alpha of 255
                    WriteKeyValue(cls.Properties[PropIndex].ShortDescription, prop.Value.Substring(0, prop.Value.Length - 3).TrimEnd(), wr);
                }
                else if (prop.Key == "angles")
                {
                    var val = prop.Value.Split(' ') ;
                    float a, b, c;
                    a = float.Parse(val[0]); if (a < 0) a = 360 + a; if (a != 0) a = 360 - a;
                    b = float.Parse(val[1]); if (b < 0) b = 360 + b;
                    c = float.Parse(val[2]); if (c < 0) c = 360 + c; 
                    string valout = c.ToString() + ' ' + b.ToString() + ' ' + a.ToString();
                    WriteKeyValue(cls.Properties[PropIndex].ShortDescription, valout, wr);

                }
                else WriteKeyValue(cls.Properties[PropIndex].ShortDescription, prop.Value, wr);
            }

            WriteProperty("End", "CEntity", wr);
        }
        private void WriteMapStats(Dictionary<string, string> stats, List<Solid> solids, List<Entity> entities, DataStructures.MapObjects.Map map, StreamWriter wr)
        {
            //WriteProperty("3dtVersion", "1.31", wr);
            WriteProperty("3dtVersion", "1.32", wr);

            //The count on the Visgroups is off by 1 because it is counting Auto. We do not use Auto in 3DT or RFEdit.
            var NumGroups = (map.Visgroups.Count() - 1).ToString();

            //We know what the map header is supposed to look like now
            WriteProperty("TextureLib", stats["TextureLib"], wr, true);
            WriteProperty("HeadersDir", stats["HeadersDir"], wr, true);
            WriteProperty("NumEntities", entities.Count.ToString(), wr);
            WriteProperty("NumModels", stats["NumModels"], wr);
            WriteProperty("NumGroups", NumGroups, wr);
            WriteProperty("Brushlist", solids.Count.ToString(), wr);


            /*
            foreach(var entry in stats)
            {
                // skippers
                if (entry.Key == "ActorsDir" || entry.Key == "PawnIni")
                    continue;

                if (entry.Key == "TextureLib" || entry.Key == "HeadersDir")
                    WriteProperty(entry.Key, entry.Value, wr, true);
                else
                    WriteProperty(entry.Key, entry.Value, wr);
            }
            */
        }
        private void WriteMotions(List<Motion> motions, StreamWriter wr)
        {
            foreach (var motion in motions)
            {
                foreach (var line in motion.RawModelLines)
                    wr.WriteLine(line);
            }
        }
        
        /// <summary>
        /// Reads a map from a stream in 3DT format.
        /// </summary>
        /// <param name="stream">The stream to read from</param>
        /// <returns>The parsed map</returns>
        protected override DataStructures.MapObjects.Map GetFromStream(Stream stream, string fgd = null)
        {
            using (var rdr = new StreamReader(stream))
            {
                var map = new DataStructures.MapObjects.Map();
                
                var stats = ReadMapStats(rdr);
                map.WorldSpawn.MetaData.Set("stats", stats);

                var gd = GameData.GameDataProvider.GetGameDataFromFile(fgd);
                gd.CreateDependencies();
                ClassList = gd.Classes;

                EntityCounts = new Dictionary<string, UInt32>();
                for (int x = 0; x < ClassList.Count(); x++)
                {
                    if (ClassList[x].ClassType != DataStructures.GameData.ClassType.Base) EntityCounts.Add(ClassList[x].Name, 0);
                }

                var allEntities = ReadAllEntities(rdr, map.IDGenerator);
                var worldspawn = allEntities.FirstOrDefault(x => x.EntityData.Name == "worldspawn")
                                 ?? new Entity(0) { EntityData = { Name = "worldspawn" } };
                allEntities.Remove(worldspawn);
                map.WorldSpawn.EntityData = worldspawn.EntityData;
                allEntities.ForEach(x => x.SetParent(map.WorldSpawn, false));
                foreach (var obj in worldspawn.GetChildren().ToArray())
                {
                    obj.SetParent(map.WorldSpawn, false);
                }
                map.WorldSpawn.UpdateBoundingBox(false);

                var numMotions = int.Parse(stats["NumModels"]);
                map.Motions = ReadMotions(numMotions, rdr);
                var numGroups = int.Parse(stats["NumGroups"]);
                map.Visgroups.AddRange(ReadGroups(numGroups, rdr));

                string other = rdr.ReadToEnd();
                map.WorldSpawn.MetaData.Set("stuff", other);
                
                
                Dictionary<string, UInt32> CustomBrushFlags = new Dictionary<string, UInt32>();

                int Index = FindClassIndex(ClassList, "BaseEnums");
                var enums = ClassList[Index].Properties.ToArray();
                foreach(Chisel.DataStructures.GameData.Property p in enums)
                {
                    CustomBrushFlags[p.Name] = Convert.ToUInt32(p.DefaultValue, 16);
                }
                map.WorldSpawn.MetaData.Set("CustomBrushFlags", CustomBrushFlags);
                map.WorldSpawn.MetaData.Set("EntityCounts", EntityCounts);

                return map;
            }
        }

        private static void FlattenTree(MapObject parent, List<Solid> solids, List<Entity> entities, List<Group> groups)
        {
            foreach (var mo in parent.GetChildren())
            {
                if (mo is Solid)
                {
                    solids.Add((Solid)mo);
                }
                else if (mo is Entity)
                {
                    entities.Add((Entity)mo);
                }
                else if (mo is Group)
                {
                    groups.Add((Group)mo);
                    FlattenTree(mo, solids, entities, groups);
                }
            }
        }

        protected override void SaveToStream(Stream stream, DataStructures.MapObjects.Map map)
        {
            using (var sw = new StreamWriter(stream))
            {
                // Gather everything we need
                var solids = new List<Solid>();
                var entities = new List<Entity>();
                var groups = new List<Group>();
                var stats = map.WorldSpawn.MetaData.Get<Dictionary<string, string>>("stats");

                // Populate the solids entities and groups
                FlattenTree(map.WorldSpawn, solids, entities, groups);

                // Write Map Header
                WriteMapStats(stats, solids, entities, map, sw);
                
                // write world entity aka Brushes
                WriteWorldEntity(solids, sw);

                // write other entities
                WriteProperty("Class", "CEntList", sw);
                WriteProperty("EntCount", entities.Count().ToString(), sw);
                WriteProperty("CurEnt", "0", sw);

                foreach (var entity in entities)
                {
                    WriteEntity(entity, sw);
                }

                // write motions
                WriteMotions(map.Motions, sw);

                // write groups
                foreach (var visgroup in map.Visgroups)
                {
                    // Do not save the group automatically created by Chisel
                    if (visgroup.Name == "Auto")
                        continue;

                    WriteProperty("Group", visgroup.Name, sw, true);
                    WriteProperty("GroupId", visgroup.ID.ToString(), sw, false, 1);
                    WriteProperty("Visible", visgroup.Visible ? "1" : "0", sw, false, 1);
                    WriteProperty("Locked", "0", sw, false, 1);
                    WriteProperty("Color", FormatColor(visgroup.Colour), sw);
                }

                /* Extra ArchData for 3DT Version >= 1.34, that we need to skip for outputting version 1.31
                Sides 3
                CW 0
                Shape 0
                Radius2 64.000000
                Height 0.000000
                Massive 0
                Steps 0
                */

                var lastBlock = map.WorldSpawn.MetaData.Get<string>("stuff");
                var lines = lastBlock.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                bool inArch = false, archTCut = false;
                foreach (var lineUntrimmed in lines)
                {
                    var line = lineUntrimmed.Trim();

                    
                    if (string.Equals(line, "ArchTemplate", StringComparison.OrdinalIgnoreCase))
                    {
                        inArch = true;
                    }
                    else if (line.EndsWith("Template", StringComparison.OrdinalIgnoreCase))
                    {
                        // Lines ending in "Template" are not in the ArchTemplate, 
                        inArch = false;
                    }
                    else if (line.StartsWith("TemplatePos", StringComparison.OrdinalIgnoreCase))
                    {
                        // lines which are TemplatePos signify the ending 3 lines of the 3dt
                        inArch = false;
                    }

                    // If we are in the arch template, and the line is TCut, then we have reached the
                    // end of the valid arch template data in 3DT version < 1.34. 
                    var isTCut = line.StartsWith("TCut", StringComparison.OrdinalIgnoreCase);
                    if (inArch && isTCut)
                        archTCut = true;

                    // The rest of the arch template data we don't care about
                    if (inArch && archTCut && !isTCut)
                        continue;

                    sw.WriteLine(line);
                }

                //sw.Write(map.WorldSpawn.MetaData.Get<string>("stuff"));
            }
        }
    }
}
