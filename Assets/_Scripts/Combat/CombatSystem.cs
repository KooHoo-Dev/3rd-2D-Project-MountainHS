using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Utilities;

namespace FTL
{
    public class CombatSystem : SingletonBase<CombatSystem>
    {

        [SerializeField] private Collider2D[] shipAreas;
        [SerializeField] private ShipController[] ships;
        private Dictionary<Collider2D, EShipSide> colToNextShipAreaDict = new Dictionary<Collider2D, EShipSide>();
        private Dictionary<EShipSide, Collider2D> shipAreaToColDict = new Dictionary<EShipSide, Collider2D>();

        protected override void OnInitialize()
        {
            base.OnInitialize();
            shipAreaToColDict.Add(EShipSide.Player, shipAreas[0]);
            shipAreaToColDict.Add(EShipSide.Enemy0, shipAreas[1]);
            colToNextShipAreaDict.Add(shipAreas[0], EShipSide.Enemy0);
            colToNextShipAreaDict.Add(shipAreas[1], EShipSide.Player);
        }

        /// <summary>
        ///- 투사체 피격 판정 
        ///    - 멤버
        ///        - 피격 판정
        ///        - 도달한 Room 정보
        ///    - 피격된 Room에 피격 시행 (TakeDamage)
        ///        - Room의 TakeDamage 호출
        ///        - Room의 시설
        ///        - Room의 함선에 체력 피해 전달
        /// </summary>
        /// <param name="col"></param>
        /// <param name="projectile"></param>
        public void MoveToEnemyArea(Collider2D col, ProjectileController projectile)
        {
            EShipSide nextShipArea = colToNextShipAreaDict[col];
            Collider2D nextShipCol = shipAreaToColDict[nextShipArea];

            Vector3 spawnPos = GetRandomPositionOnColliderEdge(nextShipCol, projectile.transform.position.z);

            // 스폰장소 - 목표 콜라이더 중심 바라보게 회전시키기 right 이용
            projectile.SetEndCol(nextShipCol);
            projectile.transform.position = spawnPos;
            Vector3 direction = nextShipCol.bounds.center - spawnPos;
            direction.z = projectile.transform.position.z;
            projectile.transform.right = direction;
        }

        public bool HandleShieldCollision(ProjectileController projectile)
        {
            // projectile 주인 확인
            bool isPlayerProjectile = projectile.StartCol.CompareTag(Tags.PlayerShip);
            int opponentShipIndex = isPlayerProjectile ? 1 : 0;
            ShieldSystem oppenentShieldSystem = (ShieldSystem)ships[opponentShipIndex].GetFacilitySystemByERoomType(ERoomType.Shield);
            // 실드 시스템의 현재 실드가 1 이상이면
            // 실드 시스템 TakeDamage 호출
            // return true;
            // 현재 실드가 0이라면 
            // return false
            if (oppenentShieldSystem.ShieldCount > 0)
            {
                oppenentShieldSystem.TakeDamageToShield();
                return true;
            }
            return false;
        }

        // 미구현
        public bool HandleRoomCollision(ProjectileController projectile)
        {
            bool isPlayerProjectile = projectile.StartCol.CompareTag(Tags.PlayerShip);
            int opponentShipIndex = isPlayerProjectile ? 1 : 0;
            ShipController oppenentShipController = ships[opponentShipIndex];
            ProjectileTargetData targetData = new ProjectileTargetData(projectile.Target, projectile.TargetRoomId);

            // 명중 판정 로직 추가 (추후 구현)
            oppenentShipController.TakeDamageToRoom(projectile.Damage, targetData.RoomId);
            return true;
        }

        private Vector3 GetRandomPositionOnColliderEdge(Collider2D col, float z)
        {
            Bounds bounds = col.bounds;
            Vector3[] colFaceStarts = new Vector3[4];
            Vector3[] colFaceEnds = new Vector3[4];
            colFaceStarts[0] = new Vector3(bounds.min.x, bounds.min.y); // bottom-left
            colFaceStarts[1] = new Vector3(bounds.max.x, bounds.min.y); // bottom-right
            colFaceStarts[2] = new Vector3(bounds.max.x, bounds.max.y); // top-right
            colFaceStarts[3] = new Vector3(bounds.min.x, bounds.max.y); // top-left
            colFaceEnds[0] = new Vector3(bounds.max.x, bounds.min.y); // bottom-right
            colFaceEnds[1] = new Vector3(bounds.max.x, bounds.max.y); // top-right
            colFaceEnds[2] = new Vector3(bounds.min.x, bounds.max.y); // top-left
            colFaceEnds[3] = new Vector3(bounds.min.x, bounds.min.y); // bottom-left

            // 선택한 면에서 선택할 위치 (보간, 비율) 구하기
            int randIndex = Random.Range(0, 4);
            float lerpRate = Random.Range(0.0f, 1.0f);
            Vector3 spawnPos = Vector3.Lerp(colFaceStarts[randIndex], colFaceEnds[randIndex], lerpRate);
            spawnPos.z = z;

            return spawnPos;
        }

        public List<Room> GetPlayerRooms()
        {
            // Implementation for getting player rooms
            return ships[0].Rooms;
        }

        public Room GetRoomOrNull(EShipSide shipSide, Vector2 position)
        {
            List<Room> rooms = ships[(int)shipSide].Rooms;
            for (int i = 0; i < rooms.Count; i++)
            {
                bool isMatchingPosition = rooms[i].GlobalPosition == position;
                if (isMatchingPosition)
                {
                    return rooms[i];
                }
            }

            return null;
        }
    }
}
