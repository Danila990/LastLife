using UnityEngine;

namespace Core.Entity
{
    public interface ICameraTargetEntity
    {
        Transform CameraTargetRoot { get; }
        bool TargetIsActive { get; }
        float AdditionalCameraDistance { get; }
    }
}