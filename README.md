# Pi18n
A fully dynamic and simple solution for multi-lingual support

## Features

- Load and switch languages at runtime without recompiling or restarting.
- Simple setup and usage with minimal code changes necessary.

## Getting Started

### Setup

1. **Resource Files**: Create resource files for different languages.  
Example:
```
// .resource/language-resource-ja-JP.ui.i18n
// .resource/language-resource-ja-JP.msg.i18n
// .resource/language-resource-zh-CN.ui.i18n
// .resource/language-resource-zh-CN.msg.i18n
```

2. **Configuration**: Set up the `ResourceManager` with the appropriate resource path and naming convention.

```csharp
ResourceManager.SetUp("resource", "language-resource-{I18N}.{ANY}.i18n");
ResourceManager.SetLanguage(ResourceManager.CultureInfoList[0].Name);
```

### Usage

#### Use directly in Code-Behind

Access the localized strings using the `ResourceManager`.

```csharp
MessageBox.Show(ResourceManager["MessageText"]);
```

#### Use MVVM for data binding

```csharp
public class ViewModel : ObservableObject
{
    public ResourceManager ResourceManager => ResourceManager.Instance;
}
```
```xaml
<TextBlock Text="{Binding ResourceManager[Hello]}"/>
```

## APIS

### Properties

- Instance : Get instance of ResourceManager
- CurrentCulture : Get current CultureInfo instance
- CultureInfoList : Get list of CultureInfo instance
- CultureCodeList : Get list of culture code (like "en-US")
- CultureNameList : Get list of culture name (like "English (United States)")

### Functions

- SetUp : Sets up the ResourceManager with the appropriate resource path and naming convention.
- SetLanguage : Sets or switches the current language by culture code.
