using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemVisuals : MonoBehaviour
{
    // All items MUST have a gameObject named "Visuals" for it to be considered valid
    public const string ITEM_VISUALS_NAME = "Visuals";

    [Header("Item Holding Offset")]

    [Tooltip("The position offset the item should have from the prefab Origin when being held in the Player's hand")]
    public Vector3 handPositionOffset = Vector3.zero;

    [Tooltip("The rotation offset (in euler angles) the item should have from the prefab Origin when being held in the Player's hand")]
    public Vector3 handRotationOffset = Vector3.zero;

    public void EnableHandVisuals()
    {
        transform.localPosition = handPositionOffset;
        transform.localEulerAngles = handRotationOffset;
    }
}
