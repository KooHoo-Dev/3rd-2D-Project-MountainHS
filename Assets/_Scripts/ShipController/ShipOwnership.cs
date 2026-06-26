using UnityEngine;

namespace FTL
{
    public class ShipOwnership: MonoBehaviour
    {
        [field: SerializeField] public EShipSide ShipSide { get; set; }
    }
}
