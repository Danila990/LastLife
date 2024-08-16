using System.Linq;
using Core.CameraSystem;
using Core.Services;
using Dialogue.Services.Interfaces;
using Yarn.Unity;

namespace Dialogue.Services.Modules.CameraModule
{
	public class CameraDialogueModule : IDialogueModule
	{
		private readonly ICameraService _cameraService;
		private readonly IMapService _map;
		private CameraType _lastCameraType;
		private CutsceneCamera _cutsceneCamera;
		public string ModuleId => nameof(CameraDialogueModule);

		public CameraDialogueModule(ICameraService cameraService, IMapService map)
		{
			_cameraService = cameraService;
			_map = map;
		}
		
		public void OnStartDialogue(IModuleArgs moduleArgs = null)
		{
			_lastCameraType = _cameraService.CurrentCameraController.CameraType;
			_cameraService.SetCutsceneCamera();
			_cutsceneCamera = _cameraService.CurrentCameraController as CutsceneCamera;
		}
		
		public void OnDialogueEnd()
		{
			_cameraService.SetCameraByType(_lastCameraType);
		}
		
		public void AddCommand(DialogueRunner dialogueRunner)
		{
			dialogueRunner.AddCommandHandler<string>("camera", MoveCameraTo);
		}

		public void MoveCameraTo(string id)
		{
			var pointProvider = _map.MapObject.GetComponent<MapCameraPointProvider>();
			var moveTo = pointProvider.Points.FirstOrDefault(transform => transform.name == id);
			if (moveTo != null)
			{
				_cutsceneCamera.transform.position = moveTo.position;
				_cutsceneCamera.transform.forward = moveTo.forward;
			}
		}
	}
}