using UnityEngine;
using UnityEngine.InputSystem;

namespace Construct.Input
{
    // Temporary editor comfort; the pause menu will own the cursor later
    public sealed class CursorLock : MonoBehaviour
    {
        private void Start()
        {
            Lock();
        }

        private void Update()
        {
            if (Keyboard.current == null)
                return;

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
                Unlock();
            else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
                Lock();
        }

        private static void Lock()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private static void Unlock()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}