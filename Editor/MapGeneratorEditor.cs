using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor 
{
	public override void OnInspectorGUI()
	{
		var mapGen = (MapGenerator) target;
		
		if (DrawDefaultInspector() && mapGen) mapGen.DrawMapInEditor();
		
		if (GUILayout.Button("Generate")) mapGen.DrawMapInEditor();
	}
}
