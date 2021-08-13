using UnityEngine;

public enum PlayerControlSchemes {
    None,
    FreeMovementControls,
    CartControls
}

public interface PlayerControls
{
    void Move(Vector2 direction);

    void Look(Vector2 direction);

    void Jump();

    void Walk();

    void Interact();

}
