
using UnityEditor;
using UnityEngine;

public class PathGeneratorEditor : EditorWindow
{
    private PathGenerator targetAsset;

    [MenuItem("Tools/Generate Test Paths")]
    public static void ShowWindow()
    {
        GetWindow<PathGeneratorEditor>("Path Generator Tools");
    }

    private void OnGUI()
    {
        GUILayout.Label("Generate Test Paths", EditorStyles.boldLabel);

        targetAsset = (PathGenerator)EditorGUILayout.ObjectField("Path Generator Asset", targetAsset, typeof(PathGenerator), false);

        if (targetAsset == null) return;

        if (GUILayout.Button("Generate Horizontal Paths (7x7 Grid)"))
        {
            GenerateHorizontalPaths(targetAsset, 7, 7);
        }

        if (GUILayout.Button("Generate Vertical Paths (7x7 Grid)"))
        {
            GenerateVerticalPaths(targetAsset, 7, 7);
        }

        if (GUILayout.Button("Generate Diagonal Path (Top-left to Bottom-right)"))
        {
            GenerateDiagonalPath(targetAsset, 7);
        }

        EditorUtility.SetDirty(targetAsset);
        AssetDatabase.SaveAssets();
    }

    private void GenerateHorizontalPaths(PathGenerator generator, int rows, int cols)
    {
        generator.presetPaths.Clear();

        for (int row = 0; row < rows; row++)
        {
            IntPath path = new IntPath();
            path.values = new System.Collections.Generic.List<int>();
            for (int col = 0; col < cols; col++)
            {
                path.values.Add(row * cols + col);
            }
            generator.presetPaths.Add(path);
        }
    }

    private void GenerateVerticalPaths(PathGenerator generator, int rows, int cols)
    {
        generator.presetPaths.Clear();

        for (int col = 0; col < cols; col++)
        {
            IntPath path = new IntPath();
            path.values = new System.Collections.Generic.List<int>();
            for (int row = 0; row < rows; row++)
            {
                path.values.Add(row * cols + col);
            }
            generator.presetPaths.Add(path);
        }
    }

    private void GenerateDiagonalPath(PathGenerator generator, int size)
    {
        generator.presetPaths.Clear();

        IntPath path = new IntPath();
        path.values = new System.Collections.Generic.List<int>();
        for (int i = 0; i < size; i++)
        {
            path.values.Add(i * size + i);
        }
        generator.presetPaths.Add(path);
    }
}
