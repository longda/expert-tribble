﻿using UnityEngine;

public static class TextureGenerator
{
	public static Texture2D TextureFromColorMap(Color[] colorMap, int width, int height)
	{
		var texture = new Texture2D(width, height);
		texture.filterMode = FilterMode.Point;
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.SetPixels(colorMap);
		texture.Apply();
		
		return texture;
	}
	
	public static Texture2D TextureFromHeightMap(float[,] heightMap)
	{
		var width = heightMap.GetLength(0);
		var height = heightMap.GetLength(1);
		var colorMap = new Color[width * height];
		
		for (var x = 0; x < width; x++)
		{
			for (var y = 0; y < height; y++)
			{
				colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
			}
		}
		
		return TextureFromColorMap(colorMap, width, height);
	}
}
