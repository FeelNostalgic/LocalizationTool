#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LocalizationTool.Scripts.Commons
{ 
	public abstract class EditorUtils 
	{
		public static GameObject MenuItemSetInObject<T>(MenuCommand menuCommand) where T : MonoBehaviour {
			GameObject gb = null;
			if (Selection.activeObject != null) {
				gb = Selection.activeObject as GameObject;
				var tmp = gb.GetComponent<TextMeshProUGUI>();
				if(tmp.IsNull()) Debug.LogError("GameObject doesnt contains a TextMeshPro component");
				GameObjectUtility.SetParentAndAlign(gb, menuCommand.context as GameObject);
				Undo.RegisterCreatedObjectUndo(gb, "Set " + typeof(T) + " in " + gb.name);
				if (!gb.GetComponent<T>()) {
					gb.AddComponent<T>();
					//Save scene
					EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
				} else {
					Debug.LogError("GameObject already contains " + typeof(T));
				}
			} else {
				Debug.Log("No object has been selected");
			}
			return gb;
		}
		
		public static GameObject MenuItemNewObject<T>(MenuCommand menuCommand, string name) where T : MonoBehaviour {
			var go = new GameObject(name, typeof(T));
			// Ensure it gets reparented if this was a context click (otherwise does nothing)
			GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
			// Register the creation in the undo system
			Undo.RegisterCreatedObjectUndo(go, "Create " + typeof(T) + " in " + go.name);
			Selection.activeObject = go;
			return go;
		}
	}
}
#endif