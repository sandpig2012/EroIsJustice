using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.Callbacks;

namespace EIJ.BattleMap {
	public class BattleMapTemplateEditorWindow : EditorWindow {
		[MenuItem("Develop Tools/Battle Map/Template Editor", priority = 2)]
		static void Init() {
			BattleMapTemplateEditorWindow window = (BattleMapTemplateEditorWindow)GetWindow(typeof(BattleMapTemplateEditorWindow), false, "战斗地图模板编辑器");
			window.Show();
		}

		private void OnEnable() {
			minSize = new Vector2(530, 350);
			Shader GUIShader = Shader.Find("Hidden/EditorGUI/BattleMapEditorShader");
			if (GUIShader == null) {
				EditorUtility.DisplayDialog("Error", "BattleMapArea Editor: GUIShader not Found!", "OK");
				Close();
			}
			GUIMaterial = new Material(GUIShader);

			Undo.undoRedoPerformed += Repaint;
			Undo.undoRedoPerformed += CancleSelecting;

			////////////////////////////
			////    Get Prop ID    ////
			//////////////////////////
			#region [ Get Prop ID ]
			PID.MainTex = Shader.PropertyToID("_MainTex");
			PID.Color = Shader.PropertyToID("_Color");
			PID.SrcBlend = Shader.PropertyToID("_SrcBlend");
			PID.DstBlend = Shader.PropertyToID("_DstBlend");
			PID.ColorIn = Shader.PropertyToID("_ColorIn");
			PID.ColorOut = Shader.PropertyToID("_ColorOut");
			PID.Bounds = Shader.PropertyToID("_Bounds");
			#endregion
		}

		private void OnDisable() {
			CloseFile();
			Undo.undoRedoPerformed -= Repaint;
			Undo.undoRedoPerformed -= CancleSelecting;
		}

		private void OnGUI() {
			if (FileOpened) {
				if (Opened == null) {
					CloseFile();
				}
				else {
					TargetSerializedObject.Update();
				}
			}
			if (GUIMaterial == null) {
				Shader GUIShader = Shader.Find("Hidden/EditorGUI/BattleMapEditorShader");
				if (GUIShader == null) {
					EditorUtility.DisplayDialog("Error", "BattleMapArea Editor: GUIShader not Found!", "OK");
					Close();
				}
				GUIMaterial = new Material(GUIShader);
			}
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical();
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("关闭")) CloseFile();
			if (GUILayout.Button("打开选中文件")) TryOpenSelected();
			EditorGUILayout.EndHorizontal();
			EditorGUI.BeginDisabledGroup(true);
			GuiLine();
			EditorGUILayout.ObjectField(Opened, typeof(BattleMapTemplate), false);
			EditorGUI.EndDisabledGroup();
			if (FileOpened) {
				ChangeCheck();
				DrawSizeSetting();
				DrawAdvanced();
				DrawHotKeyList();
			}
			EditorGUILayout.EndVertical();
			KeyboardListener();
			Rect mapWindowRect = EditorGUILayout.GetControlRect(new GUILayoutOption[] {
				GUILayout.Width(position.width - 180),
				GUILayout.Height(position.height) });
			DrawMapWindow(mapWindowRect);
			if (!FileOpened) {
				DrawNoFileOpenText(mapWindowRect);
			}
			EditorGUILayout.EndHorizontal();
			if (FileOpened) {
				TargetSerializedObject.ApplyModifiedProperties();
			}
		}

		Vector2 ScrollPos = Vector2.zero;
		static readonly int DrawMapWindowHash = "EIJ.BattleMap.BattleMapTemplateEditorWindow.DrawMapWindow".GetHashCode();
		static bool ReimportAlert = true;

		//------------------------- Varients & Functions -------------------------//
		//////////////////////
		////    Styles    ///
		////////////////////
		#region [ Styles ]
		static class Styles {
			public static Color BackGroundColor = new Color32(25, 25, 25, 255);
			public static Color EditableAreaColor = new Color32(35, 35, 35, 255);
			public static Color GridColor = new Color32(255, 255, 255, 50);
			public static Color MaxSizeFrameColor = new Color32(0, 150, 150, 255);
			public static Color[] RangeColors = new Color[]{
				new Color32(255, 140, 0, 150),
				new Color32(255, 140, 0, 0),
				new Color32(230, 180, 0, 255)};
			public static Color[] AreaCellColors = new Color[] {
				new Color32(230, 230, 230, 100), //normal
				new Color32(220, 40, 0, 100),
				new Color32(240, 230, 110, 255), //entry
				new Color32(230, 130, 0, 255),
				new Color32(230, 230, 150, 170), //selecting
				new Color32(220, 150, 0, 170),
				new Color(0.2f, 0.95f, 0.5f, 1.0f), //connected entry
			};
			public static Color[] AreaOutlineColors = new Color[] {
				new Color32(50, 255, 255, 200),
				new Color32(230, 0, 150, 200),
				new Color32(230, 230, 100, 230), //selecting
				new Color32(230, 150, 150, 230),
				new Color(1.0f, 1.0f, 1.0f, 0.0f), //white clear
			};
			public static Color OverlapColor = new Color32(200, 0, 0, 150);
			public static Color[] DragInColors = new Color[] {
				new Color32(20, 200, 20, 150),
				new Color32(140, 200, 20, 150),
			};

