# UNI.Core.Library

`UNI.Core.Library` is a foundational .NET library providing base entities and POCO models for the UNI Framework ecosystem. It serves as the core building block for defining common data structures and base models used across UNI applications.

## Features

- **Base Entities**: Core model classes and base entities
- **Metadata Support**: Built-in support for model metadata
- **Cross-Platform**: Built on .NET Standard 2.0 for maximum compatibility
- **Lightweight**: Minimal external dependencies

## Installation

You can install the `UNI.Core.Library` package via NuGet:
```shell
dotnet add package UNI.Core.Library
```
Or, in the NuGet Package Manager Console:
```shell
Install-Package UNI.Core.Library
```

## Getting Started

### 1. Reference the Package

Add a reference to UNI.Core.Library in your project.

### 2. Import Namespaces

```csharp
using UNI.Core.Library.Models;
using UNI.Core.Library.Entities;
```

### 3. Use Base Models

Inherit from base models to create your domain entities:

```csharp
public class MyEntity : BaseModel
{
    public string Name { get; set; }
    public string Description { get; set; }
}
```

## Model Features

- **BaseModel**: Provides common properties like ID, creation date, and metadata
- **Metadata Support**: Extensible metadata system for entity tracking
- **Serialization**: Built-in support for proper serialization
- **Type Safety**: Strong typing for all models

## Dependencies

- .NET Standard 2.0

## Contributing

Contributions are welcome! If you'd like to contribute, please fork the repository and submit a pull request.

## License

This project is licensed under the MIT License.

## Support

If you encounter any issues or have questions, feel free to open an issue in the GitHub repository.

---

Happy coding!