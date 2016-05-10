namespace Compose3D
{
	using System;
	using Maths;
	using Reactive;

	public interface IView3D
	{
		string Name { get; }
		Reaction<double> Render { get; }
		Reaction<Vec2> Resized { get; }
	}
}