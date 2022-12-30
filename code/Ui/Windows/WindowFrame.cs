using Sandbox.UI;
using Sandbox;
using System.Collections.Generic;
using System;

public partial class WindowFrame
{
	public readonly static int MIN_WIDTH = 650;
	public readonly static int MIN_HEIGHT = 350;

	public Vector2 Position { get; set; }
	public string Name { get; set; }

	public event Action OnClose;

	private bool IsDragging { get; set; }
	private Vector2 LastMousePosition{ get; set; }
	private bool IsResizing { get; set; }
	public float Width { get; set; }
	public float Height { get; set; }

	private Tab CurrentSelectedTab = null;
	private readonly List<Tab> Tabs = new List<Tab>();
	
	private Panel Container { get; set; }
	private Panel BodyContainer { get; set; }

	public Tab AddTab( string name, Panel panel )
	{
		var tab = new Tab( name, panel );
		Tabs.Add( tab );

		return tab;
	}

	public void SetTab( Tab tab )
	{
		if (CurrentSelectedTab != null) 
		{
			CurrentSelectedTab.Panel.Parent = null;
		}

		tab.Panel.Parent = BodyContainer;
		CurrentSelectedTab = tab;
	}

	public override void Tick()
	{
		base.Tick();

		if ( IsDragging || IsResizing )
		{
			if (IsDragging )
			{
				Position += (Mouse.Position - LastMousePosition) * ScaleFromScreen;
				UpdatePosition();
			}

			if (IsResizing)
			{
				Width += (Mouse.Position.x - LastMousePosition.x) * ScaleFromScreen;
				Width = Math.Max( MIN_WIDTH, Width );
				Height += (Mouse.Position.y - LastMousePosition.y) * ScaleFromScreen;
				Height = Math.Max( MIN_HEIGHT, Height );
				UpdateSize();
			}

			LastMousePosition = Mouse.Position;
		}
	}

	public void Close()
	{
		OnClose?.Invoke();
	}

	protected override void OnAfterTreeRender( bool firstTime )
	{
		base.OnAfterTreeRender( firstTime );

		if (firstTime)
		{
			SetTab( Tabs[0] );

			Position = Screen.Size / 3;
			UpdatePosition();

			Width = MIN_WIDTH;
			Height = MIN_HEIGHT;
			UpdateSize();
		}
	}

	protected override int BuildHash()
	{
		return HashCode.Combine( Tabs, CurrentSelectedTab );
	}

	private void StartDragging()
	{
		LastMousePosition = Mouse.Position;
		IsDragging = true;
	}

	private void StartResizing()
	{
		LastMousePosition = Mouse.Position;
		IsResizing = true;
	}

	private void StopDraggingAndResizing()
	{
		IsDragging = false;
		IsResizing = false;
	}

	private void UpdatePosition()
	{
		Container.Style.Left = Length.Pixels( Position.x );
		Container.Style.Top = Length.Pixels( Position.y );
		Container.Style.Dirty();
	}

	private void UpdateSize()
	{
		Container.Style.Width = Length.Pixels( Width );
		Container.Style.Height = Length.Pixels( Height );
		Container.Style.Dirty();
	}

	public class Tab
	{
		public string Name { get; init; }
		public Panel Panel { get; init; }

		public Tab( string name, Panel panel )
		{
			Name = name;
			Panel = panel;
		}
	}
}
