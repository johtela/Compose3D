namespace Compose3D.Imaging
{
    using System.Linq;
    using CLTypes;
    using Maths;
    using Reactive;
    using SignalEditors;
    using Textures;
    using UI;

    public abstract class Material
    {
        public readonly CLContext Context;
        public readonly CLCommandQueue Queue;
		public readonly CLProgram Program;

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
            Context = CLContext.CreateContextForDevices (CLContext.Gpus.First ());
            Queue = new CLCommandQueue (Context);
			Program = new CLProgram (Context, Kernels ());
		}

		protected abstract AnySignalEditor[] RootEditors ();

		protected abstract CLKernel[] Kernels ();

		protected void Render<T> (AnySignalEditor editor, Vec2i size, CLKernel<T, Buffer<uint>> kernel, T arg1)
			where T : KernelArg
		{
			kernel.Execute (Queue, arg1, KernelArg.WriteBuffer (editor.Buffer), size.X, size.Y);
		}

		public Control Editor
        {
            get { return SignalEditor.EditorUI (FileName, Texture, Size, Updater, RootEditors ());  }
        }
    }
}
