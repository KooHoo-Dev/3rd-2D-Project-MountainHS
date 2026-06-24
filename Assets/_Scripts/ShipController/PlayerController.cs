using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

namespace FTL
{
    public class PlayerController : ShipController, IDamageable
    {
        [SerializeField] protected ERoomType aKeyFacilityType = ERoomType.Shield;
        [SerializeField] protected ERoomType sKeyFacilityType = ERoomType.Engine;
        [SerializeField] protected ERoomType dKeyFacilityType = ERoomType.Weapon;
        private bool oneShotBool = true;
        private bool oneShotBool2 = true;
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

            // 사용자 클릭 정보 리턴
            HandleMouse();
        }

        private void HandleMouse()
        {
            Vector3 mousePosition = Mouse.current.position.value;
            //Vector3 mousePositionInput = Input.mousePosition;
            if (oneShotBool)
                StartCoroutine(Coroutine1(mousePosition, mousePosition));

            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(mousePosition);
            Collider2D clickCol = Physics2D.OverlapPoint(worldPoint);
            if (oneShotBool2)
                StartCoroutine(Coroutine2(clickCol));
        }

        private IEnumerator Coroutine2(Collider2D clickCol)
        {
            oneShotBool2 = false;
            yield return new WaitForSeconds(1f);
            if (clickCol != null)
            {
                Debug.Log($"{clickCol.gameObject.name}");
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
