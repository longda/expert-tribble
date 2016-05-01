﻿using UnityEngine;
using System.Collections;

public class MapDisplay : MonoBehaviour 
{
	public Renderer textureRenderer;
	
	public void DrawNoiseMap(float[,] noiseMap)
	{
		var width = noiseMap.GetLength(0);
		var height = noiseMap.GetLength(1);
		var texture = new Texture2D(width, height);
		var colorMap = new Color[width * height];
		
		for (var x = 0; x < width; x++)
		{
			for (var y = 0; y < height; y++)
			{
				colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
			}
		}
		
		texture.SetPixels(colorMap);
		texture.Apply();
		
		textureRenderer.sharedMaterial.mainTexture = texture;
		textureRenderer.transform.localScale = new Vector3(width, 1, height);
	}
}
