using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.Callbacks;

namespace EIJ.BattleMap
{
    public class BattleMapAreaEditorWindow : EditorWindow
    {
        [MenuItem("Develop Tools/BattleMapArea Editor")]
        static void Init()
        {
            BattleMapAreaEditorWindow window = (BattleMapAreaEditorWindow)GetWindow(typeof(BattleMapAreaEditorWindow));
            window.Show();
        }

        private void OnEnable()
        {
            minSize = new Vector2(530, 350);
            Shader GUIShader = Shader.Find("Hidden/EditorGUI/BattleMapAreaEditorShader");
            if (GUIShader == null)
            {
                EditorUtility.DisplayDialog("Error", "BattleMapArea Editor: GUIShader not Found!", "OK");
                Close();
            }
            material = new Material(GUIShader);

            Undo.undoRedoPerformed += Repaint;

            ////////////////////////////
            ////    Get Prop ID    /////
            ////////////////////////////
            #region [ Get Prop ID ]
            PID._MainTex = Shader.PropertyToID("_MainTex");
            PID._Color = Shader.PropertyToID("_Color");
            PID._SrcBlend = Shader.PropertyToID("_SrcBlend");
            PID._DstBlend = Shader.PropertyToID("_DstBlend");
            #endregion
        }

        private void OnDisable()
        {
            CloseFile();
            Undo.undoRedoPerformed -= Repaint;
        }

        /////////////////////////////
        ////    File Function    ////
        /////////////////////////////
        #region [ File Function ]
        static void OpenFile(BattleMapArea file)
        {
            if (file == null) return;
            opened = file;
            serializedObject = new SerializedObject(file);           
            if (serializedObject != null) dataOpened = true;
            //////////////////////////
            ////    Properties    ////
            //////////////////////////
            #region [ Properties ]
            #endregion
            GUI.changed = true;
        }
        static void CloseFile()
        {
            if (opened != null)
            {
                Undo.ClearUndo(opened);
            }
            opened = null;
            serializedObject = null;
            dataOpened = false;
            //////////////////////////
            ////    Properties    ////
            //////////////////////////
            #region [ Properties ]
            #endregion
            GUI.changed = true;
        }
        static BattleMapArea GetSelectedFileFromAssets()
        {
            string[] guids = Selection.assetGUIDs;
            if (guids.Length <= 0) return null;
            BattleMapArea load = null;
            foreach(string guid in guids)
            {
                load = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(BattleMapArea)) as BattleMapArea;
            }
            return load;
        }
        static void TryOpenSelected()
        {
            BattleMapArea target = GetSelectedFileFromAssets();
            if(target != null)
            {
                CloseFile();
                OpenFile(target);
            }
            GUI.changed = true;
        }

        [OnOpenAssetAttribute(1)]
        public static bool OpenFileInWindow(int instanceID, int line)
        {
            Object file = EditorUtility.InstanceIDToObject(instanceID);
            if(file.GetType() == typeof(BattleMapArea))
            {
                Init();
                CloseFile();
                OpenFile(file as BattleMapArea);
                return true;
            } else
            {
                return false;
            }
        }

        #endregion

        //////////////////////
        ////    Styles    ////
        //////////////////////
        #region [ Styles ]
        static class Styles
        {
            public static Color backgroundColor = new Color32(35, 35, 35, 255);
            public static Color gridColor = new Color32(255, 255, 255, 50);
            public static Color[] templateCellColors = new Color[]{
                new Color32(65, 65, 65, 255),
                new Color32(180, 160, 90, 255) };
            public static Color[] variantsCellColors = new Color[]{
                new Color32(55, 65, 55, 255),
                new Color32(65, 55, 55, 255),
                new Color32(180, 160, 90, 255)};

            public static GUIStyle defaultButtonStyle = new GUIStyle(GUI.skin.button);
            public static GUIStyle currentToolButtonStyle = GetCurrentToolButtonStyle();
            public static GUIStyle defaultLabelStyle = new GUIStyle(GUI.skin.label);
            public static GUIStyle currentProcessStyle = GetCurrentProcessStyle();

