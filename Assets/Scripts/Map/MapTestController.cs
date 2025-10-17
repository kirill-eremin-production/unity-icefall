using UnityEngine;
using UnityEngine.InputSystem;
using Icefall.Map.Core;

namespace Icefall.Map
{
    /// <summary>
    /// Тестовый контроллер для проверки системы карты
    /// Добавьте этот скрипт на GameObject в сцене для тестирования
    /// </summary>
    public class MapTestController : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool placeTestBuildings = true;
        [SerializeField] private int testBuildingCount = 5;

        [Header("Camera Settings")]
        [SerializeField] private float cameraMoveSpeed = 50f;
        [SerializeField] private float cameraRotateSpeed = 100f;
        [SerializeField] private float cameraZoomSpeed = 50f;
        [SerializeField] private float shiftSpeedMultiplier = 2.5f;
        [SerializeField] private float mouseDragSpeed = 1f;
        [SerializeField] private float zoomSmoothness = 10f;

        private Camera mainCamera;
        private Vector3 lastMousePosition;
        private bool isDragging = false;

        private void Start()
        {
            mainCamera = Camera.main;
            
            if (mainCamera == null)
            {
                Debug.LogError("MapTestController: No main camera found!");
                return;
            }

            // Позиционируем камеру над картой
            SetupCamera();

            // Ждём инициализации MapSystem
            Invoke(nameof(RunTests), 0.5f);
        }

        private void SetupCamera()
        {
            if (mainCamera == null) return;

            // Позиционируем камеру над центром карты
            float centerX = MapData.MAP_WIDTH * 0.5f;
            float centerZ = MapData.MAP_HEIGHT * 0.5f;
            
            mainCamera.transform.position = new Vector3(centerX, 100f, centerZ - 50f);
            mainCamera.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
        }

        private void RunTests()
        {
            if (!MapSystem.Instance.IsInitialized)
            {
                Debug.LogError("MapTestController: MapSystem not initialized!");
                return;
            }

            Debug.Log("=== MapTestController: Running Tests ===");

            // Тест 1: Проверяем статистику карты
            MapSystem.Instance.LogMapStats();

            // Тест 2: Размещаем тестовые здания
            if (placeTestBuildings)
            {
                PlaceTestBuildings();
            }

            // Тест 3: Логируем финальную статистику
            MapSystem.Instance.LogMapStats();

            Debug.Log("=== MapTestController: Tests Complete ===");
        }

        private void PlaceTestBuildings()
        {
            Debug.Log($"MapTestController: Placing {testBuildingCount} test buildings...");

            int placed = 0;
            int attempts = 0;
            int maxAttempts = testBuildingCount * 10;

            var buildingTypes = System.Enum.GetValues(typeof(BuildingType));

            while (placed < testBuildingCount && attempts < maxAttempts)
            {
                attempts++;

                // Случайная позиция в центральной части карты
                int x = Random.Range(400, 600);
                int y = Random.Range(400, 600);

                // Случайный тип здания
                BuildingType type = (BuildingType)buildingTypes.GetValue(Random.Range(0, buildingTypes.Length));

                // Пытаемся разместить
                if (MapSystem.Instance.CanPlaceBuilding(type, x, y))
                {
                    var building = MapSystem.Instance.PlaceBuilding(type, x, y);
                    if (building != null)
                    {
                        Debug.Log($"MapTestController: Placed test building #{placed + 1}: {building}");
                        placed++;
                    }
                }
            }

            Debug.Log($"MapTestController: Successfully placed {placed}/{testBuildingCount} buildings in {attempts} attempts");
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

            // Space для размещения случайного здания
            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                PlaceRandomBuilding();
            }

            // R для регенерации карты
            if (keyboard.rKey.wasPressedThisFrame)
            {
                Debug.Log("MapTestController: Regenerating map...");
                MapSystem.Instance.RegenerateMap();
            }

            // L для логирования статистики
            if (keyboard.lKey.wasPressedThisFrame)
            {
                MapSystem.Instance.LogMapStats();
            }
        }

        private void PlaceRandomBuilding()
        {
            // Позиция перед камерой
            Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                int x = Mathf.FloorToInt(hit.point.x);
                int z = Mathf.FloorToInt(hit.point.z);

                var buildingTypes = System.Enum.GetValues(typeof(BuildingType));
                BuildingType type = (BuildingType)buildingTypes.GetValue(Random.Range(0, buildingTypes.Length));

                if (MapSystem.Instance.CanPlaceBuilding(type, x, z))
                {
                    var building = MapSystem.Instance.PlaceBuilding(type, x, z);
                    Debug.Log($"MapTestController: Placed building at ({x}, {z}): {building}");
                }
                else
                {
                    Debug.LogWarning($"MapTestController: Cannot place building at ({x}, {z})");
                }
            }
        }

        private void OnGUI()
        {
            // Простой UI для инструкций
            GUILayout.BeginArea(new Rect(10, 10, 350, 280));
            GUILayout.Box("Map Test Controller");
            GUILayout.Label("WASD - Move camera (SHIFT - faster)");
            GUILayout.Label("Q/E - Rotate camera");
            GUILayout.Label("Mouse Drag (LMB) - Pan camera");
            GUILayout.Label("Mouse Scroll - Zoom to cursor");
            GUILayout.Label("Space - Place random building");
            GUILayout.Label("R - Regenerate map");
            GUILayout.Label("L - Log statistics");
            GUILayout.Space(10);
            
            if (MapSystem.Instance != null && MapSystem.Instance.IsInitialized)
            {
                GUILayout.Label($"Buildings: {MapSystem.Instance.PlacementManager.GetBuildingCount()}");
            }
            
            GUILayout.EndArea();
        }
    }
}