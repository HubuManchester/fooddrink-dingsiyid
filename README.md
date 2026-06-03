# CampusEats
 
 A cross-platform mobile application for campus food and restaurant discovery, built with .NET MAUI 8.0.
 
 
 **Author**: SIYI DING (丁思怡)
 **Course Info**: Developed for MMU Module: 6G6Z0014 – Mobile Computing, Coursework: Developing a Cross-Platform Mobile App
 
 
 **GitHub Repositories**:
 
 - `https://github.com/HubuManchester/fooddrink-dingsiyid` 
 - `https://github.com/dingsiyid/CampusEats` 
 
 
 ## Table of Contents
 
 - [Introduction](#introduction)
 
 - [Features](#features)
 
 - [Hardware Integration](#hardware-integration)
 
 - [Accessibility](#accessibility)
 
 - [Technical Stack](#technical-stack)
 
 - [Project Structure](#project-structure)
 
 - [Getting Started](#getting-started)
 
 - [Deployment](#deployment)
 
 - [GitHub Development Record](#github-development-record)
 
 - [Future Improvements](#future-improvements)
 
 - [License](#license)
 
 
 ## Introduction
 
 CampusEats is a comprehensive food and drink application designed to help users discover restaurants, explore recipes, track meal plans, and utilize mobile hardware features for an enhanced experience. The app focuses on providing a user-friendly interface with strong accessibility features and full cross-platform compatibility, complying with the official coursework development requirements.
 
 
 ## Features
 
 ### Core Functionality
 
 1. **Restaurant Discovery** - Browse nearby restaurants with location-based recommendations
 
 2. **Recipe Library** - Explore a collection of recipes with search and category filtering
 
 3. **Meal Planning** - Create and manage daily meal plans with calorie tracking
 
 4. **Food Recognition** - Take photos to identify food items using camera integration
 
 5. **Shake Recommendation** - Shake your device to get random recipe suggestions
 
 
 ### UI/UX Features
 
 - Modern, clean interface design with consistent styling
 
 - Smooth navigation with Shell routing
 
 - Responsive layout for phones, tablets, and desktop
 
 - Dark/Light theme switching
 
 - Font size adjustment for accessibility
 
 
 ## Hardware Integration
 
 CampusEats utilizes five different valid mobile hardware features as required by marking criteria, all fully functional and deeply integrated into core app logic:
 
 
 | Hardware | Usage |
 |----------|-------|
 | **Camera** | Capture food photos for AI-based food recognition and restaurant photo upload; realize basic computer vision food classification to match relevant recipes |
 | **Location Services** | Get current location (supports emulator spoof location) for nearby restaurant recommendations |
 | **Accelerometer** | Detect shake motion to trigger random recipe recommendation feature |
 | **Text-to-Speech** | Read recipe instructions and app notifications aloud |
 | **Vibration** | Provide haptic feedback after successful operations like favourite saving and shake trigger |
 
 
 ## Accessibility
 
 The app strictly follows **WCAG 2.0 accessibility guidelines**, key accessibility implementations listed below:
 
 - ✅ Dynamic font size adjustment supporting system-level text scaling without text truncation
 
 - ✅ Dark/Light theme switching with automatic system sync
 
 - ✅ High contrast design (text-background contrast ≥4.5:1) complying with WCAG 1.4.3
 
 - ✅ Full screen reader compatibility with `AutomationProperties.Name` set for all interactive controls
 
 - ✅ Clear visual feedback for every user operation
 
 - ✅ Large touch targets (minimum 44pt size) following WCAG 2.5.5 requirement
 
 
 ## Technical Stack
 
 - **Framework**: .NET MAUI 8.0
 
 - **Language**: C# 12
 
 - **UI**: XAML entirely for layout definition, implemented with MVVM architectural pattern
 
 - **Local Database**: SQLite with Entity Framework Core
 
 - **Third-party Library**: MVVM Community Toolkit, MAUI Community Toolkit
 
 - **Supported Platforms**: Android, Windows, iOS, MacCatalyst
 
 
 ## Project Structure
 
 ```
 
 CampusEats/
 
 ├── Converters/          # Value converters for XAML data binding
 
 ├── Models/              # Core data models (Restaurant, Recipe, Dish, MealPlanItem)
 
 ├── Platforms/           # Platform-specific implementation code
 
 │   ├── Android/
 
 │   ├── Windows/
 
 │   ├── iOS/
 
 │   └── MacCatalyst/
 
 ├── Resources/           # Static resources (styles, images, fonts)
 
 │   ├── Styles/          # Global XAML styles and theme resources
 
 │   ├── Images/          # App image assets
 
 │   └── Fonts/           # Custom font files
 
 ├── Services/            # Independent reusable business & hardware services
 
 │   ├── HardwareManager.cs    # Unified management for all hardware functions
 
 │   ├── DatabaseService.cs    # Local CRUD database operations
 
 │   ├── ThemeService.cs       # Dark/Light mode management
 
 │   ├── TextToSpeechService.cs # TTS core logic
 
 │   └── MockService.cs        # Static mock test data
 
 ├── ViewModels/          # All view models following MVVM standard
 
 ├── Views/               # All XAML page files for app UI
 
 ├── App.xaml.cs          # Global application logic
 
 ├── AppShell.xaml        # Shell-based app navigation configuration
 
 └── MauiProgram.cs       # Dependency injection and service registration
 
 ```
 
 
 ## Getting Started
 
 ### Prerequisites
 
 - .NET 8.0 SDK or later
 
 - Visual Studio 2022 with installed MAUI workload
 
 - Android SDK (for Android emulator/physical device testing)
 
 - Windows SDK (for Windows desktop deployment)
 
 
 ### Installation
 
 ```bash
 
 # Clone primary repository
 
 git clone `https://github.com/dingsiyid/CampusEats.git` 
 # Alternative coursework assigned repository
 
 # git clone `https://github.com/HubuManchester/fooddrink-dingsiyid` 
 
 cd CampusEats
 
 
 # Restore all referenced NuGet dependencies
 
 dotnet restore
 
 
 # Build the whole solution
 
 dotnet build
 
 
 # Run on specified target platform
 
 # Android
 
 dotnet run -f net8.0-android
 
 # Windows
 
 dotnet run -f net8.0-windows10.0.19041.0
 
 # iOS (Mac machine with Xcode required)
 
 dotnet run -f net8.0-ios
 
 ```
 
 
 ## Deployment
 
 ### Android
 
 1. Generate signed release APK package:
 
 ```bash
 
 dotnet publish -f net8.0-android -c Release /p:AndroidKeyStore=true /p:AndroidSigningKeyStore=myapp.keystore /p:AndroidSigningKeyAlias=mykey /p:AndroidSigningKeyPass=password
 
 ```
 
 2. Install generated APK on physical Android device or Android tablet emulator.
 
 
 ### Windows
