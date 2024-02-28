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
        private SerializedProperty boneMapProperty;
        protected void OnEnable()
        {
            animatorProperty = serializedObject.FindProperty("animator");
            boneMapProperty = serializedObject.FindProperty("bone_map"); // ������Ǹ� Asset
        }
        public override void OnInspectorGUI()
        {
            Perform.MeoActor actor = (Perform.MeoActor)target;
            EditorGUILayout.HelpBox("������̬��T-POSE��ʼ���ȵ������İ�ť�Բ�����׼ƫ�ơ�", MessageType.Info);
            EditorGUILayout.BeginHorizontal();
            if (actor.animator != null)
            {
                if (actor.animator.enabled && actor.animator.isHuman)
                {
                    // Todo: Append pre-processing
                }
                else
                {
                    EditorGUILayout.HelpBox("Animator��Avatar���ͱ���Ϊ����!", MessageType.Error);
                }

            }
            else
            {
                EditorGUILayout.HelpBox("������һ�� Animator!", MessageType.Error);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(animatorProperty);

            if (GUILayout.Button("ʹ�ø�Object��Animator"))
            {
                actor.animator = actor.GetComponentInChildren<Animator>();
            }

            if (actor.animator == null)
            {
                EditorGUILayout.HelpBox("MeoActor������������һ��ӵ�� animator ��Object�ϣ���Ҫ��ӵ������avatar!", MessageType.Error);
            }
            else if (!actor.animator.isHuman)
            {
                EditorGUILayout.HelpBox("AnimatorҪ��ӵ������avatar!", MessageType.Error);

            }

            if (GUILayout.Button("ע�� T-POSE ����"))
            {
                actor.CalculateTPose();
                actor.bone_map.UpdateListsFromDictionaries();
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(actor.bone_map);
                AssetDatabase.SaveAssets();

            }

            EditorGUILayout.HelpBox("���������MeocapActor�����ļ�!", MessageType.Info);

            EditorGUILayout.PropertyField(boneMapProperty);

            serializedObject.ApplyModifiedProperties();
        }
    }

}