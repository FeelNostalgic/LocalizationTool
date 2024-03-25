
using UnityEditor;
using UnityEngine;

namespace PlayerEditor
{ 
	public class PlayerEditor : EditorWindow
	{
		#region PLAYER VARIABLES

		private string m_playerID = "WYou can find me in AllPlayers.txt";

		private int m_PlayerHealth = 100;
		private int m_PlayerArmor = 50;

		private int m_Str;
		private int m_Dex;
		private int m_Int;

		private float m_Overpowered = 1;
		private bool m_IsImmortal = false;
		
		#endregion

		#region EDITOR VARIABLES

		private bool m_CustomStats = false;
		private bool m_AdminMode = false;

		private string m_KeyID = "\"ID\"";
		private string m_KeyHealth = "\"Health\"";
		private string m_KeyArmor = "\"Armor\"";
		private string m_KeyClass = "\"Class\"";
		private string m_KeyStats = "\"Stats\"";
		
		#endregion

		[MenuItem("Game/PlayerEditor")]
		public static void ShowWindow()
		{
			//Show existing window instance. If one doesn't exist, make one.
			EditorWindow window = EditorWindow.GetWindow(typeof(PlayerEditor));
			window.minSize = new Vector2(400, 350);
			window.titleContent = new GUIContent("Creador de personajes");
		}

		private void OnGUI()
		{
			ShowBaseSettings();
			ShowSpecialSettings();
			ShowAdminSettings();
			ShowPlayerJSON();
			ShowButtons();
		}
		
		#region BASE

		private void ShowBaseSettings()
		{
			ShowHeader("Base Settings");
			ShowPlayerFields();
		}

		private void ShowPlayerFields()
		{
			m_playerID = EditorGUILayout.TextField("Player ID: ", m_playerID);
			m_PlayerHealth = EditorGUILayout.IntField("Health: ", m_PlayerHealth);
			m_PlayerArmor = EditorGUILayout.IntField("Armor: ", m_PlayerArmor);
		}

		#endregion

		#region SPECIAL

		private void ShowSpecialSettings()
		{
			ShowCustomStats();
		}

		private void ShowCustomStats()
		{
			m_CustomStats = EditorGUILayout.Toggle("Edit Stats: ", m_CustomStats);
			if (m_CustomStats)
				ShowCustomStatsFields();
			else
				ResetCustomStats();
		}

		private void ShowCustomStatsFields()
		{
			m_Str = EditorGUILayout.IntField("Strength: ", m_Str);
			m_Dex = EditorGUILayout.IntField("Dexterity: ", m_Dex);
			m_Int = EditorGUILayout.IntField("Wisdon: ", m_Int);
		}

		private void ResetCustomStats()
		{
			m_Str = 0;
			m_Dex = 0;
			m_Int = 0;
		}

		#endregion

		#region ADMIN

		private void ShowAdminSettings()
		{
			ShowHeader("Admin Settings");
			ShowAdminFields();
		}

		private void ShowAdminFields()
		{
			m_AdminMode = EditorGUILayout.BeginToggleGroup("Enabled: ", m_AdminMode);
			m_Overpowered = EditorGUILayout.Slider("Overpowered: ", m_Overpowered, 0, 3);
			m_IsImmortal = EditorGUILayout.Toggle("Make Immortal: ", m_IsImmortal);
			EditorGUILayout.EndToggleGroup();
		}

		#endregion

		#region JSON

		private void ShowPlayerJSON()
		{
			ShowHeader("Player JSON");
			EditorGUILayout.TextArea(GenerateJSON(), TextAreaStyle(), GUILayout.Height(50));
		}

		private string GenerateJSON()
		{
			var playerJson = "{" + 
			                    $"{m_KeyID}:\"{m_playerID}\"," +
			                    $"{m_KeyHealth}:\"{m_PlayerHealth}\","+
			                    $"{m_KeyArmor}:\"{m_PlayerArmor}\","
			                    + "}";
			return playerJson;
		}

		#endregion

		#region BUTTONS

		private void ShowButtons()
		{
			ShowHeader("Buttons");
			ExportJSONButton();
		}

		private void ExportJSONButton()
		{
			if(GUILayout.Button("Export Player Data")) Debug.Log(GenerateJSON());
		}

		#endregion

		private void ShowHeader(string name)
		{
			GUILayout.Space(10);
			GUILayout.Label(name, HeaderStyle());
			GUILayout.Space(10);
		}

		private GUIStyle HeaderStyle()
		{
			var style = new GUIStyle
			{
				fontStyle = FontStyle.Bold,
				alignment = TextAnchor.MiddleCenter,
				normal =
				{
					textColor = Color.white
				},
				hover =
				{
					textColor = Color.white
				},
				active =
				{
					textColor = Color.white
				}
			};

			return style;
		}

		private GUIStyle TextAreaStyle()
		{
			var style = new GUIStyle
			{
				wordWrap = true,
				normal =
				{
					textColor = Color.white
				}
			};

			return style;
		}
	}
}