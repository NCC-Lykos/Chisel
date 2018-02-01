using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using Chisel.DataStructures.Geometric;
using Chisel.DataStructures.Transformations;
using Chisel.Extensions;
using System.Diagnostics;

namespace Chisel.DataStructures.MapObjects
{
    [Serializable]
    public class Face : ISerializable
    {
        public long ID { get; set; }
        public Color Colour { get; set; }
        public Plane Plane { get; set; }

        public bool IsSelected { get; set; }
        public bool IsHidden { get; set; }


        public int Light { get; set; }

        public Coordinate LightScale { get; set; }

        public TextureReference Texture { get; set; }
        public List<Vertex> Vertices { get; set; }

        public Solid Parent { get; set; }

        public Box BoundingBox { get; set; }

        public Color HighlightColor { get; set; }
        public Color WireframeColor { get; set; }
        public bool IgnoreTexture { get; set; }

        public Face(long id)
        {
            ID = id;
            Texture = new TextureReference();
            Vertices = new List<Vertex>();
            IsSelected = false;
        }

        protected Face(SerializationInfo info, StreamingContext context)
        {
            ID = info.GetInt64("ID");
            Colour = Color.FromArgb(info.GetInt32("Colour"));
            Plane = (Plane)info.GetValue("Plane", typeof(Plane));
            Texture = (TextureReference)info.GetValue("Texture", typeof(TextureReference));
            Vertices = ((Vertex[])info.GetValue("Vertices", typeof(Vertex[]))).ToList();
            Vertices.ForEach(x => x.Parent = this);
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ID", ID);
            info.AddValue("Colour", Colour.ToArgb());
            info.AddValue("Plane", Plane);
            info.AddValue("Texture", Texture);
            info.AddValue("Vertices", Vertices.ToArray());
        }

        public virtual Face Copy(IDGenerator generator)
        {
            var f = new Face(generator.GetNextFaceID())
            {
                Plane = Plane.Clone(),
                Colour = Colour,
                IsSelected = IsSelected,
                IsHidden = IsHidden,
                Texture = Texture.Clone(),
                Parent = Parent,
                BoundingBox = BoundingBox.Clone()
            };
            foreach (var v in Vertices.Select(x => x.Clone()))
            {
                v.Parent = f;
                f.Vertices.Add(v);
            }
            return f;
        }

        public virtual Face Clone()
        {
            var f = Copy(new IDGenerator());
            f.ID = ID;
            return f;
        }

        public virtual void Paste(Face f)
        {
            Plane = f.Plane.Clone();
            Colour = f.Colour;
            IsSelected = f.IsSelected;
            IsHidden = f.IsHidden;
            Texture = f.Texture.Clone();
            Parent = f.Parent;
            BoundingBox = f.BoundingBox.Clone();
            Vertices.Clear();
            foreach (var v in f.Vertices.Select(x => x.Clone()))
            {
                v.Parent = this;
                Vertices.Add(v);
            }
        }

        public virtual void Unclone(Face f)
        {
            Paste(f);
            ID = f.ID;
        }

        public virtual IEnumerable<Line> GetLines()
        {
            return GetEdges();
        }

        public virtual IEnumerable<Line> GetEdges()
        {
            for (var i = 0; i < Vertices.Count; i++)
            {
                yield return new Line(Vertices[i].Location, Vertices[(i + 1) % Vertices.Count].Location);
            }
        }

        public virtual IEnumerable<Vertex> GetIndexedVertices()
        {
            return Vertices;
        }

        public virtual IEnumerable<uint> GetTriangleIndices()
        {
            for (uint i = 1; i < Vertices.Count - 1; i++)
            {
                yield return 0;
                yield return i;
                yield return i + 1;
            }
        }

        public virtual IEnumerable<uint> GetLineIndices()
        {
            for (uint i = 0; i < Vertices.Count; i++)
            {
                var ni = (uint)((i + 1) % Vertices.Count);
                yield return i;
                yield return ni;
            }
        }

        public virtual IEnumerable<Vertex[]> GetTriangles()
        {
            for (var i = 1; i < Vertices.Count - 1; i++)
            {
                yield return new[]
                                 {
                                     Vertices[0],
                                     Vertices[i],
                                     Vertices[i + 1]
                                 };
            }
        }

        public IEnumerable<Vertex> GetNonPlanarVertices(decimal epsilon = 0.001m)
        {
            return Vertices.Where(x => Plane.OnPlane(x.Location, epsilon) != 0);
        }

