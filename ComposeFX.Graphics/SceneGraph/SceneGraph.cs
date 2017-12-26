namespace ComposeFX.Graphics.SceneGraph
{
	using DataStructures;
	using Maths;

	public class SceneGraph
	{
		public SceneGraph ()
		{
			Root = new SceneGroup (this);
			Index = new BoundingBoxTree<SceneNode> ();
		}

		public IBoundingTree<Vec3, SceneNode> Index { get; internal set; }

		public SceneGroup Root { get; private set; }

		public GlobalLighting GlobalLighting { get; set; }
	}
}
