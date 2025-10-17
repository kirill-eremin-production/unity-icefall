# Система строительства зданий

## Что реализовано

Добавлена полнофункциональная система строительства зданий с UI и визуальным предпросмотром.

### Основные компоненты

#### 1. BuildingController
**Файл:** [`BuildingController.cs`](BuildingController.cs)

Главный контроллер системы строительства:
- ✅ Обработка выбора типа здания
- ✅ Управление режимом размещения
- ✅ Интеграция с Input System
- ✅ Raycast для определения позиции на карте
- ✅ Валидация возможности размещения
- ✅ Размещение зданий через MapSystem

**События:**
- `OnBuildingTypeSelected` - здание выбрано
- `OnBuildingPlaced` - здание размещено
- `OnPlacementCancelled` - размещение отменено

#### 2. BuildingPreview
**Файл:** [`BuildingPreview.cs`](BuildingPreview.cs)

Визуальный предпросмотр здания:
- ✅ Отображение прозрачного куба размером здания
- ✅ Зеленый цвет - можно разместить
- ✅ Красный цвет - нельзя разместить
- ✅ Динамическое обновление позиции за курсором
- ✅ Автоматическая настройка размеров под тип здания

#### 3. BuildingSelectionUI
**Файл:** [`UI/BuildingSelectionUI.cs`](UI/BuildingSelectionUI.cs)

UI панель для выбора типа здания:
- ✅ Автогенерация кнопок для всех типов зданий
- ✅ Русские названия зданий
- ✅ Подсветка выбранного здания
- ✅ Поддержка кастомных префабов кнопок
- ✅ Интеграция с BuildingController

### Workflow пользователя

```
1. Пользователь открывает UI выбора зданий
   ↓
2. Выбирает тип здания (клик по кнопке)
   ↓
3. Появляется предпросмотр здания, следующий за курсором
   ↓
4. Предпросмотр показывает цвет валидности (зеленый/красный)
   ↓
5. Клик мыши размещает здание
   ↓
6. ESC отменяет режим размещения
```

## Быстрый старт

### 1. Создание GameObject'ов в сцене

```
Hierarchy:
- BuildingSystem (пустой GameObject)
  ├─ BuildingController (компонент BuildingController)
  └─ BuildingPreview (компонент BuildingPreview)

Canvas:
- BuildingSelectionPanel (Panel с BuildingSelectionUI)
  └─ ButtonsContainer (с VerticalLayoutGroup)
```

### 2. Настройка BuildingController

В Inspector:
- Main Camera → Camera.main (автоматически)
- Building Preview → перетащить объект BuildingPreview
- Pointer Position Action → Input Action для позиции мыши
- Click Action → Input Action для клика
- Cancel Action → Input Action для ESC
- Terrain Layer → выбрать слой terrain

### 3. Настройка BuildingSelectionUI

В Inspector:
- Building Controller → перетащить BuildingController
- Buttons Container → перетащить контейнер для кнопок
- Panel → перетащить GameObject панели
- Show All Buildings → ✓ (показывать все типы)

### 4. Input Actions

Необходимые Input Actions в `InputSystem_Actions.inputactions`:
- **PointerPosition** (Vector2, Mouse Position)
- **Click** (Button, Mouse Left Button)
- **Cancel** (Button, Keyboard Escape)

## Примеры использования

### Программный выбор здания

```csharp
var controller = FindFirstObjectByType<BuildingController>();
controller.SelectBuilding(BuildingType.House);
```

### Открытие UI

```csharp
var ui = FindFirstObjectByType<BuildingSelectionUI>();
ui.Show();
```

### Подписка на события

```csharp
buildingController.OnBuildingPlaced += () => {
    Debug.Log("Здание построено!");
};
```

## Доступные типы зданий

- **House** (Дом) - 2x2
- **Warehouse** (Склад) - 3x4
- **PowerPlant** (Электростанция) - 4x4
- **Farm** (Ферма) - 5x4
- **Workshop** (Мастерская) - 3x3
- **Hospital** (Больница) - 4x3
- **School** (Школа) - 3x3
- **Mine** (Шахта) - 3x3
- **WoodCutter** (Лесопилка) - 2x2
- **HeatingPlant** (Теплостанция) - 3x3

## Горячие клавиши (BuildingSystemExample)

- **B** - Открыть/закрыть UI выбора
- **1-9** - Быстрый выбор типа здания
- **ESC** - Отменить размещение

## Дополнительные файлы

- [`BUILDING_SYSTEM_GUIDE.md`](BUILDING_SYSTEM_GUIDE.md) - Полное руководство по интеграции
- [`Examples/BuildingSystemExample.cs`](Examples/BuildingSystemExample.cs) - Пример использования

## Интеграция с существующей системой

Система автоматически интегрируется с:
- ✅ [`MapSystem`](Core/MapSystem.cs) - для размещения зданий
- ✅ [`BuildingPlacementManager`](Placement/BuildingPlacementManager.cs) - управление размещением
- ✅ [`BuildingPlacementValidator`](Placement/BuildingPlacementValidator.cs) - валидация
- ✅ [`MapVisualizationController`](Visualization/MapVisualizationController.cs) - обновление визуализации

## Архитектура

```
BuildingController
    ├─ Управляет процессом строительства
    ├─ Обрабатывает input
    └─ Связывает UI, Preview и MapSystem

BuildingPreview
    ├─ Визуализирует предпросмотр
    └─ Показывает валидность размещения

BuildingSelectionUI
    ├─ UI для выбора типа здания
    └─ Генерирует кнопки

MapSystem
    └─ Обрабатывает фактическое размещение
```

## Требования

- ✅ Unity 2021.3+
- ✅ Universal Render Pipeline
- ✅ Input System Package
- ✅ TextMeshPro
- ✅ Существующая MapSystem

## Что дальше?

### Возможные улучшения:
1. Замена простого куба на 3D модели зданий
2. Анимация размещения
3. Звуковые эффекты
4. Отображение стоимости зданий
5. Требования к ресурсам
6. Показ зоны влияния здания
7. Rotation зданий (поворот)
8. Различные варианты размеров одного типа здания

### Для создания визуальных моделей:
1. Создайте префабы зданий
2. Модифицируйте `BuildingPreview.CreatePreviewObject()`
3. Загружайте соответствующий префаб вместо примитива

## Тестирование

Используйте `BuildingSystemExample.cs` для тестирования:
- Показать информацию о системе: `ShowSystemInfo()`
- Очистить все здания: `ClearAllBuildings()`
- Быстрая постройка: `QuickBuild(BuildingType)`

---

**Статус:** ✅ Полностью функционально  
**Дата:** 2025-01-17  
**Автор:** Kilo Code