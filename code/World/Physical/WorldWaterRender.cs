using Sandbox;

namespace TycoonGame.World.Physical;

[SceneCamera.AutomaticRenderHook]
public class WorldWaterRender : RenderHook
{
	public override void OnStage( SceneCamera target, Stage renderStage )
	{
		if ( renderStage == Stage.AfterOpaque )
		{
			RenderEffect();
		}
	}

	public static void RenderEffect()
	{
		var water = TycoonGame.Instance.WorldManager.WorldWaterEntity;
		Graphics.GrabFrameTexture( "ColorBuffer", water.SceneObject.Attributes );
		Graphics.GrabDepthTexture( "DepthBuffer", water.SceneObject.Attributes );
	}
}
