# План реализации системы генерации карты

## 1. Обзор системы

### 1.1 Требования
- **Размер карты**: 1000x1000 квадратов
- **Структура**: Карта разделена на блоки (чанки) для оптимизации
- **Размещение зданий**: Здания могут занимать несколько квадратов (2x2, 3x5 и т.д.)
- **Типизация**: Каждый квадрат имеет тип (пусто, строится, занято зданием)
- **Инициализация**: Генерация при запуске новой сессии

### 1.2 Основные цели
- Эффективное управление большим количеством данных (1 млн квадратов)
- Удобная система размещения и валидации зданий
- Масштабируемость для будущих механик
- Оптимизация производительности при визуализации

---

## 2. Архитектура системы

### 2.1 Общая структура

```
MapSystem (Singleton)
├── MapData
│   ├── Chunks (2D массив чанков для оптимизации)
│   ├── MapGrid (1000x1000 квадратов)
│   └── BuildingPlacements (список размещённых зданий)
├── MapGenerator (генератор начальной карты)
├── BuildingPlacementValidator (валидация размещения)
└── MapVisualizationController (визуализация в Unity)
```

### 2.2 Иерархия данных

**Карта** (1000x1000 квадратов)
  ↓
**Чанки** (рекомендуемо 50x50 или 100x100 квадратов)
  ↓
**Квадраты** (Grid Cell - базовая единица)
  ↓
**Здания** (занимают несколько квадратов)

---

## 3. Структура данных

### 3.1 GridCell (Квадрат сетки)

```csharp
public struct GridCell
{
    public int X;                    // Координата X
    public int Y;                    // Координата Y
    public CellState State;          // Состояние ячейки
    public int BuildingId;           // ID здания (если занято)
    public byte BuildingLocalX;      // Локальная позиция в здании (0-255)
    public byte BuildingLocalY;      // Локальная позиция в здании (0-255)
    
    // Дополнительные данные для будущих механик
    public TerrainType Terrain;      // Тип терраина
    public float Height;             // Высота для вариативности
}

public enum CellState
{
    Empty,           // Пусто
    Building,        // Занято зданием
    Blocked,         // Заблокировано (препятствие)
    Water,          // Вода (опционально)
}

public enum TerrainType
{
    Snow,           // Снег
    Rock,           // Скала
    Ice,            // Лёд
    Dirt,           // Грязь
}
```

### 3.2 BuildingData (Данные здания)

```csharp
public class BuildingData
{
    public int BuildingId;           // Уникальный ID
    public BuildingType Type;        // Тип здания
    public int PositionX;            // X позиция (левый-верхний угол)
    public int PositionY;            // Y позиция (левый-верхний угол)
    public int Width;                // Ширина
    public int Height;               // Высота
    public BuildingState State;      // Состояние (строится, готово, разрушается)
    public float ConstructionProgress;  // Прогресс строительства (0-100)
}

public enum BuildingType
{
    House,
    Warehouse,
    PowerPlant,
    Farm,
    Workshop,
    // ... остальные типы
}

public enum BuildingState
{
    Planned,         // Запланировано
    UnderConstruction, // Строится
    Completed,       // Завершено
    Damaged,         // Повреждено
    Destroyed,       // Разрушено
}
```

### 3.3 MapChunk (Чанк карты для оптимизации)

```csharp
public class MapChunk
{
    public int ChunkX;               // Позиция чанка в сетке чанков
    public int ChunkY;
    
    public GridCell[] Cells;         // Массив ячеек (размер = CHUNK_SIZE * CHUNK_SIZE)
    public List<int> BuildingIds;    // IDs зданий в этом чанке
    
    public bool IsLoaded;            // Загружена ли визуализация
    public GameObject ChunkObject;   // Unity GameObject чанка
}
```

### 3.4 MapData (Главный контейнер данных)

```csharp
public class MapData
{
    public const int MAP_WIDTH = 1000;
    public const int MAP_HEIGHT = 1000;
    public const int CHUNK_SIZE = 50;  // или 100
    
    private GridCell[,] grid;
    private MapChunk[,] chunks;
    
    private Dictionary<int, BuildingData> buildings;
    private int nextBuildingId = 1;
    
    public MapData()
    {
        grid = new GridCell[MAP_WIDTH, MAP_HEIGHT];
        chunks = new MapChunk[MAP_WIDTH / CHUNK_SIZE, MAP_HEIGHT / CHUNK_SIZE];
        buildings = new Dictionary<int, BuildingData>();
    }
}
```

