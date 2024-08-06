
using System;
using UnityEngine;

namespace UIManager
{ 
	public class UIManager : MonoBehaviour
	{
		#region Inspector Variables

		[SerializeField] private GameObject mainMenuGroup;
		[SerializeField] private GameObject optionsGroup;

		#endregion

		#region Public Variables
		
		#endregion

		#region Private Variables

		#endregion

		#region Unity Methods

		private void Awake()
		{
			//TODO: LOAD data on localization tool
		}

		#endregion

		#region Public Methods

		#endregion

		#region Private Methods

		public void GoToOptions()
		{
			mainMenuGroup.SetActive(false);
			optionsGroup.SetActive(true);
		}

		public void GoToMainMenu()
		{
			mainMenuGroup.SetActive(true);
			optionsGroup.SetActive(false);
		}
		
		#endregion
	}
}