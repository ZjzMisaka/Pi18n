# Pi18n
A fully dynamic and easy-to-use solution for multi-lingual support

## Features

- Load and switch languages at runtime without recompiling or restarting.
- Simple setup and usage with minimal code changes necessary.

## Installation
If you want to include Pi18n in your project, you can [install it directly from NuGet](https://www.nuget.org/packages/Pi18n/).  
Support: Net40+ | Net5.0+ | netstandard2.0+  

## Getting Started

### Setup

1. **Resource Files**: Create resource files for different languages.  
Example:
    ```
    .resource/language-resource-ja-JP.ui.i18n
    .resource/language-resource-ja-JP.msg.i18n
    .resource/language-resource-zh-CN.ui.i18n
    .resource/language-resource-zh-CN.msg.i18n
    ```
    Resource content style:
    ```
    Key=Value
    ```

2. **Configuration**: Set up the `ResourceManager` with the appropriate resource path and naming convention.

    ```csharp
    ResourceManager.SetUp("resource", "language-resource-{I18N}.{ANY}.i18n");
    ResourceManager.SetLanguage(ResourceManager.CultureInfoList[0].Name);
    ```

### Usage

#### Use directly in Code-Behind

Access the localized strings using the `Resource`.
```csharp
public class CustomClass
{
    public dynamic Resource => ResourceManager.Instance;
}
```
```csharp
MessageBox.Show(Resource["MessageText"]);
```
```csharp
MessageBox.Show(Resource.MessageText);
```

#### Use MVVM for data binding
```csharp
public class ViewModel : ObservableObject
{
    public dynamic Resource => ResourceManager.Instance;
}
```
```xaml
<TextBlock Text="{Binding Resource[Hello]}"/>
```
```xaml
<TextBlock Text="{Binding Resource.Hello}"/>
```

## APIS

### Properties

- Instance : Get instance of ResourceManager
- DefaultCulture : Get or set default CultureInfo instance
- CurrentCulture : Get or set current CultureInfo instance
- CultureInfoList : Get list of CultureInfo instance
- CultureCodeList : Get list of culture code (like "en-US")
- CultureNameList : Get list of culture name (like "English (United States)")

### Functions

- `void SetUp(string path, string format)` : Sets up the ResourceManager with the appropriate resource path and naming convention.
- `bool SetDefault(CultureInfo cultureInfo)` : Sets the default language by CultureInfo object.
- `bool SetDefault(string cultureCode)` : Sets the default language by culture code.
- `bool SetDefault(CultureType cultureType)` : Sets the default language by culture type (CurrentUICulture | CurrentCulture).
- `bool SetLanguage(CultureInfo cultureInfo)` : Sets or switches the current language by CultureInfo object.
- `bool SetLanguage(string cultureCode)` : Sets or switches the current language by culture code.
- `string GetFormat(string key, params object[] args)` : Get formatted string.

### Events

- LanguageChanged : Language changed
    - LanguageChangedEventArgs.OldCultureInfo
    - LanguageChangedEventArgs.NewCultureInfo
