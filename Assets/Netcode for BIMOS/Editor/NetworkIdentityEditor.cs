using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BIMOS.Netcode.Editor
{
    [CustomEditor(typeof(NetworkIdentity), true), CanEditMultipleObjects]
    public class NetworkIdentityEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.IntField("Network Id", (target as NetworkIdentity).netId);
            EditorGUI.EndDisabledGroup();
            base.OnInspectorGUI();
            if (GUILayout.Button("Bake AssetId"))
            {
                foreach (var thisTarget in targets)
                {
                    var target = (NetworkIdentity)thisTarget;

                    var prefab = target.gameObject;
                    EditorUtility.SetDirty(prefab);
                    string path = string.Empty;
                    if (PrefabStageUtility.GetCurrentPrefabStage() != null && PrefabStageUtility.GetPrefabStage(prefab) != null)
                    {
                        path = PrefabStageUtility.GetPrefabStage(prefab).assetPath;
                    }
                    else
                    {
                        path = AssetDatabase.GetAssetPath(prefab);
                    }
                    var guid = AssetDatabase.GUIDFromAssetPath(path);
                    prefab.GetComponent<NetworkIdentity>().assetId = (uint)guid.GetHashCode();
                    AssetDatabase.SaveAssetIfDirty(guid);
                }
            }
        }
    }
}
