using UnityEngine;
using System.Collections;

namespace Antymology.UI
{
    public class FlyCamera : MonoBehaviour
    {

        /*
        Writen by Windexglow 11-13-10.  Use it, edit it, steal it I don't care.  
        Converted to C# 27-02-13 - no credit wanted.
        Simple flycam I made, since I couldn't find any others made public.*/

        /// <summary>
        /// Controls:
        /// - WASD: Move forward/back/left/right
        /// - Q/E: Move down/up
        /// - Arrow Keys: Rotate camera
        /// - Scroll Wheel: Change movement speed
        /// - Hold Shift: Move faster
        /// </summary>

        [Header("Movement")]
        public float moveSpeed = 50f;
        public float fastSpeedMultiplier = 3f;
        public float rotationSpeed = 100f;

        [Header("Mouse Look (Optional)")]
        public bool enableMouseLook = true;
        public float mouseSensitivity = 3f;

        private float currentSpeed;
        private float rotationX = 0f;
        private float rotationY = 0f;

        void Start()
        {
            currentSpeed = moveSpeed;
            
            // Initialize rotation from current camera rotation
            Vector3 rot = transform.localEulerAngles;
            rotationY = rot.y;
            rotationX = rot.x;
        }

        void Update()
        {
            HandleMovement();
            HandleRotation();
            HandleSpeedAdjustment();
        }

        void HandleMovement()
        {
            // Get speed multiplier
            float speed = currentSpeed;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                speed *= fastSpeedMultiplier;

            // Calculate movement direction
            Vector3 move = Vector3.zero;

            // WASD movement (relative to camera direction)
            if (Input.GetKey(KeyCode.W)) move += transform.forward;
            if (Input.GetKey(KeyCode.S)) move -= transform.forward;
            if (Input.GetKey(KeyCode.A)) move -= transform.right;
            if (Input.GetKey(KeyCode.D)) move += transform.right;

            // Q/E for up/down (world space)
            if (Input.GetKey(KeyCode.E)) move += Vector3.up;
            if (Input.GetKey(KeyCode.Q)) move -= Vector3.up;

            // Apply movement
            transform.position += move.normalized * speed * Time.deltaTime;
        }

        void HandleRotation()
        {
            // Arrow key rotation
            if (Input.GetKey(KeyCode.UpArrow))    rotationX -= rotationSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.DownArrow))  rotationX += rotationSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.LeftArrow))  rotationY -= rotationSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.RightArrow)) rotationY += rotationSpeed * Time.deltaTime;

            // Mouse look (right-click + drag)
            if (enableMouseLook && Input.GetMouseButton(1))
            {
                rotationY += Input.GetAxis("Mouse X") * mouseSensitivity;
                rotationX -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            }

            // Clamp vertical rotation
            rotationX = Mathf.Clamp(rotationX, -90f, 90f);

            // Apply rotation
            transform.localEulerAngles = new Vector3(rotationX, rotationY, 0);
        }

        void HandleSpeedAdjustment()
        {
            // Scroll wheel to adjust speed
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                currentSpeed += scroll * 10f;
                currentSpeed = Mathf.Clamp(currentSpeed, 1f, 200f);
            }
        }
    }
}
