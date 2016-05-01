using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor 
{
	public override void OnInspectorGUI()
	{
		var mapGen = (MapGenerator) target;
		
		if (DrawDefaultInspector() && mapGen) mapGen.GenerateMap();
		
		if (GUILayout.Button("Generate")) mapGen.GenerateMap();
	}
}
