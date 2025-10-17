# Руководство по системе строительства

## Обзор

Система строительства позволяет игрокам выбирать и размещать здания на карте с визуальным предпросмотром. Система состоит из трех основных компонентов:

1. **BuildingController** - Управляет процессом строительства
2. **BuildingPreview** - Отображает предпросмотр здания
3. **BuildingSelectionUI** - UI панель для выбора типа здания

## Компоненты

### BuildingController

Главный контроллер, который управляет:
- Выбором типа здания
- Режимом размещения
- Обработкой кликов мыши
- Размещением зданий на карте

**Параметры:**
- `Main Camera` - Ссылка на камеру (по умолчанию Camera.main)
- `Building Preview` - Ссылка на компонент предпросмотра
- `Pointer Position Action` - Input Action для позиции курсора
- `Click Action` - Input Action для клика мыши
- `Cancel Action` - Input Action для отмены (ESC)
- `Terrain Layer` - LayerMask для определения поверхности карты
- `Raycast Distance` - Дистанция рейкаста (по умолчанию 1000)

### BuildingPreview

Визуализирует предпросмотр здания:
- Отображает прозрачное здание
- Показывает зеленый цвет если можно разместить
- Показывает красный цвет если нельзя разместить

**Параметры:**
- `Valid Placement Material` - Материал для валидного размещения (зеленый)
- `Invalid Placement Material` - Материал для невалидного размещения (красный)
- `Height Offset` - Высота над поверхностью (по умолчанию 0.1)

### BuildingSelectionUI

UI панель для выбора типа здания:
- Автоматически создает кнопки для всех доступных зданий
- Подсвечивает выбранное здание
- Интегрируется с BuildingController

**Параметры:**
- `Building Controller` - Ссылка на контроллер строительства
- `Buttons Container` - Transform контейнера для кнопок
- `Building Button Prefab` - Префаб кнопки (опционально)
- `Panel` - GameObject панели UI
- `Close Button` - Кнопка закрытия панели
- `Title Text` - Текст заголовка панели
- `Show All Buildings` - Показывать все типы зданий
- `Available Buildings` - Список доступных типов зданий

## Настройка в сцене

### Шаг 1: Создание объектов системы строительства

1. Создайте пустой GameObject с именем "BuildingSystem"
2. Добавьте компонент `BuildingController`
3. Создайте дочерний GameObject "BuildingPreview"
4. Добавьте на него компонент `BuildingPreview`

```
Scene Hierarchy:
- BuildingSystem (BuildingController)
  └─ BuildingPreview (BuildingPreview)
```

### Шаг 2: Настройка UI

1. Создайте Canvas если его еще нет
2. Создайте Panel для UI выбора зданий
3. Создайте дочерний объект для контейнера кнопок (с Layout Group)
4. Добавьте компонент `BuildingSelectionUI` на Panel
5. Настройте ссылки в Inspector

```
Canvas:
- BuildingSelectionPanel (BuildingSelectionUI)
  ├─ Title (TextMeshProUGUI)
  ├─ CloseButton (Button)
  └─ ButtonsContainer (VerticalLayoutGroup)
```

**Рекомендуемые настройки для ButtonsContainer:**
- Добавьте компонент `Vertical Layout Group`
- Child Alignment: Upper Center
- Child Force Expand: Width = true, Height = false
- Spacing: 10

### Шаг 3: Настройка Input System

Откройте `Assets/InputSystem_Actions.inputactions` и добавьте следующие действия:

#### Pointer Position Action
- Name: "PointerPosition"
- Action Type: Value
- Control Type: Vector2
- Binding: Mouse > Position

#### Click Action
- Name: "Click"
- Action Type: Button
- Binding: Mouse > Left Button

#### Cancel Action
- Name: "Cancel"
- Action Type: Button
- Binding: Keyboard > Escape

**Привязка в BuildingController:**
1. В Inspector найдите поля Input Actions
2. Перетащите соответствующие действия из InputSystem_Actions

### Шаг 4: Настройка LayerMask

1. Убедитесь что у вашего terrain есть слой (например "Terrain")
2. В BuildingController установите `Terrain Layer` на этот слой

### Шаг 5: Подключение компонентов

В Inspector для BuildingController:
- `Building Preview` → перетащите GameObject с компонентом BuildingPreview
- `Pointer Position Action` → выберите PointerPosition из InputSystem_Actions
- `Click Action` → выберите Click
- `Cancel Action` → выберите Cancel
- `Terrain Layer` → выберите слой вашего terrain

В Inspector для BuildingSelectionUI:
- `Building Controller` → перетащите GameObject с BuildingController
- `Buttons Container` → перетащите Transform контейнера кнопок
- `Panel` → перетащите GameObject панели
- `Close Button` → перетащите кнопку закрытия (если есть)

## Использование

### Открытие UI выбора зданий

