# Achievement Translation Editor

Internal WPF tool for translating video game achievements.

## Requirements

- Windows 10 / 11  
- .NET 8 SDK with Desktop / WPF workload  
- Visual Studio 2022 or dotnet CLI  

## Build

```bash
cd AchievementTranslator
dotnet restore
dotnet build -c Release
```

Or open `AchievementTranslator.csproj` in Visual Studio 2022 and press **F5**.

## Usage

1. **Load files**  
   Click 📂 or place the two JSON files next to the executable:
   - `AchievementInfoMap.json`
   - `AchievementStringMap.json`

2. **Select target language**  
   Choose a language in the ComboBox (e.g. `fr_fr`)

3. **Navigate the tree**  
   - Use the left panel  
   - Nodes in orange contain missing translations (red indicator)

4. **Translate fields**  
   - Edit values in the right panel  
   - Empty fields are highlighted in red

5. **Save**  
   - Press `Ctrl+S` or click 💾

## Keyboard Shortcuts

| Shortcut   | Action                      |
|------------|-----------------------------|
| Ctrl+S     | Save                        |
| Ctrl+Tab   | Go to next untranslated item |

## Architecture

```
AchievementTranslator/
├── Models/
│   ├── Achievement.cs
│   ├── AchievementStringMap.cs
│   └── ULongConverter.cs
├── Services/
│   ├── JsonLoader.cs
│   ├── AchievementTreeBuilder.cs
│   └── TranslationService.cs
├── ViewModels/
│   ├── BaseViewModel.cs
│   ├── RelayCommand.cs
│   ├── TranslationFieldViewModel.cs
│   ├── AchievementNodeViewModel.cs
│   └── MainViewModel.cs
└── Views/
    ├── MainWindow.xaml
    └── MainWindow.xaml.cs
```

## Technical Notes

- String IDs in `AchievementInfoMap.json` use unsigned 64-bit integers (`ulong`) and may exceed `long.MaxValue`. A custom `ULongConverter` (Newtonsoft) handles this.
- Shared translations (same `stringId` used across multiple achievements) are updated in a single operation via a centralized dictionary in `TranslationService`.
- Keys are sorted on save to ensure stable Git diffs.

