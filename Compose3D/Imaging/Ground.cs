namespace Compose3D.Imaging
{
    using Maths;
    using Reactive;
    using SignalEditors;
    using Textures;

    public class Ground : Material
    {
        public Ground (Vec2i size, string fileName, Texture texture, DelayedReactionUpdater updater)
            : base (size, fileName, texture, updater) { }

        protected override AnySignalEditor[] RootEditors ()
        {
            var worley = SignalEditor.Worley ("worley", WorleyNoiseKind.F1, ControlPointKind.Random,
                10, 0, DistanceKind.Euclidean, 0f, true);
            var transform = worley.Transform ("transform", -30f, 0.5f);
            var dv = new Vec2 (1f) / new Vec2 (Size.X, Size.Y);
            var perlin = SignalEditor.Perlin ("perlin", new Vec2 (10f));
            var spectral = perlin.SpectralControl ("spectral", 0, 2, 1f, 0.5f, 0.2f);
            var warp = transform.Warp ("warp", spectral, 0.001f, dv);
            var signal = warp.Colorize ("signal", ColorMap<Vec3>.GrayScale ());
            var normal = warp.NormalMap ("normal", 1f, dv);

            return new AnySignalEditor[] { normal, signal };
        }
    } 
}