---

## 4. Основные компоненты системы

### 4.1 MapGenerator (Генератор карты)

**Ответственность**: Инициализация карты при запуске новой сессии

**Методы**:
- `GenerateNewMap()` - полная генерация карты
- `GenerateTerrainVariation()` - создание вариативности терраина
- `GenerateInitialBuildings()` - размещение стартовых зданий (опционально)
- `ClearMap()` - очистка карты
- `RandomizeHeights()` - добавление высоты для визуального разнообразия

**Процесс генерации**:
1. Инициализация пустой сетки (все ячейки = Empty)
2. Добавление террейна (Noise-based или простое разнообразие)
3. Добавление препятствий (опциональные блокировки)
4. Размещение стартовых зданий (если требуется)
5. Инициализация чанков
6. Сохранение начального состояния (для перезагрузки сессии)

### 4.2 BuildingPlacementValidator (Валидатор размещения)

**Ответственность**: Проверка возможности размещения здания

**Методы**:
- `CanPlaceBuilding(BuildingType type, int x, int y, int width, int height)` → bool
- `GetBuildingFootprint(int x, int y, int width, int height)` → List<GridCell>
- `ValidateBuildingPosition(BuildingData building)` → ValidationResult
- `CheckCollisions(int x, int y, int width, int height)` → List<GridCell> (конфликты)
- `GetAvailablePlacementSpots(BuildingType type)` → List<(int x, int y)>

**Проверки**:
- Границы карты (не выходит за пределы 1000x1000)
- Соседние ячейки свободны
- Нет водных препятствий (если применимо)
- Нет уже размещённых зданий

### 4.3 BuildingPlacementManager (Менеджер размещения)

**Ответственность**: Управление размещением и снятием зданий

**Методы**:
- `PlaceBuilding(BuildingType type, int x, int y)` → BuildingData или Error
- `RemoveBuilding(int buildingId)` → bool
- `MoveBuilding(int buildingId, int newX, int newY)` → bool (если поддерживается)
- `GetBuildingAt(int x, int y)` → BuildingData или null
- `GetAllBuildingsInArea(int x, int y, int width, int height)` → List<BuildingData>

**Процесс размещения**:
1. Валидация позиции
2. Обновление GridCell.State для всех затронутых квадратов
3. Создание BuildingData
4. Добавление в словарь buildings
5. Обновление чанков
6. Триггер события OnBuildingPlaced

### 4.4 MapVisualizationController (Визуализация)

**Ответственность**: Отображение карты в Unity

**Методы**:
- `InitializeVisualization()` - создание GameObjects
- `UpdateChunkVisuals(int chunkX, int chunkY)` - обновление визуала чанка
- `RenderGridCell(GridCell cell)` - отрисовка одного квадрата
- `HighlightAvailablePlacementArea(int x, int y, int width, int height)` - подсветка
- `HideAvailablePlacementArea()` - скрыть подсветку
- `SetCameraPosition(Vector3 pos)` - управление камерой

**Оптимизация**:
- Использование Object Pooling для визуальных элементов
- Отрисовка только видимых чанков
- Batching материалов для уменьшения draw calls
- LOD система для больших расстояний

### 4.5 MapSystem (Главный менеджер - Singleton)

**Ответственность**: Координация всех систем

```csharp
public class MapSystem : MonoBehaviour
{
    public static MapSystem Instance { get; private set; }
    
    public MapData MapData { get; private set; }
    public MapGenerator Generator { get; private set; }
    public BuildingPlacementValidator Validator { get; private set; }
    public BuildingPlacementManager PlacementManager { get; private set; }
    public MapVisualizationController VisualizationController { get; private set; }
    
    // События
    public event System.Action<BuildingData> OnBuildingPlaced;
    public event System.Action<int> OnBuildingRemoved;
    public event System.Action OnMapGenerated;
    public event System.Action OnMapReset;
    
    // Инициализация при запуске сессии
    private void Initialize()
    {
        MapData = new MapData();
        Generator = new MapGenerator(MapData);
        Validator = new BuildingPlacementValidator(MapData);
        PlacementManager = new BuildingPlacementManager(MapData, Validator);
        VisualizationController = new MapVisualizationController(MapData);
        
        Generator.GenerateNewMap();
        VisualizationController.InitializeVisualization();
        OnMapGenerated?.Invoke();
    }
}
```

