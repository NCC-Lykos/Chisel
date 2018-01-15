using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Chisel.Common;
using Chisel.DataStructures.Geometric;
using Chisel.DataStructures.MapObjects;
using Chisel.DataStructures.Models;
using Chisel.Editor.Extensions;
using Chisel.Graphics.Arrays;
using Chisel.Graphics.Helpers;
using BeginMode = OpenTK.Graphics.OpenGL.BeginMode;
using GL = OpenTK.Graphics.OpenGL.GL;

namespace Chisel.Editor.Rendering.Arrays
{
    public class MapObjectArray : VBO<MapObject, MapObjectVertex>
    {
        private const int Textured = 0;
        private const int Transparent = 1;
        private const int BrushWireframe = 2;
        private const int EntityWireframe = 3;

        public MapObjectArray(IEnumerable<MapObject> data)
            : base(data)
        {
        }

        public void RenderTextured(IGraphicsContext context)
        {
            foreach (var subset in GetSubsets<ITexture>(Textured).Where(x => x.Instance != null))
            {
                var tex = (ITexture)subset.Instance;
                tex.Bind();
                Render(context, PrimitiveType.Triangles, subset);
            }
        }
        public void RenderUntextured(IGraphicsContext context, Coordinate location)
        {
            TextureHelper.Unbind();
            foreach (var subset in GetSubsets<ITexture>(Textured).Where(x => x.Instance == null))
            {
                Render(context, PrimitiveType.Triangles, subset);
            }
            foreach (var subset in GetSubsets<Entity>(Textured))
            {
                var e = (Entity) subset.Instance;
                if (!Chisel.Settings.View.DisableModelRendering && e.HasModel() && e.HideDistance() > (location - e.Origin).VectorMagnitude()) continue;
                Render(context, PrimitiveType.Triangles, subset);
            }
        }

        public void RenderTransparent(IGraphicsContext context, Action<bool> isTextured, Coordinate cameraLocation)
        {

            var sorted =
                from subset in GetSubsets<Face>(Transparent)
                let face = subset.Instance as Face
                where face != null
                orderby (cameraLocation - face.BoundingBox.Center).LengthSquared() descending
                select subset;
            foreach (var subset in sorted)
            {
                var tex = ((Face) subset.Instance).Texture;
                if (tex.Texture != null) tex.Texture.Bind();
                else TextureHelper.Unbind();
                isTextured(tex.Texture != null);
                //program.Set("isTextured", tex.Texture != null);
                Render(context, PrimitiveType.Triangles, subset);
            }
        }

        public void RenderWireframe(IGraphicsContext context)
        {
            foreach (var subset in GetSubsets(BrushWireframe))
            {
                Render(context, PrimitiveType.Lines, subset);
            }

            foreach (var subset in GetSubsets(EntityWireframe))
            {
                Render(context, PrimitiveType.Lines, subset);
            }
        }

        public void RenderVertices(IGraphicsContext context, int pointSize)
        {
            GL.PointSize(pointSize);
            foreach (var subset in GetSubsets(BrushWireframe))
            {
                Render(context, PrimitiveType.Points, subset);
            }

        }

        public void UpdatePartial(IEnumerable<MapObject> objects)
        {
            UpdatePartial(objects.OfType<Solid>().SelectMany(x => x.Faces));
            UpdatePartial(objects.OfType<Entity>().Where(x => !x.HasChildren));
        }

        public void UpdatePartial(IEnumerable<Face> faces)
        {
            foreach (var face in faces)
            {
                var offset = GetOffset(face);
                if (offset < 0) continue;
                var conversion = Convert(face);
                Update(offset, conversion);
            }
        }

        public void UpdatePartial(IEnumerable<Entity> entities)
        {
            foreach (var entity in entities)
            {
                var offset = GetOffset(entity);
                if (offset < 0) continue;
                var conversion = entity.GetBoxFaces().SelectMany(Convert);
                Update(offset, conversion);
            }
        }

