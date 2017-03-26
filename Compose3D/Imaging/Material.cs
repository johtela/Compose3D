namespace Compose3D.Imaging
{
	using System.Linq;
	using System.Reflection;
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

		private CLKernel[] Kernels ()
		{
			return (from f in GetType ().GetFields (BindingFlags.NonPublic | BindingFlags.Static)
					where f.FieldType.IsSubclassOf (typeof (CLKernel))
					select (CLKernel)f.GetValue (null)).ToArray ();
		}

		protected void RunKernel<T1> (AnySignalEditor editor, Vec2i size, 
			CLKernel<T1, Buffer<uint>> kernel, T1 arg1)
			where T1 : KernelArg
		{
			kernel.Execute (Queue, arg1, KernelArg.WriteBuffer (editor.Buffer), size.X, size.Y);
		}

		protected void RunKernel<T1, T2> (AnySignalEditor editor, Vec2i size, 
			CLKernel<T1, T2, Buffer<uint>> kernel, T1 arg1, T2 arg2)
			where T1 : KernelArg
			where T2 : KernelArg
		{
			kernel.Execute (Queue, arg1, arg2, KernelArg.WriteBuffer (editor.Buffer), size.X, size.Y);
		}

		protected void RunKernel<T1, T2, T3> (AnySignalEditor editor, Vec2i size, 
			CLKernel<T1, T2, T3, Buffer<uint>> kernel, T1 arg1, T2 arg2, T3 arg3)
			where T1 : KernelArg
			where T2 : KernelArg
			where T3 : KernelArg
		{
			kernel.Execute (Queue, arg1, arg2, arg3, KernelArg.WriteBuffer (editor.Buffer), 
				size.X, size.Y);
		}

		protected void RunKernel<T1, T2, T3, T4> (AnySignalEditor editor, Vec2i size,
			CLKernel<T1, T2, T3, T4, Buffer<uint>> kernel, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
			where T1 : KernelArg
			where T2 : KernelArg
			where T3 : KernelArg
			where T4 : KernelArg
		{
			kernel.Execute (Queue, arg1, arg2, arg3, arg4, KernelArg.WriteBuffer (editor.Buffer), 
				size.X, size.Y);
		}

		protected void RunKernel<T1, T2, T3, T4, T5> (AnySignalEditor editor, Vec2i size,
			CLKernel<T1, T2, T3, T4, T5, Buffer<uint>> kernel, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
			where T1 : KernelArg
			where T2 : KernelArg
			where T3 : KernelArg
			where T4 : KernelArg
			where T5 : KernelArg
		{
			kernel.Execute (Queue, arg1, arg2, arg3, arg4, arg5, KernelArg.WriteBuffer (editor.Buffer),
				size.X, size.Y);
		}

		public Control Editor
        {
            get { return SignalEditor.EditorUI (FileName, Texture, Size, Updater, RootEditors ());  }
        }
    }
}