---

## 5. Сценарий использования

### 5.1 Инициализация игры

```
1. MapSystem инициализируется (Awake)
2. MapGenerator создаёт 1000x1000 сетку квадратов
3. Генерируется вариативность терраина
4. Создаются чанки (20x20 = 400 чанков при CHUNK_SIZE=50)
5. MapVisualizationController инициализирует GameObject'ы для видимых чанков
6. Камера позиционируется на центр/стартовую точку
7. Система готова к взаимодействию
```

### 5.2 Размещение здания

```
1. Игрок выбирает тип здания и позицию на карте
2. BuildingPlacementValidator проверяет допустимость
3. Если валидно: BuildingPlacementManager создаёт BuildingData
4. GridCell'ы обновляются (State = Building, BuildingId установлен)
5. MapVisualizationController обновляет визуал затронутых чанков
6. Триггерится событие OnBuildingPlaced
7. UI и другие системы реагируют на событие
```

### 5.3 Снятие здания

```
1. Игрок выбирает здание для снятия
2. BuildingPlacementManager удаляет здание
3. GridCell'ы возвращаются в состояние Empty
4. Здание удаляется из словаря
5. MapVisualizationController обновляет визуал
6. Триггерится событие OnBuildingRemoved
```

---

## 6. Оптимизация производительности

### 6.1 Стратегия чанкирования

- **Размер чанка**: 50x50 или 100x100 (экспериментировать)
- **Всего чанков**: 400 (при 50x50) или 100 (при 100x100)
- **Преимущества**:
  - Поиск ячеек O(1) через индекс чанка
  - Локализированное обновление визуала
  - Возможность асинхронной загрузки чанков
  - Кэширование данных о зданиях в чанке

### 6.2 Оптимизация памяти

- **GridCell**: struct (16-24 байта) → 1000x1000 = ~16-24 МБ
- **Dictionary<int, BuildingData>**: только размещённые здания (обычно 50-200)
- **Чанки**: только активные/видимые инициализируются визуально

### 6.3 Оптимизация визуализации

- **Object Pooling**: переиспользование материалов и meshes
- **Mesh batching**: группировка геометрии по типам
- **Frustum culling**: отрисовка только видимых чанков
- **LOD система**: упрощённые модели для дальних зданий

### 6.4 Асинхронные операции

- Генерация MapData синхронно (быстро)
- Инициализация визуализации чанков асинхронно
- Сохранение/загрузка состояния в отдельном потоке (опционально)

---

## 7. Расширения для будущих функций

### 7.1 Подготовка к дополнениям

**В GridCell предусмотрены поля для**:
- Различные типы терраина (снег, скала, лёд, грязь)
- Высота для вариативности ландшафта
- Будущие параметры (плодородие, загрязнение и т.д.)

**В BuildingData предусмотрены поля для**:
- Состояние здания (строится, готово, повреждено)
- Прогресс строительства
- ID владельца/фракции (для мультиплеера)

**Модульность позволяет легко добавлять**:
- Систему дорог
- Систему зон
- Механики загрязнения
- Динамическое изменение терраина

### 7.2 Система сохранений

```csharp
public class MapSaveData
{
    public GridCell[,] Grid;
    public Dictionary<int, BuildingData> Buildings;
    public int NextBuildingId;
    public DateTime SaveTime;
}
```

---

## 8. Структура файлов проекта