        public bool IsConvex(decimal epsilon = 0.001m)
        {
            return new Polygon(Vertices.Select(x => x.Location)).IsConvex(epsilon);
        }

        #region Textures

        public enum BoxAlignMode
        {
            Left,
            Right,
            Center,
            Top,
            Bottom
        }

        public virtual void CalculateTextureCoordinates(bool minimizeShiftValues)
        {
            if (minimizeShiftValues) MinimiseTextureShiftValues();
            Vertices.ForEach(c => c.TextureU = c.TextureV = 0);

            if (Texture.Texture == null) return;
            if (Texture.Texture.Width == 0 || Texture.Texture.Height == 0) return;
            if (Texture.XScale == 0 || Texture.YScale == 0) return;

            var udiv = Texture.Texture.Width * Texture.XScale;
            var uadd = Texture.XShift / Texture.Texture.Width;
            var vdiv = Texture.Texture.Height * Texture.YScale;
            var vadd = Texture.YShift / Texture.Texture.Height;

            foreach (var v in Vertices)
            {
                if (IsTextureAlignedToWorld())
                {
                    v.TextureU = (v.Location.Dot(Texture.UAxis) / udiv) + uadd;
                    v.TextureV = (v.Location.Dot(Texture.VAxis) / vdiv) + vadd;
                } else
                {
                    decimal x, y;
                    Coordinate uVec, vVec;

                    uVec = Texture.UAxis.Clone();
                    vVec = Texture.VAxis.Clone();

                    uVec *= (1 / Texture.XScale);
                    vVec *= (1 / Texture.YScale);

                    //Match precision of RF.
                    uVec.X = (decimal)((float)uVec.X);
                    uVec.Y = (decimal)((float)uVec.Y);
                    uVec.Z = (decimal)((float)uVec.Z);
                    vVec.X = (decimal)((float)vVec.X);
                    vVec.Y = (decimal)((float)vVec.Y);
                    vVec.Z = (decimal)((float)vVec.Z);

                    float uO, vO;
                    uO = (float)uVec.Dot(Texture.PositionRF);
                    vO = (float)vVec.Dot(Texture.PositionRF);

                    x = (Texture.XShift - (decimal)uO) / Texture.Texture.Width;
                    y = (Texture.YShift - (decimal)vO) / Texture.Texture.Height;

                    v.TextureU = (v.Location.Dot(uVec) / Texture.Texture.Width) + (x);
                    v.TextureV = (v.Location.Dot(vVec) / Texture.Texture.Height) + (y);

                }
            }
        }

