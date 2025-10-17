# Технический стек проекта Icefall

## Движок и версия

- **Unity**: 6000.2.8f1 (6000.2)
- **Target Framework**: .NET Standard 2.1
- **C# Language Version**: 9.0
- **Scripting Backend**: Mono

## Render Pipeline

- **Universal Render Pipeline (URP)**: версия 17.2.0
- Настроены профили:
  - [`Mobile_Renderer`](Assets/Settings/Mobile_Renderer.asset) и [`Mobile_RPAsset`](Assets/Settings/Mobile_RPAsset.asset) для мобильных устройств
  - [`PC_Renderer`](Assets/Settings/PC_Renderer.asset) и [`PC_RPAsset`](Assets/Settings/PC_RPAsset.asset) для ПК
  - [`DefaultVolumeProfile`](Assets/Settings/DefaultVolumeProfile.asset) - базовый профиль пост-обработки
  - [`SampleSceneProfile`](Assets/Settings/SampleSceneProfile.asset) - профиль для демо-сцены

## Установленные пакеты Unity

### Основные пакеты
- **AI Navigation**: 2.0.9 - навигация и pathfinding
- **Input System**: 1.14.2 - новая система ввода
- **Universal RP**: 17.2.0 - графический конвейер
- **Visual Scripting**: 1.9.7 - визуальное программирование

### Инструменты разработки
- **Rider IDE**: 3.0.38
- **Visual Studio**: 2.0.23
- **Collab Proxy**: 2.9.3
- **Multiplayer Center**: 1.0.0

### Дополнительные пакеты
- **Timeline**: 1.8.9 - система анимации и кинематографии
- **Test Framework**: 1.6.0 - модульное тестирование
- **UI (UGUI)**: 2.0.0 - система пользовательского интерфейса

## Платформы

- **Основная платформа**: Windows 64-bit
- **Разрешение по умолчанию**: 1024x768
- **Поддержка**: Standalone Windows

## Системы компиляции

- **Burst Compiler**: Включен (для оптимизации производительности)
- **IL2CPP**: Доступен как опция