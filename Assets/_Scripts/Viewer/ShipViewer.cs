using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace FTL
{
    /// <summary>
    /// 함선 내의 Room, RoomViewer 정보를 통해 시각화
    /// </summary>
    public class ShipViewer : MonoBehaviour
    {
        [SerializeField] private ShipController shipController;
        [SerializeField] private RoomViewer[] roomViewers;
        [SerializeField] private bool showRoomConnections = true;
        [SerializeField] private Color roomConnectionColor = new Color(0.1f, 0.8f, 1f, 0.8f);
        [SerializeField] private float roomConnectionWidth = 0.08f;
        [SerializeField] private float roomConnectionZOffset = -0.1f;

        private static FieldInfo neighborRoomIdsField;
        private readonly List<LineRenderer> roomConnectionLines = new List<LineRenderer>();
        private Material roomConnectionMaterial;
        private GameObject tempLineGroup;

        private void Awake()
        {
            if (showRoomConnections)
            {
                tempLineGroup = new GameObject("Line Group");
                tempLineGroup.transform.SetParent(transform, false);
            }
        }
        private void Start()
        {
            roomViewers = GetComponentsInChildren<RoomViewer>();
            List<Room> rooms = shipController.Rooms;
            for (int i = 0; i < rooms.Count; i++)
            {
                roomViewers[i].Room = rooms[i];
                roomViewers[i].SetRoomState();
            }

            if (showRoomConnections)
            {
                CreateRoomConnectionLines();
            }
        }

        private void OnDestroy()
        {
            ClearRoomConnectionLines();
        }

        #region Room Connection Visualize
        private void OnDrawGizmos()
        {
            if (showRoomConnections == false)
            {
                return;
            }

            DrawRoomConnectionGizmos();
        }

        private void CreateRoomConnectionLines()
        {
            ClearRoomConnectionLines();

            if (showRoomConnections == false)
            {
                Debug.LogWarning("Room connection lines are disabled. No lines will be drawn.");
                return;
            }

            if (shipController == null)
            {
                Debug.LogWarning("ShipController is not assigned. Cannot create room connection lines.");
                return;
            }

            List<Room> rooms = shipController.Rooms;
            if (rooms == null)
            {
                Debug.LogWarning("ShipController.Rooms is null. Cannot create room connection lines.");
                return;
            }

            for (int i = 0; i < rooms.Count; i++)
            {
                Room room = rooms[i];
                List<int> neighborRoomIds = GetNeighborRoomIds(room);
                if (neighborRoomIds == null)
                {
                    continue;
                }

                for (int j = 0; j < neighborRoomIds.Count; j++)
                {
                    Room neighborRoom = FindRoomById(rooms, neighborRoomIds[j]);
                    if (neighborRoom == null)
                    {
                        continue;
                    }

                    if (IsConnectionAlreadyDrawn(rooms, i, neighborRoom.Id))
                    {
                        continue;
                    }

                    LineRenderer lineRenderer = CreateRoomConnectionLineRenderer();
                    lineRenderer.SetPosition(0, GetRoomLocalPosition(room));
                    lineRenderer.SetPosition(1, GetRoomLocalPosition(neighborRoom));
                    roomConnectionLines.Add(lineRenderer);
                }
            }
        }

        private LineRenderer CreateRoomConnectionLineRenderer()
        {
            GameObject lineObject = new GameObject("Room Connection Line");
            lineObject.transform.SetParent(tempLineGroup.transform, false);

            LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = false;
            lineRenderer.startWidth = roomConnectionWidth;
            lineRenderer.endWidth = roomConnectionWidth;
            lineRenderer.startColor = roomConnectionColor;
            lineRenderer.endColor = roomConnectionColor;
            lineRenderer.sortingOrder = 20;
            lineRenderer.material = GetRoomConnectionMaterial();

            return lineRenderer;
        }

        private Material GetRoomConnectionMaterial()
        {
            if (roomConnectionMaterial != null)
            {
                return roomConnectionMaterial;
            }

            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }

            roomConnectionMaterial = new Material(shader);
            roomConnectionMaterial.color = roomConnectionColor;
            return roomConnectionMaterial;
        }

        private void ClearRoomConnectionLines()
        {
            for (int i = 0; i < roomConnectionLines.Count; i++)
            {
                if (roomConnectionLines[i] != null)
                {
                    Destroy(roomConnectionLines[i].gameObject);
                }
            }

            roomConnectionLines.Clear();
        }

        private void DrawRoomConnectionGizmos()
        {
            List<Room> rooms = GetRoomsForGizmos();
            if (rooms == null)
            {
                return;
            }

            Gizmos.color = roomConnectionColor;

            for (int i = 0; i < rooms.Count; i++)
            {
                Room room = rooms[i];
                List<int> neighborRoomIds = GetNeighborRoomIds(room);
                if (neighborRoomIds == null)
                {
                    continue;
                }

                for (int j = 0; j < neighborRoomIds.Count; j++)
                {
                    Room neighborRoom = FindRoomById(rooms, neighborRoomIds[j]);
                    if (neighborRoom == null)
                    {
                        continue;
                    }

                    if (IsConnectionAlreadyDrawn(rooms, i, neighborRoom.Id))
                    {
                        continue;
                    }

                    Gizmos.DrawLine(GetRoomWorldPosition(room), GetRoomWorldPosition(neighborRoom));
                }
            }
        }

        private List<Room> GetRoomsForGizmos()
        {
            if (shipController == null)
            {
                return null;
            }

            if (shipController.Rooms != null &&
                shipController.Rooms.Count > 0)
            {
                return shipController.Rooms;
            }

            if (shipController.ShipData == null)
            {
                return null;
            }

            return shipController.ShipData.Rooms;
        }


        private bool IsConnectionAlreadyDrawn(List<Room> rooms, int roomIndex, int neighborRoomId)
        {
            Room room = rooms[roomIndex];
            for (int i = 0; i < roomIndex; i++)
            {
                Room previousRoom = rooms[i];
                if (previousRoom.Id != neighborRoomId)
                {
                    continue;
                }

                List<int> previousNeighborRoomIds = GetNeighborRoomIds(previousRoom);
                if (ContainsRoomId(previousNeighborRoomIds, room.Id))
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        private Vector3 GetRoomLocalPosition(Room room)
        {
            Vector3 worldPosition = GetRoomWorldPosition(room);
            Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
            localPosition.z = roomConnectionZOffset;
            return localPosition;
        }

        private Vector3 GetRoomWorldPosition(Room room)
        {
            if (Application.isPlaying)
            {
                return new Vector3(room.GlobalPosition.x, room.GlobalPosition.y, transform.position.z + roomConnectionZOffset);
            }

            if (room.GlobalPosition != Vector2.zero)
            {
                return new Vector3(room.GlobalPosition.x, room.GlobalPosition.y, transform.position.z + roomConnectionZOffset);
            }

            Vector3 localPosition = new Vector3(room.LocalPosition.x, room.LocalPosition.y, roomConnectionZOffset);
            return transform.TransformPoint(localPosition);
        }

        private Room FindRoomById(List<Room> rooms, int roomId)
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                bool isMatchingRoom = rooms[i].Id == roomId;
                if (isMatchingRoom)
                {
                    return rooms[i];
                }
            }

            return null;
        }

        private bool ContainsRoomId(List<int> roomIds, int roomId)
        {
            if (roomIds == null)
            {
                return false;
            }

            for (int i = 0; i < roomIds.Count; i++)
            {
                bool isMatchingRoom = roomIds[i] == roomId;
                if (isMatchingRoom)
                {
                    return true;
                }
            }

            return false;
        }

        private List<int> GetNeighborRoomIds(Room room)
        {
            if (room == null)
            {
                return null;
            }

            if (neighborRoomIdsField == null)
            {
                neighborRoomIdsField = typeof(Room).GetField("neighborRoomIds", BindingFlags.Instance | BindingFlags.NonPublic);
            }

            if (neighborRoomIdsField == null)
            {
                return null;
            }

            return neighborRoomIdsField.GetValue(room) as List<int>;
        }
    }
}
