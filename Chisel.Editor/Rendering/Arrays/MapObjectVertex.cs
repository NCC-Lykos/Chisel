using OpenTK;
using OpenTK.Graphics;

namespace Chisel.Editor.Rendering.Arrays
{
    public struct MapObjectVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 Texture;
        public Color4 Colour;
        public float IsSelected;
        public Color4 HighlightColor;
        public float HasWireframe;
        public Color4 WireframeColor;
    }
}