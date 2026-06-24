using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

namespace FTL
{
    public class PlayerController : ShipController, IDamageable
    {
        [SerializeField] private ERoomType aKeyFacilityType = ERoomType.Shield;
        [SerializeField] private ERoomType sKeyFacilityType = ERoomType.Engine;
        [SerializeField] private ERoomType dKeyFacilityType = ERoomType.Weapon;
        [SerializeField] private WeaponSystem weaponSystem;
        private bool oneShotBool = true;
        private bool oneShotBool2 = true;
        [SerializeField] private LayerMask clickableLayer;
        private void Update()
        {
            bool isShiftPressed = SimpleInput.GetKey(Key.LeftShift) || SimpleInput.GetKey(Key.RightShift);

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

            if (SimpleInput.GetKeyDown(Key.Space))
            {
                powerSystem.AddReactorPower(1);
            }

            if (SimpleInput.GetKeyDown(Key.Digit5))
            {
                // 무기 시스템의 선택한 무기를 1번 무기로 교체
                weaponSystem.SetSelectWeapon(1);
            }

            // 사용자 클릭 정보 리턴
            HandleMouse();
        }

        private void HandleMouse()
        {
            Vector3 mousePosition = SimpleInput.GetMousePosition();
            //TestMousePosition(mousePosition);

            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(mousePosition);
            //if (Mouse.current.leftButton.wasPressedThisFrame)
            if (SimpleInput.GetMouseButtonDown(SimpleInput.MouseButton.Left))
            {
                Collider2D clickCol = Physics2D.OverlapPoint(worldPoint, clickableLayer);
                //TestOverlapPoint(clickCol);
            }
        }

        private void TestOverlapPoint(Collider2D clickCol)
        {
            if (oneShotBool2)
                StartCoroutine(Coroutine2(clickCol));
        }

        private void TestMousePosition(Vector3 mousePosition)
        {
            if (oneShotBool)
                StartCoroutine(Coroutine1(mousePosition, mousePosition));
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

        private IEnumerator Coroutine1(Vector3 a, Vector3 b)
        {
            oneShotBool = false;
            yield return new WaitForSeconds(1f);
            Debug.Log($"n:{a}, o:{b}");
            oneShotBool = true;
        }
    }
}
