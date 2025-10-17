using UnityEngine;
using Icefall.Map.Core;

namespace Icefall.Map
{
    /// <summary>
    /// Визуальный предпросмотр здания при размещении
    /// Отображает прозрачное здание, которое следует за курсором
    /// </summary>
    public class BuildingPreview : MonoBehaviour
    {
        [Header("Visual Settings")]
        [SerializeField] private Material validPlacementMaterial;
        [SerializeField] private Material invalidPlacementMaterial;
        [SerializeField] private float heightOffset = 0.1f;
        
        [Header("Prefabs")]
        [SerializeField] private GameObject defaultBuildingPrefab;

        // Текущий визуальный объект
        private GameObject currentPreviewObject;
        private MeshRenderer[] previewRenderers;
        
        // Состояние
        private BuildingType currentBuildingType;
        private Vector2Int currentSize;
        private bool isVisible = false;
        private bool canPlace = false;

        [Header("Debug")]
        [SerializeField] private bool verboseLogs = false;

        private void LogV(string message)
        {
            if (verboseLogs)
                Debug.Log(message);
        }

        private void Awake()
        {
            // Создаем материалы если они не назначены
            if (validPlacementMaterial == null)
            {
                validPlacementMaterial = CreatePreviewMaterial(new Color(0, 1, 0, 0.5f));
            }

            if (invalidPlacementMaterial == null)
            {
                invalidPlacementMaterial = CreatePreviewMaterial(new Color(1, 0, 0, 0.5f));
            }
        }

        /// <summary>
        /// Устанавливает тип здания для предпросмотра
        /// </summary>
        public void SetBuildingType(BuildingType buildingType)
        {
            currentBuildingType = buildingType;
            currentSize = BuildingDefaults.GetDefaultSize(buildingType);
            
            LogV($"BuildingPreview: SetBuildingType called - Type: {buildingType}, Size: {currentSize}");
            
            // Пересоздаем визуальный объект
            CreatePreviewObject();
        }

        /// <summary>
        /// Обновляет позицию предпросмотра
        /// </summary>
        public void UpdatePosition(Vector2Int mapPosition, bool canPlaceHere)
        {
            if (currentPreviewObject == null)
            {
                Debug.LogWarning("BuildingPreview: currentPreviewObject is NULL in UpdatePosition");
                return;
            }

            canPlace = canPlaceHere;

            // Центр по XZ согласно размеру здания
            Vector3 centerXZ = MapToWorldPosition(mapPosition);
            float targetY = centerXZ.y + heightOffset;

            // Пробуем определить точную высоту террейна лучом сверху вниз
            Vector3 rayOrigin = new Vector3(centerXZ.x, 1000f, centerXZ.z);
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 2000f))
            {
                // Ставим центр куба чуть выше поверхности (0.5f = половина высоты предпросмотра)
                float epsilon = Mathf.Max(heightOffset, 0.02f);
                targetY = hit.point.y + 0.5f + epsilon;
            }

            Vector3 worldPosition = new Vector3(centerXZ.x, targetY, centerXZ.z);
            currentPreviewObject.transform.position = worldPosition;

            LogV($"BuildingPreview: Updated position - Map: {mapPosition}, World: {worldPosition}, CanPlace: {canPlaceHere}");

            // Обновляем материал в зависимости от валидности размещения
            UpdateMaterial();
        }

        /// <summary>
        /// Показывает предпросмотр
        /// </summary>
        public void Show()
        {
            isVisible = true;
            
            if (currentPreviewObject != null)
            {
                currentPreviewObject.SetActive(true);
                LogV($"BuildingPreview: Show() - Object activated at position: {currentPreviewObject.transform.position}");
            }
            else
            {
                Debug.LogError("BuildingPreview: Show() called but currentPreviewObject is NULL!");
            }
        }

        /// <summary>
        /// Скрывает предпросмотр
        /// </summary>
        public void Hide()
        {
            isVisible = false;
            if (currentPreviewObject != null)
                currentPreviewObject.SetActive(false);
        }

        /// <summary>
        /// Создает визуальный объект предпросмотра
        /// </summary>
        private void CreatePreviewObject()
        {
            // Удаляем старый объект если есть
            if (currentPreviewObject != null)
            {
                LogV("BuildingPreview: Destroying old preview object");
                Destroy(currentPreviewObject);
            }

            // Создаем простой куб для предпросмотра
            currentPreviewObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            currentPreviewObject.name = $"BuildingPreview_{currentBuildingType}";
            currentPreviewObject.transform.SetParent(transform);
            
            LogV($"BuildingPreview: Created preview cube - Name: {currentPreviewObject.name}, Parent: {transform.name}");
            
            // Настраиваем размер
            currentPreviewObject.transform.localScale = new Vector3(
                currentSize.x,
                1f, // высота
                currentSize.y
            );
            
            // Удаляем коллайдер (не нужен для предпросмотра)
            var collider = currentPreviewObject.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);
            
            // Получаем рендереры
            previewRenderers = currentPreviewObject.GetComponentsInChildren<MeshRenderer>();
            LogV($"BuildingPreview: Found {previewRenderers.Length} renderers");
            
            // Применяем материал
            UpdateMaterial();
            
            // Скрываем если нужно
            if (!isVisible)
            {
                currentPreviewObject.SetActive(false);
                LogV("BuildingPreview: Preview object created but hidden (isVisible=false)");
            }
            else
            {
                LogV("BuildingPreview: Preview object created and visible");
            }
        }

        /// <summary>
        /// Обновляет материал предпросмотра
        /// </summary>
        private void UpdateMaterial()
        {
            if (previewRenderers == null || previewRenderers.Length == 0)
                return;

            Material materialToUse = canPlace ? validPlacementMaterial : invalidPlacementMaterial;
            
            foreach (var renderer in previewRenderers)
            {
                renderer.material = materialToUse;
            }
        }

        /// <summary>
        /// Преобразует координаты карты в мировые координаты
        /// </summary>
        private Vector3 MapToWorldPosition(Vector2Int mapPosition)
        {
            // Центрируем позицию в середине занимаемой области
            float centerX = mapPosition.x + currentSize.x * 0.5f;
            float centerZ = mapPosition.y + currentSize.y * 0.5f;

            // Базовая высота = середина предпросмотра, offset добавляется после raycast
            return new Vector3(centerX, 0.5f, centerZ);
        }

        /// <summary>
        /// Создает прозрачный материал для предпросмотра
        /// </summary>
        private Material CreatePreviewMaterial(Color color)
        {
            // Используем стандартный шейдер с прозрачностью
            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetColor("_BaseColor", color);
            
            // Настраиваем прозрачность
            material.SetFloat("_Surface", 1); // Transparent
            material.SetFloat("_Blend", 0); // Alpha
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);

            // Чуть выше стандартной прозрачности, чтобы уменьшить артефакты порядка рисования
            material.renderQueue = 4000;
            
            return material;
        }

        private void OnDestroy()
        {
            if (currentPreviewObject != null)
                Destroy(currentPreviewObject);
        }
    }
}