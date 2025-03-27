using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using GameManager.GameResources;

namespace NodeGraph.Editor
{
    public class RoomNodeGraphEditor : EditorWindow
    {
        private GUIStyle roomNodeStyle;
        private GUIStyle selectedRoomNodeStyle;
        private static RoomNodeGraphSO currentRoomNodeGraph;
        private static RoomNodeSO currentRoomNode;
        private RoomNodeTypeListSO roomNodeTypeList;

        private const float NODE_WIDTH = 160f;
        private const float NODE_HEIGHT = 75f;
        private const int NODE_PADDING = 25;
        private const int NODE_BORDER = 12;
        private const float CONNECTING_LINE_WIDTH = 3f;
        private const float CONNECTING_LINE_ARROW_SIZE = 10f;

        [MenuItem("Room Node Graph Editor", menuItem = "Window/Dungeon Editor/Room Node Graph Editor")]
        private static void OpenWindow()
        {
            GetWindow<RoomNodeGraphEditor>("Room Node Graph Window");
        }

        private void OnEnable()
        {
            Selection.selectionChanged += InspectorSelectionChanged;


            roomNodeStyle = new GUIStyle();
            roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
            roomNodeStyle.normal.textColor = Color.white;
            roomNodeStyle.padding = new RectOffset(NODE_PADDING, NODE_PADDING, NODE_PADDING, NODE_PADDING);
            roomNodeStyle.border = new RectOffset(NODE_BORDER, NODE_BORDER, NODE_BORDER, NODE_BORDER);

            selectedRoomNodeStyle = new GUIStyle();
            selectedRoomNodeStyle.normal.background = EditorGUIUtility.Load("node1 on") as Texture2D;
            selectedRoomNodeStyle.normal.textColor = Color.white;
            selectedRoomNodeStyle.padding = new RectOffset(NODE_PADDING, NODE_PADDING, NODE_PADDING, NODE_PADDING);
            selectedRoomNodeStyle.border = new RectOffset(NODE_BORDER, NODE_BORDER, NODE_BORDER, NODE_BORDER);

            roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= InspectorSelectionChanged;
        }

        private void InspectorSelectionChanged()
        {
            RoomNodeGraphSO roomNodeGraph = Selection.activeObject as RoomNodeGraphSO;
            if(roomNodeGraph != null)
            {
                currentRoomNodeGraph = roomNodeGraph;
                GUI.changed = true;
            }
        }

        [OnOpenAsset(0)]
        public static bool OnDoubleClickAsset(int instanceID, int line)
        {
            RoomNodeGraphSO roomNodeGraph = EditorUtility.InstanceIDToObject(instanceID) as RoomNodeGraphSO;
            if (roomNodeGraph != null)
            {
                OpenWindow();
                currentRoomNodeGraph = roomNodeGraph;
                return true;
            }
            return false;
        }

        private void OnGUI()
        {
            if (currentRoomNodeGraph == null) return;

            DrawDraggedLine();
            ProcessEvents(Event.current);

            DrawRoomNodeConnections();

            DrawRoomNodes();

            if (GUI.changed)
                Repaint();
        }

