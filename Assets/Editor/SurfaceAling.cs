﻿using UnityEngine;
using UnityEditor;
 using Utils;

 class SurfaceAling : ScriptableObject
{
    [MenuItem("Aling/Aling to surface _g")]
    static void Look()
    {
        
        var camera = SceneView.lastActiveSceneView.camera;
        RaycastHit rayHitInfo;
        if (camera)
        {
            foreach (Transform transform in Selection.transforms)
            {
                Undo.RegisterCompleteObjectUndo(Selection.transforms, " aling to surface");
                Ray rayToObj = new Ray(camera.transform.position, transform.position - camera.transform.position);
                if (Physics.Raycast(rayToObj,out rayHitInfo, 1000, LayerMasks.Environment))
                {
                    transform.rotation = Quaternion.FromToRotation(transform.up, rayHitInfo.normal) * transform.rotation;
                    Ray rayToSurface = new Ray(transform.position + transform.up * 3, -transform.up);
                    if(Physics.Raycast(rayToSurface,out rayHitInfo, 1000, LayerMasks.Environment))
                    {
                        transform.position = rayHitInfo.point+transform.up*transform.localScale.y;
                    }
                }
            }
        }
    }

    [MenuItem("Aling/Aling to surface _g", true)]
    static bool ValidateSelection()
    {
        return Selection.transforms.Length != 0;
    }
}