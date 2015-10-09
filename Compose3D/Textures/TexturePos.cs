namespace Compose3D.Textures
{
    using Arithmetics;

    public static class TexturePos
    {
        public static Vec2 BottomLeft
        {
            get { return new Vec2 (0f, 0f); }
        }

        public static Vec2 BottomRight
        {
            get { return new Vec2 (1f, 0f); }
        }

        public static Vec2 TopLeft
        {
            get { return new Vec2 (0f, 1f); }
        }

        public static Vec2 TopRight
        {
            get { return new Vec2 (1f, 1f); }
        }
    }
}
