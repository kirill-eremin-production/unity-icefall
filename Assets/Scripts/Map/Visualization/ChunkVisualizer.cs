using System.Collections.Generic;
using UnityEngine;
using Icefall.Map.Core;

namespace Icefall.Map.Visualization
{
    /// <summary>
    /// Визуализатор чанка - создаёт процедурный mesh для чанка
    /// Оптимизировано для WebGL: один mesh на чанк вместо тысяч отдельных объектов
    /// </summary>
    public class ChunkVisualizer
    {
        private const float CELL_SIZE = 1f;           // Размер одной ячейки в Unity единицах
        private const float HEIGHT_MULTIPLIER = 0.5f; // Множитель для высоты терраина

        private Material[] terrainMaterials;          // Материалы для разных типов терраина
        private Material buildingMaterial;            // Материал для зданий

        private MapData mapData;

        // Debug
        private bool verboseLogs = false;
        private void LogV(string message)
        {
            if (verboseLogs)
                Debug.Log(message);
        }

        public ChunkVisualizer(Material[] terrainMaterials, Material buildingMaterial, MapData mapData)
        {
            this.terrainMaterials = terrainMaterials;
            this.buildingMaterial = buildingMaterial;
            this.mapData = mapData;
        }

        public ChunkVisualizer(Material[] terrainMaterials, Material buildingMaterial, MapData mapData, bool verboseLogs)
        {
            this.terrainMaterials = terrainMaterials;
            this.buildingMaterial = buildingMaterial;
            this.mapData = mapData;
            this.verboseLogs = verboseLogs;
        }

        /// <summary>
        /// Создаёт или обновляет визуализацию чанка
        /// </summary>
        public void VisualizeChunk(MapChunk chunk, Transform parent)
        {
            // Если GameObject уже существует, обновляем его
            if (chunk.ChunkObject != null)
            {
                UpdateChunkVisualization(chunk);
                return;
            }

            // Создаём новый GameObject для чанка
            CreateChunkObject(chunk, parent);
        }

        /// <summary>
        /// Создаёт GameObject для чанка
        /// </summary>
        private void CreateChunkObject(MapChunk chunk, Transform parent)
        {
            // Создаём родительский объект чанка
            var chunkObj = new GameObject($"Chunk_{chunk.ChunkX}_{chunk.ChunkY}");
            chunkObj.layer = LayerMask.NameToLayer("Default"); // Устанавливаем слой Default
            chunkObj.transform.parent = parent;
            
            // Позиционируем чанк в мировых координатах
            Vector2Int worldPos = chunk.WorldPosition;
            chunkObj.transform.position = new Vector3(worldPos.x * CELL_SIZE, 0, worldPos.y * CELL_SIZE);

            chunk.ChunkObject = chunkObj;

            // Создаём mesh для терраина
            CreateTerrainMesh(chunk);

            // Создаём визуализацию зданий (если есть)
            CreateBuildingVisualization(chunk);

            chunk.IsLoaded = true;
            chunk.NeedsVisualizationUpdate = false;
        }

        /// <summary>
        /// Обновляет существующую визуализацию чанка
        /// </summary>
        private void UpdateChunkVisualization(MapChunk chunk)
        {
            if (!chunk.NeedsVisualizationUpdate)
                return;

            // Очищаем старые mesh-ы
            ClearChunkMeshes(chunk);

            // Пересоздаём визуализацию
            CreateTerrainMesh(chunk);
            CreateBuildingVisualization(chunk);

            chunk.NeedsVisualizationUpdate = false;
        }