        private Matrix MatrixMultiplyRF(Matrix m1, Matrix m2)
        {
            Matrix r = new Matrix();
            /*
            0-AX  1-AY  2-AZ
            4-BX  5-BY  6-BZ
            8-CX  9-CY 10-CZ
            3-TX  7-TY 11-TZ
            */
            decimal AX1, AY1, AZ1, AX2, AY2, AZ2;
            decimal BX1, BY1, BZ1, BX2, BY2, BZ2;
            decimal CX1, CY1, CZ1, CX2, CY2, CZ2;
            decimal TX1, TY1, TZ1, TX2, TY2, TZ2;
            AX1 = m1.Values[0]; AY1 = m1.Values[1]; AZ1 = m1.Values[2];
            BX1 = m1.Values[4]; BY1 = m1.Values[5]; BZ1 = m1.Values[6];
            CX1 = m1.Values[8]; CY1 = m1.Values[9]; CZ1 = m1.Values[10];
            TX1 = m1.Values[3]; TY1 = m1.Values[7]; TZ1 = m1.Values[11];

            AX2 = m2.Values[0]; AY2 = m2.Values[1]; AZ2 = m2.Values[2];
            BX2 = m2.Values[4]; BY2 = m2.Values[5]; BZ2 = m2.Values[6];
            CX2 = m2.Values[8]; CY2 = m2.Values[9]; CZ2 = m2.Values[10];
            TX2 = m2.Values[3]; TY2 = m2.Values[7]; TZ2 = m2.Values[11];

            r.Values[0] = AX1 * AX2 + AY1 * BX2 + AZ1 * CX2;
            r.Values[1] = AX1 * AY2 + AY1 * BY2 + AZ1 * CY2;
            r.Values[2] = AX1 * AZ2 + AY1 * BZ2 + AZ1 * CZ2;

            r.Values[4] = BX1 * AX2 + BY1 * BX2 + BZ1 * CX2;
            r.Values[5] = BX1 * AY2 + BY1 * BY2 + BZ1 * CY2;
            r.Values[6] = BX1 * AZ2 + BY1 * BZ2 + BZ1 * CZ2;

            r.Values[8] = CX1 * AX2 + CY1 * BX2 + CZ1 * CX2;
            r.Values[9] = CX1 * AY2 + CY1 * BY2 + CZ1 * CY2;
            r.Values[10] = CX1 * AZ2 + CY1 * BZ2 + CZ1 * CZ2;

            r.Values[3] = AX1 * TX2 + AY1 * TY2 + AZ1 * TZ2 + TX1;
            r.Values[7] = BX1 * TX2 + BY1 * TY2 + BZ1 * TZ2 + TY1;
            r.Values[11] = CX1 * TX2 + CY1 * TY2 + CZ1 * TZ2 + TX1;
            return r;
        }
        private Matrix QuaternionToMatrixRF(Quaternion q)
        {
            Matrix r = new Matrix();
            decimal X2, Y2, Z2;     //2*QX, 2*QY, 2*QZ
            decimal XX2, YY2, ZZ2;  //2*QX*QX, 2*QY*QY, 2*QZ*QZ
            decimal XY2, XZ2, XW2;  //2*QX*QY, 2*QX*QZ, 2*QX*QW
            decimal YZ2, YW2, ZW2;    // ...

            X2 = 2 * q.X; XX2 = X2 * q.X; XY2 = X2 * q.Y; XZ2 = X2 * q.Z; XW2 = X2 * q.W;
            Y2 = 2 * q.Y; YY2 = Y2 * q.Y; YZ2 = Y2 * q.Z; YW2 = Y2 * q.W;
            Z2 = 2 * q.Z; ZZ2 = Z2 * q.Z; ZW2 = Z2 * q.W;

            /*
            0-AX  1-AY  2-AZ
            4-BX  5-BY  6-BZ
            8-CX  9-CY 10-CZ
            3-TX  7-TY 11-TZ
            */

            r.Values[0] = 1 - YY2 - ZZ2; r.Values[1] = XY2 - ZW2; r.Values[2] = XZ2 + YW2;
            r.Values[4] = XY2 + ZW2; r.Values[5] = 1 - XX2 - ZZ2; r.Values[6] = YZ2 - XW2;
            r.Values[8] = XZ2 - YW2; r.Values[9] = YZ2 + XW2; r.Values[10] = 1 - XX2 - YY2;
            r.Values[3] = r.Values[7] = r.Values[11] = 0;

            return r;
        }
        private Matrix SetZRotationRF(double Rotation)
        {
            //0-AX  1-AY  2-AZ 3-TX
            //4-BX  5-BY  6-BZ 7-TY
            //8-CX  9-CY 10-CZ 11-TZ
            Matrix r = new Matrix();
            double cos, sin;
            cos = Math.Cos(Rotation);
            sin = Math.Sin(Rotation);

            for (int x = 0; x < 16; x++)
            {
                r.Values[x] = 0;
            }

            r.Values[0] = r.Values[5] = (decimal)cos;
            r.Values[1] = (decimal)-sin;
            r.Values[4] = (decimal)sin;
            r.Values[10] = 1;

            return r;
        }
        private Coordinate RotateRF(Matrix m, Coordinate p)
        {
            //0-AX  1-AY  2-AZ 3-TX
            //4-BX  5-BY  6-BZ 7-TY
            //8-CX  9-CY 10-CZ 11-TZ
            Coordinate r = new Coordinate(0, 0, 0);

            r.X = (p.X * m.Values[0]) + (p.Y * m.Values[1]) + (p.Z * m.Values[2]);
            r.Y = (p.X * m.Values[4]) + (p.Y * m.Values[5]) + (p.Z * m.Values[6]);
            r.Z = (p.X * m.Values[8]) + (p.Y * m.Values[9]) + (p.Z * m.Values[10]);

            return r;
        }

        private Coordinate ToRF(Coordinate c)
        {
            //X,-Z,Y
            // ->
            //X,Y,-Z
            Coordinate r = new Coordinate(c.X, c.Z, -c.Y);
            return r;
        }
        private Coordinate ToChisel(Coordinate c)
        {
            //X,Y,-Z
            // ->
            //X,-Z,Y
            Coordinate r = new Coordinate(c.X, -c.Z, c.Y);
            return r;
        }

