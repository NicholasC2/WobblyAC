using System.Collections;
using System.Collections.Generic;
using BepInEx;
using HawkNetworking;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using WobblyMenu.UI;

namespace WobblyMenu
{
    [BepInPlugin("com.wobblymenu.plugin", "Wobbly Menu", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private Rect spawnMenuWindowRect;
        private Rect itemEditorWindowRect;
        private Rect variableMenuWindowRect;

        private Vector2 variableScrollPosition;
        private Vector2 itemEditorScrollPosition;
        private Vector2 itemSpawnScrollPosition;

        private const int SPAWN_WINDOW_ID = 0;
        private const int VARIABLE_WINDOW_ID = 1;
        private const int ITEM_EDITOR_WINDOW_ID = 2;

        private GameObject currentlySelectedItem = null;
        private bool isItemEditorOpen = false;
        private int itemSpawnAmount = 1;
        private bool isMenuVisible = true;

        private string itemSearch = "";
        private string itemEditorSearch = "";
        private string variableSearch = "";

        private int moneyAmount = 10;

        private GUIStyle leftButtonStyle;
        private bool guiInit;

        private List<GameObject> assets = new List<GameObject>();
        private HashSet<GameObject> loadedAssets = new HashSet<GameObject>();

        private static Dictionary<string, string> variableEditCache = new Dictionary<string, string>();

        void Awake()
        {
            Debug.Log("PLUGIN: Awake");
            ModRegistry.RegisterAll();
            StartCoroutine(WaitThenDump());
        }

        IEnumerator WaitThenDump()
        {
            yield return new WaitForSeconds(1f);

            foreach (var locator in Addressables.ResourceLocators)
            {
                foreach (var key in locator.Keys)
                {
                        Addressables.LoadAssetAsync<GameObject>(key.ToString()).Completed += handle =>
                        {
                            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
                            {
                                Debug.LogError("Failed to load: " + key.ToString());
                                return;
                            }

                            GameObject prefab = handle.Result;

                            if (prefab.GetComponent<HawkTransformSync>() != null)
                            {
                                if (loadedAssets.Add(prefab))
                                {
                                    assets.Add(prefab);
                                }
                            }
                        };
                }
            }
        }

        void Update()
        {
            spawnMenuWindowRect = new Rect(20, 20, 200, Screen.height - 40);
            itemEditorWindowRect = new Rect(250, 20, 250, 400);
            variableMenuWindowRect = new Rect(Screen.width - 220, 20, 200, Screen.height - 40);

            if (Input.GetKeyDown(KeyCode.Insert))
                isMenuVisible = !isMenuVisible;
        }

        void InitGUI()
        {
            if (guiInit) return;

            leftButtonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true
            };

            guiInit = true;
        }

        void OnGUI()
        {
            InitGUI();
            if (!isMenuVisible) return;

            GUI.Window(SPAWN_WINDOW_ID, spawnMenuWindowRect, DrawSpawnMenuWindow, "Spawn Menu");
            GUI.Window(VARIABLE_WINDOW_ID, variableMenuWindowRect, DrawVariableWindow, "Variable Menu");

            if (isItemEditorOpen && currentlySelectedItem != null)
            {
                GUI.Window(ITEM_EDITOR_WINDOW_ID, itemEditorWindowRect, DrawItemEditorWindow,
                    "Item Spawn Config: " + currentlySelectedItem);
            }
        }

        void DrawVariableWindow(int id)
        {
            GUILayout.Label("Search:");
            variableSearch = GUILayout.TextField(variableSearch);

            variableScrollPosition = GUILayout.BeginScrollView(variableScrollPosition);

            GUILayout.BeginVertical("box");

            foreach (var variable in VariableRegistry.Variables)
            {
                var value = variable.GetValue();
                if (value == null) continue;

                string key = variable.name;

                string displayValue = value.ToString();

                if (!variableEditCache.TryGetValue(key, out string text))
                {
                    text = displayValue;
                }

                GUILayout.BeginHorizontal();

                GUILayout.Label(variable.name, GUILayout.Width(120));

                string newText = GUILayout.TextField(text);
                variableEditCache[key] = newText;

                if (newText == displayValue)
                {
                    variableEditCache[key] = displayValue;
                }

                if (newText != displayValue)
                {
                    try
                    {
                        if (variable.type == typeof(int) && int.TryParse(newText, out int i))
                            variable.SetValue(i);

                        else if (variable.type == typeof(float) && float.TryParse(newText, out float f))
                            variable.SetValue(f);

                        else if (variable.type == typeof(string))
                            variable.SetValue(newText);
                    }
                    catch { }
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();

            GUILayout.EndScrollView();
        }

        void CopyComponentValues(Component source, Component target)
        {
            var fields = source.GetType().GetFields(
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            foreach (var field in fields)
            {
                if (field.IsStatic) continue;

                field.SetValue(target, field.GetValue(source));
            }
        }

        void DrawItemEditorWindow(int id)
        {
            GUILayout.Label("Search:");
            itemEditorSearch = GUILayout.TextField(itemEditorSearch);

            itemEditorScrollPosition = GUILayout.BeginScrollView(itemEditorScrollPosition);
            
            foreach (Component component in currentlySelectedItem.GetComponents<Component>())
            {
                if (component == null)
                    continue;

                if(component is MoneyBag moneyBag)
                {
                    GUILayout.BeginVertical("box");

                    GUILayout.Label(component.GetType().Name);

                    GUILayout.BeginHorizontal();

                    GUILayout.Label("Money Amount");

                    int.TryParse(GUILayout.TextField(moneyAmount.ToString()), out moneyAmount);

                    GUILayout.EndHorizontal();
                    
                    GUILayout.EndVertical();
                }
            }

            GUILayout.EndScrollView();

            GUILayout.Label("Spawn Amount:");
            string input = GUILayout.TextField(itemSpawnAmount.ToString());
            int.TryParse(input, out itemSpawnAmount);

            if (GUILayout.Button("Spawn"))
            {
                var players = UnitySingleton<GameInstance>.Instance.GetLocalPlayerControllers();

                foreach (PlayerController player in players)
                {
                    if (!player) continue;

                    Vector3 pos = player.GetPlayerTransform().position;

                    for (int i = 0; i < itemSpawnAmount; i++)
                    {
                        Vector3 spawnPos = pos + new Vector3(0, 1f + i * 0.5f, 0);

                        GameObject instance = Instantiate(currentlySelectedItem, spawnPos, Quaternion.identity);

                        foreach (Component component in instance.GetComponents<Component>())
                        {
                            if (component == null)
                                continue;

                            if(component is MoneyBag moneyBag)
                            {
                                moneyBag.SetMoney(moneyAmount);
                            }
                        }
                    }
                }
            }

            if (GUILayout.Button("Close"))
            {
                isItemEditorOpen = false;
                currentlySelectedItem = null;
            }
        }

        void DrawSpawnMenuWindow(int id)
        {
            GUILayout.Label("Search:");
            itemSearch = GUILayout.TextField(itemSearch);

            itemSpawnScrollPosition = GUILayout.BeginScrollView(itemSpawnScrollPosition);

            string search = itemSearch.ToLower();

            int maxDraw = 200;
            int drawn = 0;

            foreach (GameObject obj in assets)
            {
                if (drawn >= maxDraw) break;

                if (!obj.name.ToLower().Contains(search))
                    continue;

                if (GUILayout.Button(obj.name, leftButtonStyle))
                {
                    currentlySelectedItem = obj;
                    isItemEditorOpen = true;
                }

                drawn++;
            }

            GUILayout.EndScrollView();
        }
    }
}