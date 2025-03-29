using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using GameManager.GameResources;
using UnityEditor.Animations;

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

            if (parentRoomNodeIDList.Count > 0 || roomNodeType.isEntrance)
            {
                EditorGUILayout.LabelField(roomNodeType.roomNodeTypeName);
            }
            else
            {
                int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);
                int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay());
                roomNodeType = roomNodeTypeList.list[selection];

                if (roomNodeTypeList.list[selected].isCorridor && !roomNodeTypeList.list[selection].isCorridor || !roomNodeTypeList.list[selected].isCorridor && roomNodeTypeList.list[selection].isCorridor || !roomNodeTypeList.list[selected].isBossRoom && roomNodeTypeList.list[selection].isBossRoom)
                {
                    if (childRoomNodeIDList.Count > 0)
                    {
                        for (int i = 0; i <childRoomNodeIDList.Count; i++)
                        {
                            RoomNodeSO childRoomNode = roomNodeGraph.GetRoomNode(childRoomNodeIDList[i]);

                            if (childRoomNode != null)
                            {
                                RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);
                                childRoomNode.RemoveParentRoomNodeIDFromRoomNode(id);
                            }
                        }
                    }
                }
            }

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
            switch (currentEvent.type)
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
            if (currentEvent.button == 0)
            {
                ProcessLeftClickDownEvent();
            }
            else if (currentEvent.button == 1)
            {
                ProcessRightClickDownEvent(currentEvent);
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

        private void ProcessRightClickDownEvent(Event currentEvent)
        {
            roomNodeGraph.SetNodeToDrawConnectionLineFrom(this, currentEvent.mousePosition);
        }

        private void ProcessLeftClickUpEvent()
        {
            if (isLeftClickDragging)
            {
                isLeftClickDragging = false;
            }
        }

        private void DragNode(Vector2 delta)
        {
            rect.position += delta;
            EditorUtility.SetDirty(this);
        }

        public bool AddChildRoomNodeIDToRoomNode(string childID)
        {
            if (IsChildRoomValid(childID))
            {
                childRoomNodeIDList.Add(childID);
                return true;
            }
            return false;
        }

        private bool IsChildRoomValid(string childID)
        {
            bool isConnectedBoosNodeAlready = false;
            foreach (RoomNodeSO roomNode in roomNodeGraph.roomNodeList)
            {
                if (roomNode.roomNodeType.isBossRoom && roomNode.parentRoomNodeIDList.Count > 0)
                {
                    isConnectedBoosNodeAlready = true;
                }
            }

            RoomNodeSO roomNodeToCheck = roomNodeGraph.GetRoomNode(childID);
            if (roomNodeToCheck.roomNodeType.isBossRoom && isConnectedBoosNodeAlready) return false;
            if (roomNodeToCheck.roomNodeType.isNone) return false;
            if (id == childID) return false;
            if (childRoomNodeIDList.Contains(childID)) return false;
            if (parentRoomNodeIDList.Contains(childID)) return false;
            if (roomNodeToCheck.parentRoomNodeIDList.Count > 0) return false;
            if (roomNodeToCheck.roomNodeType.isCorridor && roomNodeType.isCorridor) return false;
            if (!roomNodeToCheck.roomNodeType.isCorridor && !roomNodeType.isCorridor) return false;
            if (roomNodeToCheck.roomNodeType.isCorridor && childRoomNodeIDList.Count >= Settings.MAX_CHILD_CORRIDORS) return false;
            if (!roomNodeToCheck.roomNodeType.isCorridor && childRoomNodeIDList.Count > 0) return false;
            if (roomNodeToCheck.roomNodeType.isEntrance) return false;

            return true;
        }

        public bool AddParentRoomNodeIDToRoomNode(string parentID)
        {
            parentRoomNodeIDList.Add(parentID);
            return true;
        }

        public bool RemoveChildRoomNodeIDFromRoomNode(string childID)
        {
            if (childRoomNodeIDList.Contains(childID))
            {
                childRoomNodeIDList.Remove(childID);
                return true;
            }
            return false;
        }

        public bool RemoveParentRoomNodeIDFromRoomNode(string parentID)
        {
            if (parentRoomNodeIDList.Contains(parentID))
            {
                parentRoomNodeIDList.Remove(parentID);
                return true;
            }
            return false;
        }

#endif
    }
}