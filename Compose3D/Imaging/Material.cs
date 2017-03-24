namespace Compose3D.Imaging
{
    using Maths;
    using Reactive;
    using SignalEditors;
    using Textures;
    using UI;

    public abstract class Material
    {
        public readonly Vec2i Size;
        public readonly string FileName;
        public readonly Texture Texture;
        public readonly DelayedReactionUpdater Updater;

        public Material (Vec2i size, string fileName, Texture texture, DelayedReactionUpdater updater)
        {
            Size = size;
            FileName = fileName;
            Texture = texture;
            Updater = updater;
        }

        protected abstract AnySignalEditor[] RootEditors ();

        public Control Editor
        {
            get { return SignalEditor.EditorUI (FileName, Texture, Size, Updater, RootEditors ());  }
        }
    }
}
