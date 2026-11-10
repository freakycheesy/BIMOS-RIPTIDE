using UnityEditor;
using UnityEngine;

namespace BIMOS.Netcode.Editor
{
    [CustomEditor(typeof(NetworkSceneManager))]
    public class NetworkLevelEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var target = (NetworkSceneManager)this.target;
            base.OnInspectorGUI();
            if(GUILayout.Button("Bake NetworkIdentities"))
            {
                EditorUtility.SetDirty(target);
                target.FindIdentities();
                AssetDatabase.SaveAssetIfDirty(target);
            }
        }
    }
}
