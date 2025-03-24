using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using GameManager.GameResources;

namespace NodeGraph
{
    public class RoomNodeSO : ScriptableObject
    {
        [HideInInspector] public string id;
        [HideInInspector] public List<string> parentRoomNodeIDList = new List<string>();
        [HideInInspector] public List<string> childRoomNodeIDList = new List<string>();
        [HideInInspector] public RoomNodeGraphSO roomNodeGraph;
        [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList;
        public RoomNodeTypeSO roomNodeType;

#if UNITY_EDITOR

        [HideInInspector] public Rect rect;
        [HideInInspector] public bool isLeftClickDragging = false;
        [HideInInspector] public bool isSelected = false;

        public void Initialize(Rect rect, RoomNodeGraphSO nodeGraph, RoomNodeTypeSO roomNodeType)
        {
            this.rect = rect;
            this.id = Guid.NewGuid().ToString();
            this.name = "RoomNode";
            this.roomNodeGraph = nodeGraph;
            this.roomNodeType = roomNodeType;

            roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
        }

        public void Draw(GUIStyle nodeStyle)
        {
            GUILayout.BeginArea(rect, nodeStyle);
            EditorGUI.BeginChangeCheck();

            int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);
            int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay());
            roomNodeType = roomNodeTypeList.list[selection];

            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(this);

            GUILayout.EndArea();
        }

        private string[] GetRoomNodeTypesToDisplay()
        {
            string[] roomArray = new string[roomNodeTypeList.list.Count];

            for (int i = 0; i < roomNodeTypeList.list.Count; i++)
            {
                if (roomNodeTypeList.list[i].displayInNodeGraphEditor)
                {
                    roomArray[i] = roomNodeTypeList.list[i].roomNodeTypeName;
                }
            }
            return roomArray;
        }

        public void ProcessEvents(Event currentEvent)
        {
            switch(currentEvent.type)
            {
                case EventType.MouseDown:
                    ProcessMouseDownEvent(currentEvent);
                    break;

                case EventType.MouseUp:
                    ProcessMouseUpEvent(currentEvent);
                    break;

                case EventType.MouseDrag:
                    ProcessMouseDragEvent(currentEvent);
                    break;

                default:
                    break;
            }
        }

        private void ProcessMouseDownEvent(Event currentEvent)
        {
            if(currentEvent.button == 0)
            {
                ProcessLeftClickDownEvent();
            }
        }

        private void ProcessMouseUpEvent(Event currentEvent)
        {
            if (currentEvent.button == 0)
            {
                ProcessLeftClickUpEvent();
            }
        }

        private void ProcessMouseDragEvent(Event currentEvent)
        {
            if (currentEvent.button == 0)
            {
                ProcessLeftMouseDragEvent(currentEvent);
            }
        }

        private void ProcessLeftMouseDragEvent(Event currentEvent)
        {
            isLeftClickDragging = true;
            DragNode(currentEvent.delta);
            GUI.changed = true;
        }

        private void ProcessLeftClickDownEvent()
        {
            Selection.activeObject = this;
            isSelected = !isSelected;
        }

        private void ProcessLeftClickUpEvent()
        {
            if(isLeftClickDragging)
            {
                isLeftClickDragging = false;
            }
        }

        private void DragNode(Vector2 delta)
        {
            rect.position += delta;
            EditorUtility.SetDirty(this);
        }
#endif
    }
}