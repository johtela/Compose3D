namespace Compose3D.Viewer
{
	using System;
	using System.Linq;
	using Compose3D.Maths;
	using Compose3D.Geometry;
	using Compose3D.Reactive;
	using Compose3D.SceneGraph;
	using Compose3D.Shaders;
	using Compose3D;
	using OpenTK;
	using OpenTK.Graphics;
	using OpenTK.Input;
	using Visuals;
	using Compose3D.UI;

	public class Viewer
	{
		private IView3D _currentView;
		internal GameWindow _window;

		public Viewer (IView3D view)
		{
			_window = new GameWindow (800, 600, GraphicsMode.Default, "Compose3D Viewer");
			CurrentView = view;
		}

		public IView3D CurrentView
		{
			get { return _currentView; }
			set
			{
				if (value != _currentView)
				{
					_currentView = value;
					ChangeView ();
				}
			}
		}

		private void ChangeView ()
		{
			_currentView.Setup ();
			_currentView.Render
				.WhenRendered (_window)
				.Evoke ();
			_currentView.Resized
				.WhenResized (_window)
				.Evoke ();
		}
	}
} 