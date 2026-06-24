using FTL;
using System;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public bool IsReady => isReady;
    public bool IsPowered { set; get; } = false;
    [field: SerializeField] public float ChargeProgress { get; private set; } = 0f;

    [field: SerializeField] public float ChargeDuration { get; private set; } = 7f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private bool isReady;
    [SerializeField] private bool prevIsPowered = false;

    private void Awake()
    {
        isReady = false;
        IsPowered = false;
    }

    private void Update()
    {
        if (IsPowered)
        {
            if (prevIsPowered == false)
                ChargeProgress = 0f;

            ChargeProgress = Mathf.Min(ChargeProgress + Time.deltaTime, ChargeDuration);

            // chargeSprites에 있는 스프라이트 수 - 1만큼 간격 검증
        }
        else
        {
            isReady = false;
            ChargeProgress = 0f;
        }

        UpdateIsReady();

        prevIsPowered = IsPowered;
    }

    private void UpdateIsReady()
    {
        if (ChargeProgress == ChargeDuration)
        {
            isReady = true;
        }
        else
        {
            isReady = false;
        }
    }
}