        protected override void CreateArray(IEnumerable<MapObject> objects)
        {
            var obj = objects.Where(x => !x.IsVisgroupHidden && !x.IsCodeHidden).ToList();
            var faces = obj.OfType<Solid>().SelectMany(x => x.Faces).ToList();
            var entities = obj.OfType<Entity>().Where(x => !x.HasChildren).ToList();

            StartSubset(BrushWireframe);

            // Render solids
            foreach (var group in faces.GroupBy(x => new { x.Texture.Texture, Transparent = HasTransparency(x) }))
            {
                var subset = group.Key.Transparent ? Transparent : Textured;
                if (!group.Key.Transparent) StartSubset(subset);

                foreach (var face in group)
                {
                    if (group.Key.Transparent) StartSubset(subset);

                    PushOffset(face);
                    var index = PushData(Convert(face));
                    if (!face.Parent.IsRenderHidden3D && face.Texture.Opacity > 0) PushIndex(subset, index, face.GetTriangleIndices());
                    if (!face.Parent.IsRenderHidden2D) PushIndex(BrushWireframe, index, face.GetLineIndices());

                    if (group.Key.Transparent) PushSubset(subset, face);
                }

                if (!group.Key.Transparent) PushSubset(subset, group.Key.Texture);
            }

            PushSubset(BrushWireframe, (object)null);
            StartSubset(EntityWireframe);

            // Render entities
            foreach (var g in entities.GroupBy(x => x.HasModel()))
            {
                // key = false -> no model, put in the untextured group
                // key = true  -> model, put in the entity group
                if (!g.Key) StartSubset(Textured);
                foreach (var entity in g)
                {
                    if (g.Key) StartSubset(Textured);
                    PushOffset(entity);
                    foreach (var face in entity.GetBoxFaces())
                    {
                        var index = PushData(Convert(face));
                        if (!face.Parent.IsRenderHidden3D) PushIndex(Textured, index, face.GetTriangleIndices());
                        if (!face.Parent.IsRenderHidden2D) PushIndex(EntityWireframe, index, face.GetLineIndices());
                    }
                    if (g.Key) PushSubset(Textured, entity);
                }
                if (!g.Key) PushSubset(Textured, (ITexture) null);
            }

            PushSubset(EntityWireframe, (object)null);
        }

        private bool HasTransparency(Face face)
        {
            return (float)face.Texture.Opacity < 0.95
                   || (face.Texture.Texture != null && face.Texture.Texture.HasTransparency());
        }

        protected IEnumerable<MapObjectVertex> Convert(Face face)
        {
            float nx = (float)face.Plane.Normal.DX,
              ny = (float)face.Plane.Normal.DY,
              nz = (float)face.Plane.Normal.DZ;
            float r = face.Colour.R / 255f,
                  g = face.Colour.G / 255f,
                  b = face.Colour.B / 255f,
                  a = (float)face.Texture.Opacity;

            float r2, g2, b2, a2;
            r2 = g2 = b2 = a2 = 1.0f;
            if (!face.HighlightColor.IsEmpty && !Chisel.Settings.View.DisableGBSPFlagHighlights)
            {
                r2 = face.HighlightColor.R / 255f;
                g2 = face.HighlightColor.G / 255f;
                b2 = face.HighlightColor.B / 255f;
                a2 = face.HighlightColor.A / 255f;
            }

            float r3, g3, b3, a3, RenderWireframe = 0;
            r3 = g3 = b3 = a3 = 1.0f;
            if (!face.WireframeColor.IsEmpty && !Chisel.Settings.View.DisableGBSPFlagHighlights)
            {
                r3 = face.WireframeColor.R / 255f;
                g3 = face.WireframeColor.G / 255f;
                b3 = face.WireframeColor.B / 255f;
                a3 = face.WireframeColor.A / 255f;
                RenderWireframe = 1;
            }

            return face.GetIndexedVertices().Select(vert => new MapObjectVertex
            {
                Position = new Vector3((float)vert.Location.DX, (float)vert.Location.DY, (float)vert.Location.DZ),
                Normal = new Vector3(nx, ny, nz),
                Texture = new Vector2((float)vert.TextureU, (float)vert.TextureV),
                Colour = new Color4(r, g, b, a),
                IsSelected = face.IsSelected || (face.Parent != null && face.Parent.IsSelected) ? 1 : 0,
                HighlightColor = new Color4(r2, g2, b2, a2),
                HasWireframe = RenderWireframe,
                WireframeColor = new Color4(r3, g3, b3, a3)
            });
        }
    }
}