using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Meocap;
namespace Meocap.UI
{
    [CustomEditor(typeof(Perform.MeoActor))]
    public class MeoActorEditor : Editor
    {
        private SerializedProperty animatorProperty;
        private SerializedProperty targetTransformProperty;
        private SerializedProperty boneMapProperty;
        protected void OnEnable()
        {
            animatorProperty = serializedObject.FindProperty("animator");
            targetTransformProperty = serializedObject.FindProperty("target");
            boneMapProperty = serializedObject.FindProperty("bone_map"); // 这就是那个 Asset
        }
        public override void OnInspectorGUI()
        {
            Perform.MeoActor actor = (Perform.MeoActor)target;
            EditorGUILayout.BeginHorizontal();
            if (actor.animator != null)
            {
                if (actor.animator.enabled && actor.animator.isHuman)
                {
                    // Todo: Append pre-processing
                }
                else
                {
                    EditorGUILayout.HelpBox("Animator的Avatar类型必须为人形!", MessageType.Error);
                }

            }
            else
            {
                EditorGUILayout.HelpBox("请设置一个 Animator!", MessageType.Error);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(animatorProperty);
            EditorGUILayout.PropertyField(targetTransformProperty);
            if (GUILayout.Button("自动设置Transform和Animator"))
            {
                actor.animator = actor.GetComponentInChildren<Animator>();
                actor.target = actor.transform;
            }

            if (actor.animator == null)
            {
                EditorGUILayout.HelpBox("MeoActor组件必须挂载在一个拥有 animator 的Object上！且要求拥有人形avatar!", MessageType.Error);
            }
            else if (!actor.animator.isHuman)
            {
                EditorGUILayout.HelpBox("Animator要求拥有人形avatar!", MessageType.Error);

            }

            serializedObject.ApplyModifiedProperties();
        }
    }

}
