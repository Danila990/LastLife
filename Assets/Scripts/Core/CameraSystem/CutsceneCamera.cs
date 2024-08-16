
namespace Core.CameraSystem
{
	public class CutsceneCamera : CameraController
	{
		public override CameraType CameraType => CameraType.Cutscene;

		// public override float GetAxisValue(int axis)
		// {
		// 	return 0;
		// }


		protected override void OnSetFollowTarget()
		{
			
		}
	}
}