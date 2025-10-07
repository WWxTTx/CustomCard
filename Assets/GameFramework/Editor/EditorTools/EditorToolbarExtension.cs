using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using GameFramework;
using System.Linq;
using System.Reflection;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Unity.CodeEditor;
using System.IO;

namespace UGF.EditorTools
{
    public class EditorToolbarExtension
    {
        private static GUIContent switchSceneBtContent;
        private static GUIContent toolsDropBtContent;
        private static GUIContent openCsProjectBtContent;

        //Toolbar栏工具箱下拉列表
        private static List<Type> editorToolList;
        [InitializeOnLoadMethod]
        static void Init()
        {
            editorToolList = new List<Type>();
            var curOpenSceneName = EditorSceneManager.GetActiveScene().name;

            switchSceneBtContent = EditorGUIUtility.TrTextContentWithIcon(string.IsNullOrEmpty(curOpenSceneName) ? "Switch Scene" : curOpenSceneName, "切换场景", "UnityLogo");
            toolsDropBtContent = EditorGUIUtility.TrTextContentWithIcon("Tools", "工具箱", "CustomTool");
            openCsProjectBtContent = EditorGUIUtility.TrTextContentWithIcon("Open C# Project", "打开C#工程", "dll Script Icon");
            EditorSceneManager.sceneOpened += OnSceneOpened;
            ScanEditorToolClass();

            UnityEditorToolbar.LeftToolbarGUI.Add(OnRightToolbarGUI);
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            switchSceneBtContent.text = scene.name;
        }
        /// <summary>
        /// 获取所有EditorTool扩展工具类,用于显示到Toolbar的Tools菜单栏
        /// </summary>
        static void ScanEditorToolClass()
        {
            editorToolList.Clear();
            var editorDll = Utility.Assembly.GetAssemblies().First(dll => dll.GetName().Name.CompareTo("Assembly-CSharp-Editor") == 0);
            var allEditorTool = editorDll.GetTypes().Where(tp => (tp.IsClass && !tp.IsAbstract && tp.IsSubclassOf(typeof(EditorToolBase)) && tp.GetCustomAttribute(typeof(EditorToolMenuAttribute)) != null));

            editorToolList.AddRange(allEditorTool);
            editorToolList.Sort((x, y) =>
            {
                int xOrder = x.GetCustomAttribute<EditorToolMenuAttribute>().MenuOrder;
                int yOrder = y.GetCustomAttribute<EditorToolMenuAttribute>().MenuOrder;
                return xOrder.CompareTo(yOrder);
            });
        }

        private static void OnRightToolbarGUI()
        {
            if (EditorGUILayout.DropdownButton(switchSceneBtContent, FocusType.Passive, EditorStyles.toolbarPopup, GUILayout.MaxWidth(90)))
            {
                GenericMenu sceneMenu = new GenericMenu();
                string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");
                foreach (string guid in sceneGuids)
                {
                    if(AssetDatabase.GUIDToAssetPath(guid).EndsWith(".unity") && AssetDatabase.GUIDToAssetPath(guid).Contains("Game"))
                    {
                        string scenePath = AssetDatabase.GUIDToAssetPath(guid);
                        string sceneName = Path.GetFileNameWithoutExtension(scenePath);
                        sceneMenu.AddItem(new GUIContent(sceneName), sceneName == EditorSceneManager.GetActiveScene().name, OnSceneMenuClicked, scenePath);
                    }
                }
                sceneMenu.ShowAsContext();
            }

            EditorGUILayout.Space(10);
            if (EditorGUILayout.DropdownButton(toolsDropBtContent, FocusType.Passive, EditorStyles.toolbarPopup, GUILayout.MaxWidth(90)))
            {
                DrawEditorToolDropdownMenus();
            }
            EditorGUILayout.Space(10);
            if (GUILayout.Button(openCsProjectBtContent, EditorStyles.toolbarButton, GUILayout.MaxWidth(120)))
            {
                OpenCSharpProject();
            }
            GUILayout.FlexibleSpace();
        }

        private static void OnSceneMenuClicked(object userData)
        {
            string scenePath = (string)userData;
            EditorSceneManager.OpenScene(scenePath);
        }

        static void OpenCSharpProject()
        {
            // Ensure that the mono islands are up-to-date
            AssetDatabase.Refresh();
            CodeEditor.Editor.CurrentCodeEditor.SyncAll();

            CodeEditor.Editor.CurrentCodeEditor.OpenProject();
        }
       

        static void DrawEditorToolDropdownMenus()
        {
            GenericMenu popMenu = new GenericMenu();
            for (int i = 0; i < editorToolList.Count; i++)
            {
                var toolAttr = editorToolList[i].GetCustomAttribute<EditorToolMenuAttribute>();
                popMenu.AddItem(new GUIContent(toolAttr.ToolMenuPath), false, menuIdx => { ClickToolsSubmenu((int)menuIdx, toolAttr.IsUtility); }, i);
            }
            popMenu.ShowAsContext();
        }
        static void ClickToolsSubmenu(int menuIdx, bool showAsUtility = false)
        {
            var editorTp = editorToolList[menuIdx];
            var win = EditorWindow.GetWindow(editorTp);
            if (showAsUtility)
            {
                win.ShowUtility();
            }
            else
            {
                win.Show();
            }
        }
    }

}