        private int MatrixDirection(Matrix m)
        {
            int r = 0;
            decimal x, y, z;
            //0-AX  1-AY  2-AZ 3-TX
            //4-BX  5-BY  6-BZ 7-TY
            //8-CX  9-CY 10-CZ 11-TZ

            //X = 1, Y = 2, Z = 3
            x = m.Values[0]; y = m.Values[5]; z = m.Values[10];
            if (x > y && x > z) r = 1;
            else if (y > x && y > z) r = 2;
            else if (z > x && z > y) r = 3;
            else if (x == z || x == y) r = 1; // Multi axis rotation, not supported in RF, all bets off
            else if (z == y) r = 3; // Multi axis rotation, not supported in RF, all bets off

            return r;
        }

        private Matrix ToRF(Matrix m)
        {
            int x = MatrixDirection(m);
            Matrix r = new Matrix();

            switch (x)
            {
                case 1: //x
                    r = m;
                    break;
                case 2: //y->x
                    r.Values[0] = m.Values[0]; r.Values[1] = m.Values[2]; r.Values[2] = m.Values[1];
                    r.Values[4] = m.Values[8]; r.Values[5] = m.Values[10]; r.Values[6] = m.Values[9];
                    r.Values[8] = m.Values[4]; r.Values[9] = m.Values[6]; r.Values[10] = m.Values[5];
                    break;
                case 3: //z->x
                    r.Values[0] = m.Values[5]; r.Values[1] = m.Values[6]; r.Values[2] = m.Values[4];
                    r.Values[4] = m.Values[9]; r.Values[5] = m.Values[10]; r.Values[6] = m.Values[8];
                    r.Values[8] = m.Values[1]; r.Values[9] = m.Values[2]; r.Values[10] = m.Values[0];
                    break;
            }
            return r;
        }
        private TransformFlags GetTransformFlags(Matrix m)
        {
            TransformFlags f = (TransformFlags)0;

            //0-AX  1-AY  2-AZ 3-TX
            //4-BX  5-BY  6-BZ 7-TY
            //8-CX  9-CY 10-CZ 11-TZ
            if (m.Values[0] == 1 && m.Values[5] == 1 && m.Values[10] == 1)
            {
                //if everything else is zero than its a move else skew;
                if (Math.Round(m.Values[1], 4) == 0 && Math.Round(m.Values[2], 4) == 0 && Math.Round(m.Values[6], 4) == 0 &&
                    Math.Round(m.Values[4], 4) == 0 && Math.Round(m.Values[9], 4) == 0 && Math.Round(m.Values[8], 4) == 0) f |= TransformFlags.Translate;
                else f |= TransformFlags.Skew;
            }
            else if (Math.Round(m.Values[1], 4) == 0 && Math.Round(m.Values[2], 4) == 0 && Math.Round(m.Values[6], 4) == 0 &&
                     Math.Round(m.Values[4], 4) == 0 && Math.Round(m.Values[9], 4) == 0 && Math.Round(m.Values[8], 4) == 0)
            {
                f |= TransformFlags.Translate;
            }
            else if (m.Values[0] == 1 || m.Values[5] == 1 || m.Values[10] == 1)
            {
                f |= TransformFlags.Rotation;
            }


            return f;
        }
        
        public void InitFaceAngle()
        {
            
            Plane RFPlane = new Plane(ToRF(Vertices[0].Location), ToRF(Vertices[1].Location), ToRF(Vertices[2].Location));
            Coordinate ax,p2 = RFPlane.Normal, p = RFPlane.Normal.Absolute();
            Matrix r = new Matrix();
            double cosV, theta;

            if (p.X > p.Y)
            {
                if (p.X > p.Z) { if (p2.X > 0) p2 *= -1; }
                else { if (p2.Z > 0) p2 *= -1; }
            } else {
                if (p.Y > p.Z) { if (p2.Y > 0) p2 *= -1; }
                else { if (p2.Z > 0) p2 *= -1; }
            }
            Coordinate d = new Coordinate(0,0,1);
            ax = d.Cross(p2);
            cosV = (double)d.Dot(p2);
            if (cosV > 1) cosV = 1;
            if (cosV < -1) cosV = -1;
            theta = Math.Acos(cosV);
            double sinT, cosT;
            ax = ax.Normalise();
            if ((ax.X == 0) && (ax.Y == 0) && (ax.Z == 0))
            {
                r.Values[0] = r.Values[5] = r.Values[10] = 1; //Set Identity
                Matrix t = SetZRotationRF(-theta);
                r = MatrixMultiplyRF(t, r);
            }
            else
            {
                theta = theta * 0.5f;
                sinT = Math.Sin(-theta); cosT = Math.Cos(-theta);
                ax *= (decimal)sinT;
                Quaternion q = new Quaternion(ax, (decimal)cosT);
                r = QuaternionToMatrixRF(q);
            }

            Texture.TransformAngleRF = r;
        }