```
Assets/
├── Scripts/
│   ├── Map/
│   │   ├── Core/
│   │   │   ├── MapSystem.cs                    (Singleton + координатор)
│   │   │   ├── MapData.cs                      (Контейнер данных)
│   │   │   ├── GridCell.cs                     (Структура ячейки)
│   │   │   ├── BuildingData.cs                 (Структура здания)
│   │   │   └── MapChunk.cs                     (Структура чанка)
│   │   ├── Generation/
│   │   │   ├── MapGenerator.cs                 (Генератор карты)
│   │   │   ├── TerrainGenerator.cs             (Генератор терраина)
│   │   │   └── BuildingDefaults.cs             (Параметры зданий)
│   │   ├── Placement/
│   │   │   ├── BuildingPlacementValidator.cs   (Валидатор)
│   │   │   └── BuildingPlacementManager.cs     (Менеджер размещения)
│   │   ├── Visualization/
│   │   │   ├── MapVisualizationController.cs   (Главный контроллер визуала)
│   │   │   ├── ChunkVisualizer.cs              (Визуализация чанка)
│   │   │   ├── GridCellVisualizer.cs           (Визуализация ячейки)
│   │   │   ├── BuildingVisualizer.cs           (Визуализация здания)
│   │   │   └── MapCameraController.cs          (Управление камерой)
│   │   └── Utils/
│   │       ├── CoordinateConverter.cs          (Конвертер координат)
│   │       └── MapValidator.cs                 (Проверка целостности)
│   └── UI/
│       └── MapUI.cs                            (UI взаимодействий)
├── Prefabs/
│   ├── GridCell.prefab
│   ├── Building.prefab
│   └── Chunk.prefab
├── Materials/
│   ├── Grid.mat
│   ├── Building.mat
│   └── Terrain.mat
└── Scenes/
    └── GameScene.unity
```

---

## 9. Этапы реализации

### Этап 1: Базовая структура данных (1-2 дня)
- [ ] Создать GridCell.cs, BuildingData.cs, MapChunk.cs
- [ ] Реализовать MapData с базовыми операциями
- [ ] Написать unit-тесты для структур данных

### Этап 2: Генератор карты (1 день)
- [ ] Реализовать MapGenerator
- [ ] Генерировать пустую 1000x1000 сетку
- [ ] Добавить вариативность терраина

### Этап 3: Система размещения (1-2 дня)
- [ ] Реализовать BuildingPlacementValidator
- [ ] Реализовать BuildingPlacementManager
- [ ] Добавить различные типы зданий

### Этап 4: Визуализация (2-3 дня)
- [ ] Создать MapVisualizationController
- [ ] Реализовать визуализацию чанков
- [ ] Оптимизировать производительность
- [ ] Реализовать камеру

### Этап 5: Интеграция и тестирование (1-2 дня)
- [ ] Интегрировать MapSystem в сцену
- [ ] Тестирование производительности
- [ ] Оптимизация при необходимости

---

## 10. Диаграмма взаимодействия компонентов

```
┌─────────────────────────────────────────────────────────────┐
│                        MapSystem                            │
│                    (Singleton, Coordinator)                 │
└─────────────────────────────────────────────────────────────┘
                              │
          ┌───────────────────┼───────────────────┐
          │                   │                   │
          ▼                   ▼                   ▼
    ┌──────────────┐   ┌──────────────┐   ┌──────────────────┐
    │  MapData     │   │ MapGenerator │   │ BuildingPlacement│
    │ (1000x1000)  │   │              │   │    Manager       │
    ├──────────────┤   ├──────────────┤   ├──────────────────┤
    │ GridCell[][] │──▶│ Generate()   │   │ PlaceBuilding()  │
    │ Buildings{}  │   │ RandomTerrain│   │ RemoveBuilding() │
    │ Chunks[][]   │   │              │   │                  │
    └──────────────┘   └──────────────┘   └──────────────────┘
          │                                        │
          │                   ┌────────────────────┘
          │                   │
          └───────┬───────────┤
                  │           │
                  ▼           ▼
    ┌──────────────────────────────────────────┐
    │ BuildingPlacementValidator               │
    ├──────────────────────────────────────────┤
    │ CanPlaceBuilding()                       │
    │ ValidateBuildingPosition()               │
    │ CheckCollisions()                        │
    └──────────────────────────────────────────┘

    ┌──────────────────────────────────────────┐
    │ MapVisualizationController               │
    ├──────────────────────────────────────────┤
    │ InitializeVisualization()                │
    │ UpdateChunkVisuals()                     │
    │ RenderGridCell()                         │
    │ HighlightPlacementArea()                 │
    └──────────────────────────────────────────┘
              │
              ├─▶ ChunkVisualizer
              ├─▶ GridCellVisualizer
              ├─▶ BuildingVisualizer
              └─▶ MapCameraController
```

