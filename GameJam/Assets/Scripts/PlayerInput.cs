using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerInput : MonoBehaviour
{
    public enum ControlScheme { WASD, Arrows, Joystick }

    [SerializeField] private ControlScheme controlScheme = ControlScheme.WASD;
    [SerializeField] private int joystickIndex = 1; // 1-based index for joysticks
    [SerializeField] private float Speed = 10f;

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        Vector2 movement = Vector2.zero;

        switch (controlScheme)
        {
            case ControlScheme.WASD:
                movement.x = (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f);
                movement.y = (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f);
                break;

            case ControlScheme.Arrows:
                movement.x = (Input.GetKey(KeyCode.RightArrow) ? 1f : 0f) - (Input.GetKey(KeyCode.LeftArrow) ? 1f : 0f);
                movement.y = (Input.GetKey(KeyCode.UpArrow) ? 1f : 0f) - (Input.GetKey(KeyCode.DownArrow) ? 1f : 0f);
                break;

            case ControlScheme.Joystick:
                var names = Input.GetJoystickNames();
                if (joystickIndex >= 1 && joystickIndex <= names.Length && !string.IsNullOrEmpty(names[joystickIndex - 1]))
                {
                    // Expectation: define axes in Input Manager like "Joy1_Horizontal", "Joy1_Vertical", "Joy2_Horizontal", etc.
                    string hAxis = $"Joy{joystickIndex}_Horizontal";
                    string vAxis = $"Joy{joystickIndex}_Vertical";

                    try
                    {
                        movement.x = Input.GetAxis(hAxis);
                        movement.y = Input.GetAxis(vAxis);
                    }
                    catch
                    {
                        // Fallback: if custom axes aren't configured, try the default "Horizontal"/"Vertical" (may mix inputs).
                        movement.x = Input.GetAxis("Horizontal");
                        movement.y = Input.GetAxis("Vertical");
                    }
                }
                else
                {
                    // No joystick connected at that index: fallback to arrows to avoid no-input situation
                    movement.x = (Input.GetKey(KeyCode.RightArrow) ? 1f : 0f) - (Input.GetKey(KeyCode.LeftArrow) ? 1f : 0f);
                    movement.y = (Input.GetKey(KeyCode.UpArrow) ? 1f : 0f) - (Input.GetKey(KeyCode.DownArrow) ? 1f : 0f);
                }
                break;
        }

        // Keep movement speed uniform when moving diagonally
        if (movement.sqrMagnitude > 1f) movement = movement.normalized;

        rb.linearVelocity = movement * Speed;
    }
}