        private UInt32 DetermineAxis(Coordinate v)
        {
            UInt32 Result = 0;
            float x, y, z;
            //NOTE(SVK): used the RF xyz coords here
            //Round is a hack to avoid precision issues with better datatypes used in code.
            //TODO(SVK): Calc a plane normal that matches in precision RF
            x = (float)Math.Round(Math.Abs(v.X), 7);
            y = (float)Math.Round(Math.Abs(v.Z), 7);
            z = (float)Math.Round(Math.Abs(v.Y), 7);

            if (y > x)
            {
                if (z > y) { Result = 2; }
                else { Result = 1; }
            }
            else if (z > x) { Result = 2; }
            return (Result);
        }

        private void AlignTextureToWorld()
        {
            float cosA, sinA, angle = (((float)-Texture.Rotation * (float)3.14159265358979323846f) / (float)180.0f);
            sinA = (float)Math.Sin(angle);
            cosA = (float)Math.Cos(angle);
            UInt32 Axis = DetermineAxis(this.Plane.Normal);

            //Note Y is Z and Z is Y
            switch (Axis)
            {
                case 0:
                    Texture.UAxis = new Coordinate(0, (decimal)-cosA, (decimal)sinA);
                    Texture.VAxis = new Coordinate(0, (decimal)-sinA, (decimal)-cosA);
                    break;
                case 1:
                    Texture.UAxis = new Coordinate((decimal)cosA, (decimal)-sinA, 0);
                    Texture.VAxis = new Coordinate((decimal)-sinA, (decimal)-cosA, 0);
                    break;
                case 2:
                    Texture.UAxis = new Coordinate((decimal)cosA, 0, (decimal)sinA);
                    Texture.VAxis = new Coordinate((decimal)sinA, 0, (decimal)-cosA);
                    break;
            }
            CalculateTextureCoordinates(true);

        }
        
        private void AlignTextureToFace()
        {
            //0-AX  1-AY  2-AZ 3-TX
            //4-BX  5-BY  6-BZ 7-TY
            //8-CX  9-CY 10-CZ 11-TZ
            
            //Clear Rotation
            Texture.TransformAngleRF.Values[3] = Texture.TransformAngleRF.Values[7] = Texture.TransformAngleRF.Values[11] = 0;
            UInt32 Axis = DetermineAxis(this.Plane.Normal);

            Matrix t = SetZRotationRF(((double)Texture.Rotation * Math.PI / 180.0f));
            
            t = Texture.TransformAngleRF * t;
            
            // Z = Y
            // Y = -Z
            switch (Axis)
            {
                case 0: //X
                    Texture.UAxis = new Coordinate(-t.X.X, t.Z.X, -t.Y.X);
                    Texture.VAxis = new Coordinate(-t.X.Y, t.Z.Y, -t.Y.Y);
                    break;
                case 1: //Y
                    Texture.UAxis = new Coordinate(t.X.X, -t.Z.X, t.Y.X);
                    Texture.VAxis = new Coordinate(-t.X.Y, t.Z.Y, -t.Y.Y);
                    break;
                case 2: //Z
                    Texture.UAxis = new Coordinate(t.X.X, -t.Z.X, t.Y.X);
                    Texture.VAxis = new Coordinate(t.X.Y, -t.Z.Y, t.Y.Y);
                    break;
            }

            //Scaling set in calc texter coords
            
            CalculateTextureCoordinates(true);
        }

        public void FlipTransform(Matrix m, int d)
        {
            

            switch (d)
            {
                case 1: //x i.e. u
                    Texture.TransformAngleRF.Values[0] *= -1;
                    Texture.TransformAngleRF.Values[4] *= -1;
                    Texture.TransformAngleRF.Values[8] *= -1;
                    break;
                case 2:
                    Texture.TransformAngleRF.Values[1] *= -1;
                    Texture.TransformAngleRF.Values[5] *= -1;
                    Texture.TransformAngleRF.Values[9] *= -1;
                    break;
            }
        }
        