        /// <summary>
        /// Создаёт процедурный mesh для терраина чанка
        /// </summary>
        private void CreateTerrainMesh(MapChunk chunk)
        {
            var meshData = GenerateChunkMesh(chunk);

            // Создаём GameObject для mesh
            var meshObj = new GameObject("TerrainMesh");
            meshObj.layer = 0; // Layer 0 = Default (используем номер вместо имени)
            meshObj.transform.parent = chunk.ChunkObject.transform;
            meshObj.transform.localPosition = Vector3.zero;

            // Добавляем компоненты
            var meshFilter = meshObj.AddComponent<MeshFilter>();
            var meshRenderer = meshObj.AddComponent<MeshRenderer>();
            var meshCollider = meshObj.AddComponent<MeshCollider>();
            
            LogV($"ChunkVisualizer: Created TerrainMesh at world position {meshObj.transform.position}, layer: {meshObj.layer}, has collider: {meshCollider != null}");

            // Создаём и назначаем mesh
            var mesh = new Mesh();
            mesh.name = $"ChunkMesh_{chunk.ChunkX}_{chunk.ChunkY}";
            mesh.vertices = meshData.vertices.ToArray();
            mesh.triangles = meshData.triangles.ToArray();
            mesh.colors = meshData.colors.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            meshFilter.mesh = mesh;
            
            // ВАЖНО: Настраиваем MeshCollider ПЕРЕД назначением mesh
            meshCollider.convex = false;  // Для terrain НЕ должен быть convex!
            meshCollider.cookingOptions = MeshColliderCookingOptions.CookForFasterSimulation
                                         | MeshColliderCookingOptions.EnableMeshCleaning
                                         | MeshColliderCookingOptions.WeldColocatedVertices;
            meshCollider.sharedMesh = null;  // Сначала очищаем
            meshCollider.sharedMesh = mesh;  // Потом назначаем заново
            chunk.ChunkMesh = mesh;

            LogV($"ChunkVisualizer: Mesh stats - Vertices: {mesh.vertexCount}, Triangles: {mesh.triangles.Length / 3}, Bounds: {mesh.bounds}");
            LogV($"ChunkVisualizer: MeshCollider - Convex: {meshCollider.convex}, Enabled: {meshCollider.enabled}, SharedMesh: {meshCollider.sharedMesh != null}");

            // Назначаем материал (используем первый материал терраина)
            if (terrainMaterials != null && terrainMaterials.Length > 0 && terrainMaterials[0] != null)
            {
                meshRenderer.material = terrainMaterials[0];
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                meshRenderer.receiveShadows = false;
            }
            else
            {
                Debug.LogError($"ChunkVisualizer: No valid material for chunk ({chunk.ChunkX}, {chunk.ChunkY})!");
            }
            
            // КРИТИЧНО: Синхронизируем physics после создания
            Physics.SyncTransforms();
        }

        /// <summary>
        /// Генерирует данные mesh для чанка
        /// Использует общую сетку вершин для устранения разрывов
        /// </summary>
        private MeshData GenerateChunkMesh(MapChunk chunk)
        {
            var meshData = new MeshData();
            int gridSize = chunk.ChunkSize + 1; // Сетка вершин на 1 больше, чем количество ячеек

            // Создаём сетку вершин (общие вершины между соседними ячейками)
            var vertexGrid = new int[gridSize, gridSize];

            // Создаём вершины с цветами, используя глобальные координаты для детерминированности
            Vector2Int worldPos = chunk.WorldPosition;
            
            for (int localX = 0; localX < gridSize; localX++)
            {
                for (int localY = 0; localY < gridSize; localY++)
                {
                    float posX = localX * CELL_SIZE;
                    float posZ = localY * CELL_SIZE;
                    
                    // Вычисляем глобальные координаты вершины
                    int globalX = worldPos.x + localX;
                    int globalY = worldPos.y + localY;
                    
                    // Получаем данные ячейки из глобальной сетки
                    // Вершина использует данные ячейки, чей нижний левый угол совпадает с вершиной
                    // Для правой и верхней границ используем предыдущую ячейку
                    int cellX = Mathf.Min(globalX, MapData.MAP_WIDTH - 1);
                    int cellY = Mathf.Min(globalY, MapData.MAP_HEIGHT - 1);
                    
                    var cell = mapData.GetCell(cellX, cellY);
                    float height = cell.Height * HEIGHT_MULTIPLIER;
                    Color vertexColor = GetTerrainColor(cell.Terrain, cell.State);

                    vertexGrid[localX, localY] = meshData.vertices.Count;
                    meshData.vertices.Add(new Vector3(posX, height, posZ));
                    meshData.colors.Add(vertexColor);
                }
            }

            // Создаём треугольники для каждой ячейки
            for (int localX = 0; localX < chunk.ChunkSize; localX++)
            {
                for (int localY = 0; localY < chunk.ChunkSize; localY++)
                {
                    var cell = chunk.GetCell(localX, localY);
                    
                    // Пропускаем ячейки со зданиями (они рендерятся отдельно)
                    if (cell.HasBuilding)
                        continue;

                    AddCellToMesh(meshData, cell, localX, localY, vertexGrid);
                }
            }

            return meshData;
        }

