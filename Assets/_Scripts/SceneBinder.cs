using UnityEngine;

namespace FTL
{
    public class SceneBinder : MonoBehaviour
    {
        public void BindSceneObjects(GameContext gameContext)
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();
            EnemyController enemy = FindFirstObjectByType<EnemyController>();
            player.Hp = gameContext.ShipHp;
            player.ScrapAmount = gameContext.ScrapAmount;
            player.SetReactorMaxPower(gameContext.ShipReactorCapacity);
            player.SetReactorPower(gameContext.ShipPower);
            player.SetFacilitySystems(gameContext.FacilityStates);
            player.SetCrewsState(gameContext.CrewStates);
            player.SetWeaponTypes(gameContext.WeaponTypes);
            BindCombatSystem(player, enemy);
        }

        private void BindCombatSystem(PlayerController player, EnemyController enemy)
        {
            CombatSystem.Instance.InitCombatSystem(player, enemy);
        }

    }
}