        public bool IsTextureAlignedToWorld()
        {
            return !Texture.Flags.HasFlag(FaceFlags.TextureLocked);
        }

        public bool IsTextureAlignedToFace()
        {
            return Texture.Flags.HasFlag(FaceFlags.TextureLocked);
        }

        public void AlignTexture()
        {
            if (IsTextureAlignedToWorld()) AlignTextureToWorld();
            else AlignTextureToFace();
        }
        
        private void MinimiseTextureShiftValues()
        {
            if (Texture.Texture == null) return;
            // Keep the shift values to a minimum
            Texture.XShift = Texture.XShift % Texture.Texture.Width;
            Texture.YShift = Texture.YShift % Texture.Texture.Height;
            if (Texture.XShift < -Texture.Texture.Width / 2m) Texture.XShift += Texture.Texture.Width;
            if (Texture.YShift < -Texture.Texture.Height / 2m) Texture.YShift += Texture.Texture.Height;
        }

        public void FitTextureToPointCloud(Cloud cloud, int tileX, int tileY)
        {
            if (Texture.Texture == null) return;
            if (tileX <= 0) tileX = 1;
            if (tileY <= 0) tileY = 1;

            // Scale will change, no need to use it in the calculations
            var xvals = cloud.GetExtents().Select(x => x.Dot(Texture.UAxis)).ToList();
            var yvals = cloud.GetExtents().Select(x => x.Dot(Texture.VAxis)).ToList();

            var minU = xvals.Min();
            var minV = yvals.Min();
            var maxU = xvals.Max();
            var maxV = yvals.Max();

            Texture.XScale = (maxU - minU) / (Texture.Texture.Width * tileX);
            Texture.YScale = (maxV - minV) / (Texture.Texture.Height * tileY);
            Texture.XShift = -minU / Texture.XScale;
            Texture.YShift = -minV / Texture.YScale;

            CalculateTextureCoordinates(true);
        }

        public void AlignTextureWithPointCloud(Cloud cloud, BoxAlignMode mode)
        {
            if (Texture.Texture == null) return;

            var xvals = cloud.GetExtents().Select(x => x.Dot(Texture.UAxis) / Texture.XScale).ToList();
            var yvals = cloud.GetExtents().Select(x => x.Dot(Texture.VAxis) / Texture.YScale).ToList();

            var minU = xvals.Min();
            var minV = yvals.Min();
            var maxU = xvals.Max();
            var maxV = yvals.Max();

            switch (mode)
            {
                case BoxAlignMode.Left:
                    Texture.XShift = -minU;
                    break;
                case BoxAlignMode.Right:
                    Texture.XShift = -maxU + Texture.Texture.Width;
                    break;
                case BoxAlignMode.Center:
                    var avgU = (minU + maxU) / 2;
                    var avgV = (minV + maxV) / 2;
                    Texture.XShift = -avgU + Texture.Texture.Width / 2m;
                    Texture.YShift = -avgV + Texture.Texture.Height / 2m;
                    break;
                case BoxAlignMode.Top:
                    Texture.YShift = -minV;
                    break;
                case BoxAlignMode.Bottom:
                    Texture.YShift = -maxV + Texture.Texture.Height;
                    break;
            }
            CalculateTextureCoordinates(true);
        }
        
        public void SetTextureRotation(decimal rotate)
        {
            Texture.Rotation = rotate;
        }
        #endregion

        public void SetOpacity()
        {
            if (Texture.Flags.HasFlag(FaceFlags.Transparent)) Texture.Opacity = Texture.Translucency / 255.0m;
            else Texture.Opacity = 1;
        }

