using Sandbox;

namespace TycoonGame.Utilities.Extensions;

public static class VertexBufferExtensions
{
	public static void CreateFaceingQuad( this VertexBuffer vertexBuffer, Vector3 centerPoint, Rotation rotation, float width, float height )
	{
		var ray = new Ray( centerPoint, rotation.Forward );
		var actualWidth = rotation.Left * width * 0.5f;
		var actualHeight = rotation.Up * height;

		vertexBuffer.AddQuad( ray, actualWidth, actualHeight );
	}
}
