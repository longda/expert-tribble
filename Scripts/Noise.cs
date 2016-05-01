using UnityEngine;
using System.Collections;

public static class Noise 
{
	public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale)
	{
		var noiseMap = new float[mapWidth, mapHeight];
		
		if (scale <= 0) scale = 0.0001f;
		
		for (var x = 0; x < mapWidth; x++)
		{
			for (var y = 0; y < mapHeight; y++)
			{
				var sampleX = x / scale;
				var sampleY = y / scale;
				var perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
				
				noiseMap[x, y] = perlinValue;
			}
		}
		
		return noiseMap;
	}
}
