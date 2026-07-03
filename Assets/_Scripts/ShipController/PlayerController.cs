using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

namespace FTL
{
    public class PlayerController : ShipController
    {
        [Header("Player Settings")]
        [field: SerializeField] public int ScrapAmount { get; set; }
        [SerializeField] private LayerMask clickableLayer;

        private ERoomType aKeyFacilityType = ERoomType.Shield;
        private ERoomType sKeyFacilityType = ERoomType.Engine;
        private ERoomType dKeyFacilityType = ERoomType.Weapon;
        private bool oneShotBool = true;
        private bool oneShotBool2 = true;

        protected override void Awake()
        {
            base.Awake();
            GameManager.Instance.Player = this;
            if (GameManager.Instance.IsGameContextEmpty)
            {
                GameManager.Instance.SetGameContext(this);
            }
        }
        private void Start()
        {
            Ownership.ShipSide = EShipSide.Player;
        }

        private void Update()
        {
            bool isShiftPressed = SimpleInput.GetKey(Key.LeftShift) || SimpleInput.GetKey(Key.RightShift);

            TestHandleFacilityPower(isShiftPressed);

            TestIncreaseReactorPower();

            if (SimpleInput.GetKeyDown(Key.Digit5))
            {
                // 무기 시스템의 선택한 무기를 1번 무기로 교체
                weaponSystem.SetSelectedWeapon(0);
            }
            if (SimpleInput.GetKeyDown(Key.Digit6))
            {
                // 무기 시스템의 선택한 무기를 1번 무기로 교체
                weaponSystem.SetSelectedWeapon(1);
            }

            if (SimpleInput.GetKeyDown(Key.Escape))
            {
                weaponSystem.ResetSelectedWeapon();
                crewSystem.ResetSelectedCrews();
            }

            // 사용자 클릭 정보 리턴
            HandleMouse();

        }

        private void HandleMouse()
        {
            Vector3 mousePosition = SimpleInput.GetMousePosition();
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(mousePosition);

            Collider2D[] clickCols = Physics2D.OverlapPointAll(worldPoint, clickableLayer);
            Collider2D roomCol = null;
            CrewController crew = null;
            EShipSide shipSide = EShipSide.None;
            LayerMask roomLayer = LayerMask.GetMask(Tags.ROOM_LAYER);
            LayerMask shipLayer = LayerMask.GetMask(Tags.SHIP_LAYER);
            LayerMask crewLayer = LayerMask.GetMask(Tags.CREW_LAYER);

            bool isLeftMouseClick = SimpleInput.GetMouseButtonDown(SimpleInput.MouseButton.Left);
            bool isRightMouseClick = SimpleInput.GetMouseButtonDown(SimpleInput.MouseButton.Right);

            if (isLeftMouseClick || isRightMouseClick)
            {
                foreach (Collider2D col in clickCols)
                {
                    ReadClickedCollider(col, roomLayer, shipLayer, crewLayer, ref roomCol, ref shipSide, ref crew);
                }

                if (isLeftMouseClick)
                {
                    HandleLeftMouseClick(roomCol, shipSide, crew);
                }

                if (isRightMouseClick)
                {
                    HandleRightMouseClick(roomCol);
                }
            }
        }

        private void ReadClickedCollider(
            Collider2D col,
            LayerMask roomLayer,
            LayerMask shipLayer,
            LayerMask crewLayer,
            ref Collider2D roomCol,
            ref EShipSide shipSide,
            ref CrewController crew)
        {
            int colLayer = col.gameObject.layer;
            bool isRoomLayer = roomLayer.Contains(colLayer);
            bool isShipLayer = shipLayer.Contains(colLayer);
            bool isCrewLayer = crewLayer.Contains(colLayer);

            if (isRoomLayer)
            {
                roomCol = col;
                Debug.Log($"Room Clicked: {roomCol.gameObject.name}");
            }
            else if (isShipLayer)
            {
                shipSide = col.GetComponent<ShipOwnership>().ShipSide;
                Debug.Log($"Ship Clicked!");
            }
            else if (isCrewLayer)
            {
                crew = col.GetComponentInParent<CrewController>();
            }
        }

