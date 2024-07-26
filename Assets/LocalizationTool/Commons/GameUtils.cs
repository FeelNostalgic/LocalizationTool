using UnityEditor;
using UnityEngine;

namespace LocalizationTool.Commons
{ 
	public abstract class GameUtils 
	{
		public static Texture2D GetColoredIcon(string iconName, Color color)
		{
			var originalTexture = EditorGUIUtility.IconContent(iconName).image as Texture2D;
			if (originalTexture == null)
			{
				return null;
			}

			var coloredTexture = new Texture2D(originalTexture.width, originalTexture.height, TextureFormat.RGBA32, false);

			Graphics.CopyTexture(originalTexture, coloredTexture);

			for (var y = 0; y < coloredTexture.height; y++)
			{
				for (var x = 0; x < coloredTexture.width; x++)
				{
					var originalColor = coloredTexture.GetPixel(x, y);
					var newColor = originalColor * color;
					coloredTexture.SetPixel(x, y, newColor);
				}
			}

			coloredTexture.Apply();
			return coloredTexture;
		}
		
		public static Texture2D GetTexture2DFromColor(Color color) {
			var texture = new Texture2D(20, 20);
			for (var y = 0; y < texture.height; y++) {
				for (var x = 0; x < texture.width; x++) {
					texture.SetPixel(x, y, color);
				}
			}
			texture.Apply();
			return texture;
		}
		
	}
}