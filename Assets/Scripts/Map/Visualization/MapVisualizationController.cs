using UnityEngine;
using Icefall.Map.Core;

namespace Icefall.Map.Visualization
{
    /// <summary>
    /// Главный контроллер визуализации карты
    /// Управляет отрисовкой всех чанков и оптимизацией
    /// </summary>
    public class MapVisualizationController
    {
        private MapData mapData;
        private ChunkVisualizer chunkVisualizer;
        private Transform mapRoot;

        // Материалы
        private Material[] terrainMaterials;
        private Material buildingMaterial;

        // Настройки визуализации
        private int renderDistance = 5;  // Количество чанков вокруг камеры для отрисовки

        public MapVisualizationController(MapData mapData)
        {
            this.mapData = mapData;
        }

        /// <summary>
        /// Инициализирует визуализацию
        /// </summary>
        public void InitializeVisualization()
        {
            Debug.Log("MapVisualizationController: Initializing visualization...");

            // Создаём корневой объект для карты
            CreateMapRoot();

            // Создаём материалы
            CreateMaterials();

            // Инициализируем визуализатор чанков
            chunkVisualizer = new ChunkVisualizer(terrainMaterials, buildingMaterial, mapData);

            // Визуализируем все чанки (или только видимые при большой карте)
            VisualizeAllChunks();

            Debug.Log("MapVisualizationController: Visualization initialized");
        }

        /// <summary>
        /// Создаёт корневой GameObject для карты
        /// </summary>
        private void CreateMapRoot()
        {
            var rootObj = GameObject.Find("MapRoot");
            if (rootObj == null)
            {
                rootObj = new GameObject("MapRoot");
                rootObj.transform.position = Vector3.zero;
            }
            mapRoot = rootObj.transform;
        }

        /// <summary>
        /// Создаёт базовые материалы для визуализации
        /// </summary>
        private void CreateMaterials()
        {
            terrainMaterials = new Material[1];
            terrainMaterials[0] = CreateVertexColorMaterial("TerrainMaterial");
            buildingMaterial = CreateVertexColorMaterial("BuildingMaterial");
        }

        /// <summary>
        /// Создаёт материал с поддержкой vertex colors
        /// </summary>
        private Material CreateVertexColorMaterial(string name)
        {
            // Используем Sprites/Default shader - он поддерживает vertex colors
            Shader shader = Shader.Find("Sprites/Default");
            
            if (shader == null)
            {
                shader = Shader.Find("Standard");
                Debug.LogWarning("MapVisualizationController: Sprites/Default not found, using Standard");
            }

            var material = new Material(shader);
            material.name = name;
            material.color = Color.white;
            
            return material;
        }

        /// <summary>
        /// Визуализирует все чанки карты
        /// </summary>
        public void VisualizeAllChunks()
        {
            var chunks = mapData.GetAllChunks();

            int visualizedCount = 0;
            for (int cx = 0; cx < MapData.CHUNKS_X; cx++)
            {
                for (int cy = 0; cy < MapData.CHUNKS_Y; cy++)
                {
                    var chunk = chunks[cx, cy];
                    if (chunk != null)
                    {
                        chunkVisualizer.VisualizeChunk(chunk, mapRoot);
                        visualizedCount++;
                    }
                }
            }

            Debug.Log($"MapVisualizationController: Visualized {visualizedCount} chunks");
        }

        /// <summary>
        /// Обновляет визуализацию конкретного чанка
        /// </summary>
        public void UpdateChunkVisuals(int chunkX, int chunkY)
        {
            var chunk = mapData.GetChunk(chunkX, chunkY);
            if (chunk != null)
            {
                chunkVisualizer.VisualizeChunk(chunk, mapRoot);
            }
        }

        /// <summary>
        /// Обновляет все чанки, помеченные для обновления
        /// </summary>
        public void UpdateDirtyChunks()
        {
            var chunks = mapData.GetAllChunks();
            int updatedCount = 0;

            for (int cx = 0; cx < MapData.CHUNKS_X; cx++)
            {
                for (int cy = 0; cy < MapData.CHUNKS_Y; cy++)
                {
                    var chunk = chunks[cx, cy];
                    if (chunk != null && chunk.NeedsVisualizationUpdate)
                    {
                        chunkVisualizer.VisualizeChunk(chunk, mapRoot);
                        updatedCount++;
                    }
                }
            }

            if (updatedCount > 0)
            {
                Debug.Log($"MapVisualizationController: Updated {updatedCount} dirty chunks");
            }
        }

        /// <summary>
        /// Очищает всю визуализацию
        /// </summary>
        public void ClearVisualization()
        {
            if (mapRoot != null)
            {
                Object.Destroy(mapRoot.gameObject);
                mapRoot = null;
            }

            Debug.Log("MapVisualizationController: Visualization cleared");
        }

        /// <summary>
        /// Обновляет визуализацию на основе позиции камеры (для оптимизации)
        /// Загружает только видимые чанки
        /// </summary>
        public void UpdateVisualizationBasedOnCamera(Vector3 cameraPosition)
        {
            // Вычисляем чанк, в котором находится камера
            int cameraCellX = Mathf.FloorToInt(cameraPosition.x);
            int cameraCellY = Mathf.FloorToInt(cameraPosition.z);
            
            int cameraChunkX = cameraCellX / MapData.CHUNK_SIZE;
            int cameraChunkY = cameraCellY / MapData.CHUNK_SIZE;

            var chunks = mapData.GetAllChunks();

            // Выгружаем далёкие чанки и загружаем близкие
            for (int cx = 0; cx < MapData.CHUNKS_X; cx++)
            {
                for (int cy = 0; cy < MapData.CHUNKS_Y; cy++)
                {
                    var chunk = chunks[cx, cy];
                    if (chunk == null) continue;

                    int distance = Mathf.Max(Mathf.Abs(cx - cameraChunkX), Mathf.Abs(cy - cameraChunkY));

                    if (distance <= renderDistance)
                    {
                        // Загружаем чанк, если он не загружен
                        if (!chunk.IsLoaded)
                        {
                            chunkVisualizer.VisualizeChunk(chunk, mapRoot);
                        }
                    }
                    else
                    {
                        // Выгружаем чанк, если он загружен
                        if (chunk.IsLoaded)
                        {
                            chunkVisualizer.DestroyChunkVisualization(chunk);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Устанавливает дистанцию рендеринга (в чанках)
        /// </summary>
        public void SetRenderDistance(int distance)
        {
            renderDistance = Mathf.Max(1, distance);
            Debug.Log($"MapVisualizationController: Render distance set to {renderDistance} chunks");
        }

        /// <summary>
        /// Возвращает корневой Transform карты
        /// </summary>
        public Transform GetMapRoot()
        {
            return mapRoot;
        }

        /// <summary>
        /// Возвращает статистику визуализации
        /// </summary>
        public string GetVisualizationStats()
        {
            int loadedChunks = 0;
            var chunks = mapData.GetAllChunks();

            for (int cx = 0; cx < MapData.CHUNKS_X; cx++)
            {
                for (int cy = 0; cy < MapData.CHUNKS_Y; cy++)
                {
                    if (chunks[cx, cy]?.IsLoaded == true)
                        loadedChunks++;
                }
            }

            return $"Loaded chunks: {loadedChunks}/{MapData.TOTAL_CHUNKS}, Render distance: {renderDistance}";
        }
    }
}