            public static GUIContent[] typePopupString = new GUIContent[] {
                new GUIContent("Normal"), new GUIContent("Home"), new GUIContent("Spawn") };
            public static int[] typePopupValue = new int[] { 0, 1, 2 };
            public static GUIStyle typeLabelStyle = GetTypeLabelStyle();
        }

        static GUIStyle GetCurrentToolButtonStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.button) { };
            style.fontStyle = FontStyle.Bold;
            Color textColor = new Color32(30, 30, 250, 255);
            style.normal.textColor = textColor;
            style.hover.textColor = textColor;
            style.active.textColor = textColor;
            return style;
        }

        static GUIStyle GetCurrentProcessStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label) { fixedHeight = 20 };
            style.fontStyle = FontStyle.Bold;
            style.fontSize = 15;
            Color textColor = new Color32(150, 30, 250, 255);
            style.normal.textColor = textColor;
            return style;
        }

        static GUIStyle GetTypeLabelStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label) { fixedHeight = 20 };
            style.fontStyle = FontStyle.Bold;
            Color textColor = new Color32(0, 100, 0, 255);
            style.normal.textColor = textColor;
            return style;
        }
        #endregion

        Vector2 scrollPos = Vector2.zero;
        static readonly int s_DrawMapWindowHash = "EIJ.BattleMap.BattleMapAreaEditorWindow.DrawMapWindow".GetHashCode();

        ///////////////////////////////
        ////    Object Variants    ////
        ///////////////////////////////
        #region [ Object Variants ]
        static BattleMapArea opened = null;
        static SerializedObject serializedObject = null;
        static bool dataOpened = false;
        #endregion

        //////////////////////////////
        ////    Paint Variants    ////
        //////////////////////////////
        #region [ Paint Variants ]
        enum TemplateToolType
        {
            Normal = 0,
            Entry = 1,
        }
        enum VariantsToolType
        {
            Path,
            Platform,
            Home,
            Spawn,
        }

        TemplateToolType currentTemplateToolType = TemplateToolType.Normal;
        VariantsToolType currentVariantsToolType = VariantsToolType.Path;
        #endregion

        ///////////////////////////////
        ////    Canvas Variants    ////
        ///////////////////////////////
        #region [ Canvas Variants ]
        const float cameraRange = 10f;
        Vector2 cameraPosition = Vector2.zero;
        float viewScale = 1.0f;
        Vector2 recordMousePosition = Vector2.zero;
        Vector2 recordCameraPosition = Vector2.zero;
        Material material = null;

        ///////////////////////
        ////    Control    ////
        ///////////////////////
        #region [ Control ]
        enum Action
        {
            None,
            Dragging,
            Painting,
            Erazing,
        }

        Action currentAction = Action.None;
        #endregion

        ////////////////////////
        ////    Prop ID    /////
        ////////////////////////
        #region [ Prop ID ]
        static class PID
        {
            public static int _MainTex;
            public static int _Color;
            public static int _SrcBlend;
            public static int _DstBlend;
        }
        #endregion
        #endregion

        private void OnGUI()
        {
            if(dataOpened) serializedObject.Update();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Close")) CloseFile();
            if (GUILayout.Button("Open Selected")) TryOpenSelected();
            EditorGUILayout.EndHorizontal();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(opened, typeof(BattleMapArea), false);
            EditorGUI.EndDisabledGroup();
            GuiLine();
            if (dataOpened)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Type:", EditorStyles.boldLabel, GUILayout.Width(50));
                if (opened.processTemplate)
                {
                    BattleMapAreaType newType = (BattleMapAreaType)EditorGUILayout.IntPopup((int)opened.type, Styles.typePopupString, Styles.typePopupValue);
                    if (newType != opened.type)
                    {
                        Undo.RegisterCompleteObjectUndo(opened, "Change Type");
                        opened.type = newType;
                    }
                }
                else
                {
                    EditorGUILayout.LabelField(Styles.typePopupString[(int)opened.type], Styles.typeLabelStyle, GUILayout.Width(50));
                }
                EditorGUILayout.EndHorizontal();
            }
            DrawProcess();
            DrawTools();
            GuiLine();
            EditorGUILayout.LabelField("[ Variants ]", EditorStyles.boldLabel, GUILayout.Width(100));
            GuiLine();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            //variants list
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            Rect mapWindowRect = EditorGUILayout.GetControlRect(new GUILayoutOption[] {
            GUILayout.Width(position.width - 180),
            GUILayout.Height(position.height) });
            SetCursor(mapWindowRect);
            DrawMapWindow(mapWindowRect);
            EditorGUILayout.EndHorizontal();
            if(dataOpened) serializedObject.ApplyModifiedProperties();
        }

        //////////////////////
        ////    Cursor    ////
        //////////////////////
        #region [ Cursor ]
        void SetCursor(Rect rect)
        {
            if (currentAction == Action.Dragging)
            {
                EditorGUIUtility.AddCursorRect(rect, MouseCursor.Pan);
            } else {
                if (opened != null)
                {
                    if (!opened.processCompleted)
                    {
                        if (currentAction == Action.Painting)
                            EditorGUIUtility.AddCursorRect(rect, MouseCursor.ArrowPlus);
                        else if (currentAction == Action.Erazing)
                            EditorGUIUtility.AddCursorRect(rect, MouseCursor.ArrowMinus);
                    }
                }
            }
        }
        #endregion
        ////////////////////////////
        ////    Draw Process    ////
        ////////////////////////////
        #region [ Draw Process ]
        void DrawProcess()
        {
            GuiLine();
            EditorGUILayout.LabelField("[ Process ]", EditorStyles.boldLabel, GUILayout.Width(100));
            GuiLine();
            if(opened == null)
            {
                EditorGUILayout.LabelField("No file opened", GUILayout.Width(100));
            } 
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Template", opened.processTemplate ? Styles.currentProcessStyle : Styles.defaultLabelStyle, GUILayout.Width(100));
                if(GUILayout.Button(opened.processTemplate ? "Apply" : "Modify"))
                {
                    if (opened.processTemplate)
                    {
                        Undo.RegisterCompleteObjectUndo(opened, "Apply Template");
                        string log = opened.ApplyTemplate();
                        if(log.Length > 0)
                        {
                            EditorUtility.DisplayDialog("x_x!", "Failed to apply template!\n" + log, "OK");
                        }
                    }
                    else
                    {
                        if (EditorUtility.DisplayDialog("'o'!", "All varients need to be recomplied if template changed!",
                            "Confirm", "Cancel"))
                        {
                            Undo.RegisterCompleteObjectUndo(opened, "Reedit Template");
                            opened.ReeditTemplate();
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.LabelField("Varients", opened.processVarient ? Styles.currentProcessStyle : Styles.defaultLabelStyle, GUILayout.Width(100));
                EditorGUILayout.LabelField("Complete", opened.processCompleted ? Styles.currentProcessStyle : Styles.defaultLabelStyle, GUILayout.Width(100));
            }
            GuiLine();
        }
        #endregion
        //////////////////////////
        ////    Draw Tools    ////
        //////////////////////////
        #region [ Draw Tools ]
        void DrawTools()
        {
            GuiLine();
            EditorGUILayout.LabelField ("[ Tools ]", EditorStyles.boldLabel, GUILayout.Width(100));
            GuiLine();
            SwitchTool();
            if (opened != null)
            {
                if (opened.processTemplate)
                {
                    //////////////////////////////
                    ////    Template Tools    ////
                    //////////////////////////////
                    #region [ Template Tools ]
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Normal", currentTemplateToolType == TemplateToolType.Normal ? 
                        Styles.currentToolButtonStyle : Styles.defaultButtonStyle)) 
                        currentTemplateToolType = TemplateToolType.Normal;
                    if (GUILayout.Button("Entry", currentTemplateToolType == TemplateToolType.Entry ?
                        Styles.currentToolButtonStyle : Styles.defaultButtonStyle))
                        currentTemplateToolType = TemplateToolType.Entry;
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(7);
                    if (GUILayout.Button("Clear"))
                    {
                        Undo.RegisterCompleteObjectUndo(opened, "Clear Template");
                        opened.ClearAllHolders();
                    }
                    GUILayout.Space(3);
                    #endregion
                }
            } 
            else
            {
                EditorGUILayout.LabelField("Empty", GUILayout.Width(100));
            }
            GuiLine();
        }
        #endregion
        ///////////////////////////////
        ////    Draw Map Window    ////
        ///////////////////////////////
        #region [ Draw Map Window ]
        void DrawMapWindow(Rect rect)
        {
            Event currentEvent = Event.current;
            int controlID = GUIUtility.GetControlID(s_DrawMapWindowHash, FocusType.Passive, rect);

            switch (currentEvent.GetTypeForControl(controlID))
            {
                ///////////////////////
                ////    Control    ////
                ///////////////////////
                #region [ Control ]
                case EventType.MouseDown:
                    if (rect.Contains(currentEvent.mousePosition))
                    {
                        if (currentEvent.button == 2 && currentAction == Action.None)
                        {
                            currentAction = Action.Dragging;
                            recordMousePosition = currentEvent.mousePosition;
                            recordCameraPosition = cameraPosition;
                            GUIUtility.hotControl = controlID;
                            currentEvent.Use();
                        }
                        if ((currentEvent.button == 0 || currentEvent.button == 1) && currentAction == Action.None)
                        {
                            /////////////////////////////
                            ////    Paint Control    ////
                            /////////////////////////////
                            #region [ Paint Control ]
                            if (opened != null)
                            {
                                if(!opened.processCompleted)
                                {
                                    Vector2 mousePos = currentEvent.mousePosition;
                                    Vector2 screenOffset = new Vector2(
                                        mousePos.x - rect.x - rect.width * 0.5f,
                                        mousePos.y - rect.y - rect.height * 0.5f);
                                    Vector2 canvasPos = screenOffset / rect.width * 20f * viewScale * new Vector2(1.0f, -1.0f) + cameraPosition;
                                    GUI.changed = true;
                                    currentAction = currentEvent.button == 0 ? Action.Painting : Action.Erazing;
                                    GUIUtility.hotControl = controlID;
                                    Undo.RegisterCompleteObjectUndo(opened, "Edit Map");
                                    EditPaint(canvasPos, currentAction);
                                    currentEvent.Use();
                                }
                            }
                            #endregion
                        }
                    }
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlID)
                    {
                        if (currentAction == Action.Dragging)
                        {
                            GUI.changed = true;
                            Vector2 mouseOffset = currentEvent.mousePosition - recordMousePosition;
                            mouseOffset.y = -mouseOffset.y;
                            Vector2 newCamPos = recordCameraPosition - mouseOffset / rect.width * 20f * viewScale;
                            cameraPosition.x = Mathf.Clamp(newCamPos.x, -cameraRange, cameraRange);
                            cameraPosition.y = Mathf.Clamp(newCamPos.y, -cameraRange, cameraRange);
                            currentEvent.Use();
                        }
                        if (currentAction == Action.Painting || currentAction == Action.Erazing)
                        {
                            GUI.changed = true;
                            Vector2 mousePos = currentEvent.mousePosition;
                            Vector2 screenOffset = new Vector2(
                                mousePos.x - rect.x - rect.width * 0.5f,
                                mousePos.y - rect.y - rect.height * 0.5f);
                            Vector2 canvasPos = screenOffset / rect.width * 20f * viewScale * new Vector2(1.0f, -1.0f) + cameraPosition;
                            EditPaint(canvasPos, currentAction);
                            currentEvent.Use();
                        }
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID)
                    {
                        currentAction = Action.None;
                        GUIUtility.hotControl = 0;
                        currentEvent.Use();
                    }
                    break;
                case EventType.ScrollWheel:
                    if (rect.Contains(currentEvent.mousePosition))
                    {
                        if (focusedWindow == this && currentAction == Action.None)
                        {
                            GUI.changed = true;
                            float scaleChange = 1.0f;
                            if (currentEvent.delta.y > 0) //wheel up
                                scaleChange = 1.1f;
                            else if (currentEvent.delta.y < 0) //wheel down
                                scaleChange = 1f / 1.1f;
                            float newViewScale = Mathf.Clamp(viewScale * scaleChange, 0.5f, 2.0f);
                            scaleChange = newViewScale / viewScale;
                            Vector2 mousePos = currentEvent.mousePosition;
                            Vector2 screenOffset = new Vector2(
                                mousePos.x - rect.x - rect.width * 0.5f,
                                mousePos.y - rect.y - rect.height * 0.5f);
                            Vector2 newCamPos = cameraPosition -
                                (screenOffset / rect.width * 20f * viewScale * (scaleChange - 1.0f)) * new Vector2(1.0f, -1.0f);
                            cameraPosition.x = Mathf.Clamp(newCamPos.x, -cameraRange, cameraRange);
                            cameraPosition.y = Mathf.Clamp(newCamPos.y, -cameraRange, cameraRange);
                            viewScale = newViewScale;
                            currentEvent.Use();
                        }
                    }
                    break;

                #endregion

                case EventType.Repaint:
                    DrawMapCanvas(rect);
                    break;
            }
        }

        void DrawMapCanvas(Rect rect)
        {
            GUI.BeginGroup(rect);
            GL.PushMatrix();

            Rect viewportRect = rect;
            if (viewportRect.position.x < 0f)
            {
                viewportRect.width += viewportRect.position.x;
                viewportRect.position = new Vector2(0f, viewportRect.position.y);
                if (viewportRect.width <= 0f)
                    return;
            }
            if (viewportRect.position.y < 0f)
            {
                viewportRect.height += viewportRect.position.y;
                viewportRect.position = new Vector2(viewportRect.position.x, 0f);
                if (viewportRect.height <= 0f)
                    return;
            }

            viewportRect.y = position.height - viewportRect.y - viewportRect.height;
            GL.Viewport(EditorGUIUtility.PointsToPixels(viewportRect));


            ////////////////////////////////
            ////    Clear Background    ////
            ////////////////////////////////
            #region [ Clear Background ]
            GL.LoadIdentity();
            GL.LoadProjectionMatrix(Matrix4x4.Ortho(0f, 1f, 0f, 1f, -1f, 1f));

            material.SetInt(PID._SrcBlend, (int)BlendMode.One);
            material.SetInt(PID._DstBlend, (int)BlendMode.Zero);
            material.SetPass(0);

            GL.Begin(GL.TRIANGLE_STRIP);
            GL.Color(Styles.backgroundColor);
            GL.Vertex3(1, 0, 0);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(1, 1, 0);
            GL.Vertex3(0, 1, 0);
            GL.End();
            #endregion

            ////////////////////////////
            ////    Draw Content    ////
            ////////////////////////////
            #region [ Draw Content ]
            GL.LoadIdentity();
            float aspect = viewportRect.height / viewportRect.width;
            Vector2 visibleArea = new Vector2(10f * viewScale, 10f * aspect * viewScale);
            Matrix4x4 projectionMatrix = Matrix4x4.Ortho(-visibleArea.x, visibleArea.x, -visibleArea.y, visibleArea.y, -1f, 1f);
            GL.LoadProjectionMatrix(projectionMatrix);
            Matrix4x4 viewMatrix =
                 Matrix4x4.Translate(new Vector3(-cameraPosition.x, -cameraPosition.y, 0));
            GL.MultMatrix(viewMatrix);

            if (dataOpened)
            {
                int cellCount = 0;
                Int2[] locations;
                if (opened.processTemplate)
                {
                    material.SetInt(PID._SrcBlend, (int)BlendMode.One);
                    material.SetInt(PID._DstBlend, (int)BlendMode.Zero);
                    material.SetPass(0);
                    BattleMapAreaHolderType[] types;
                    if (opened.GetTemplateInfo(out cellCount, out locations, out types))
                    {
                        for (int i = 0; i < cellCount; i++)
                        {
                            Int2 pos = locations[i];
                            GL.Begin(GL.TRIANGLE_STRIP);
                            GL.Color(Styles.templateCellColors[(int)types[i]]);
                            GL.Vertex3(1 + pos.x, 0 + pos.y, 0);
                            GL.Vertex3(0 + pos.x, 0 + pos.y, 0);
                            GL.Vertex3(1 + pos.x, 1 + pos.y, 0);
                            GL.Vertex3(0 + pos.x, 1 + pos.y, 0);
                            GL.End();
                        }
                    }
                }
                else if (opened.processVarient)
                {
                    material.SetInt(PID._SrcBlend, (int)BlendMode.One);
                    material.SetInt(PID._DstBlend, (int)BlendMode.Zero);
                    material.SetPass(0);
                    VariantCellHolderType[] types;
                    if(opened.GetHolderInfo(out cellCount, out locations, out types))
                    {
                        for (int i = 0; i < cellCount; i++)
                        {
                            Int2 pos = locations[i];
                            GL.Begin(GL.TRIANGLE_STRIP);
                            GL.Color(Styles.variantsCellColors[(int)types[i]]);
                            GL.Vertex3(1 + pos.x, 0 + pos.y, 0);
                            GL.Vertex3(0 + pos.x, 0 + pos.y, 0);
                            GL.Vertex3(1 + pos.x, 1 + pos.y, 0);
                            GL.Vertex3(0 + pos.x, 1 + pos.y, 0);
                            GL.End();
                        }
                    }
                }
            }
            #endregion

            /////////////////////////
            ////    Draw Grid    ////
            /////////////////////////
            #region [ Draw Grid ]
            material.SetInt(PID._SrcBlend, (int)BlendMode.SrcAlpha);
            material.SetInt(PID._DstBlend, (int)BlendMode.OneMinusSrcAlpha);
            material.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Color(Styles.gridColor);
            float loc = Mathf.Floor(-11f * viewScale + cameraPosition.x);
            for (int i = 0; i < 23 * viewScale; i++, loc += 1f)
            {
                GL.Vertex3(loc, visibleArea.y + cameraPosition.y, 0);
                GL.Vertex3(loc, -visibleArea.y + cameraPosition.y, 0);
            }
            loc = Mathf.Floor(-11f * aspect * viewScale + cameraPosition.y);
            for (int i = 0; i < 23 * aspect * viewScale; i++, loc += 1f)
            {
                GL.Vertex3(visibleArea.x + cameraPosition.x, loc, 0);
                GL.Vertex3(-visibleArea.x + cameraPosition.x, loc, 0);
            }
            GL.End();
            #endregion

            //draw axis
            material.SetInt(PID._SrcBlend, (int)BlendMode.One);
            material.SetInt(PID._DstBlend, (int)BlendMode.Zero);
            material.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Color(Color.green);
            GL.Vertex3(0f, 0f, 0f);
            GL.Vertex3(0f, 1f, 0f);
            GL.Color(Color.red);
            GL.Vertex3(0f, 0f, 0f);
            GL.Vertex3(1f, 0f, 0f);
            GL.End();

            GL.PopMatrix();
            GUI.EndGroup();
        }
        #endregion
        //////////////////////////////
        ////    Paint Function    ////
        //////////////////////////////
        #region [ Paint Function ]
        void EditPaint(Vector2 canvasPosition, Action action)
        {
            if (!dataOpened) return;
            Int2 location = new Int2(Mathf.FloorToInt(canvasPosition.x), Mathf.FloorToInt(canvasPosition.y));
            if (opened.processTemplate) {
                if (action == Action.Painting)
                {
                    opened.AddBattleMapAreaHolder(location, (BattleMapAreaHolderType)currentTemplateToolType);
                }
                else if (action == Action.Erazing)
                {
                    opened.RemoveBattleMapAreaHolder(location);
                }
            }
        }

        void SwitchTool()
        {
            if (!dataOpened) return;
            if (opened.processCompleted) return;
            if(focusedWindow == this && currentAction == Action.None)
            {
                Event currentEvent = Event.current;
                switch (currentEvent.type)
                {
                    case EventType.KeyDown:
                        if (opened.processTemplate) {
                            switch (currentEvent.keyCode)
                            {
                                case KeyCode.Alpha1:                           
                                    currentTemplateToolType = TemplateToolType.Normal;
                                    GUI.changed = true;
                                    break;
                                case KeyCode.Alpha2:
                                    currentTemplateToolType = TemplateToolType.Entry;
                                    GUI.changed = true;
                                    break;
                            }
                        }
                        break;
                }
            }
        }
        #endregion
        ///////////////////////////////
        ////    Helper Function    ////
        ///////////////////////////////
        #region [ Helper Function ]
        void GuiLine(int height = 1)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, height);
            rect.height = height;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
        }
        #endregion
    }
}
