# dotnet_exercise

A personal repository for learning and experiments with .NET and C# desktop and cross-platform apps. The goal is to document progress, reference sample apps and small experiments while learning WPF and related technologies.

Projects
--------
- BeautyBookingApp
- BudgetTracker
- CppCSharpInteropDemo
- FileComparerApp
- MauiPuzzleHeroGame
- ServiceDemo
- StudentGradeManager
- SystemMonitorApp
- TimetableGenerator
- WeatherForecastApp
- UartClientApp

UartClientApp (project)
-----------------------
Location: `project/UartClientApp/UartClientApp`

Purpose: a small WPF-based UART (serial) client application used to learn WPF, data binding, and serial port communication in C# (.NET 8).

Key features:
- Enumerate and select serial (COM) ports
- Configure baud rate, data bits, parity, and stop bits
- Open/close serial connection and send/receive text data
- Simple log and received-data viewer in the UI
- Basic disconnect detection and safe UI updates from serial callbacks

Getting started
---------------
Prerequisites:
- .NET 8 SDK
- Visual Studio 2022/2025/2026 or VS Code with C# extensions on Windows
- Access to serial/COM ports for testing

Build & run (Windows, WPF):
1. Open `project/UartClientApp/UartClientApp.slnx` in Visual Studio.
2. Restore NuGet packages and build the solution.
3. Run the `UartClientApp` project.

Notes
-----
- The repository is organized as a collection of small demo and learning projects. Each project may include its own README with more details.
- UartClientApp is licensed under MIT (see license headers in source files).

Contributing / Personal notes
-----------------------------
This repo is primarily a personal learning log. You can open issues or PRs if you want to suggest improvements.

Contact
-------
Maintainer: doremichen (GitHub)