        /// <summary>
        /// Добавляет ячейку в mesh используя общую сетку вершин
        /// </summary>
        private void AddCellToMesh(MeshData meshData, GridCell cell, int localX, int localY, int[,] vertexGrid)
        {
            // Получаем индексы вершин из общей сетки
            int v0 = vertexGrid[localX, localY];
            int v1 = vertexGrid[localX + 1, localY];
            int v2 = vertexGrid[localX + 1, localY + 1];
            int v3 = vertexGrid[localX, localY + 1];

            // Создаём два треугольника для квадрата
            // Первый треугольник
            meshData.triangles.Add(v0);
            meshData.triangles.Add(v1);
            meshData.triangles.Add(v2);

            // Второй треугольник
            meshData.triangles.Add(v0);
            meshData.triangles.Add(v2);
            meshData.triangles.Add(v3);
        }

        /// <summary>
        /// Возвращает цвет для типа терраина
        /// </summary>
        private Color GetTerrainColor(TerrainType terrain, CellState state)
        {
            // Базовые цвета для разных типов терраина
            Color baseColor = terrain switch
            {
                TerrainType.Snow => new Color(0.95f, 0.95f, 1.0f),     // Белый с голубым оттенком
                TerrainType.Rock => new Color(0.5f, 0.5f, 0.55f),      // Серый
                TerrainType.Ice => new Color(0.7f, 0.85f, 1.0f),       // Светло-голубой
                TerrainType.Dirt => new Color(0.4f, 0.3f, 0.25f),      // Коричневый
                _ => Color.white
            };

            // Модифицируем цвет в зависимости от состояния
            if (state == CellState.Blocked)
            {
                baseColor *= 0.5f; // Затемняем заблокированные ячейки
            }
            else if (state == CellState.Water)
            {
                baseColor = new Color(0.2f, 0.4f, 0.8f); // Синий для воды
            }

            return baseColor;
        }

        /// <summary>
        /// Создаёт визуализацию зданий в чанке
        /// </summary>
        private void CreateBuildingVisualization(MapChunk chunk)
        {
            // Для каждого здания в чанке создаём простой куб
            // В будущем можно заменить на более сложные модели

            foreach (var buildingId in chunk.BuildingIds)
            {
                // Здания будут создаваться отдельной системой BuildingVisualizer
                // Пока оставляем пустым - placeholder
            }
        }

        /// <summary>
        /// Очищает mesh-ы чанка
        /// </summary>
        private void ClearChunkMeshes(MapChunk chunk)
        {
            if (chunk.ChunkObject == null)
                return;

            // Удаляем все дочерние объекты
            var children = new List<GameObject>();
            foreach (Transform child in chunk.ChunkObject.transform)
            {
                children.Add(child.gameObject);
            }

            foreach (var child in children)
            {
                Object.Destroy(child);
            }

            if (chunk.ChunkMesh != null)
            {
                Object.Destroy(chunk.ChunkMesh);
                chunk.ChunkMesh = null;
            }
        }

        /// <summary>
        /// Удаляет визуализацию чанка
        /// </summary>
        public void DestroyChunkVisualization(MapChunk chunk)
        {
            if (chunk.ChunkObject != null)
            {
                Object.Destroy(chunk.ChunkObject);
                chunk.ChunkObject = null;
            }

            if (chunk.ChunkMesh != null)
            {
                Object.Destroy(chunk.ChunkMesh);
                chunk.ChunkMesh = null;
            }

            chunk.IsLoaded = false;
        }

        /// <summary>
        /// Вспомогательная структура для хранения данных mesh
        /// </summary>
        private class MeshData
        {
            public List<Vector3> vertices = new List<Vector3>();
            public List<int> triangles = new List<int>();
            public List<Color> colors = new List<Color>();
        }
    }
}