Из кода:
```csharp
BuildingSelectionUI ui = FindFirstObjectByType<BuildingSelectionUI>();
ui.Show();
```

Или создайте кнопку в UI и привяжите метод `Show()` к `OnClick`.

### Программный выбор здания

```csharp
BuildingController controller = FindFirstObjectByType<BuildingController>();
controller.SelectBuilding(BuildingType.House);
```

### Отмена размещения

Пользователь может нажать ESC или:

```csharp
BuildingController controller = FindFirstObjectByType<BuildingController>();
controller.CancelPlacement();
```

### Подписка на события

```csharp
BuildingController controller = FindFirstObjectByType<BuildingController>();

controller.OnBuildingTypeSelected += (buildingType) => {
    Debug.Log($"Выбран тип здания: {buildingType}");
};

controller.OnBuildingPlaced += () => {
    Debug.Log("Здание размещено!");
};

controller.OnPlacementCancelled += () => {
    Debug.Log("Размещение отменено");
};
```

## Workflow пользователя

1. Игрок открывает UI выбора зданий (кнопка или клавиша)
2. Игрок кликает на тип здания в UI
3. Появляется предпросмотр здания, следующий за курсором
4. Предпросмотр показывает зеленый цвет если можно разместить, красный если нельзя
5. Игрок кликает мышью чтобы разместить здание
6. Здание размещается на карте
7. Режим размещения продолжается (можно разместить еще одно здание)
8. Игрок нажимает ESC чтобы выйти из режима размещения

## Пример полной настройки

```csharp
using UnityEngine;
using Icefall.Map;
using Icefall.Map.Core;

public class BuildingSystemExample : MonoBehaviour
{
    [SerializeField] private BuildingController buildingController;
    [SerializeField] private BuildingSelectionUI buildingUI;

    private void Start()
    {
        // Подписываемся на события
        buildingController.OnBuildingPlaced += OnBuildingPlaced;
        buildingController.OnPlacementCancelled += OnPlacementCancelled;
        
        // Показываем UI при старте
        buildingUI.Show();
    }

    private void OnBuildingPlaced()
    {
        Debug.Log("Здание успешно размещено!");
        // Можно добавить звуковой эффект, анимацию и т.д.
    }

    private void OnPlacementCancelled()
    {
        Debug.Log("Размещение отменено");
        // Можно скрыть UI или показать другое меню
    }

    // Метод для кнопки "Построить дом"
    public void BuildHouse()
    {
        buildingController.SelectBuilding(BuildingType.House);
    }
}
```

## Интеграция с MapSystem

Система строительства автоматически интегрируется с MapSystem:
- Использует `MapSystem.Instance.CanPlaceBuilding()` для проверки
- Использует `MapSystem.Instance.PlaceBuilding()` для размещения
- События размещения автоматически обрабатываются MapSystem

## Troubleshooting

### Предпросмотр не появляется
- Проверьте что BuildingPreview правильно подключен в BuildingController
- Проверьте что материалы назначены в BuildingPreview
- Проверьте что LayerMask настроен на правильный слой terrain

### Клик не размещает здание
- Проверьте что Input Actions правильно настроены
- Проверьте что у terrain есть Collider
- Проверьте что LayerMask включает слой terrain
- Проверьте что MapSystem инициализирован

### Кнопки UI не создаются
- Проверьте что ButtonsContainer назначен
- Проверьте что BuildingController подключен к BuildingSelectionUI
- Проверьте наличие TextMeshPro в проекте

### Здание размещается в неправильном месте
- Проверьте метод `WorldToMapPosition` в BuildingController
- Убедитесь что масштаб terrain соответствует координатам карты
- Настройте преобразование координат если нужно

## Дополнительные возможности

### Кастомизация предпросмотра
Вы можете заменить стандартный куб на свой префаб:
1. Создайте префаб здания
2. Модифицируйте метод `CreatePreviewObject()` в BuildingPreview
3. Используйте префаб вместо CreatePrimitive

### Добавление стоимости зданий
Расширьте BuildingButton для отображения стоимости:
```csharp
public class ExtendedBuildingButton : BuildingButton
{
    [SerializeField] private TextMeshProUGUI costText;
    
    public void SetCost(int cost)
    {
        if (costText != null)
            costText.text = $"${cost}";
    }
}
```

### Горячие клавиши
Добавьте Input Actions для быстрого выбора зданий:
```csharp
[SerializeField] private InputActionReference buildHouseAction;

private void OnEnable()
{
    buildHouseAction.action.performed += ctx => 
        buildingController.SelectBuilding(BuildingType.House);
}
```

## См. также

- [INTEGRATION_GUIDE.md](INTEGRATION_GUIDE.md) - Общее руководство по системе карты
- [README.md](README.md) - Обзор системы карты
- [BuildingData.cs](Core/BuildingData.cs) - Типы и данные зданий
- [BuildingPlacementManager.cs](Placement/BuildingPlacementManager.cs) - Управление размещением