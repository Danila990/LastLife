using UnityEditor;
using UnityEngine;

namespace AillieoUtils.UI
{
    [CustomEditor(typeof(RadialLayoutGroup))]
    public class RadialLayoutGroupEditor : UnityEditor.Editor
    {
        private RadialLayoutGroup radialLayoutGroup;
        private SerializedProperty mAngleConstraint;
        private SerializedProperty mRadiusConstraint;
        private SerializedProperty mLayoutDir;
        private SerializedProperty mRadiusStart;
        private SerializedProperty mRadiusDelta;
        private SerializedProperty mRadiusRange;
        private SerializedProperty mAngleDelta;
        private SerializedProperty mAngleStart;
        private SerializedProperty mAngleCenter;
        private SerializedProperty mAngleRange;
        private SerializedProperty mChildRotate;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(mLayoutDir, new GUIContent("Layout Dir"));

            EditorGUILayout.PropertyField(mAngleConstraint, new GUIContent("Angle Constraint"));
            EditorGUILayout.PropertyField(mRadiusConstraint, new GUIContent("Radius Constraint"));

            // angles
            if (radialLayoutGroup.LayoutDir != RadialLayoutGroup.Direction.Bidirectional)
            {
                EditorGUILayout.PropertyField(mAngleStart, new GUIContent("Angle Start"));
            }
            else
            {
                EditorGUILayout.PropertyField(mAngleCenter, new GUIContent("Angle Center"));
            }
            if(radialLayoutGroup.AngleConstraint == RadialLayoutGroup.ConstraintMode.Interval)
            {
                EditorGUILayout.PropertyField(mAngleDelta, new GUIContent("Angle Delta"));
            }
            else
            {
                EditorGUILayout.PropertyField(mAngleRange, new GUIContent("Angle Range"));
            }
            // radius
            EditorGUILayout.PropertyField(mRadiusStart, new GUIContent("Radius Start"));
            if (radialLayoutGroup.RadiusConstraint == RadialLayoutGroup.ConstraintMode.Interval)
            {
                EditorGUILayout.PropertyField(mRadiusDelta, new GUIContent("Radius Delta"));
            }
            else
            {
                EditorGUILayout.PropertyField(mRadiusRange, new GUIContent("Radius Range"));
            }
            EditorGUILayout.PropertyField(mChildRotate, new GUIContent("Child Rotate"));

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            if (target == null)
            {
                return;
            }
            radialLayoutGroup = target as RadialLayoutGroup;

            var serObj = serializedObject;
            mAngleConstraint = serObj.FindProperty("mAngleConstraint");
            mRadiusConstraint = serObj.FindProperty("mRadiusConstraint");
            mLayoutDir = serObj.FindProperty("mLayoutDir");
            mRadiusStart = serObj.FindProperty("mRadiusStart");
            mRadiusDelta = serObj.FindProperty("mRadiusDelta");
            mRadiusRange = serObj.FindProperty("mRadiusRange");
            mAngleDelta = serObj.FindProperty("mAngleDelta");
            mAngleStart = serObj.FindProperty("mAngleStart");
            mAngleCenter = serObj.FindProperty("mAngleCenter");
            mAngleRange = serObj.FindProperty("mAngleRange");
            mChildRotate = serObj.FindProperty("mChildRotate");
        }
    }

}