			public static Texture2D[] AreaIconsTextures = {
				AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath("8d2e4a23b86a45d4281a1aa754438021")), //unknown
				AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath("08bfca6942d96f846b05e7eddf36cc5b")), //home
				AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath("40974586677b74847a9228bb67b25c76")), //spawn
			};
			public static Color[] AreaIconColors = new Color[] {
				new Color32(230, 0, 150, 130), //out part
				new Color32(250, 120, 160, 220), //unknown
				new Color32(100, 250, 250, 200), //home
				new Color32(250, 170, 250, 200), //Spawn
			};

			public static GUIStyle NoFileOpenedTextStyle = GetNoFileOpenedTextStyle();
		}
		///////////////////////////////
		////    Style Generator    ///
		/////////////////////////////
		#region [ Style Generator ]
		static GUIStyle GetNoFileOpenedTextStyle() {
			GUIStyle style = new GUIStyle(GUI.skin.label) { fixedHeight = 54 };
			style.fontStyle = FontStyle.Bold;
			style.fontSize = 32;
			Color textColor = new Color32(100, 100, 100, 140);
			style.normal.textColor = textColor;
			return style;
		}
		#endregion
		#endregion
		/////////////////////////////
		////    File Function    ///
		///////////////////////////
		#region [ File Function ]
		static void OpenFile(BattleMapTemplate file) {
			if (file == null) return;
			Opened = file;
			if (file.CheckAreaChanged()) {
				file.CheckUpdateAndReimport();
			}
			TargetSerializedObject = new SerializedObject(file);
			if (TargetSerializedObject != null) FileOpened = true;
			ResetTools();
			EditorUtility.SetDirty(Opened);
			GUI.changed = true;
		}
		static void CloseFile() {
			if (Opened != null) {
				Opened.CancleSelecting();
				Undo.ClearUndo(Opened);
				EditorUtility.SetDirty(Opened);
			}
			Opened = null;
			TargetSerializedObject = null;
			DragInArea = null;
			DragInTarget = Int2.Zero;
			FileOpened = false;
			ResetTools();
			GUI.changed = true;
		}
		static BattleMapTemplate GetSelectedFileFromAssets() {
			string[] guids = Selection.assetGUIDs;
			if (guids.Length <= 0) {
				return null;
			}
			BattleMapTemplate load = null;
			foreach (string guid in guids) {
				load = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(BattleMapTemplate)) as BattleMapTemplate;
			}
			return load;
		}
		static void TryOpenSelected() {
			BattleMapTemplate target = GetSelectedFileFromAssets();
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
			if (file.GetType() == typeof(BattleMapTemplate)) {
				Init();
				CloseFile();
				OpenFile(file as BattleMapTemplate);
				return true;
			}
			else {
				return false;
			}
		}
		#endregion
		///////////////////////////////
		////    Object Variants    ///
		/////////////////////////////
		#region [ Object Variants ]
		static BattleMapTemplate Opened = null;
		static SerializedObject TargetSerializedObject = null;
		static bool FileOpened = false;
		#endregion
		///////////////////////////////
		////    Canvas Variants    ///
		/////////////////////////////
		#region [ Canvas Variants ]
		static Vector2 MaxEditableSize = new Vector2(100f, 100f);
		static Vector4 CameraRange = new Vector4(0f, 0f, MaxEditableSize.x, MaxEditableSize.y);
		const float MinScale = 0.5f;
		const float MaxScale = 6.0f;
		Vector2 CameraPosition = MaxEditableSize * 0.5f;
		float ViewScale = MaxScale;
		Vector2 RecordMousePosition = Vector2.zero;
		Vector2 RecordCameraPosition = Vector2.zero;
		Material GUIMaterial = null;
		bool ShowAreaTypeIcon = true;

		//Drag In
		static BattleMapArea DragInArea = null;
		static Int2 DragInTarget = Int2.Zero;

		//circle point position
		const int CircleSubdivision = 32;
		static Vector2[] CirclePoints = GetCirclePoints(CircleSubdivision);
		static Vector2[] GetCirclePoints(int circleSubdivision) {
			Vector2[] points = new Vector2[circleSubdivision];
			float angle = Mathf.PI * 2f / circleSubdivision;
			for (int i = 0; i < circleSubdivision; i++) {
				points[i] = new Vector2(Mathf.Sin(angle * i), Mathf.Cos(angle * i));
			}
			return points;
		}
		#endregion
		///////////////////////
		////    Prop ID    ///
		/////////////////////
		#region [ Prop ID ]
		static class PID {
			public static int MainTex;
			public static int Color;
			public static int SrcBlend;
			public static int DstBlend;
			public static int ColorIn;
			public static int ColorOut;
			public static int Bounds;
		}
		#endregion
		/////////////////////////////
		////    Tool Variants    ///
		///////////////////////////
		#region [ Tool Variants ]
		static void ResetTools() {
			//reset tools
		}
		#endregion
		///////////////////////
		////    Control    ///
		/////////////////////
		#region [ Control ]
		enum Action {
			None,
			Panning,
			Selecting,
			Dragging
		}
		Action CurrentAction = Action.None;
		bool SuccessSelectOnMouseDown = false;
		Int2 RecordAreaCenterPosition = Int2.Zero;
		bool MovingAreaUndoRecorded = false;
		#endregion
		#region [ Change Check ]
		void ChangeCheck() {
			if (Opened.CheckAreaChanged()) {
				if (ReimportAlert) {
					if (!EditorUtility.DisplayDialog("'o'!", "检测到已导入模板的改动，将执行重新导入。会丢失所有的操作历史。", "OK", "不再提醒")) {
						ReimportAlert = false;
					}
				}
				Undo.ClearUndo(Opened);
				Opened.CheckUpdateAndReimport();
			}
		}
		#endregion
		//------------------------------------------------------------------------//

		//------------------------------- Draw GUI -------------------------------//
		/////////////////////////////////
		////    Draw Size Setting    ///
		///////////////////////////////
		#region [ Draw Size Setting ]
		void DrawSizeSetting() {
			EditorGUILayout.LabelField("模板尺寸:", EditorStyles.boldLabel, GUILayout.Width(100));
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("横", GUILayout.Width(15));
			int newWidth = EditorGUILayout.IntField(Opened.TemplateSize.x, GUILayout.Width(30));
			GUILayout.Space(10);
			EditorGUILayout.LabelField("纵", GUILayout.Width(15));
			int newHeight = EditorGUILayout.IntField(Opened.TemplateSize.y, GUILayout.Width(30));
			if (newWidth < 10) {
				newWidth = 10;
			}
			else if (newWidth > 100) {
				newWidth = 100;
			}
			if (newHeight < 10) {
				newHeight = 10;
			}
			else if (newHeight > 100) {
				newHeight = 100;
			}
			if (newWidth != Opened.TemplateSize.x || newHeight != Opened.TemplateSize.y) {
				Undo.RegisterCompleteObjectUndo(Opened, "Change Size");
				Opened.SetTemplateSize(new Int2(newWidth, newHeight));
			}
			EditorGUILayout.EndHorizontal();
		}
		#endregion
		////////////////////////////////
		////    Draw Hotkey List    ///
		//////////////////////////////
		#region [ Draw Hotkey List ]
		void DrawHotKeyList() {
			GuiLine();
			EditorGUILayout.LabelField("快捷键说明", GUILayout.Width(100));
			GuiLine();
			ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos);
			EditorGUILayout.LabelField("当选中模块时：", GUILayout.Width(100));
			EditorGUILayout.LabelField("[R] - 删除", GUILayout.Width(100));
			GUILayout.Space(5);
			EditorGUILayout.LabelField("[E] - 顺时针旋转", GUILayout.Width(100));
			EditorGUILayout.LabelField("[Q] - 逆时针旋转", GUILayout.Width(100));
			GUILayout.Space(5);
			EditorGUILayout.LabelField("[W] - 上移", GUILayout.Width(100));
			EditorGUILayout.LabelField("[S] - 下移", GUILayout.Width(100));
			EditorGUILayout.LabelField("[A] - 左移", GUILayout.Width(100));
			EditorGUILayout.LabelField("[D] - 右移", GUILayout.Width(100));
			EditorGUILayout.EndScrollView();
			GuiLine();
		}
		#endregion
		/////////////////////////////
		////    Draw Advanced    ///
		///////////////////////////
		#region [ Draw Advanced ]
		void DrawAdvanced() {
			GuiLine();
			GUILayout.BeginHorizontal();
			ShowAreaTypeIcon = EditorGUILayout.Toggle(ShowAreaTypeIcon, GUILayout.Width(20));
			EditorGUILayout.LabelField("显示区域类型图标", GUILayout.Width(100));
			GUILayout.EndHorizontal();
			GuiLine();
		}
		#endregion
		///////////////////////////////
		////    Draw Map Window    ///
		/////////////////////////////
		#region [ Draw Map Window ]
		void DrawMapWindow(Rect rect) {
			Event currentEvent = Event.current;
			int controlID = GUIUtility.GetControlID(DrawMapWindowHash, FocusType.Passive, rect);
			switch (currentEvent.GetTypeForControl(controlID)) {
			///////////////////////////////
			////    Drag In Control    ///
			/////////////////////////////
			#region [ Drag In Control ]
			case EventType.DragUpdated:
				if (FileOpened) {
					if (rect.Contains(currentEvent.mousePosition)) {
						Object[] objects = DragAndDrop.objectReferences;
						if (objects.Length > 1) {
							return;
						}
						if (objects[0].GetType() != typeof(BattleMapArea)) {
							return;
						}
						GUI.changed = true;
						Vector2 mousePos = currentEvent.mousePosition;
						Vector2 screenOffset = new Vector2(
							mousePos.x - rect.x - rect.width * 0.5f,
							mousePos.y - rect.y - rect.height * 0.5f);
						Vector2 canvasPos = screenOffset / rect.width * 20f * ViewScale * new Vector2(1.0f, -1.0f) + CameraPosition;
						Int2 targetLocation = new Int2(Mathf.FloorToInt(canvasPos.x), Mathf.FloorToInt(canvasPos.y));
						Int2 maxEditableSizeInt = new Int2(Mathf.FloorToInt(MaxEditableSize.x), Mathf.FloorToInt(MaxEditableSize.y));
						targetLocation.x = targetLocation.x < 0 ? 0 : targetLocation.x;
						targetLocation.x = targetLocation.x > maxEditableSizeInt.x ? maxEditableSizeInt.x : targetLocation.x;
						targetLocation.y = targetLocation.y < 0 ? 0 : targetLocation.y;
						targetLocation.y = targetLocation.y > maxEditableSizeInt.y ? maxEditableSizeInt.y : targetLocation.y;
						DragInTarget = targetLocation;
						DragInArea = objects[0] as BattleMapArea;
						DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
						GUIUtility.hotControl = controlID;
						currentEvent.Use();
					}
					else {
						GUIUtility.hotControl = 0;
						DragInArea = null;
					}
				}
				break;
			case EventType.DragPerform:
				if (FileOpened && rect.Contains(currentEvent.mousePosition) && GUIUtility.hotControl == controlID) {
					Object[] objects = DragAndDrop.objectReferences;
					if (objects.Length > 1) {
						return;
					}
					if (objects[0].GetType() != typeof(BattleMapArea)) {
						return;
					}
					GUI.changed = true;
					Vector2 mousePos = currentEvent.mousePosition;
					Vector2 screenOffset = new Vector2(
						mousePos.x - rect.x - rect.width * 0.5f,
						mousePos.y - rect.y - rect.height * 0.5f);
					Vector2 canvasPos = screenOffset / rect.width * 20f * ViewScale * new Vector2(1.0f, -1.0f) + CameraPosition;
					Int2 targetLocation = new Int2(Mathf.FloorToInt(canvasPos.x), Mathf.FloorToInt(canvasPos.y));
					Int2 maxEditableSizeInt = new Int2(Mathf.FloorToInt(MaxEditableSize.x), Mathf.FloorToInt(MaxEditableSize.y));
					targetLocation.x = targetLocation.x < 0 ? 0 : targetLocation.x;
					targetLocation.x = targetLocation.x > maxEditableSizeInt.x ? maxEditableSizeInt.x : targetLocation.x;
					targetLocation.y = targetLocation.y < 0 ? 0 : targetLocation.y;
					targetLocation.y = targetLocation.y > maxEditableSizeInt.y ? maxEditableSizeInt.y : targetLocation.y;
					DragAndDrop.AcceptDrag();
					Undo.RegisterCompleteObjectUndo(Opened, "Add Area");
					Opened.DragAddNewArea(objects[0] as BattleMapArea, targetLocation);
					GUIUtility.hotControl = 0;
					currentEvent.Use();
					DragInArea = null;
				}
				break;
			#endregion
			///////////////////////
			////    Control    ///
			/////////////////////
			#region [ Control ]
			case EventType.MouseDown:
				if (rect.Contains(currentEvent.mousePosition)) {
					if (currentEvent.button == 2 && CurrentAction == Action.None) {
						CurrentAction = Action.Panning;
						RecordMousePosition = currentEvent.mousePosition;
						RecordCameraPosition = CameraPosition;
						GUIUtility.hotControl = controlID;
						currentEvent.Use();
					}
					if (currentEvent.button == 0 && CurrentAction == Action.None) {
						/////////////////////////////
						////    Click Control    ///
						///////////////////////////
						#region [ Click Control ]
						if (FileOpened) {
							Vector2 mousePos = currentEvent.mousePosition;
							Vector2 screenOffset = new Vector2(
								mousePos.x - rect.x - rect.width * 0.5f,
								mousePos.y - rect.y - rect.height * 0.5f);
							Vector2 canvasPos = screenOffset / rect.width * 20f * ViewScale * new Vector2(1.0f, -1.0f) + CameraPosition;
							RecordMousePosition = currentEvent.mousePosition;
							MouseLeftClick(canvasPos);
							if (Opened.Selecting >= 0) {
								RecordAreaCenterPosition = Opened.AreaLocations[Opened.Selecting];
							}
							GUI.changed = true;
							GUIUtility.hotControl = controlID;
							currentEvent.Use();
						}
						#endregion
					}
					if (currentEvent.button == 1 && CurrentAction == Action.None) {
						if (FileOpened) {
							Vector2 mousePos = currentEvent.mousePosition;
							Vector2 screenOffset = new Vector2(
								mousePos.x - rect.x - rect.width * 0.5f,
								mousePos.y - rect.y - rect.height * 0.5f);
							Vector2 canvasPos = screenOffset / rect.width * 20f * ViewScale * new Vector2(1.0f, -1.0f) + CameraPosition;
							BattleMapArea checkArea = Opened.MouseRightClickSelect(canvasPos);
							if (checkArea != null) {
								Repaint();
								Selection.objects = new Object[] { checkArea };
							}
						}
					}
				}
				break;
			case EventType.MouseDrag:
				if (GUIUtility.hotControl == controlID) {
					if (CurrentAction == Action.Panning) {
						GUI.changed = true;
						Vector2 mouseOffset = currentEvent.mousePosition - RecordMousePosition;
						mouseOffset.y = -mouseOffset.y;
						Vector2 newCamPos = RecordCameraPosition - mouseOffset / rect.width * 20f * ViewScale;
						CameraPosition.x = Mathf.Clamp(newCamPos.x, CameraRange.x, CameraRange.z);
						CameraPosition.y = Mathf.Clamp(newCamPos.y, CameraRange.y, CameraRange.w);
						currentEvent.Use();
					}
				}
				if (CurrentAction == Action.Selecting && FileOpened) {
					if (Opened.Selecting >= 0) {
						GUI.changed = true;
						Vector2 mouseOffset = currentEvent.mousePosition - RecordMousePosition;
						mouseOffset.y = -mouseOffset.y;
						Vector2 canvasOffset = mouseOffset / rect.width * 20f * ViewScale;
						if (canvasOffset.x < -1 || canvasOffset.x > 1 || canvasOffset.y < -1 || canvasOffset.y > 1 || MovingAreaUndoRecorded) {
							Int2 moveDirection = new Int2(Mathf.FloorToInt(canvasOffset.x), Mathf.FloorToInt(canvasOffset.y));
							if (moveDirection.x < 0) {
								moveDirection.x++;
							}
							if (moveDirection.y < 0) {
								moveDirection.y++;
							}
							Int2 targetLocation = RecordAreaCenterPosition + moveDirection;
							Int2 maxEditableSizeInt = new Int2(Mathf.FloorToInt(MaxEditableSize.x), Mathf.FloorToInt(MaxEditableSize.y));
							targetLocation.x = targetLocation.x < 0 ? 0 : targetLocation.x;
							targetLocation.x = targetLocation.x > maxEditableSizeInt.x ? maxEditableSizeInt.x : targetLocation.x;
							targetLocation.y = targetLocation.y < 0 ? 0 : targetLocation.y;
							targetLocation.y = targetLocation.y > maxEditableSizeInt.y ? maxEditableSizeInt.y : targetLocation.y;
							if (!MovingAreaUndoRecorded) {
								Undo.RegisterCompleteObjectUndo(Opened, "Move Area");
								MovingAreaUndoRecorded = true;
							}
							Opened.MoveSelectingAreaTo(targetLocation);
						}
						currentEvent.Use();
					}
				}
				break;
			case EventType.MouseUp:
				if (GUIUtility.hotControl == controlID) {
					if (CurrentAction == Action.Selecting && currentEvent.button == 0 && rect.Contains(currentEvent.mousePosition)) {
						if (FileOpened) {
							Vector2 mousePos = currentEvent.mousePosition;
							Vector2 screenOffset = new Vector2(
								mousePos.x - rect.x - rect.width * 0.5f,
								mousePos.y - rect.y - rect.height * 0.5f);
							Vector2 canvasPos = screenOffset / rect.width * 20f * ViewScale * new Vector2(1.0f, -1.0f) + CameraPosition;
							if (!SuccessSelectOnMouseDown) {
								MouseUpSelect(canvasPos);
							}
						}
					}
					CurrentAction = Action.None;
					GUIUtility.hotControl = 0;
					currentEvent.Use();
				}
				MovingAreaUndoRecorded = false;
				SuccessSelectOnMouseDown = false;
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
						float newViewScale = Mathf.Clamp(ViewScale * scaleChange, MinScale, MaxScale);
						scaleChange = newViewScale / ViewScale;
						Vector2 mousePos = currentEvent.mousePosition;
						Vector2 screenOffset = new Vector2(
							mousePos.x - rect.x - rect.width * 0.5f,
							mousePos.y - rect.y - rect.height * 0.5f);
						Vector2 newCamPos = CameraPosition -
							(screenOffset / rect.width * 20f * ViewScale * (scaleChange - 1.0f)) * new Vector2(1.0f, -1.0f);
						CameraPosition.x = Mathf.Clamp(newCamPos.x, CameraRange.x, CameraRange.z);
						CameraPosition.y = Mathf.Clamp(newCamPos.y, CameraRange.y, CameraRange.w);
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
		/////////////////////////////
		////    Render Canvas    ///
		///////////////////////////
		#region [ Render Canvas ]
		void RenderCanvas(Rect rect) {
			GUI.BeginGroup(rect);
			GL.PushMatrix();
			//////////////////////////////
			////    Setup Viewport    ///
			////////////////////////////
			#region [ Setup Viewport ]
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
			#endregion
			////////////////////////////////
			////    Clear Background    ///
			//////////////////////////////
			#region [ Clear Background ]
			GL.LoadIdentity();
			GL.LoadProjectionMatrix(Matrix4x4.Ortho(0f, 1f, 0f, 1f, -1f, 1f));

			GUIMaterial.SetInt(PID.SrcBlend, (int)BlendMode.One);
			GUIMaterial.SetInt(PID.DstBlend, (int)BlendMode.Zero);
			GUIMaterial.SetPass(0);

			GL.Begin(GL.TRIANGLE_STRIP);
			GL.Color(Styles.BackGroundColor);
			GL.Vertex3(1, 0, 0);
			GL.Vertex3(0, 0, 0);
			GL.Vertex3(1, 1, 0);
			GL.Vertex3(0, 1, 0);
			GL.End();
			#endregion
			//////////////////////////////
			////    Setup Matrices    ///
			////////////////////////////
			#region [ Setup Matrices ]
			GL.LoadIdentity();
			float aspect = viewportRect.height / viewportRect.width;
			Vector2 visibleArea = new Vector2(10f * ViewScale, 10f * aspect * ViewScale);
			Matrix4x4 projectionMatrix = Matrix4x4.Ortho(-visibleArea.x, visibleArea.x, -visibleArea.y, visibleArea.y, -1f, 1f);
			GL.LoadProjectionMatrix(projectionMatrix);
			Matrix4x4 viewMatrix =
				 Matrix4x4.Translate(new Vector3(-CameraPosition.x, -CameraPosition.y, 0));
			GL.MultMatrix(viewMatrix);
			#endregion
			/////////////////////////////
			////    Editable Area    ///
			///////////////////////////
			#region [ Editable Area ]
			GUIMaterial.SetInt(PID.SrcBlend, (int)BlendMode.One);
			GUIMaterial.SetInt(PID.DstBlend, (int)BlendMode.Zero);
			GUIMaterial.SetPass(0);
			GL.Begin(GL.TRIANGLE_STRIP);
			GL.Color(Styles.EditableAreaColor);
			GL.Vertex3(MaxEditableSize.x, 0, 0);
			GL.Vertex3(0, 0, 0);
			GL.Vertex3(MaxEditableSize.x, MaxEditableSize.y, 0);
			GL.Vertex3(0, MaxEditableSize.y, 0);
			GL.End();
			#endregion
			////////////////////////////
			////    Draw Content    ///
			//////////////////////////
			#region [ Draw Content ]
			if (FileOpened) {
				GUIMaterial.SetInt(PID.SrcBlend, (int)BlendMode.SrcAlpha);
				GUIMaterial.SetInt(PID.DstBlend, (int)BlendMode.OneMinusSrcAlpha);
				GUIMaterial.SetVector(PID.Bounds, new Vector4(0, 0, Opened.TemplateSize.x, Opened.TemplateSize.y));
				GUIMaterial.SetTexture(PID.MainTex, Texture2D.whiteTexture);
				List<TemplateAreaEditorCache> areaEditorCaches = Opened.AreaEditorCaches;
				int numArea = areaEditorCaches.Count;
				List<Int2> cells;
				List<Int2> entries;
				List<Vector3> outlineVertices;
				TemplateAreaEditorCache currentCache;
				bool selecting;
				List<Int2> allEntries = new List<Int2>();
				for (int i = 0; i < numArea; i++) {
					foreach (Int2 entry in Opened.AreaEditorCaches[i].AreaEntryLocationsCache) {
						if (entry.x >= 0 && entry.x < Opened.TemplateSize.x && entry.y >= 0 && entry.y < Opened.TemplateSize.y) {
							allEntries.Add(entry);
						}
					}
				}
				for (int i = 0; i < numArea; i++) {
					GUIMaterial.SetTexture(PID.MainTex, Texture2D.whiteTexture);
					selecting = Opened.Selecting == i;
					currentCache = areaEditorCaches[i];
					cells = currentCache.AreaCellLocationsCache;
					entries = currentCache.AreaEntryLocationsCache;
					outlineVertices = currentCache.AreaOutlineGUICache;
					/////////////////////
					////    Cells    ///
					///////////////////
					#region [ Cells ]
					GUIMaterial.SetColor(PID.ColorIn, selecting ? Styles.AreaCellColors[4] : Styles.AreaCellColors[0]);
					GUIMaterial.SetColor(PID.ColorOut, selecting ? Styles.AreaCellColors[5] : Styles.AreaCellColors[1]);
					GUIMaterial.SetPass(1);
					GL.Begin(GL.QUADS);
					GL.Color(Color.white);
					foreach (Int2 cell in cells) {
						GL.Vertex3(cell.x, cell.y, 0f);
						GL.Vertex3(cell.x, cell.y + 1, 0f);
						GL.Vertex3(cell.x + 1, cell.y + 1, 0f);
						GL.Vertex3(cell.x + 1, cell.y, 0f);
					}
					GL.End();
					#endregion
					///////////////////////
					////    Entries    ///
					/////////////////////
					#region [ Entries ]
					GUIMaterial.SetColor(PID.ColorIn, Styles.AreaCellColors[2]);
					GUIMaterial.SetColor(PID.ColorOut, Styles.AreaCellColors[3]);
					GUIMaterial.SetPass(1);
					bool Entryconnected;
					Int2 distance;
					GL.Begin(GL.QUADS);
					foreach (Int2 entry in entries) {
						Entryconnected = false;
						if (entry.x >= 0 && entry.x < Opened.TemplateSize.x && entry.y >= 0 && entry.y < Opened.TemplateSize.y) {
							foreach (Int2 otherEntry in allEntries) {
								distance = otherEntry - entry;
								if (distance.y == 0 && (distance.x == 1 || distance.x == -1)) {
									Entryconnected = true;
									break;
								}
								else if (distance.x == 0 && (distance.y == 1 || distance.y == -1)) {
									Entryconnected = true;
									break;
								}
							}
						}
						if (Entryconnected) {
							GL.Color(Styles.AreaCellColors[6]);
						}
						else {
							GL.Color(Color.white);
						}
						GL.Vertex3(entry.x, entry.y, 0f);
						GL.Vertex3(entry.x, entry.y + 1, 0f);
						GL.Vertex3(entry.x + 1, entry.y + 1, 0f);
						GL.Vertex3(entry.x + 1, entry.y, 0f);
					}
					GL.End();
					#endregion
					////////////////////////
					////    Outlines    ///
					//////////////////////
					#region [ Outlines ]
					GUIMaterial.SetColor(PID.ColorIn, selecting ? Styles.AreaCellColors[2] : Styles.AreaOutlineColors[0]);
					GUIMaterial.SetColor(PID.ColorOut, selecting ? Styles.AreaCellColors[3] : Styles.AreaOutlineColors[1]);
					GUIMaterial.SetPass(1);
					GL.Begin(GL.QUADS);
					for (int j = 0; j < outlineVertices.Count; j += 4) {
						GL.Color(Color.white);
						GL.Vertex(outlineVertices[j]);
						GL.Vertex(outlineVertices[j + 1]);
						GL.Color(Styles.AreaOutlineColors[4]);
						GL.Vertex(outlineVertices[j + 2]);
						GL.Vertex(outlineVertices[j + 3]);
					}
					GL.End();
					#endregion
					/////////////////////////
					////    Area Icon    ///
					///////////////////////
					#region [ Area Icon ]
					Int2 iconCenter = Opened.AreaLocations[i];
					GUIMaterial.SetColor(PID.ColorOut, Styles.AreaIconColors[0]);
					//unknown
					if (Opened.Areas[i].UniqueID.Length <= 0) {
						GUIMaterial.SetColor(PID.ColorIn, Styles.AreaIconColors[1]);
						GUIMaterial.SetTexture(PID.MainTex, Styles.AreaIconsTextures[0]);
						GUIMaterial.SetPass(1);
						DrawCanvasIconQuad(iconCenter, 1.0f);
					}
					else if (ShowAreaTypeIcon) {
						if (Opened.Areas[i].Type == BattleMapAreaType.Home) {
							GUIMaterial.SetColor(PID.ColorIn, Styles.AreaIconColors[2]);
							GUIMaterial.SetTexture(PID.MainTex, Styles.AreaIconsTextures[1]);
							GUIMaterial.SetPass(1);
							DrawCanvasIconQuad(iconCenter, 1.2f);
						}
						else if (Opened.Areas[i].Type == BattleMapAreaType.Spawn) {
							GUIMaterial.SetColor(PID.ColorIn, Styles.AreaIconColors[3]);
							GUIMaterial.SetTexture(PID.MainTex, Styles.AreaIconsTextures[2]);
							GUIMaterial.SetPass(1);
							DrawCanvasIconQuad(iconCenter, 1.2f);
						}
					}
					#endregion
				}
				///////////////////////
				////    Overlap    ///
				/////////////////////
				#region [ Overlap ]
				GUIMaterial.SetTexture(PID.MainTex, Texture2D.whiteTexture);
				GUIMaterial.SetPass(0);
				GL.Begin(GL.QUADS);
				GL.Color(Styles.OverlapColor);
				int[] overlapCache = Opened.OverlapCache;
				int currentOverlapCacheIndex;
				for (int y = 0; y < Opened.TemplateSize.y; y++) {
					for (int x = 0; x < Opened.TemplateSize.x; x++) {
						currentOverlapCacheIndex = x + y * 100;
						if (overlapCache[currentOverlapCacheIndex] > 1) {
							GL.Vertex3(x + 0.3f, y + 0.3f, 0f);
							GL.Vertex3(x + 0.7f, y + 0.3f, 0f);
							GL.Vertex3(x + 0.7f, y + 0.7f, 0f);
							GL.Vertex3(x + 0.3f, y + 0.7f, 0f);
						}
					}
				}
				GL.End();
				#endregion
				///////////////////////
				////    Drag In    ///
				/////////////////////
				#region [ Drag In ]
				if (DragInArea != null) {
					GUIMaterial.SetTexture(PID.MainTex, Texture2D.whiteTexture);
					GUIMaterial.SetPass(0);
					GL.Begin(GL.QUADS);
					GL.Color(Styles.DragInColors[0]);
					if (DragInArea.UniqueID.Length <= 0) {
						GL.Vertex3(DragInTarget.x - 1, DragInTarget.y - 1, 0f);
						GL.Vertex3(DragInTarget.x + 1, DragInTarget.y - 1, 0f);
						GL.Vertex3(DragInTarget.x + 1, DragInTarget.y + 1, 0f);
						GL.Vertex3(DragInTarget.x - 1, DragInTarget.y + 1, 0f);
					}
					else {
						foreach (Int2 cell in DragInArea.CellHolderLocations) {
							GL.Vertex3(DragInTarget.x + cell.x, DragInTarget.y + cell.y, 0f);
							GL.Vertex3(DragInTarget.x + cell.x + 1, DragInTarget.y + cell.y, 0f);
							GL.Vertex3(DragInTarget.x + cell.x + 1, DragInTarget.y + cell.y + 1, 0f);
							GL.Vertex3(DragInTarget.x + cell.x, DragInTarget.y + cell.y + 1, 0f);
						}
						GL.Color(Styles.DragInColors[1]);
						for (int i = 0; i < DragInArea.CellHolderLocations.Count; i++) {
							if (DragInArea.VariantCellHolderTypes[i] == VariantCellHolderType.Entry) {
								Int2 currentLocation = DragInArea.CellHolderLocations[i] + DragInTarget;
								GL.Vertex3(currentLocation.x, currentLocation.y, 0f);
								GL.Vertex3(currentLocation.x + 1, currentLocation.y, 0f);
								GL.Vertex3(currentLocation.x + 1, currentLocation.y + 1, 0f);
								GL.Vertex3(currentLocation.x, currentLocation.y + 1, 0f);
							}
						}
					}
					GL.End();
				}
				#endregion
			}
			#endregion
			/////////////////////////
			////    Draw Grid    ///
			///////////////////////
			#region [ Draw Grid ]
			//grid
			Color gridFadeColor = Styles.GridColor;
			gridFadeColor.a *= 0.5f;
			if (ViewScale > 5f) {
				gridFadeColor.a /= ViewScale * 0.2f;
			}
			GUIMaterial.SetInt(PID.SrcBlend, (int)BlendMode.SrcAlpha);
			GUIMaterial.SetInt(PID.DstBlend, (int)BlendMode.OneMinusSrcAlpha);
			GUIMaterial.SetPass(0);
			GL.Begin(GL.LINES);
			float loc = Mathf.Floor(-11f * ViewScale + CameraPosition.x);
			int locInt = Mathf.RoundToInt(loc);
			for (int i = 0; i < 23 * ViewScale; i++, loc += 1f, locInt++) {
				if (locInt < 0 || locInt > Mathf.RoundToInt(MaxEditableSize.x)) {
					continue;
				}
				GL.Color(((locInt / 10) * 10) == locInt ? Styles.GridColor : gridFadeColor);
				GL.Vertex3(loc, Mathf.Clamp(visibleArea.y + CameraPosition.y, 0f, MaxEditableSize.y), 0f);
				GL.Vertex3(loc, Mathf.Clamp(-visibleArea.y + CameraPosition.y, 0f, MaxEditableSize.y), 0f);
			}
			loc = Mathf.Floor(-11f * aspect * ViewScale + CameraPosition.y);
			locInt = Mathf.RoundToInt(loc);
			for (int i = 0; i < 23 * aspect * ViewScale; i++, loc += 1f, locInt++) {
				if (locInt < 0 || locInt > Mathf.RoundToInt(MaxEditableSize.y)) {
					continue;
				}
				GL.Color((locInt / 10 * 10) == locInt ? Styles.GridColor : gridFadeColor);
				GL.Vertex3(Mathf.Clamp(visibleArea.x + CameraPosition.x, 0f, MaxEditableSize.x), loc, 0f);
				GL.Vertex3(Mathf.Clamp(-visibleArea.x + CameraPosition.x, 0f, MaxEditableSize.x), loc, 0f);
			}
			GL.End();
			#endregion
			/////////////////////////////
			////    Draw Max Size    ///
			///////////////////////////
			#region [ Draw Max Size ]
			GUIMaterial.SetInt(PID.SrcBlend, (int)BlendMode.One);
			GUIMaterial.SetInt(PID.DstBlend, (int)BlendMode.Zero);
			GUIMaterial.SetPass(0);
			GL.Begin(GL.LINE_STRIP);
			GL.Color(Styles.MaxSizeFrameColor);
			GL.Vertex3(0f, 0f, 0f);
			GL.Vertex3(0f, 100f, 0f);
			GL.Vertex3(100f, 100f, 0f);
			GL.Vertex3(100f, 0f, 0f);
			GL.Vertex3(0f, 0f, 0f);
			GL.End();
			#endregion
			//////////////////////////
			////    Draw Range    ///
			////////////////////////
			#region [ Draw Range ]
			if (FileOpened) {
				Vector2 size = new Vector2(Opened.TemplateSize.x, Opened.TemplateSize.y);
				GUIMaterial.SetInt(PID.SrcBlend, (int)BlendMode.SrcAlpha);
				GUIMaterial.SetInt(PID.DstBlend, (int)BlendMode.OneMinusSrcAlpha);
				GUIMaterial.SetPass(0);
				/////////////////////
				////    Edges    ///
				///////////////////
				#region [ Edges ]
				//left
				GL.Begin(GL.TRIANGLE_STRIP);
				GL.Color(Styles.RangeColors[0]);
				GL.Vertex3(0f, 0f, 0f);
				GL.Vertex3(0f, size.y, 0f);
				GL.Color(Styles.RangeColors[1]);
				GL.Vertex3(-1f, 0f, 0f);
				GL.Vertex3(-1f, size.y, 0f);
				GL.End();
				//right
				GL.Begin(GL.TRIANGLE_STRIP);
				GL.Color(Styles.RangeColors[0]);
				GL.Vertex3(size.x, 0f, 0f);
				GL.Vertex3(size.x, size.y, 0f);
				GL.Color(Styles.RangeColors[1]);
				GL.Vertex3(size.x + 1, 0f, 0f);
				GL.Vertex3(size.x + 1, size.y, 0f);
				GL.End();
				//bottom
				GL.Begin(GL.TRIANGLE_STRIP);
				GL.Color(Styles.RangeColors[0]);
				GL.Vertex3(0f, 0f, 0f);
				GL.Vertex3(size.x, 0f, 0f);
				GL.Color(Styles.RangeColors[1]);
				GL.Vertex3(0f, -1f, 0f);
				GL.Vertex3(size.x, -1f, 0f);
				GL.End();
				//top
				GL.Begin(GL.TRIANGLE_STRIP);
				GL.Color(Styles.RangeColors[0]);
				GL.Vertex3(0f, size.y, 0f);
				GL.Vertex3(size.x, size.y, 0f);
				GL.Color(Styles.RangeColors[1]);
				GL.Vertex3(0f, size.y + 1f, 0f);
				GL.Vertex3(size.x, size.y + 1f, 0f);
				GL.End();
				#endregion
				///////////////////////
				////    Corners    ///
				/////////////////////
				#region [ Corners ]
				Vector2 currentCenter;
				Vector2[] centerOffsets = new Vector2[4];
				centerOffsets[0] = new Vector2(size.x, size.y);
				centerOffsets[1] = new Vector2(size.x, 0f);
				centerOffsets[2] = new Vector2(0f, 0f);
				centerOffsets[3] = new Vector2(0f, size.y);
				int numPerCornerTris = CircleSubdivision / 4;
				int circlePointIndex0, circlePointIndex1;
				GL.Begin(GL.TRIANGLES);
				for (int i = 0; i < 4; i++) {
					currentCenter = centerOffsets[i];
					for (int j = 0; j < numPerCornerTris; j++) {
						circlePointIndex0 = i * numPerCornerTris + j;
						circlePointIndex1 = circlePointIndex0 + 1;
						if (circlePointIndex1 >= CircleSubdivision) {
							circlePointIndex1 = 0;
						}
						GL.Color(Styles.RangeColors[0]);
						GL.Vertex3(currentCenter.x, currentCenter.y, 0f);
						GL.Color(Styles.RangeColors[1]);
						GL.Vertex3(currentCenter.x + CirclePoints[circlePointIndex0].x, currentCenter.y + CirclePoints[circlePointIndex0].y, 0f);
						GL.Vertex3(currentCenter.x + CirclePoints[circlePointIndex1].x, currentCenter.y + CirclePoints[circlePointIndex1].y, 0f);
					}
				}
				GL.End();
				#endregion
				//frame
				GL.Begin(GL.LINE_STRIP);
				GL.Color(Styles.RangeColors[2]);
				GL.Vertex3(0f, 0f, 0f);
				GL.Vertex3(0f, size.y, 0f);
				GL.Vertex3(size.x, size.y, 0f);
				GL.Vertex3(size.x, 0f, 0f);
				GL.Vertex3(0f, 0f, 0f);
				GL.End();
			}
			#endregion
			GL.PopMatrix();
			GUI.EndGroup();
		}
		#endregion
		#endregion
		////////////////////////////
		////    Data Control    ///
		//////////////////////////
		#region [ Data Control ]
		void MouseLeftClick(Vector2 clickPosition) {
			if (!FileOpened) {
				return;
			}
			SuccessSelectOnMouseDown = Opened.MouseDownSelect(clickPosition);
			CurrentAction = Action.Selecting;
		}
		void MouseUpSelect(Vector2 clickPosition) {
			if (!FileOpened) {
				return;
			}
			Opened.MouseUpSelect(clickPosition);
			SuccessSelectOnMouseDown = false;
		}
		#endregion
		/////////////////////////////////
		////    Keyboard Listener    ///
		///////////////////////////////
		#region [ Keyboard Listener ]
		void KeyboardListener() {
			if (!FileOpened) {
				return;
			}
			if (Opened.Selecting < 0) {
				return;
			}
			if (focusedWindow == this && CurrentAction == Action.None) {
				Event currentEvent = Event.current;
				switch (currentEvent.type) {
				case EventType.KeyDown:
					switch (currentEvent.keyCode) {
					case KeyCode.E:
						Undo.RegisterCompleteObjectUndo(Opened, "Rotate Area");
						Opened.RotateSelecting(true);
						currentEvent.Use();
						GUI.changed = true;
						break;
					case KeyCode.Q:
						Undo.RegisterCompleteObjectUndo(Opened, "Rotate Area");
						Opened.RotateSelecting(false);
						currentEvent.Use();
						GUI.changed = true;
						break;
					case KeyCode.W:
						Undo.RegisterCompleteObjectUndo(Opened, "Move Area");
						Opened.MoveSelectingAreaTo(Opened.AreaLocations[Opened.Selecting] + new Int2(0, 1));
						currentEvent.Use();
						GUI.changed = true;
						break;
					case KeyCode.S:
						Undo.RegisterCompleteObjectUndo(Opened, "Move Area");
						Opened.MoveSelectingAreaTo(Opened.AreaLocations[Opened.Selecting] + new Int2(0, -1));
						currentEvent.Use();
						GUI.changed = true;
						break;
					case KeyCode.A:
						Undo.RegisterCompleteObjectUndo(Opened, "Move Area");
						Opened.MoveSelectingAreaTo(Opened.AreaLocations[Opened.Selecting] + new Int2(-1, 0));
						currentEvent.Use();
						GUI.changed = true;
						break;
					case KeyCode.D:
						Undo.RegisterCompleteObjectUndo(Opened, "Move Area");
						Opened.MoveSelectingAreaTo(Opened.AreaLocations[Opened.Selecting] + new Int2(1, 0));
						currentEvent.Use();
						GUI.changed = true;
						break;
					case KeyCode.R:
					case KeyCode.Delete:
						Undo.RegisterCompleteObjectUndo(Opened, "Delete Area");
						Opened.DeleteSelecting();
						currentEvent.Use();
						GUI.changed = true;
						break;
					}
					break;
				}
			}
		}
		#endregion
		////////////////////////////////////
		////    Draw NoFileOpen Text    ///
		//////////////////////////////////
		#region [ Draw NoFileOpen Text ]
		void DrawNoFileOpenText(Rect rect) {
			Rect textRect = new Rect(rect.x + rect.width - 250, rect.y, rect.x + rect.width, rect.y + 20);
			GUI.Label(textRect, "No File Opened", Styles.NoFileOpenedTextStyle);
		}
		#endregion
		//------------------------------------------------------------------------//

		//-------------------------------- Helper --------------------------------//
		///////////////////////////////
		////    Helper Function    ///
		/////////////////////////////
		#region [ Helper Function ]
		void GuiLine(int height = 1) {
			Rect rect = EditorGUILayout.GetControlRect(false, height);
			rect.height = height;
			EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
		}
		void CancleSelecting() {
			if (FileOpened) {
				Opened.CancleSelecting();
			}
		}
		void DrawCanvasIconQuad(Int2 center, float size) {
			GL.Begin(GL.QUADS);
			GL.Color(Color.white);
			GL.TexCoord(new Vector3(0f, 0f, 0f));
			GL.Vertex3(center.x - size, center.y - size, 0f);
			GL.TexCoord(new Vector3(0f, 1f, 0f));
			GL.Vertex3(center.x - size, center.y + size, 0f);
			GL.TexCoord(new Vector3(1f, 1f, 0f));
			GL.Vertex3(center.x + size, center.y + size, 0f);
			GL.TexCoord(new Vector3(1f, 0f, 0f));
			GL.Vertex3(center.x + size, center.y - size, 0f);
			GL.End();
		}
		#endregion
		//------------------------------------------------------------------------//
	}
}
