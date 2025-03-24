using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using GameManager.GameResources;

namespace NodeGraph.Editor
{
    public class RoomNodeGraphEditor : EditorWindow
    {
        private GUIStyle roomNodeStyle;
        private static RoomNodeGraphSO currentRoomNodeGraph;
        private static RoomNodeSO currentRoomNode;
        private RoomNodeTypeListSO roomNodeTypeList;

        private const float NODE_WIDTH = 160f;
        private const float NODE_HEIGHT = 75f;
        private const int NODE_PADDING = 25;
        private const int NODE_BORDER = 12;

        [MenuItem("Room Node Graph Editor", menuItem = "Window/Dungeon Editor/Room Node Graph Editor")]
        private static void OpenWindow()
        {
            GetWindow<RoomNodeGraphEditor>("Room Node Graph Window");
        }

        private void OnEnable()
        {
            roomNodeStyle = new GUIStyle();
            roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
            roomNodeStyle.normal.textColor = Color.white;
            roomNodeStyle.padding = new RectOffset(NODE_PADDING, NODE_PADDING, NODE_PADDING, NODE_PADDING);
            roomNodeStyle.border = new RectOffset(NODE_BORDER, NODE_BORDER, NODE_BORDER, NODE_BORDER);

            roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
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

            ProcessEvents(Event.current);

            DrawRoomNodes();

            if (GUI.changed)
                Repaint();
        }

        private void ProcessEvents(Event currentEvent)
        {
            if (currentRoomNode == null || !currentRoomNode.isLeftClickDragging)
            {
                currentRoomNode = IsMouseOverRoomNode(currentEvent);
            }

            if(currentRoomNode == null)
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
        }

        private void ShowContextMenu(Vector2 mousePosition)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Create Room Node"), false, CreateRoomNode, mousePosition);
            menu.ShowAsContext();
        }

        private void CreateRoomNode(object mousePositionObject)
        {
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
        }

        void DrawRoomNodes()
        {
            foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
            {
                roomNode.Draw(roomNodeStyle);
            }

            GUI.changed = true;
        }
    }
}