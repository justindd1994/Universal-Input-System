﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class UIS_Profiles
{
    public static string SavePath;
    public List<UniversalInputSystem.BindingDictionary> jsonImportData = new List<UniversalInputSystem.BindingDictionary>();
    public int lastKnownFileSize = -1;
    private bool Initialized = false;
    [HideInInspector] public int lastKnownSelectedProfileOption;

    public void Init()
    {
        if (Initialized == true)
            return;

        SavePath = ExportUISProfile.SearchFile(Application.dataPath, "UniversalInputSystem.cs") + @"\Profiles";
        DirectoryInfo d = new DirectoryInfo(SavePath);
        FileInfo[] Files = d.GetFiles("*.uisp");

        if (lastKnownFileSize == Files.Length)
            return;

        jsonImportData = new List<UniversalInputSystem.BindingDictionary>();
        UniversalInputSystem.profileOptions.Clear();

        foreach (FileInfo file in Files)
        {
            string contents = File.ReadAllText(file.FullName);
            UniversalInputSystem.profileOptions.Add(file.Name.Replace(".uisp", ""));
            jsonImportData.Add(JsonUtility.FromJson<UniversalInputSystem.BindingDictionary>(contents));
        }

        lastKnownFileSize = Files.Length;
        Initialized = true;
    }

    public void ExportProfile()
    {
        ExportUISProfile.Init();
    }

    public void ImportProfile()
    {

    }

    public void SetProfile(UniversalInputSystem inputSystem, UniversalInputSystem.BindingDictionary profile)
    {
        if (UIS_Settings.Instance.selectedProfileOption != lastKnownSelectedProfileOption)
        {
            lastKnownSelectedProfileOption = UIS_Settings.Instance.selectedProfileOption;
            inputSystem.definedBindings = profile;

            foreach (var item in inputSystem.definedBindings)
            {
                item.Value.editing = false;
            }

            UIS_Settings.Instance.SaveData();
        }
    }
}

public class ExportUISProfile : EditorWindow
{
    private string profileName;
    private static string Json;
    private static string SavePath;
    public static string format = ".uisp";

    private static GUIStyle ValidGUI;
    private static  GUIStyle InvalidGUI;

    public static void Init()
    {
        var window = GetWindow<ExportUISProfile>("Export UIS Profile");
        var position = window.position;
        position.center = new Rect(0f, 0f, Screen.currentResolution.width, Screen.currentResolution.height).center;
        window.position = position;

        Json = JsonUtility.ToJson(UniversalInputSystem.Instance.definedBindings, true);
        SavePath = SearchFile(Application.dataPath, "UniversalInputSystem.cs") + @"\Profiles";

        ValidGUI = new GUIStyle(EditorStyles.label);
        ValidGUI.normal.textColor = Color.green;

        InvalidGUI = new GUIStyle(EditorStyles.label);
        InvalidGUI.normal.textColor = Color.red;

        window.ShowPopup();
    }

    public void OnGUI()
    {
        EditorGUILayout.LabelField("Enter a name for your profile:", EditorStyles.wordWrappedLabel);
        profileName = EditorGUILayout.TextField("Profile Name:", profileName);

        var isValid = !string.IsNullOrEmpty(profileName) &&
                profileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0 &&
                !File.Exists(Path.Combine(SavePath, profileName));

        GUILayout.Space(25);

        string path = ("Path:             " + SavePath).Replace(@"\", @"/");
        string name = ("Name:           " + profileName + format).Replace(@"\", @"/");
        string combined = ("Combined:   " + SavePath + @"/" + profileName + format).Replace(@"\", @"/");

        EditorGUILayout.LabelField("Saving Info:");
        EditorGUILayout.LabelField(path, ValidGUI);
        EditorGUILayout.LabelField(name, (isValid == true) ? ValidGUI : InvalidGUI);
        EditorGUILayout.LabelField(combined, (isValid == true) ? ValidGUI : InvalidGUI);
        GUILayout.Space(25);

        if (isValid == false)
            EditorGUI.BeginDisabledGroup(true);

        if (GUILayout.Button("Save"))
        {
            if (isValid == false)
            {
                Debug.Log("Invalid Name!");
                return;
            }

            SavePath += @"\" + profileName + format;
            File.WriteAllText(SavePath, Json);
            this.Close();
        }

        if (isValid == false)
            EditorGUI.EndDisabledGroup();

        if (GUILayout.Button("Close"))
        {
            this.Close();
        }
    }

    public static string SearchFile(string searchPath, string fileName)
    {
        DirectoryInfo dir = new DirectoryInfo(searchPath);
        DirectoryInfo[] dirs = dir.GetDirectories();

        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            if (file.Name.Equals(fileName))
                return searchPath;
        }

        foreach (DirectoryInfo subDir in dirs)
        {
            string value = SearchFile(Path.Combine(searchPath, subDir.Name), fileName);
            if (value != null)
                return value;
        }

        return null;
    }
}
