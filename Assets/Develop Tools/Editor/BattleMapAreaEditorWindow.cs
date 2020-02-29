using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.Callbacks;

namespace EIJ.BattleMap {
	/// <summary>
	/// 战斗地图模板编辑器窗口
	/// </summary>
	public class BattleMapAreaEditorWindow : EditorWindow {
		[MenuItem("Develop Tools/BattleMapArea Editor")]
		static void Init() {
			BattleMapAreaEditorWindow window = (BattleMapAreaEditorWindow)GetWindow(typeof(BattleMapAreaEditorWindow), false, "战斗地图模块编辑器");
			window.Show();
		}

		private void OnEnable() {
			minSize = new Vector2(530, 350);
			Shader GUIShader = Shader.Find("Hidden/EditorGUI/BattleMapAreaEditorShader");
			if (GUIShader == null) {
				EditorUtility.DisplayDialog("Error", "BattleMapArea Editor: GUIShader not Found!", "OK");
				Close();
			}
			GUIMaterial = new Material(GUIShader);

			Undo.undoRedoPerformed += Repaint;

			////////////////////////////
			////    Get Prop ID    /////
			////////////////////////////
			#region [ Get Prop ID ]
			PID.MainTex = Shader.PropertyToID("_MainTex");
			PID.Color = Shader.PropertyToID("_Color");
			PID.SrcBlend = Shader.PropertyToID("_SrcBlend");
			PID.DstBlend = Shader.PropertyToID("_DstBlend");
			#endregion
		}

		private void OnDisable() {
			CloseFile();
			Undo.undoRedoPerformed -= Repaint;
		}

		/////////////////////////////
		////    File Function    ////
		/////////////////////////////
		#region [ File Function ]
		static void OpenFile(BattleMapArea file) {
			if (file == null) return;
			Opened = file;
			TargetSerializedObject = new SerializedObject(file);
			if (TargetSerializedObject != null) DataOpened = true;
			ResetTools();
			//////////////////////////
			////    Properties    ////
			//////////////////////////
			#region [ Properties ]
			#endregion
			GUI.changed = true;
		}
		static void CloseFile() {
			if (Opened != null) {
				Opened.SetEditing(-1);
				Undo.ClearUndo(Opened);
			}
			Opened = null;
			TargetSerializedObject = null;
			DataOpened = false;
			ResetTools();
			//////////////////////////
			////    Properties    ////
			//////////////////////////
			#region [ Properties ]
			#endregion
			GUI.changed = true;
		}
		static BattleMapArea GetSelectedFileFromAssets() {
			string[] guids = Selection.assetGUIDs;
			if (guids.Length <= 0) {
				return null;
			}
			BattleMapArea load = null;
			foreach (string guid in guids) {
				load = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(BattleMapArea)) as BattleMapArea;
			}
			return load;
		}
		static void TryOpenSelected() {
			BattleMapArea target = GetSelectedFileFromAssets();
			if (target != null) {
				CloseFile();
				OpenFile(target);
			}
			GUI.changed = true;
		}

		/// <summary>
		/// 在编辑窗口中打开
		/// </summary>
		/// <param name="instanceID">instanceID</param>
		/// <param name="line">line</param>
		/// <returns>是否成功打开</returns>
		[OnOpenAssetAttribute(1)]
		public static bool OpenFileInWindow(int instanceID, int line) {
			Object file = EditorUtility.InstanceIDToObject(instanceID);
			if (file.GetType() == typeof(BattleMapArea)) {
				Init();
				CloseFile();
				OpenFile(file as BattleMapArea);
				return true;
			}
			else {
				return false;
			}
		}

		#endregion

		//////////////////////
		////    Styles    ////
		//////////////////////
		#region [ Styles ]
		static class Styles {
			public static Color BackgroundColor = new Color32(35, 35, 35, 255);
			public static Color GridColor = new Color32(255, 255, 255, 50);
			public static Color[] TemplateCellColors = new Color[]{
				new Color32(65, 65, 65, 255),
				new Color32(180, 160, 90, 255) };
			public static Color[] HolderCellColors = new Color[]{
				new Color32(55, 65, 55, 255),
				new Color32(65, 55, 55, 255),
				new Color32(230, 190, 90, 255)};
			public static Color[] VarientsCellColors = new Color[]{
				new Color32(0, 0, 0, 0),
				new Color32(50, 155, 155, 255),
				new Color32(170, 170, 180, 255),
				new Color32(30, 230, 30, 255),
				new Color32(200, 30, 30, 255)};

