using System.Collections;
using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    [field: SerializeField] public PlayerBase.EQUIPMENT EquipmentPart { get; private set; }
    [field: SerializeField] public Sprite SpriteImage { get; private set; }


}