        private void DrawDraggedLine()
        {
            if(currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
            {
                Handles.DrawBezier(currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition, currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition, Color.white, null, CONNECTING_LINE_WIDTH);
            }
        }

        private void DrawRoomNodeConnections()
        {
            foreach(RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
            {
                if(roomNode.childRoomNodeIDList.Count > 0)
                {
                    foreach(string childRoomNodeID in roomNode.childRoomNodeIDList)
                    {
                        if(currentRoomNodeGraph.roomNodeDictionary.ContainsKey(childRoomNodeID))
                        {
                            DrawConnectionLine(roomNode, currentRoomNodeGraph.roomNodeDictionary[childRoomNodeID]);
                            GUI.changed = true;
                        }
                    }
                }
            }
        }

        private void DrawConnectionLine(RoomNodeSO parentRoomNode, RoomNodeSO childRoomNode)
        {
            Vector2 start = parentRoomNode.rect.center;
            Vector2 end = childRoomNode.rect.center;
            Vector2 mid = (start + end) / 2f;

            Vector2 dir = end - start;

            Vector2 arrowTailHeadPoint = mid + dir.normalized * CONNECTING_LINE_ARROW_SIZE;
            Vector2 arrowTailPoint1 = mid - new Vector2(-dir.y, dir.x).normalized * CONNECTING_LINE_ARROW_SIZE;
            Vector2 arrowTailPoint2 = mid + new Vector2(-dir.y, dir.x).normalized * CONNECTING_LINE_ARROW_SIZE;

            Handles.DrawBezier(arrowTailHeadPoint, arrowTailPoint1, arrowTailHeadPoint, arrowTailPoint1, Color.white, null, CONNECTING_LINE_WIDTH);
            Handles.DrawBezier(arrowTailHeadPoint, arrowTailPoint2, arrowTailHeadPoint, arrowTailPoint2, Color.white, null, CONNECTING_LINE_WIDTH);

            Handles.DrawBezier(start, end, start, end, Color.white, null, CONNECTING_LINE_WIDTH);
            GUI.changed = true;
        }

        private void ProcessEvents(Event currentEvent)
        {
            if (currentRoomNode == null || !currentRoomNode.isLeftClickDragging)
            {
                currentRoomNode = IsMouseOverRoomNode(currentEvent);
            }

            if(currentRoomNode == null || currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
            {
                ProcessRoomNodeGraphEvent(currentEvent);
            }
            else
            {
                currentRoomNode.ProcessEvents(currentEvent);
            }
        }

        private RoomNodeSO IsMouseOverRoomNode(Event currentEvent)
        {
            for(int i = 0; i<currentRoomNodeGraph.roomNodeList.Count; i++)
            {
                if (currentRoomNodeGraph.roomNodeList[i].rect.Contains(currentEvent.mousePosition))
                {
                    return currentRoomNodeGraph.roomNodeList[i];
                }
            }

            return null;
        }

        private void ProcessRoomNodeGraphEvent(Event currentEvent)
        {
            switch (currentEvent.type)
            {
                case EventType.MouseDown:
                    ProcessMouseDownEvent(currentEvent);
                    break;
                case EventType.MouseDrag:
                    ProcessMouseDragEvent(currentEvent);
                    break;
                case EventType.MouseUp:
                    ProcessMouseUpEvent(currentEvent);
                    break;
                default:
                    break;
            }
        }

        private void ProcessMouseDownEvent(Event currentEvent)
        {
            if (currentEvent.button == 1)
            {
                ShowContextMenu(currentEvent.mousePosition);
            }
            else if (currentEvent.button == 0)
            {
                ClearLineDrag();
                ClearAllSelectedRoomNodes();
            }
        }

        private void ProcessMouseDragEvent(Event currentEvent)
        {
            if (currentEvent.button == 1)
            {
                ProcessRightMouseDragEvent(currentEvent);
            }
        }

        private void ProcessMouseUpEvent(Event currentEvent)
        {
            if(currentEvent.button == 1 && currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
            {
                RoomNodeSO roomNode = IsMouseOverRoomNode(currentEvent);
                if (roomNode != null)
                {
                    currentRoomNodeGraph.roomNodeToDrawLineFrom.AddChildRoomNodeIDToRoomNode(roomNode.id);
                    roomNode.AddParentRoomNodeIDToRoomNode(currentRoomNodeGraph.roomNodeToDrawLineFrom.id);
                }

                ClearLineDrag();
            }
        }

        private void ProcessRightMouseDragEvent(Event CurrentEvent)
        {
            if(currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
            {
                DragConnectionLine(CurrentEvent.delta);
                GUI.changed = true;
            }
        }

        private void ClearLineDrag()
        {
            currentRoomNodeGraph.roomNodeToDrawLineFrom = null;
            currentRoomNodeGraph.linePosition = Vector2.zero;
            GUI.changed = true;
        }

        private void ClearAllSelectedRoomNodes()
        {
            foreach(RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
            {
                if(roomNode.isSelected)
                {
                    roomNode.isSelected = false;
                }
            }
            GUI.changed = true;
        }

        private void DragConnectionLine(Vector2 delta)
        {
            currentRoomNodeGraph.linePosition += delta;
        }

        private void ShowContextMenu(Vector2 mousePosition)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Create Room Node"), false, CreateRoomNode, mousePosition);
            menu.ShowAsContext();
        }

        private void CreateRoomNode(object mousePositionObject)
        {
            if(currentRoomNodeGraph.roomNodeList.Count == 0)
            {
                CreateRoomNode(new Vector2(200f, 200f), roomNodeTypeList.list.Find(x => x.isEntrance));
            }

            CreateRoomNode(mousePositionObject, roomNodeTypeList.list.Find(x => x.isNone));
        }

        private void CreateRoomNode(object mousePositionObject, RoomNodeTypeSO roomNodeType)
        {
            Vector2 mousePostion = (Vector2)mousePositionObject;
            RoomNodeSO roomNode = CreateInstance<RoomNodeSO>();

            currentRoomNodeGraph.roomNodeList.Add(roomNode);
            roomNode.Initialize(new Rect(mousePostion, new Vector2(NODE_WIDTH, NODE_HEIGHT)), currentRoomNodeGraph, roomNodeType);
            AssetDatabase.AddObjectToAsset(roomNode, currentRoomNodeGraph);
            AssetDatabase.SaveAssets();

            currentRoomNodeGraph.OnValidate();
        }

        void DrawRoomNodes()
        {
            foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
            {
                if(roomNode.isSelected)
                {
                    roomNode.Draw(selectedRoomNodeStyle);
                }
                else
                {
                    roomNode.Draw(roomNodeStyle);
                }
            }

            GUI.changed = true;
        }
    }
}