			public static GUIStyle DefaultButtonStyle = new GUIStyle(GUI.skin.button);
			public static GUIStyle CurrentToolButtonStyle = GetCurrentToolButtonStyle();
			public static GUIStyle DefaultLabelStyle = new GUIStyle(GUI.skin.label);
			public static GUIStyle CurrentProcessStyle = GetCurrentProcessStyle();

			public static GUIContent[] TypePopupString = new GUIContent[] {
				new GUIContent("普通"), new GUIContent("基地"), new GUIContent("刷怪点") };
			public static int[] TypePopupValue = new int[] { 0, 1, 2 };
			public static GUIStyle TypeLabelStyle = GetTypeLabelStyle();

			public static GUIStyle CurrentVariantsStyle = GetCurrentVariantStyle();
			public static GUIStyle DetachButtonStyle = GetDetachButtonStyle();
			public static GUIStyle CompiledVariantsStyle = GetCompiledVariantStyle();
			public static GUIStyle ViewButtonStyle = GetViewButtonStyle();
		}

		static GUIStyle GetCurrentToolButtonStyle() {
			GUIStyle style = new GUIStyle(GUI.skin.button) { };
			style.fontStyle = FontStyle.Bold;
			Color textColor = new Color32(30, 30, 250, 255);
			style.normal.textColor = textColor;
			style.hover.textColor = textColor;
			style.active.textColor = textColor;
			return style;
		}
		static GUIStyle GetCurrentProcessStyle() {
			GUIStyle style = new GUIStyle(GUI.skin.label) { fixedHeight = 20 };
			style.fontStyle = FontStyle.Bold;
			style.fontSize = 15;
			Color textColor = new Color32(150, 30, 250, 255);
			style.normal.textColor = textColor;
			return style;
		}
		static GUIStyle GetTypeLabelStyle() {
			GUIStyle style = new GUIStyle(GUI.skin.label) { fixedHeight = 20 };
			style.fontStyle = FontStyle.Bold;
			Color textColor = new Color32(0, 100, 0, 255);
			style.normal.textColor = textColor;
			return style;
		}
		static GUIStyle GetCurrentVariantStyle() {
			GUIStyle style = new GUIStyle(GUI.skin.label) { };
			style.fontStyle = FontStyle.Bold;
			Color textColor = new Color32(20, 140, 20, 255);
			style.normal.textColor = textColor;
			return style;
		}
		static GUIStyle GetDetachButtonStyle() {
			GUIStyle style = new GUIStyle(GUI.skin.button) { };
			style.fontStyle = FontStyle.Bold;
			Color textColor = new Color32(20, 140, 20, 255);
			style.normal.textColor = textColor;
			style.hover.textColor = textColor;
			style.active.textColor = textColor;
			return style;
		}
		static GUIStyle GetCompiledVariantStyle() {
			GUIStyle style = new GUIStyle(GUI.skin.label) { };
			style.fontStyle = FontStyle.Bold;
			Color textColor = new Color32(20, 20, 255, 255);
			style.normal.textColor = textColor;
			return style;
		}
		static GUIStyle GetViewButtonStyle() {
			GUIStyle style = new GUIStyle(GUI.skin.button) { };
			Color textColor = new Color32(20, 20, 255, 255);
			style.normal.textColor = textColor;
			style.hover.textColor = textColor;
			style.active.textColor = textColor;
			return style;
		}
		#endregion

		Vector2 ScrollPos = Vector2.zero;
		static readonly int DrawMapWindowHash = "EIJ.BattleMap.BattleMapAreaEditorWindow.DrawMapWindow".GetHashCode();

		///////////////////////////////
		////    Object Variants    ////
		///////////////////////////////
		#region [ Object Variants ]
		static BattleMapArea Opened = null;
		static SerializedObject TargetSerializedObject = null;
		static bool DataOpened = false;
		#endregion

		//////////////////////////////
		////    Paint Variants    ////
		//////////////////////////////
		#region [ Paint Variants ]
		enum TemplateToolType {
			Normal = 0,
			Entry = 1,
		}
		enum VariantsToolType {
			Path = 1,
			Platform = 2,
			Home = 3,
			Spawn = 4,
		}

		static TemplateToolType CurrentTemplateToolType = TemplateToolType.Normal;
		static VariantsToolType CurrentVariantsToolType = VariantsToolType.Path;
		static void ResetTools() {
			CurrentTemplateToolType = TemplateToolType.Normal;
			CurrentVariantsToolType = VariantsToolType.Path;
		}
		#endregion

