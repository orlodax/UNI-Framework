
![UNI Logo](UNI.Core/Graphic%20assets/Logo400pxForAssetGeneration.png)
# UNI Framework

UNI Framework is a comprehensive .NET-based development framework providing both API and UI capabilities for building modern, scalable applications.

### What UNI is about:

Provide a self-generating front-end platform, with the smallest possible codebase, with close to zero boilerplate code. We made some executive decisions regarding the shape/contract/interface of our models. We used some attributes and type T `where T : BaseModel` instantiation of archetype ViewModels which in turn drive the associated archetype Views, _creating individual children controls and bindings to the models' chosen properties at runtime_.<br>
And, these driving ViewModels and Views can be extended and customized with minimal code additions _in the UNI app that runs on top of UNI.Core.UI_ (not on the code present in this repo. See UNI.Core.Explorer for an example.) 

UNI is DRY.<br>
UNI is KISS.<br>
UNI is YAGNI.

### What UNI is NOT about:

UNI is not SOLID. (maybe just the "S")<br>
UNI is not TDD. (you're welcome to help)<br>
UNI is not DDD.<br>
UNI is not exactly CLEAN, not strictly HEXAGONAL nor a VERTICAL SLICE architecture.<br>
UNI has no DI.

UNI was created mainly by a self-taught developer with only 2-3 years of experience total, spent building WPF and UWP apps. He was tired of writing the same code over and over again.<br>
Arguably, that developer would do today a much better job, but he had to move on to other things!

## Framework Components

- **UNI.API Solution** - API framework for building scalable UNI REST API services
- **UNI.Core Solution** - UI framework for building scalable UNI UWP Windows Desktop apps

## Development

1. Choose solution based on needs:
   - `UNI.API.sln` for web services
   - `UNI.Core.sln` for UI components
2. Follow solution-specific README
3. Use provided test instances/explorers

## Contributing

1. Fork repository
2. Create feature branch
3. Implement changes with tests
4. Submit pull request
5. Await review

## Project Structure
```
UNI.Framework/
├── UNI.API/
│   ├── UNI.API/
│   ├── UNI.API.Client/
│   ├── UNI.API.Contracts/
│   └── ...
├── UNI.Core/
│   ├── UNI.Core.Library/
│   ├── UNI.Core.UI/
│   └── ...
└── README.md
```

## License

MIT License - see LICENSE file

## Support

- [Documentation](https://github.com/orlodax/UNI-Framework/wiki)
- [Issue Tracker](https://github.com/orlodax/UNI-Framework/issues)
- [Discussions](https://github.com/orlodax/UNI-Framework/discussions)
- [Code of Conduct](CODE_OF_CONDUCT.md)

## Authors

Built with ❤️ by [orlodax](https://github.com/orlodax) and [Teksistemi](https://github.com/teksistemi-software)