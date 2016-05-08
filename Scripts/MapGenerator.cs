using UnityEngine;

public class MapGenerator : MonoBehaviour 
{
	public enum DrawMode { NoiseMap, ColorMap, Mesh }
	public DrawMode drawMode;
	
	public const int MAP_CHUNK_SIZE = 241;
	[Range(0, 6)]
	public int levelOfDetail;
	public float noiseScale;
	public float meshHeightMultiplier;
	public AnimationCurve meshHeightCurve;
	
	public int octaves;
	[Range(0, 1)]
	public float persistence;
	public float lacunarity;
	
	public int seed;
	public Vector2 offset;
	
	public bool autoGenerate;
	
	public TerrainType[] regions;
	
	public void GenerateMap()
	{
		var noiseMap = Noise.GenerateNoiseMap(MAP_CHUNK_SIZE, MAP_CHUNK_SIZE, seed, noiseScale, octaves, persistence, lacunarity, offset);
		var display = FindObjectOfType<MapDisplay>();
		var colorMap = new Color[MAP_CHUNK_SIZE * MAP_CHUNK_SIZE];
		
		for (var x = 0; x < MAP_CHUNK_SIZE; x++)
		{
			for (var y = 0; y < MAP_CHUNK_SIZE; y++)
			{
				var currentHeight = noiseMap[x, y];
				for (var i = 0; i < regions.Length; i++)
				{
					if (currentHeight <= regions[i].height)
					{
						colorMap[y * MAP_CHUNK_SIZE + x] = regions[i].color;
						break;
					}
				}
			}
		}
		
		if (drawMode == DrawMode.NoiseMap)
		{
			display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));	
		}
		else if (drawMode == DrawMode.ColorMap)
		{
			display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, MAP_CHUNK_SIZE, MAP_CHUNK_SIZE));
		}
		else if (drawMode == DrawMode.Mesh)
		{
			display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColorMap(colorMap, MAP_CHUNK_SIZE, MAP_CHUNK_SIZE));
		}
	}
	
	void OnValidate()
	{
		if (lacunarity < 1) lacunarity = 1;
		if (octaves < 0) octaves = 0;
	}
}

[System.Serializable]
public struct TerrainType
{
	public string name;
	public float height;
	public Color color;
}
