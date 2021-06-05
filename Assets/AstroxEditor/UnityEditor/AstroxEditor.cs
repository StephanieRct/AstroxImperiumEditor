using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AstroxEditor
{

    public class AstroxEditorWindow : EditorWindow
    {
        public const string DefaultAstroxFolder = @"D:\game\Steam\steamapps\common\Astrox Imperium\Astrox Imperium_Data";
        [MenuItem("Astrox/Editor", false, 2020)]
        private static void ShowPreviewWindow()
        {
            var windows = (AstroxEditorWindow[])Resources.FindObjectsOfTypeAll(typeof(AstroxEditorWindow));
            foreach (var w in windows)
            {
                if (w != null) w.Close();
            }
            var window = CreateInstance<AstroxEditorWindow>();
            window.Show();
            window.titleContent = new GUIContent("Astrox Editor");


            if (Directory.Exists(DefaultAstroxFolder))
            {
                window.AstroxDir = DefaultAstroxFolder;
            }
        }
        public string AstroxDir;
        public string SaveGameFolder => AstroxDir + "\\MOD\\saves";

        public bool SaveGameFoldout = true;
        public Vector2 SaveGameScroll;

        public SaveGame CurrentSaveGame;
        public string SaveToName;

        public GameObject GalaxyObject;
        public void Update()
        {

        }
        public void OnGUI()
        {
            OnGUISaveGame();

            bool guiEnableOld = GUI.enabled;
            GUI.enabled = CurrentSaveGame != null;
            OnGUISave();
            OnGUIGalaxyRelax();
            OnGUIGalaxyScale();
            GUI.enabled = guiEnableOld;

            
        }

        public bool GalaxyRelaxHelp = false;
        public int GalaxyRelaxIteration = 30;
        public float GalaxyRelaxDist0=500;
        public float GalaxyRelaxDist1=5000;
        public int GalaxyRelaxDistWarpGateThreshold = 9;
        
        public float GalaxyRelaxDistW=0.2f;
        public float GalaxyRelaxAngleW=0.4f;
        public float GalaxyRelaxAngleMin0 = 120;
        public float GalaxyRelaxAngleMin1 = 80;
        public int GalaxyRelaxAngleWarpGateThreshold = 10;

        public void OnGUIGalaxyRelax()
        {
            GalaxyRelaxHelp = EditorGUILayout.Foldout(GalaxyRelaxHelp, "Relax Help");
            if (GalaxyRelaxHelp)
            {
                EditorGUILayout.HelpBox(
                    "Iteration: number of iteration to compute\n"
                    + "Dist WarpGate Threshold: Number of warp gate the sum of both sectors must have to use Distance 1. If under, uses Distance 0\n"
                    + "Distance 0: target distance between 2 sectors with less than 'Dist WarpGate Threshold' warp gates total\n"
                    + "Distance 1: target distance between 2 sectors with, or more than, 'Dist WarpGate Threshold' warp gates total\n"
                    + "Angle WarpGate Threshold: Number of warp gates the sum of all 3 sectors must have to use Angle 1. If less, uses Angle 0. \n"
                    + "Angle 0: Target angle between all sets of 3 connected sectors that have less than 'Angle WarpGate Threshold' warp gates\n"
                    + "Angle 0: Target angle between all sets of 3 connected sectors that have, or more than, 'Angle WarpGate Threshold' warp gates\n"
                    + "Distance Weight: How much distance constraint is performed. Between 0 and 1\n"
                    + "Angle Weight: How much angle constraint is performed. Between 0 and 1\n"
                    , MessageType.Info
                    );
            }
            GalaxyRelaxIteration = EditorGUILayout.IntField("Iteration", GalaxyRelaxIteration);
            GalaxyRelaxDistWarpGateThreshold = EditorGUILayout.IntField("Dist WarpGate Threshold", GalaxyRelaxDistWarpGateThreshold);
            GalaxyRelaxDist0 = EditorGUILayout.FloatField("Distance 0", GalaxyRelaxDist0);
            GalaxyRelaxDist1 = EditorGUILayout.FloatField("Distance 1", GalaxyRelaxDist1);

            GalaxyRelaxAngleWarpGateThreshold = EditorGUILayout.IntField("Angle WarpGate Threshold", GalaxyRelaxAngleWarpGateThreshold);
            GalaxyRelaxAngleMin0 = EditorGUILayout.FloatField("Angle 0", GalaxyRelaxAngleMin0);
            GalaxyRelaxAngleMin1 = EditorGUILayout.FloatField("Angle 1", GalaxyRelaxAngleMin1);
            GalaxyRelaxDistW = EditorGUILayout.FloatField("Distance Weight", GalaxyRelaxDistW);
            GalaxyRelaxAngleW = EditorGUILayout.FloatField("Angle Weight", GalaxyRelaxAngleW);
            if (GUILayout.Button("Relax"))
            {
                GalaxyRelaxer.RelaxSector(CurrentSaveGame, false, GalaxyRelaxIteration
                    , GalaxyRelaxDist0, GalaxyRelaxDist1, GalaxyRelaxDistWarpGateThreshold, GalaxyRelaxDistW
                    , GalaxyRelaxAngleMin0 / 180 * Math.PI, GalaxyRelaxAngleMin1 / 180 * Math.PI, GalaxyRelaxAngleWarpGateThreshold, GalaxyRelaxAngleW);
                GalaxyObject.SetActive(false);
                GalaxyObject.SetActive(true);
            }
        }


        public float GalaxyScale = 0.9f;
        public void OnGUIGalaxyScale()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Scale"))
            {
                GalaxyRelaxer.ScaleSectors(CurrentSaveGame, GalaxyScale);
                GalaxyObject.SetActive(false);
                GalaxyObject.SetActive(true);
            }
            GalaxyScale = EditorGUILayout.FloatField("Scale", GalaxyScale);
            GUILayout.EndHorizontal();
        }
        public void OnGUISave()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save"))
            {
                CurrentSaveGame.Save();
            }
            if (GUILayout.Button("SaveTo"))
            {
                CurrentSaveGame.SaveTo(SaveGameFolder + "\\" + SaveToName);
            }
            SaveToName = EditorGUILayout.TextField(SaveToName);
            GUILayout.EndHorizontal();
        }

        public void OnGUISaveGame()
        {

            SaveGameFoldout = EditorGUILayout.Foldout(SaveGameFoldout, "Save Game");
            if (SaveGameFoldout)
            {

                string newAstroxDir = EditorGUILayout.TextField("Astrox Dir:", AstroxDir);
                if (newAstroxDir != AstroxDir)
                {
                    AstroxDir = newAstroxDir;
                    UpdateSaveGameList();
                }


                EditorGUILayout.BeginScrollView(SaveGameScroll, GUILayout.MaxHeight(128));
                EditorGUILayout.BeginVertical();

                foreach (var f in Directory.EnumerateDirectories(SaveGameFolder))
                {
                    var n = Path.GetFileName(f);
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Load"))
                    {
                        AstroxEditor.Log($"Loading '{f}' ... ");
                        var sg = new SaveGame(f);
                        if (!sg.Load())
                        {
                            AstroxEditor.LogError("Failed!");
                        }
                        else
                        {
                            AstroxEditor.Log("Done!");
                            CurrentSaveGame = sg;
                        }
                        UpdateSaveGame();
                    }
                    EditorGUILayout.LabelField(n);

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndScrollView();
            }
        }
        public void UpdateSaveGameList()
        {

        }
        public void UpdateSaveGame()
        {

            if(GalaxyObject != null)
            {
                GameObject.DestroyImmediate(GalaxyObject);
            }
            var galaxyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AstroxEditor/UnityEditor/GalaxyObj.prefab");
            //Resources.Load("");
            GalaxyObject = GameObject.Instantiate(galaxyPrefab, Vector3.zero, Quaternion.identity, null);
            GalaxyRender gr = GalaxyObject.GetComponent<GalaxyRender>();
            gr.CurrentSaveGame = CurrentSaveGame;
            
        }
    }

}