        public void SetHighlights(UInt32 s = 0)
        {
            if (Texture.Flags.HasFlag(FaceFlags.FullBright)) HighlightColor = Color.FromArgb(255, 255, 255, (int)(0.5 * 255));
            else if (Texture.Flags.HasFlag(FaceFlags.Sky)) HighlightColor = Color.FromArgb(255, 0, 255, 255);
            else if (Texture.Flags.HasFlag(FaceFlags.Light)) HighlightColor = Color.FromArgb(255, 0, 0, 255);
            else HighlightColor = new Color();
            
            if ((s & (UInt32)SolidFlags.hint) != 0)
            {
                WireframeColor = Color.FromArgb(255, 0, 255, 0);//Green
                Texture.Opacity = 0.2m;
                IgnoreTexture = true;
            }
            //(s.HasFlag(SolidFlags.clip))
            else if ((s & (UInt32)SolidFlags.clip) != 0)
            {
                WireframeColor = Color.FromArgb(255, (int)(0.5 * 255), 0, 255);//Purple
                Texture.Opacity = 0.2m;
                IgnoreTexture = true;
            }
            //(s.HasFlag(SolidFlags.window) && !Texture.Flags.HasFlag(FaceFlags.Transparent))
            else if (((s & (UInt32)SolidFlags.window) != 0) && !Texture.Flags.HasFlag(FaceFlags.Transparent))
            {
                WireframeColor = Color.FromArgb(255, 255, (int)(0.5 * 255), 0);//Orange?
                Texture.Opacity = 0.2m;
                IgnoreTexture = true;
            }
            else if (Parent.MetaData.Get<string>("ModelId") != null && (int.Parse(Parent.MetaData.Get<string>("ModelId")) > 0))
            {
                WireframeColor = Color.FromArgb(255, 255, 255, 255);//White
                SetOpacity();
                IgnoreTexture = false;
            }
            else if (!WireframeColor.IsEmpty)
            {
                WireframeColor = new Color();
                SetOpacity();
                IgnoreTexture = false;
            }

        }

        public virtual void UpdateBoundingBox()
        {
            BoundingBox = new Box(Vertices.Select(x => x.Location));
        }
        
        public virtual void Transform(IUnitTransformation transform, TransformFlags flags)
        {
            foreach (var t in Vertices)
            {
                t.Location = transform.Transform(t.Location);
            }
            Plane = new Plane(Vertices[0].Location, Vertices[1].Location, Vertices[2].Location);
            Colour = Colour;

            var origin = transform.Transform(Coordinate.Zero);
            var ua = transform.Transform(Texture.UAxis) - origin;
            var va = transform.Transform(Texture.VAxis) - origin;

            if (flags.HasFlag(TransformFlags.TextureScalingLock) && Texture.Texture != null)
            {
                // Make a best-effort guess of retaining scaling. All bets are off during skew operations.
                // Transform the current texture axes
                // Multiply the scales by the magnitudes (they were normals before the transform operation)
                Texture.XScale *= ua.VectorMagnitude();
                Texture.YScale *= va.VectorMagnitude();
            }
            {
                //NOTE(SVK):Only edit solids not entities. //do entities have faces??
                if(this.Parent.Parent.GetType().Name == "World" && this.Parent.GetType().Name == "Solid")
                {
                    if (Texture.Flags.HasFlag(FaceFlags.TextureLocked))
                    {
                        Coordinate Center = this.Parent.Parent.SelCenter;
                        Matrix Xfrm = transform.GetMatrix();
                        TransformFlags f = (TransformFlags)0;
                        if (Xfrm != Matrix.Zero) f = GetTransformFlags(Xfrm);
                        else f = flags;
                        

                        if (f.HasFlag(TransformFlags.Translate))
                        {
                            Texture.PositionRF = transform.Transform(Texture.PositionRF);
                        }
                        
                        if (f.HasFlag(TransformFlags.Rotation))
                        {
                            Xfrm = ToRF(Xfrm);

                            Texture.TransformAngleRF = Xfrm * Texture.TransformAngleRF;
                            Texture.PositionRF = Texture.PositionRF - Center;
                            Texture.PositionRF = RotateRF(Xfrm, ToRF(Texture.PositionRF));
                            Texture.PositionRF = ToChisel(Texture.PositionRF);
                            Texture.PositionRF = Texture.PositionRF + Center;
                        }
                    }
                }
                else //entities etc...
                {
                    // Transform the texture axes and move them back to the origin
                    // Only do the transform if the axes end up being not perpendicular
                    // Otherwise just make a best-effort guess, same as the scaling lock

                    if (Math.Abs(ua.Dot(va)) < 0.0001m && DMath.Abs(Plane.Normal.Dot(ua.Cross(va).Normalise())) > 0.0001m)
                    {
                        Texture.UAxis = ua;
                        Texture.VAxis = va;
                    }
                    else
                    {
                        AlignTextureToWorld();
                    }
                    if (flags.HasFlag(TransformFlags.TextureLock) && Texture.Texture != null)
                    {
                        // Check some original reference points to see how the transform mutates them
                        var scaled = (transform.Transform(Coordinate.One) - transform.Transform(Coordinate.Zero)).VectorMagnitude();
                        var original = (Coordinate.One - Coordinate.Zero).VectorMagnitude();
                        // Ignore texture lock when the transformation contains a scale
                        if (DMath.Abs(scaled - original) <= 0.01m)
                        {
                            // Calculate the new shift values based on the UV values of the vertices
                            var vtx = Vertices[0];
                            Texture.XShift = Texture.Texture.Width * vtx.TextureU - (vtx.Location.Dot(Texture.UAxis)) / Texture.XScale;
                            Texture.YShift = Texture.Texture.Height * vtx.TextureV - (vtx.Location.Dot(Texture.VAxis)) / Texture.YScale;
                        }
                    }
                }
            }

            AlignTexture();
            UpdateBoundingBox();
        }

