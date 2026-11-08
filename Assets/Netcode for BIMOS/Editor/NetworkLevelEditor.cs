using UnityEditor;
using UnityEngine;

namespace BIMOS.Netcode.Editor
{
    [CustomEditor(typeof(NetworkLevel))]
    public class NetworkLevelEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var target = (NetworkLevel)this.target;
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
