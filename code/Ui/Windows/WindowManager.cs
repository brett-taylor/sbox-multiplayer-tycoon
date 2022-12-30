using System.Collections.Generic;
using System;

public partial class WindowManager
{
	private List<Window> Windows { get; set; }

	public WindowManager()
	{
		Windows = new List<Window>();
	}

	public Window ShowWindow( string name )
	{
		var windowFrame = new WindowFrame();
		windowFrame.Name = name;

		var window = new Window( name, windowFrame );
		AddChild( window.WindowFrame );
		Windows.Add( window );

		windowFrame.OnClose += () => CloseWindow( window );

		return window;
	}

	public void CloseWindow( Window window )
	{
		window.WindowFrame.Delete();
		Windows.Remove( window );
	}

	protected override int BuildHash()
	{
		return HashCode.Combine( Windows );
	}

	public class Window
	{
		public string Name { get; init; }
		public WindowFrame WindowFrame { get; init; }

		public Window( string name, WindowFrame windowFrame )
		{
			Name = name;
			WindowFrame = windowFrame;
		}
	}
}