---

## 11. Примеры кода (концептуальный псевдокод)

### 11.1 Генерация карты

```csharp
public void GenerateNewMap()
{
    // 1. Инициализировать пустую сетку
    for (int x = 0; x < MAP_WIDTH; x++)
    {
        for (int y = 0; y < MAP_HEIGHT; y++)
        {
            grid[x, y] = new GridCell 
            { 
                X = x, 
                Y = y, 
                State = CellState.Empty,
                Terrain = GetRandomTerrainType(),
                Height = Random.value * 0.5f
            };
        }
    }
    
    // 2. Инициализировать чанки
    int chunksX = MAP_WIDTH / CHUNK_SIZE;
    int chunksY = MAP_HEIGHT / CHUNK_SIZE;
    chunks = new MapChunk[chunksX, chunksY];
    
    for (int cx = 0; cx < chunksX; cx++)
    {
        for (int cy = 0; cy < chunksY; cy++)
        {
            chunks[cx, cy] = new MapChunk 
            { 
                ChunkX = cx, 
                ChunkY = cy,
                Cells = new GridCell[CHUNK_SIZE * CHUNK_SIZE]
            };
            
            // Заполнить ячейки чанка
            for (int i = 0; i < CHUNK_SIZE; i++)
            {
                for (int j = 0; j < CHUNK_SIZE; j++)
                {
                    int x = cx * CHUNK_SIZE + i;
                    int y = cy * CHUNK_SIZE + j;
                    chunks[cx, cy].Cells[i + j * CHUNK_SIZE] = grid[x, y];
                }
            }
        }
    }
}
```

### 11.2 Размещение здания

```csharp
public BuildingData PlaceBuilding(BuildingType type, int x, int y)
{
    int width = GetBuildingWidth(type);
    int height = GetBuildingHeight(type);
    
    // Валидация
    if (!Validator.CanPlaceBuilding(type, x, y, width, height))
    {
        return null; // Ошибка размещения
    }
    
    // Создать данные здания
    var building = new BuildingData
    {
        BuildingId = nextBuildingId++,
        Type = type,
        PositionX = x,
        PositionY = y,
        Width = width,
        Height = height,
        State = BuildingState.UnderConstruction,
        ConstructionProgress = 0f
    };
    
    // Обновить GridCell'ы
    for (int i = x; i < x + width; i++)
    {
        for (int j = y; j < y + height; j++)
        {
            grid[i, j].State = CellState.Building;
            grid[i, j].BuildingId = building.BuildingId;
            grid[i, j].BuildingLocalX = (byte)(i - x);
            grid[i, j].BuildingLocalY = (byte)(j - y);
        }
    }
    
    // Добавить в словарь
    buildings[building.BuildingId] = building;
    
    // Обновить визуализацию
    UpdateAffectedChunks(x, y, width, height);
    
    // Событие
    OnBuildingPlaced?.Invoke(building);
    
    return building;
}
```

---

## 12. Ключевые метрики производительности

- **Инициализация MapData**: < 100 мс
- **Генерация карты**: < 500 мс
- **Размещение здания**: < 50 мс (валидация + обновление)
- **Визуализация одного чанка**: < 10 мс
- **Draw calls на экран**: < 100
- **Memory usage (в runtime)**: 30-50 МБ (в зависимости от количества зданий)

---

## 13. Проверочные листы

### Функциональность ✓
- [ ] Карта 1000x1000 генерируется корректно
- [ ] Здания разных размеров размещаются правильно
- [ ] Валидация предотвращает наложение зданий
- [ ] Можно размещать и удалять здания
- [ ] События корректно триггерятся

### Производительность ✓
- [ ] FPS стабилен при просмотре всей карты
- [ ] Нет заметных фризов при размещении зданий
- [ ] Memory footprint в приемлемых пределах
- [ ] Выполняются целевые метрики (см. пункт 12)

### Экспериментирование ✓
- [ ] Найти оптимальный размер чанка (50x50 vs 100x100)
- [ ] Оптимизировать Object Pooling
- [ ] Профилировать и найти узкие места
