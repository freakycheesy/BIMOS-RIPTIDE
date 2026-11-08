using UnityEditor;
using UnityEngine;
using UnityEngine.Identifiers;

namespace BIMOS.Netcode.Editor
{
    [CustomEditor(typeof(NetworkManager))]
    public class NetworkManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var target = (NetworkManager)this.target;
            base.OnInspectorGUI();
            if (GUILayout.Button("Bake Network Prefabs"))
            {
                EditorUtility.SetDirty(target);
                foreach(var guid in AssetDatabase.FindAssets("t:prefab"))
                {
                    NetworkIdentity identity = AssetDatabase.LoadAssetByGUID<NetworkIdentity>(new GUID(guid));
                    if (!identity)
                    {
                        continue;
                    }

                    if (!target.spawnPrefabs.Contains(identity.gameObject))
                    {
                        target.spawnPrefabs.Add(identity.gameObject);
                    }
                }
                AssetDatabase.SaveAssetIfDirty(target);
            }
            if(GUILayout.Button("Clear Network Prefabs"))
            {
                EditorUtility.SetDirty(target);
                target.spawnPrefabs.Clear();
                AssetDatabase.SaveAssetIfDirty(target);
            }
        }
    }
}