		///////////////////////////////
		////    Canvas Variants    ////
		///////////////////////////////
		#region [ Canvas Variants ]
		const float CameraRange = 10f;
		Vector2 CameraPosition = Vector2.zero;
		float ViewScale = 1.0f;
		Vector2 RecordMousePosition = Vector2.zero;
		Vector2 RecordCameraPosition = Vector2.zero;
		Material GUIMaterial = null;

		///////////////////////
		////    Control    ////
		///////////////////////
		#region [ Control ]
		enum Action {
			None,
			Dragging,
			Painting,
			Erazing,
		}

		Action CurrentAction = Action.None;
		#endregion

		////////////////////////
		////    Prop ID    /////
		////////////////////////
		#region [ Prop ID ]
		static class PID {
			public static int MainTex;
			public static int Color;
			public static int SrcBlend;
			public static int DstBlend;
		}
		#endregion
		#endregion

		private void OnGUI() {
			if (DataOpened) TargetSerializedObject.Update();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical();
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("关闭")) CloseFile();
			if (GUILayout.Button("打开选中文件")) TryOpenSelected();
			EditorGUILayout.EndHorizontal();
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.ObjectField(Opened, typeof(BattleMapArea), false);
			EditorGUI.EndDisabledGroup();
			GuiLine();
			if (DataOpened) {
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("类型:", EditorStyles.boldLabel, GUILayout.Width(50));
				if (Opened.ProcessTemplate) {
					BattleMapAreaType newType = (BattleMapAreaType)EditorGUILayout.IntPopup((int)Opened.Type, Styles.TypePopupString, Styles.TypePopupValue);
					if (newType != Opened.Type) {
						Undo.RegisterCompleteObjectUndo(Opened, "Change Type");
						Opened.Type = newType;
					}
				}
				else {
					EditorGUILayout.LabelField(Styles.TypePopupString[(int)Opened.Type], Styles.TypeLabelStyle, GUILayout.Width(50));
				}
				EditorGUILayout.EndHorizontal();
			}
			DrawProgress();
			DrawTools();
			DrawVariantsMenu();
			ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos);
			DrawVariantsList();
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
			Rect mapWindowRect = EditorGUILayout.GetControlRect(new GUILayoutOption[] {
			GUILayout.Width(position.width - 180),
			GUILayout.Height(position.height) });
			SetCursor(mapWindowRect);
			DrawMapWindow(mapWindowRect);
			EditorGUILayout.EndHorizontal();
			if (DataOpened) TargetSerializedObject.ApplyModifiedProperties();
		}

		//////////////////////
		////    Cursor    ////
		//////////////////////
		#region [ Cursor ]
		void SetCursor(Rect rect) {
			if (CurrentAction == Action.Dragging) {
				EditorGUIUtility.AddCursorRect(rect, MouseCursor.Pan);
			}
			else {
				if (Opened != null) {
					if (!Opened.ProcessCompleted) {
						if (CurrentAction == Action.Painting)
							EditorGUIUtility.AddCursorRect(rect, MouseCursor.ArrowPlus);
						else if (CurrentAction == Action.Erazing)
							EditorGUIUtility.AddCursorRect(rect, MouseCursor.ArrowMinus);
					}
				}
			}
		}
		#endregion
		////////////////////////////
		////    Draw Process    ////
		////////////////////////////
		#region [ Draw Progress ]
		void DrawProgress() {
			GuiLine();
			EditorGUILayout.LabelField("[ 步骤 ]", EditorStyles.boldLabel, GUILayout.Width(100));
			GuiLine();
			if (Opened == null) {
				EditorGUILayout.LabelField("没有打开文件", GUILayout.Width(100));
			}
			else {
				EditorGUILayout.LabelField("基础", Opened.ProcessTemplate ? Styles.CurrentProcessStyle : Styles.DefaultLabelStyle, GUILayout.Width(100));
				if (!Opened.ProcessCompleted) {
					if (GUILayout.Button(Opened.ProcessTemplate ? "确认" : "重新编辑")) {
						if (Opened.ProcessTemplate) {
							Undo.RegisterCompleteObjectUndo(Opened, "Apply Template");
							string log = Opened.ApplyTemplate();
							if (log.Length > 0) {
								EditorUtility.DisplayDialog("x_x!", "确认模板失败！\n" + log, "取消");
							}
						}
						else {
							if (EditorUtility.DisplayDialog("'o'!", "如果重新编辑模板，所有的变种必须重新编译！",
								"确认", "取消")) {
								Undo.RegisterCompleteObjectUndo(Opened, "Reedit Template");
								Opened.ReeditTemplate();
								ResetTools();
								Focus();

							}
						}
					}
				}
				EditorGUILayout.LabelField("变种", Opened.ProcessVarients ? Styles.CurrentProcessStyle : Styles.DefaultLabelStyle, GUILayout.Width(100));
				if (Opened.ProcessVarients) {
					if (GUILayout.Button("完成模块")) {
						Undo.RegisterCompleteObjectUndo(Opened, "Complete Template");
						string log = Opened.CompleteTemplate();
						if (log.Length > 0) {
							EditorUtility.DisplayDialog("x_x!", "完成模块失败！\n" + log, "取消");
						}
					}
				}
				EditorGUILayout.LabelField("已完成", Opened.ProcessCompleted ? Styles.CurrentProcessStyle : Styles.DefaultLabelStyle, GUILayout.Width(100));
				if (Opened.ProcessCompleted) {
					if (GUILayout.Button("返回编辑")) {
						if(EditorUtility.DisplayDialog("'o'!", "重新编辑可能会影响使用这个模块的战斗地图模板！", "确定", "取消")){
							Undo.RegisterCompleteObjectUndo(Opened, "Return to Editalbe");
							Opened.ReturnToEditable();
						}
					}
				}
			}
			GuiLine();
		}
		#endregion
		//////////////////////////
		////    Draw Tools    ////
		//////////////////////////
		#region [ Draw Tools ]
		void DrawTools() {
			GuiLine();
			EditorGUILayout.LabelField("[ 工具 ]", EditorStyles.boldLabel, GUILayout.Width(100));
			GuiLine();
			SwitchTool();
			if (Opened != null) {
				if (Opened.ProcessTemplate) {
					//////////////////////////////
					////    Template Tools    ////
					//////////////////////////////
					#region [ Template Tools ]
					EditorGUILayout.BeginHorizontal();
					if (GUILayout.Button("普通", CurrentTemplateToolType == TemplateToolType.Normal ?
						Styles.CurrentToolButtonStyle : Styles.DefaultButtonStyle))
						CurrentTemplateToolType = TemplateToolType.Normal;
					if (GUILayout.Button("入口", CurrentTemplateToolType == TemplateToolType.Entry ?
						Styles.CurrentToolButtonStyle : Styles.DefaultButtonStyle))
						CurrentTemplateToolType = TemplateToolType.Entry;
					EditorGUILayout.EndHorizontal();
					GUILayout.Space(7);
					if (GUILayout.Button("清空")) {
						Undo.RegisterCompleteObjectUndo(Opened, "Clear Template");
						Opened.ClearAllHolders();
					}
					GUILayout.Space(3);
					#endregion
				}
				if (Opened.ProcessVarients) {
					//////////////////////////////
					////    Varients Tools    ////
					//////////////////////////////
					#region [ Varients Tools ]
					EditorGUI.BeginDisabledGroup(!IsVariantEditalbe());
					EditorGUILayout.BeginHorizontal();
					if (GUILayout.Button("路径", CurrentVariantsToolType == VariantsToolType.Path ?
						Styles.CurrentToolButtonStyle : Styles.DefaultButtonStyle))
						CurrentVariantsToolType = VariantsToolType.Path;
					if (GUILayout.Button("平台", CurrentVariantsToolType == VariantsToolType.Platform ?
						Styles.CurrentToolButtonStyle : Styles.DefaultButtonStyle))
						CurrentVariantsToolType = VariantsToolType.Platform;
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal();
					EditorGUI.BeginDisabledGroup(Opened.Type != BattleMapAreaType.Home);
					if (GUILayout.Button("基地", CurrentVariantsToolType == VariantsToolType.Home ?
						Styles.CurrentToolButtonStyle : Styles.DefaultButtonStyle))
						CurrentVariantsToolType = VariantsToolType.Home;
					EditorGUI.EndDisabledGroup();
					EditorGUI.BeginDisabledGroup(Opened.Type != BattleMapAreaType.Spawn);
					if (GUILayout.Button("刷怪点", CurrentVariantsToolType == VariantsToolType.Spawn ?
						Styles.CurrentToolButtonStyle : Styles.DefaultButtonStyle))
						CurrentVariantsToolType = VariantsToolType.Spawn;
					EditorGUI.EndDisabledGroup();
					EditorGUILayout.EndHorizontal();
					GUILayout.Space(7);
					if (GUILayout.Button("清空")) {
						Undo.RegisterCompleteObjectUndo(Opened, "Clear Variant");
						Opened.ClearCurrentEdit();
					}
					EditorGUI.EndDisabledGroup();
					GUILayout.Space(3);
					#endregion
				}
			}
			else {
				EditorGUILayout.LabelField("空", GUILayout.Width(100));
			}
			GuiLine();
		}
		#endregion
		/////////////////////////////////
		////    Draw Variants Menu   ////
		/////////////////////////////////
		#region [ Draw Variants Menu]
		void DrawVariantsMenu() {
			GuiLine();
			EditorGUILayout.LabelField("[ 变种 ]", EditorStyles.boldLabel, GUILayout.Width(100));
			if (DataOpened) {
				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("清空", GUILayout.Width(45))) {
					if (EditorUtility.DisplayDialog("'o'!", "将移除所有变种！", "确定", "取消")) {
						Undo.RegisterCompleteObjectUndo(Opened, "Clear All Variants");
						Opened.ClearAllVariants();
					}
				}
				EditorGUI.BeginDisabledGroup(!Opened.ProcessVarients);
				if (GUILayout.Button("添加")) {
					Undo.RegisterCompleteObjectUndo(Opened, "AddNewVariants");
					Opened.AddNewVariants();
				}
				EditorGUI.EndDisabledGroup();
				EditorGUILayout.EndHorizontal();
			}
			GuiLine();
		}
		#endregion
		/////////////////////////////////
		////    Draw Variants List   ////
		/////////////////////////////////
		#region [ Draw Variants List]
		void DrawVariantsList() {
			if (!DataOpened) return;
			int count = Opened.Variants.Count;
			EditorGUI.BeginDisabledGroup(!Opened.ProcessVarients);
			if (count == 0)
				EditorGUILayout.LabelField("没有变种", GUILayout.Width(100));
			for (int i = 0; i < count; i++) {
				GuiLine();
				EditorGUILayout.BeginHorizontal();
				string mark = Opened.EditingVariant == i ? "●" : string.Empty;
				GUIStyle indexLabelStyle = Opened.EditingVariant == i ? Styles.CurrentVariantsStyle : EditorStyles.boldLabel;
				if (Opened.Variants[i].Compiled) indexLabelStyle = Styles.CompiledVariantsStyle;
				EditorGUILayout.LabelField(mark + "[ " + i.ToString() + " ]", indexLabelStyle, GUILayout.Width(50));
				if (GUILayout.Button(new GUIContent("X", "删除"), GUILayout.Width(20))) {
					Undo.RegisterCompleteObjectUndo(Opened, "Remove Variant");
					Opened.RemoveVariantAt(i);
					break;
				}
				if (GUILayout.Button(new GUIContent("C", "复制"), GUILayout.Width(20))) {
					Undo.RegisterCompleteObjectUndo(Opened, "Copy Variant");
					Opened.CreateVariantCopyFrom(i);
				}
				if (!Opened.Variants[i].Compiled) {
					if (Opened.EditingVariant != i) {
						if (GUILayout.Button("编辑")) {
							Undo.RegisterCompleteObjectUndo(Opened, "Edit Variant");
							Opened.SetEditing(i);
						}
					}
					else {
						if (GUILayout.Button("退出", Styles.DetachButtonStyle)) {
							Undo.RegisterCompleteObjectUndo(Opened, "Detach Variant");
							Opened.SetEditing(-1);
						}
					}
					EditorGUI.EndDisabledGroup();
				}
				else {
					EditorGUI.EndDisabledGroup();
					if (Opened.EditingVariant != i) {
						if (GUILayout.Button("查看", Styles.ViewButtonStyle)) {
							Undo.RegisterCompleteObjectUndo(Opened, "Detach Variant");
							Opened.SetEditing(i);
						}
					}
					else {
						if (GUILayout.Button("隐藏", Styles.ViewButtonStyle)) {
							Undo.RegisterCompleteObjectUndo(Opened, "Detach Variant");
							Opened.SetEditing(-1);
						}
					}
				}
				EditorGUILayout.EndHorizontal();
				EditorGUI.BeginDisabledGroup(!Opened.ProcessVarients);
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("权重", GUILayout.Width(30));
				EditorGUI.BeginChangeCheck();
				float newAppearRate = EditorGUILayout.FloatField(Opened.Variants[i].AppearRate, GUILayout.Width(27));
				GUILayout.Space(8);
				if (EditorGUI.EndChangeCheck()) {
					Undo.RegisterCompleteObjectUndo(Opened, "Change Appear Rate");
					Opened.Variants[i].AppearRate = newAppearRate;
				}
				if (!Opened.Variants[i].Compiled) {
					if (GUILayout.Button("生成路径")) {
						Undo.RegisterCompleteObjectUndo(Opened, "Compile Variant");
						string log = Opened.CompileVariant(i);
						if (log.Length > 0) {
							EditorUtility.DisplayDialog("x_x!", "编译失败：\n" + log, "OK");
						}
					}
				}
				else {
					if (GUILayout.Button("修改")) {
						Undo.RegisterCompleteObjectUndo(Opened, "Unlock Compiled Variant");
						Opened.UnlockVariant(i);
						Opened.SetEditing(i);
					}
				}
				EditorGUILayout.EndHorizontal();
				GuiLine();
			}
			EditorGUI.EndDisabledGroup();
		}
		#endregion
		///////////////////////////////
		////    Draw Map Window    ////
		///////////////////////////////
		#region [ Draw Map Window ]
		void DrawMapWindow(Rect rect) {
			Event currentEvent = Event.current;
			int controlID = GUIUtility.GetControlID(DrawMapWindowHash, FocusType.Passive, rect);

			switch (currentEvent.GetTypeForControl(controlID)) {
			///////////////////////
			////    Control    ////
			///////////////////////
			#region [ Control ]
			case EventType.MouseDown:
				if (rect.Contains(currentEvent.mousePosition)) {
					if (currentEvent.button == 2 && CurrentAction == Action.None) {
						CurrentAction = Action.Dragging;
						RecordMousePosition = currentEvent.mousePosition;
						RecordCameraPosition = CameraPosition;
						GUIUtility.hotControl = controlID;
						currentEvent.Use();
					}
					if ((currentEvent.button == 0 || currentEvent.button == 1) && CurrentAction == Action.None) {
						/////////////////////////////
						////    Paint Control    ////
						/////////////////////////////
						#region [ Paint Control ]
						if (Opened != null) {
							if (Opened.ProcessTemplate || Opened.ProcessVarients) {
								Vector2 mousePos = currentEvent.mousePosition;
								Vector2 screenOffset = new Vector2(
									mousePos.x - rect.x - rect.width * 0.5f,
									mousePos.y - rect.y - rect.height * 0.5f);
								Vector2 canvasPos = screenOffset / rect.width * 20f * ViewScale * new Vector2(1.0f, -1.0f) + CameraPosition;
								GUI.changed = true;
								CurrentAction = currentEvent.button == 0 ? Action.Painting : Action.Erazing;
								GUIUtility.hotControl = controlID;
								Undo.RegisterCompleteObjectUndo(Opened, "Edit Map");
								EditPaint(canvasPos, CurrentAction);
								currentEvent.Use();
							}
						}
						#endregion
					}
				}
				break;
			case EventType.MouseDrag:
				if (GUIUtility.hotControl == controlID) {
					if (CurrentAction == Action.Dragging) {
						GUI.changed = true;
						Vector2 mouseOffset = currentEvent.mousePosition - RecordMousePosition;
						mouseOffset.y = -mouseOffset.y;
						Vector2 newCamPos = RecordCameraPosition - mouseOffset / rect.width * 20f * ViewScale;
						CameraPosition.x = Mathf.Clamp(newCamPos.x, -CameraRange, CameraRange);
						CameraPosition.y = Mathf.Clamp(newCamPos.y, -CameraRange, CameraRange);
						currentEvent.Use();
					}
					if (CurrentAction == Action.Painting || CurrentAction == Action.Erazing) {
						GUI.changed = true;
						Vector2 mousePos = currentEvent.mousePosition;
						Vector2 screenOffset = new Vector2(
							mousePos.x - rect.x - rect.width * 0.5f,
							mousePos.y - rect.y - rect.height * 0.5f);
						Vector2 canvasPos = screenOffset / rect.width * 20f * ViewScale * new Vector2(1.0f, -1.0f) + CameraPosition;
						EditPaint(canvasPos, CurrentAction);
						currentEvent.Use();
					}
				}
				break;
			case EventType.MouseUp:
				if (GUIUtility.hotControl == controlID) {
					CurrentAction = Action.None;
					GUIUtility.hotControl = 0;
					currentEvent.Use();
				}
				break;
			case EventType.ScrollWheel:
				if (rect.Contains(currentEvent.mousePosition)) {
					if (focusedWindow == this && CurrentAction == Action.None) {
						GUI.changed = true;
						float scaleChange = 1.0f;
						if (currentEvent.delta.y > 0) //wheel up
							scaleChange = 1.1f;
						else if (currentEvent.delta.y < 0) //wheel down
							scaleChange = 1f / 1.1f;
						float newViewScale = Mathf.Clamp(ViewScale * scaleChange, 0.5f, 2.0f);
						scaleChange = newViewScale / ViewScale;
						Vector2 mousePos = currentEvent.mousePosition;
						Vector2 screenOffset = new Vector2(
							mousePos.x - rect.x - rect.width * 0.5f,
							mousePos.y - rect.y - rect.height * 0.5f);
						Vector2 newCamPos = CameraPosition -
							(screenOffset / rect.width * 20f * ViewScale * (scaleChange - 1.0f)) * new Vector2(1.0f, -1.0f);
						CameraPosition.x = Mathf.Clamp(newCamPos.x, -CameraRange, CameraRange);
						CameraPosition.y = Mathf.Clamp(newCamPos.y, -CameraRange, CameraRange);
						ViewScale = newViewScale;
						currentEvent.Use();
					}
				}
				break;

			#endregion

			case EventType.Repaint:
				RenderCanvas(rect);
				break;
			}
		}

		void RenderCanvas(Rect rect) {
			GUI.BeginGroup(rect);
			GL.PushMatrix();

			Rect viewportRect = rect;
			if (viewportRect.position.x < 0f) {
				viewportRect.width += viewportRect.position.x;
				viewportRect.position = new Vector2(0f, viewportRect.position.y);
				if (viewportRect.width <= 0f)
					return;
			}
			if (viewportRect.position.y < 0f) {
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

			GUIMaterial.SetInt(PID.SrcBlend, (int)BlendMode.One);
			GUIMaterial.SetInt(PID.DstBlend, (int)BlendMode.Zero);
			GUIMaterial.SetPass(0);

			GL.Begin(GL.TRIANGLE_STRIP);
			GL.Color(Styles.BackgroundColor);
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
			Vector2 visibleArea = new Vector2(10f * ViewScale, 10f * aspect * ViewScale);
			Matrix4x4 projectionMatrix = Matrix4x4.Ortho(-visibleArea.x, visibleArea.x, -visibleArea.y, visibleArea.y, -1f, 1f);
			GL.LoadProjectionMatrix(projectionMatrix);
			Matrix4x4 viewMatrix =
				 Matrix4x4.Translate(new Vector3(-CameraPosition.x, -CameraPosition.y, 0));
			GL.MultMatrix(viewMatrix);

			if (DataOpened) {
				int cellCount = 0;
				Int2[] locations;
				if (Opened.ProcessTemplate) {
					GUIMaterial.SetInt(PID.SrcBlend, (int)BlendMode.One);
					GUIMaterial.SetInt(PID.DstBlend, (int)BlendMode.Zero);
					GUIMaterial.SetPass(0);
					BattleMapAreaHolderType[] types;
					if (Opened.GetTemplateInfo(out cellCount, out locations, out types)) {
						for (int i = 0; i < cellCount; i++) {
							Int2 pos = locations[i];
							GL.Begin(GL.TRIANGLE_STRIP);
							GL.Color(Styles.TemplateCellColors[(int)types[i]]);
							GL.Vertex3(1 + pos.x, 0 + pos.y, 0);
							GL.Vertex3(0 + pos.x, 0 + pos.y, 0);
							GL.Vertex3(1 + pos.x, 1 + pos.y, 0);
							GL.Vertex3(0 + pos.x, 1 + pos.y, 0);
							GL.End();
						}
					}
				}
				else if (Opened.ProcessVarients || Opened.ProcessCompleted) {
					GUIMaterial.SetInt(PID.SrcBlend, (int)BlendMode.One);
					GUIMaterial.SetInt(PID.DstBlend, (int)BlendMode.Zero);
					GUIMaterial.SetPass(0);
					VariantCellHolderType[] types;
					if (Opened.GetHolderInfo(out cellCount, out locations, out types)) {
						for (int i = 0; i < cellCount; i++) {
							Int2 pos = locations[i];
							GL.Begin(GL.TRIANGLE_STRIP);
							GL.Color(Styles.HolderCellColors[(int)types[i]]);
							GL.Vertex3(1 + pos.x, 0 + pos.y, 0);
							GL.Vertex3(0 + pos.x, 0 + pos.y, 0);
							GL.Vertex3(1 + pos.x, 1 + pos.y, 0);
							GL.Vertex3(0 + pos.x, 1 + pos.y, 0);
							GL.End();
						}
					}
					BattleMapCellType[] cellTypes;
					if (Opened.GetCurrentVariantInfo(out cellCount, out locations, out cellTypes)) {
						for (int i = 0; i < cellCount; i++) {
							Int2 pos = locations[i];
							GL.Begin(GL.TRIANGLE_STRIP);
							GL.Color(Styles.VarientsCellColors[(int)cellTypes[i]]);
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
			GUIMaterial.SetInt(PID.SrcBlend, (int)BlendMode.SrcAlpha);
			GUIMaterial.SetInt(PID.DstBlend, (int)BlendMode.OneMinusSrcAlpha);
			GUIMaterial.SetPass(0);
			GL.Begin(GL.LINES);
			GL.Color(Styles.GridColor);
			float loc = Mathf.Floor(-11f * ViewScale + CameraPosition.x);
			for (int i = 0; i < 23 * ViewScale; i++, loc += 1f) {
				GL.Vertex3(loc, visibleArea.y + CameraPosition.y, 0);
				GL.Vertex3(loc, -visibleArea.y + CameraPosition.y, 0);
			}
			loc = Mathf.Floor(-11f * aspect * ViewScale + CameraPosition.y);
			for (int i = 0; i < 23 * aspect * ViewScale; i++, loc += 1f) {
				GL.Vertex3(visibleArea.x + CameraPosition.x, loc, 0);
				GL.Vertex3(-visibleArea.x + CameraPosition.x, loc, 0);
			}
			GL.End();
			#endregion

			//draw axis
			GUIMaterial.SetInt(PID.SrcBlend, (int)BlendMode.One);
			GUIMaterial.SetInt(PID.DstBlend, (int)BlendMode.Zero);
			GUIMaterial.SetPass(0);
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
		/////////////////////////////
		////    Tool Function    ////
		/////////////////////////////
		#region [ Tool Function ]
		void EditPaint(Vector2 canvasPosition, Action action) {
			if (!DataOpened) return;
			Int2 location = new Int2(Mathf.FloorToInt(canvasPosition.x), Mathf.FloorToInt(canvasPosition.y));
			if (Opened.ProcessTemplate) {
				if (action == Action.Painting) {
					Opened.AddBattleMapAreaHolder(location, (BattleMapAreaHolderType)CurrentTemplateToolType);
				}
				else if (action == Action.Erazing) {
					Opened.RemoveBattleMapAreaHolder(location);
				}
			}
			else if (Opened.ProcessVarients) {
				if (action == Action.Painting) {
					Opened.AddVariantCell(location, (BattleMapCellType)CurrentVariantsToolType);
				}
				else if (action == Action.Erazing) {
					Opened.RemoveVarientCell(location);
				}
			}
		}

		void SwitchTool() {
			if (!DataOpened) return;
			if (Opened.ProcessCompleted) return;
			if (focusedWindow == this && CurrentAction == Action.None) {
				Event currentEvent = Event.current;
				switch (currentEvent.type) {
				case EventType.KeyDown:
					if (Opened.ProcessTemplate) {
						switch (currentEvent.keyCode) {
						case KeyCode.Alpha1:
							CurrentTemplateToolType = TemplateToolType.Normal;
							currentEvent.Use();
							GUI.changed = true;
							break;
						case KeyCode.Alpha2:
							CurrentTemplateToolType = TemplateToolType.Entry;
							currentEvent.Use();
							GUI.changed = true;
							break;
						}
					}
					else if (Opened.ProcessVarients && IsVariantEditalbe()) {
						switch (currentEvent.keyCode) {
						case KeyCode.Alpha1:
							CurrentVariantsToolType = VariantsToolType.Path;
							currentEvent.Use();
							GUI.changed = true;
							break;
						case KeyCode.Alpha2:
							CurrentVariantsToolType = VariantsToolType.Platform;
							currentEvent.Use();
							GUI.changed = true;
							break;
						case KeyCode.Alpha3:
							if (Opened.Type == BattleMapAreaType.Home) {
								CurrentVariantsToolType = VariantsToolType.Home;
								currentEvent.Use();
								GUI.changed = true;
							}
							break;
						case KeyCode.Alpha4:
							if (Opened.Type == BattleMapAreaType.Spawn) {
								CurrentVariantsToolType = VariantsToolType.Spawn;
								currentEvent.Use();
								GUI.changed = true;
							}
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
		void GuiLine(int height = 1) {
			Rect rect = EditorGUILayout.GetControlRect(false, height);
			rect.height = height;
			EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
		}

		bool IsVariantEditalbe() {
			if (Opened == null) return false;
			if (!Opened.ProcessVarients) return false;
			int editingIndex = Opened.EditingVariant;
			if (editingIndex < 0) return false;
			return !Opened.Variants[editingIndex].Compiled;
		}
		#endregion
	}
}