        public virtual void Flip()
        {
            Vertices.Reverse();
            Plane = new Plane(Vertices[0].Location, Vertices[1].Location, Vertices[2].Location);
            UpdateBoundingBox();
        }
        
        /// <summary>
        /// Returns the point that this line intersects with this face.
        /// </summary>
        /// <param name="line">The intersection line</param>
        /// <returns>The point of intersection between the face and the line.
        /// Returns null if the line does not intersect this face.</returns>
        public virtual Coordinate GetIntersectionPoint(Line line)
        {
            return GetIntersectionPoint(Vertices.Select(x => x.Location).ToList(), line);
        }

        /// <summary>
        /// Test all the edges of this face against a bounding box to see if they intersect.
        /// </summary>
        /// <param name="box">The box to intersect</param>
        /// <returns>True if one of the face's edges intersects with the box.</returns>
        public bool IntersectsWithLine(Box box)
        {
            // Shortcut through the bounding box to avoid the line computations if they aren't needed
            return BoundingBox.IntersectsWith(box) && GetLines().Any(box.IntersectsWith);
        }

        /// <summary>
        /// Test this face to see if the given bounding box intersects with it
        /// </summary>
        /// <param name="box">The box to test against</param>
        /// <returns>True if the box intersects</returns>
        public bool IntersectsWithBox(Box box)
        {
            var verts = Vertices.Select(x => x.Location).ToList();
            return box.GetBoxLines().Any(x => GetIntersectionPoint(verts, x, true) != null);
        }

        /// <summary>
        /// Determines if this face is behind, in front, or spanning a plane.
        /// </summary>
        /// <param name="p">The plane to test against</param>
        /// <returns>A PlaneClassification value.</returns>
        public PlaneClassification ClassifyAgainstPlane(Plane p)
        {
            int front = 0, back = 0, onplane = 0, count = Vertices.Count;

            foreach (var test in Vertices.Select(v => v.Location).Select(x => p.OnPlane(x)))
            {
                // Vertices on the plane are both in front and behind the plane in this context
                if (test <= 0) back++;
                if (test >= 0) front++;
                if (test == 0) onplane++;
            }

            if (onplane == count) return PlaneClassification.OnPlane;
            if (front == count) return PlaneClassification.Front;
            if (back == count) return PlaneClassification.Back;
            return PlaneClassification.Spanning;
        }

        protected static Coordinate GetIntersectionPoint(IList<Coordinate> coordinates, Line line, bool ignoreDirection = false)
        {
            var plane = new Plane(coordinates[0], coordinates[1], coordinates[2]);
            var intersect = plane.GetIntersectionPoint(line, ignoreDirection);
            if (intersect == null) return null;

            // http://paulbourke.net/geometry/insidepoly/

            // The angle sum will be 2 * PI if the point is inside the face
            double sum = 0;
            for (var i = 0; i < coordinates.Count; i++)
            {
                var i1 = i;
                var i2 = (i + 1) % coordinates.Count;

                // Translate the vertices so that the intersect point is on the origin
                var v1 = coordinates[i1] - intersect;
                var v2 = coordinates[i2] - intersect;

                var m1 = v1.VectorMagnitude();
                var m2 = v2.VectorMagnitude();
                var nom = m1 * m2;
                if (nom < 0.001m)
                {
                    // intersection is at a vertex
                    return intersect;
                }
                sum += Math.Acos((double)(v1.Dot(v2) / nom));
            }

            var delta = Math.Abs(sum - Math.PI * 2);
            return (delta < 0.001d) ? intersect : null;
        }

        public void Init()
        {
            InitFaceAngle();
            UpdateBoundingBox();
            AlignTexture();
        }
    }
}
