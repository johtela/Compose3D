namespace Compose3D
{
	using Maths;
	using Reactive;

	public interface IView3D
	{
		void Setup ();

		string Name { get; }
		SceneGraph.SceneGraph Scene { get; }
		Reaction<double> Render { get; }
		Reaction<Vec2> Resized { get; }
	}
}