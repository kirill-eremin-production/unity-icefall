using UnityEngine;
using UnityEngine.InputSystem;

namespace Icefall.Controllers
{
    /// <summary>
    /// Контроллер камеры для навигации по карте
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private float cameraMoveSpeed = 100f;
        [SerializeField] private float cameraRotateSpeed = 100f;
        [SerializeField] private float cameraZoomSpeed = 500f;
        [SerializeField] private float shiftSpeedMultiplier = 6f;
        [SerializeField] private float mouseDragSpeed = 100f;
        [SerializeField] private float zoomSmoothness = 10f;

        [Header("Camera Bounds")]
        [SerializeField] private bool useBounds = true;
        [SerializeField] private float minX = -100f;
        [SerializeField] private float maxX = 1100f;
        [SerializeField] private float minZ = -100f;
        [SerializeField] private float maxZ = 1100f;
        [SerializeField] private float minY = 20f;
        [SerializeField] private float maxY = 300f;

        private UnityEngine.Camera mainCamera;
        private Vector3 lastMousePosition;
        private bool isDragging = false;

        private void Start()
        {
            mainCamera = UnityEngine.Camera.main;
            
            if (mainCamera == null)
            {
                Debug.LogError("CameraController: No main camera found!");
                return;
            }

            SetupCamera();
        }

        /// <summary>
        /// Устанавливает начальную позицию камеры над центром карты
        /// </summary>
        public void SetupCamera()
        {
            if (mainCamera == null) return;

            // Позиционируем камеру над центром карты
            float centerX = Map.Core.MapData.MAP_WIDTH * 0.5f;
            float centerZ = Map.Core.MapData.MAP_HEIGHT * 0.5f;
            
            mainCamera.transform.position = new Vector3(centerX, 100f, centerZ - 50f);
            mainCamera.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
        }

        /// <summary>
        /// Фокусирует камеру на указанной позиции
        /// </summary>
        public void FocusOnPosition(Vector3 position, float height = 100f)
        {
            if (mainCamera == null) return;

            mainCamera.transform.position = new Vector3(position.x, height, position.z - 50f);
        }

        private void Update()
        {
            if (mainCamera == null) return;

            HandleCameraMovement();
        }

        private void HandleCameraMovement()
        {
            // Получаем устройства ввода
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if (keyboard == null || mouse == null) return;

            // WASD для движения камеры относительно направления взгляда
            Vector3 movement = Vector3.zero;
            
            if (keyboard.wKey.isPressed) movement += mainCamera.transform.forward;
            if (keyboard.sKey.isPressed) movement -= mainCamera.transform.forward;
            if (keyboard.aKey.isPressed) movement -= mainCamera.transform.right;
            if (keyboard.dKey.isPressed) movement += mainCamera.transform.right;

            if (movement.magnitude > 0)
            {
                // Проецируем движение на горизонтальную плоскость (игнорируем Y)
                movement.y = 0;
                
                // Применяем ускорение при зажатом SHIFT
                float currentSpeed = cameraMoveSpeed;
                if (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed)
                {
                    currentSpeed *= shiftSpeedMultiplier;
                }
                
                movement = movement.normalized * currentSpeed * Time.deltaTime;
                mainCamera.transform.position += movement;
            }

            // Перетаскивание карты левой клавишей мыши
            if (mouse.leftButton.wasPressedThisFrame)
            {
                isDragging = true;
                lastMousePosition = mouse.position.ReadValue();
            }
            else if (mouse.leftButton.wasReleasedThisFrame)
            {
                isDragging = false;
            }

            if (isDragging && mouse.leftButton.isPressed)
            {
                Vector3 currentMousePosition = mouse.position.ReadValue();
                Vector3 delta = currentMousePosition - lastMousePosition;
                
                // Конвертируем движение мыши в движение камеры
                Vector3 dragMovement = (-mainCamera.transform.right * delta.x - mainCamera.transform.forward * delta.y) * mouseDragSpeed * Time.deltaTime;
                dragMovement.y = 0; // Двигаем только по горизонтали
                
                mainCamera.transform.position += dragMovement;
                lastMousePosition = currentMousePosition;
            }

            // Q/E для вращения камеры
            if (keyboard.qKey.isPressed)
            {
                mainCamera.transform.Rotate(Vector3.up, -cameraRotateSpeed * Time.deltaTime, Space.World);
            }
            if (keyboard.eKey.isPressed)
            {
                mainCamera.transform.Rotate(Vector3.up, cameraRotateSpeed * Time.deltaTime, Space.World);
            }

            // Mouse Scroll для плавного зума к позиции курсора
            float scroll = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                // Raycast к карте под курсором
                Ray ray = mainCamera.ScreenPointToRay(mouse.position.ReadValue());
                if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
                {
                    // Вычисляем направление к точке под курсором
                    Vector3 targetPoint = hit.point;
                    Vector3 direction = (targetPoint - mainCamera.transform.position).normalized;
                    
                    // Плавно приближаемся к точке
                    float zoomAmount = scroll * cameraZoomSpeed * 0.1f;
                    mainCamera.transform.position += direction * zoomAmount * zoomSmoothness;
                }
                else
                {
                    // Если не попали в коллайдер, зумим как раньше
                    mainCamera.transform.Translate(Vector3.forward * scroll * cameraZoomSpeed * 0.01f, Space.Self);
                }
            }

            // Применяем границы камеры
            if (useBounds)
            {
                ApplyCameraBounds();
            }
        }

        private void ApplyCameraBounds()
        {
            Vector3 pos = mainCamera.transform.position;
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
            mainCamera.transform.position = pos;
        }

        /// <summary>
        /// Получает текущую камеру
        /// </summary>
        public UnityEngine.Camera GetCamera() => mainCamera;
    }
}