        private void HandleLeftMouseClick(Collider2D roomCol, EShipSide shipSide, CrewController crew)
        {
            bool hasRoomCol = roomCol != null;
            bool isEnemyShip = shipSide != EShipSide.Player && shipSide != EShipSide.None;
            bool isEnemyRoomSelected = hasRoomCol && isEnemyShip;

            if (isEnemyRoomSelected)
            {
                Room room = CombatSystem.Instance.GetRoomOrNull(shipSide, roomCol.transform.position);
                int roomId = room.Id;
                Vector2 target = roomCol.bounds.center;
                ProjectileTargetData targetData = new ProjectileTargetData(target, roomId);
                weaponSystem.SetSelectedRoomTarget(targetData);
            }

            bool isPlayerCrewSelected = crew != null && shipSide == EShipSide.Player;
            if (isPlayerCrewSelected)
            {
                crewSystem.AddSelectedCrew(crew);
            }
        }

        private void HandleRightMouseClick(Collider2D roomCol)
        {
            if (roomCol != null)
            {
                Room room = GetRoomByPositionOrNull(roomCol.transform.position);
                Debug.Log($"Target Room Id: {room.Id}");
                crewSystem.SetTargetRoom(room);
            }
            else
            {
                crewSystem.ResetSelectedCrews();
            }
        }

        public void IncreaseReactorPower(int amount)
        {
            PowerSystem.AddReactorPower(amount);
        }

        public void SetReactorMaxPower(int amount)
        {
            PowerSystem.SetReactorMaxPower(amount);
        }

        public void SetFacilitySystems(List<FacilityState> facilityStates)
        {
            for (ERoomType type = ERoomType.Weapon; type <= ERoomType.Shield; type++)
            {
                int index = (int)type;
                FacilitySystem facility = FacilitySystems[index];
                FacilityState facilityState = facilityStates[index];
                facility.ApplyState(facilityState);
                PowerSystem.ApplyState(facilityState);
            }
        }

        public void SetCrewsState(List<CrewState> crewStates)
        {
            for (int i = 0; i < Crews.Count; i++)
            {
                CrewController crew = Crews[i];
                CrewState crewState = crewStates[i];
                Room room = GetRoomByIdOrNull(crewState.LastRoomId);
                crew.ApplyCrewState(crewState, room);
            }
        }


        #region Test Method
        private void TestOverlapPoint(Collider2D clickCol)
        {
            if (oneShotBool2)
            {
                StartCoroutine(Coroutine2(clickCol));
            }
        }

        private void TestMousePosition(Vector3 mousePosition)
        {
            if (oneShotBool)
            {
                StartCoroutine(Coroutine1(mousePosition, mousePosition));
            }
        }

        private void TestIncreaseReactorPower()
        {
            if (SimpleInput.GetKeyDown(Key.Space))
            {
                IncreaseReactorPower(1);
            }
        }

        private void TestHandleFacilityPower(bool isShiftPressed)
        {
            if (SimpleInput.GetKeyDown(Key.A))
            {
                HandlePowerInput(aKeyFacilityType, isShiftPressed);
            }

            if (SimpleInput.GetKeyDown(Key.S))
            {
                HandlePowerInput(sKeyFacilityType, isShiftPressed);
            }

            if (SimpleInput.GetKeyDown(Key.D))
            {
                HandlePowerInput(dKeyFacilityType, isShiftPressed);
            }
        }

        private IEnumerator Coroutine1(Vector3 a, Vector3 b)
        {
            oneShotBool = false;
            yield return new WaitForSeconds(1f);
            Debug.Log($"n:{a}, o:{b}");
            oneShotBool = true;
        }

        private IEnumerator Coroutine2(Collider2D clickCol)
        {
            oneShotBool2 = false;
            yield return new WaitForSeconds(1f);
            if (clickCol != null)
            {
                Debug.Log($"{clickCol.gameObject.name}");
            }
            else
            {
                Debug.Log($"Nothing is clicked in targetLayer!");
            }
            oneShotBool2 = true;
            yield break;
        }

        public void SetReactorPower(int shipPower)
        {
            PowerSystem.SetReactorPower(shipPower);
        }

        #endregion
    }
}
