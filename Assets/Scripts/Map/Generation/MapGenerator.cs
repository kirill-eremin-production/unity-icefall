using UnityEngine;
using Icefall.Map.Core;

namespace Icefall.Map.Generation
{
    /// <summary>
    /// Генератор карты - создаёт начальное состояние карты
    /// Использует Perlin Noise для естественной генерации терраина
    /// </summary>
    public class MapGenerator
    {
        private MapData mapData;
        private System.Random random;

        // Параметры Perlin Noise
        private float terrainNoiseScale = 0.05f;      // Масштаб шума для типов терраина
        private float heightNoiseScale = 0.1f;        // Масштаб шума для высоты
        private float terrainOffset;                   // Случайное смещение для уникальности
        private float heightOffset;

        public MapGenerator(MapData mapData, int seed = -1)
        {
            this.mapData = mapData;
            
            // Если seed не указан, используем случайный
            if (seed < 0)
                seed = System.DateTime.Now.Millisecond;
            
            random = new System.Random(seed);
            
            // Генерируем случайные смещения для Perlin Noise
            terrainOffset = (float)random.NextDouble() * 1000f;
            heightOffset = (float)random.NextDouble() * 1000f;
        }

        /// <summary>
        /// Генерирует новую карту
        /// </summary>
        public void GenerateNewMap()
        {
            Debug.Log("MapGenerator: Starting map generation...");

            // Очистить существующие данные
            mapData.Clear();

            // Генерировать терраин и высоты
            GenerateTerrainVariation();

            // Синхронизировать чанки с сеткой
            SyncChunksWithGrid();

            Debug.Log($"MapGenerator: Map generation completed. {mapData.GetStats()}");
        }

        /// <summary>
        /// Генерирует вариативность терраина используя Perlin Noise
        /// </summary>
        private void GenerateTerrainVariation()
        {
            for (int x = 0; x < MapData.MAP_WIDTH; x++)
            {
                for (int y = 0; y < MapData.MAP_HEIGHT; y++)
                {
                    // Получаем текущую ячейку
                    var cell = mapData.GetCell(x, y);

                    // Генерируем тип терраина на основе Perlin Noise
                    cell.Terrain = GetTerrainTypeFromNoise(x, y);

                    // Генерируем высоту на основе другого Perlin Noise
                    cell.Height = GetHeightFromNoise(x, y);

                    // Обновляем ячейку
                    mapData.SetCell(x, y, cell);
                }
            }
        }

        /// <summary>
        /// Определяет тип терраина на основе Perlin Noise
        /// </summary>
        private TerrainType GetTerrainTypeFromNoise(int x, int y)
        {
            // Вычисляем значение Perlin Noise для данной позиции
            float noiseValue = Mathf.PerlinNoise(
                (x + terrainOffset) * terrainNoiseScale,
                (y + terrainOffset) * terrainNoiseScale
            );

            // Распределяем типы терраина на основе значения шума (0.0 - 1.0)
            // Snow (снег) - самый распространённый, ~50%
            // Rock (скала) - ~25%
            // Ice (лёд) - ~15%
            // Dirt (грязь) - ~10%

            if (noiseValue < 0.5f)
                return TerrainType.Snow;
            else if (noiseValue < 0.75f)
                return TerrainType.Rock;
            else if (noiseValue < 0.9f)
                return TerrainType.Ice;
            else
                return TerrainType.Dirt;
        }

        /// <summary>
        /// Вычисляет высоту на основе Perlin Noise
        /// </summary>
        private float GetHeightFromNoise(int x, int y)
        {
            // Используем отдельное поле шума для высоты
            float noiseValue = Mathf.PerlinNoise(
                (x + heightOffset) * heightNoiseScale,
                (y + heightOffset) * heightNoiseScale
            );

            // Нормализуем высоту в диапазон 0-1 и добавляем небольшую вариативность
            return noiseValue * 0.5f; // Максимальная высота = 0.5 единиц
        }

        /// <summary>
        /// Синхронизирует чанки с данными сетки
        /// </summary>
        private void SyncChunksWithGrid()
        {
            var chunks = mapData.GetAllChunks();
            
            for (int cx = 0; cx < MapData.CHUNKS_X; cx++)
            {
                for (int cy = 0; cy < MapData.CHUNKS_Y; cy++)
                {
                    var chunk = chunks[cx, cy];
                    
                    // Копируем данные из сетки в чанк
                    for (int localX = 0; localX < MapData.CHUNK_SIZE; localX++)
                    {
                        for (int localY = 0; localY < MapData.CHUNK_SIZE; localY++)
                        {
                            int worldX = cx * MapData.CHUNK_SIZE + localX;
                            int worldY = cy * MapData.CHUNK_SIZE + localY;

                            if (worldX < MapData.MAP_WIDTH && worldY < MapData.MAP_HEIGHT)
                            {
                                var cell = mapData.GetCell(worldX, worldY);
                                chunk.SetCell(localX, localY, cell);
                            }
                        }
                    }

                    // Помечаем чанк для обновления визуализации
                    chunk.MarkForUpdate();
                }
            }
        }

        /// <summary>
        /// Генерирует препятствия на карте (опционально)
        /// </summary>
        public void GenerateObstacles(int count)
        {
            Debug.Log($"MapGenerator: Generating {count} obstacles...");

            int generated = 0;
            int attempts = 0;
            int maxAttempts = count * 10; // Предотвращаем бесконечный цикл

            while (generated < count && attempts < maxAttempts)
            {
                attempts++;

                // Случайная позиция
                int x = random.Next(0, MapData.MAP_WIDTH);
                int y = random.Next(0, MapData.MAP_HEIGHT);

                // Проверяем, свободна ли ячейка
                if (mapData.IsCellAvailable(x, y))
                {
                    var cell = mapData.GetCell(x, y);
                    cell.State = CellState.Blocked;
                    mapData.SetCell(x, y, cell);
                    generated++;
                }
            }

            Debug.Log($"MapGenerator: Generated {generated} obstacles in {attempts} attempts");
        }

        /// <summary>
        /// Очищает карту
        /// </summary>
        public void ClearMap()
        {
            mapData.Clear();
            Debug.Log("MapGenerator: Map cleared");
        }

        /// <summary>
        /// Устанавливает параметры генерации
        /// </summary>
        public void SetGenerationParameters(float terrainScale, float heightScale)
        {
            terrainNoiseScale = terrainScale;
            heightNoiseScale = heightScale;
        }

        /// <summary>
        /// Генерирует тестовое здание (для отладки)
        /// </summary>
        public void GenerateTestBuilding(int x, int y, BuildingType type)
        {
            int width = BuildingDefaults.GetDefaultWidth(type);
            int height = BuildingDefaults.GetDefaultHeight(type);

            // Проверяем, можно ли разместить
            bool canPlace = true;
            for (int bx = x; bx < x + width && canPlace; bx++)
            {
                for (int by = y; by < y + height && canPlace; by++)
                {
                    if (!mapData.IsCellAvailable(bx, by))
                        canPlace = false;
                }
            }

            if (canPlace)
            {
                var building = mapData.AddBuilding(type, x, y, width, height);
                Debug.Log($"MapGenerator: Created test building {building}");
            }
            else
            {
                Debug.LogWarning($"MapGenerator: Cannot place test building at ({x}, {y})");
            }
        }
    }
}