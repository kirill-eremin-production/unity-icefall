using UnityEngine;
using UnityEngine.InputSystem;
using Icefall.Map.Core;
using Icefall.Map.Placement;

namespace Icefall.Map
{
    /// <summary>
    /// Контроллер строительства зданий
    /// Управляет выбором, предпросмотром и размещением зданий
    /// </summary>
    public class BuildingController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private BuildingPreview buildingPreview;
        
        [Header("Input")]
        [SerializeField] private InputActionReference pointerPositionAction;
        [SerializeField] private InputActionReference clickAction;
        [SerializeField] private InputActionReference cancelAction;
        
        [Header("Settings")]
        [SerializeField] private LayerMask terrainLayer;
        [SerializeField] private float raycastDistance = 1000f;

        // Состояние
        private BuildingType? selectedBuildingType = null;
        private Vector2Int? previewPosition = null;
        private bool isPlacementMode = false;

        // События
        public System.Action<BuildingType> OnBuildingTypeSelected;
        public System.Action OnBuildingPlaced;
        public System.Action OnPlacementCancelled;

        [Header("Debug")]
        [SerializeField] private bool verboseLogs = false;

        private void LogV(string message)
        {
            if (verboseLogs)
                Debug.Log(message);
        }

        private void Awake()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;
        }

        private void OnEnable()
        {
            // Активируем все Input Actions
            if (pointerPositionAction != null)
            {
                pointerPositionAction.action.Enable();
                LogV("BuildingController: PointerPosition action enabled");
            }
            else
            {
                Debug.LogError("BuildingController: PointerPosition action is NULL!");
            }
            
            if (clickAction != null)
            {
                clickAction.action.Enable();
                clickAction.action.performed += OnClickPerformed;
                LogV("BuildingController: Click action enabled");
            }
            else
            {
                Debug.LogError("BuildingController: Click action is NULL!");
            }
            
            if (cancelAction != null)
            {
                cancelAction.action.Enable();
                cancelAction.action.performed += OnCancelPerformed;
                LogV("BuildingController: Cancel action enabled");
            }
            else
            {
                Debug.LogWarning("BuildingController: Cancel action is NULL");
            }
        }

        private void OnDisable()
        {
            // Деактивируем все Input Actions
            if (pointerPositionAction != null)
                pointerPositionAction.action.Disable();
            
            if (clickAction != null)
            {
                clickAction.action.performed -= OnClickPerformed;
                clickAction.action.Disable();
            }
            
            if (cancelAction != null)
            {
                cancelAction.action.performed -= OnCancelPerformed;
                cancelAction.action.Disable();
            }
        }

        private void Update()
        {
            if (!isPlacementMode || !selectedBuildingType.HasValue)
                return;

            UpdatePreview();
        }

        /// <summary>
        /// Выбирает тип здания для размещения
        /// </summary>
        public void SelectBuilding(BuildingType buildingType)
        {
            selectedBuildingType = buildingType;
            isPlacementMode = true;

            LogV($"BuildingController: SelectBuilding called - Type: {buildingType}, Preview: {(buildingPreview != null ? "OK" : "NULL")}");

            if (buildingPreview != null)
            {
                buildingPreview.SetBuildingType(buildingType);
                buildingPreview.Show();
                LogV($"BuildingController: Preview shown for {buildingType}");
            }
            else
            {
                Debug.LogError("BuildingController: BuildingPreview is NULL!");
            }

            OnBuildingTypeSelected?.Invoke(buildingType);
        }

        /// <summary>
        /// Отменяет режим размещения
        /// </summary>
        public void CancelPlacement()
        {
            isPlacementMode = false;
            selectedBuildingType = null;
            previewPosition = null;

            if (buildingPreview != null)
                buildingPreview.Hide();

            OnPlacementCancelled?.Invoke();
            LogV("BuildingController: Placement cancelled");
        }

        /// <summary>
        /// Обновляет предпросмотр здания
        /// </summary>
        private void UpdatePreview()
        {
            if (pointerPositionAction == null)
            {
                Debug.LogWarning("BuildingController: pointerPositionAction is NULL in UpdatePreview");
                return;
            }

            if (buildingPreview == null)
            {
                Debug.LogWarning("BuildingController: buildingPreview is NULL in UpdatePreview");
                return;
            }

            if (mainCamera == null)
            {
                Debug.LogWarning("BuildingController: mainCamera is NULL in UpdatePreview");
                return;
            }

            Vector2 screenPosition = pointerPositionAction.action.ReadValue<Vector2>();
            Ray ray = mainCamera.ScreenPointToRay(screenPosition);

            // Debug ray visualization
            Debug.DrawRay(ray.origin, ray.direction * raycastDistance, Color.red, 0.1f);

            // Детальное логирование раз в секунду
            if (Time.frameCount % 60 == 0)
            {
                LogV($"BuildingController Raycast Debug:\n" +
                         $"  Screen Pos: {screenPosition}\n" +
                         $"  Camera Pos: {mainCamera.transform.position}\n" +
                         $"  Camera Forward: {mainCamera.transform.forward}\n" +
                         $"  Ray Origin: {ray.origin}\n" +
                         $"  Ray Direction: {ray.direction}\n" +
                         $"  Distance: {raycastDistance}\n" +
                         $"  LayerMask Value: {terrainLayer.value}");
                
                // Проверяем попадание в любой collider (без layer mask)
                if (Physics.Raycast(ray, out RaycastHit anyHit, raycastDistance))
                {
                    LogV($"  → Hit SOMETHING: {anyHit.collider.gameObject.name} at {anyHit.point}, layer: {anyHit.collider.gameObject.layer}");
                }
                else
                {
                    LogV("  → Hit NOTHING (даже без layer mask!)");
                }
            }

            // 1) Пытаемся попасть лучом по слою террейна
            if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, terrainLayer))
            {
                Vector2Int mapPosition = WorldToMapPosition(hit.point);
                if (previewPosition != mapPosition)
                {
                    previewPosition = mapPosition;
                    LogV($"BuildingController: Raycast hit at world: {hit.point}, map: {mapPosition}");
                    UpdatePreviewVisuals(mapPosition);
                }
            }
            else
            {
                // 2) Fallback: пробуем без layer mask (вдруг слой настроен неправильно)
                if (Physics.Raycast(ray, out RaycastHit anyHitNoMask, raycastDistance))
                {
                    Vector2Int mapPosition = WorldToMapPosition(anyHitNoMask.point);
                    if (previewPosition != mapPosition)
                    {
                        previewPosition = mapPosition;
                        if (Time.frameCount % 60 == 0)
                            LogV($"BuildingController: Fallback raycast (no mask) hit at {anyHitNoMask.point}, map: {mapPosition}, object: {anyHitNoMask.collider.gameObject.name}, layer: {anyHitNoMask.collider.gameObject.layer}");
                        UpdatePreviewVisuals(mapPosition);
                    }
                }
                else
                {
                    // 3) Последний Fallback: пересечение с плоскостью Y=0, чтобы предпросмотр всё равно следовал курсору
                    Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
                    if (groundPlane.Raycast(ray, out float t))
                    {
                        Vector3 groundPoint = ray.GetPoint(t);
                        Vector2Int mapPosition = WorldToMapPosition(groundPoint);
                        if (previewPosition != mapPosition)
                        {
                            previewPosition = mapPosition;
                            if (Time.frameCount % 60 == 0)
                                LogV($"BuildingController: Using ground plane fallback at world: {groundPoint}, map: {mapPosition}");
                            UpdatePreviewVisuals(mapPosition);
                        }
                    }
                    else
                    {
                        if (Time.frameCount % 60 == 0) // Логируем раз в секунду
                        {
                            LogV($"BuildingController: Raycast MISSED with layer mask {terrainLayer.value}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Обновляет визуализацию предпросмотра
        /// </summary>
        private void UpdatePreviewVisuals(Vector2Int mapPosition)
        {
            if (!selectedBuildingType.HasValue || buildingPreview == null)
                return;

            // Проверяем возможность размещения
            bool canPlace = MapSystem.Instance.CanPlaceBuilding(selectedBuildingType.Value, mapPosition.x, mapPosition.y);

            // Обновляем позицию и состояние предпросмотра
            buildingPreview.UpdatePosition(mapPosition, canPlace);
        }

        /// <summary>
        /// Обработчик клика мыши
        /// </summary>
        private void OnClickPerformed(InputAction.CallbackContext context)
        {
            if (!isPlacementMode || !selectedBuildingType.HasValue || !previewPosition.HasValue)
                return;

            TryPlaceBuilding();
        }

        /// <summary>
        /// Обработчик отмены
        /// </summary>
        private void OnCancelPerformed(InputAction.CallbackContext context)
        {
            if (isPlacementMode)
                CancelPlacement();
        }

        /// <summary>
        /// Пытается разместить здание
        /// </summary>
        private void TryPlaceBuilding()
        {
            if (!selectedBuildingType.HasValue || !previewPosition.HasValue)
                return;

            var buildingType = selectedBuildingType.Value;
            var position = previewPosition.Value;

            // Проверяем возможность размещения
            if (!MapSystem.Instance.CanPlaceBuilding(buildingType, position.x, position.y))
            {
                Debug.LogWarning($"BuildingController: Cannot place {buildingType} at ({position.x}, {position.y})");
                return;
            }

            // Размещаем здание
            var building = MapSystem.Instance.PlaceBuilding(buildingType, position.x, position.y);
            
            if (building != null)
            {
                LogV($"BuildingController: Placed {buildingType} at ({position.x}, {position.y})");
                OnBuildingPlaced?.Invoke();
                
                // Продолжаем режим размещения (можно разместить еще одно здание того же типа)
                // Если нужно выйти из режима после размещения, раскомментируйте:
                // CancelPlacement();
            }
        }

        /// <summary>
        /// Преобразует мировые координаты в координаты карты
        /// </summary>
        private Vector2Int WorldToMapPosition(Vector3 worldPosition)
        {
            // Предполагаем, что мировые координаты соответствуют координатам карты
            // Можно настроить масштаб если нужно
            int x = Mathf.RoundToInt(worldPosition.x);
            int z = Mathf.RoundToInt(worldPosition.z);
            
            // Ограничиваем границами карты
            x = Mathf.Clamp(x, 0, MapData.MAP_WIDTH - 1);
            z = Mathf.Clamp(z, 0, MapData.MAP_HEIGHT - 1);
            
            return new Vector2Int(x, z);
        }

        /// <summary>
        /// Возвращает true если сейчас активен режим размещения
        /// </summary>
        public bool IsInPlacementMode => isPlacementMode;

        /// <summary>
        /// Возвращает текущий выбранный тип здания
        /// </summary>
        public BuildingType? SelectedBuildingType => selectedBuildingType;
    }
}