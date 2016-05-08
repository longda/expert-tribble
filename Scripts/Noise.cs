using UnityEngine;

public static class Noise 
{
	public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistence, float lacunarity, Vector2 offset)
	{
		var rand = new System.Random(seed);
		var noiseMap = new float[mapWidth, mapHeight];
		var octaveOffsets = new Vector2[octaves];
		var maxNoiseHeight = float.MinValue;
		var minNoiseHeight = float.MaxValue;
		var halfWidth = mapWidth / 2f;
		var halfHeight = mapHeight / 2f;
		
		for (var i = 0; i < octaves; i++)
		{
			var offsetX = rand.Next(-100000, 100000) + offset.x;
			var offsetY = rand.Next(-100000, 100000) + offset.y;
			octaveOffsets[i] = new Vector2(offsetX, offsetY);
		}
		
		if (scale <= 0) scale = 0.0001f;
		
		for (var x = 0; x < mapWidth; x++)
		{
			for (var y = 0; y < mapHeight; y++)
			{
				var amplitude = 1f;
				var frequency = 1f;
				var noiseHeight = 0f;
				
				for (var i = 0; i < octaves; i++)
				{
					var sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
					var sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;
					var perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
					
					noiseHeight += perlinValue * amplitude;
					amplitude *= persistence;
					frequency *= lacunarity;
				}
				
				if (noiseHeight > maxNoiseHeight)
				{
					maxNoiseHeight = noiseHeight;
				}
				else if (noiseHeight < minNoiseHeight)
				{
					minNoiseHeight = noiseHeight;
				}
				
				noiseMap[x, y] = noiseHeight;
			}
		}
		
		for (var x = 0; x < mapWidth; x++)
		{
			for (var y = 0; y < mapHeight; y++)
			{
				noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
			}
		}
		
		return noiseMap;
	}
}
