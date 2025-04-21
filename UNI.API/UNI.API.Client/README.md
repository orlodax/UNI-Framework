# UNI.API.Client

`UNI.API.Client` is a lightweight and extensible .NET library designed to simplify communication with the UNI API. It provides a clean and intuitive interface for authenticating users, making API requests, and handling responses.

## Features

- **Authentication**: Easily authenticate users and manage tokens (JWT).
- **Extensibility**: Designed to be extended for custom API endpoints.
- **Asynchronous Operations**: Fully supports asynchronous programming with `async`/`await`.
- **Error Handling**: Built-in mechanisms for handling API errors and token validation.

## Installation

You can install the `UNI.API.Client` package via NuGet:
```shell
dotnet add package UNI.API.Client
```
Or, in the NuGet Package Manager Console:
```shell
Install-Package UNI.API.Client
```

## Getting Started

### 1. Initialize the Client

To start using the `UNI.API.Client`, create an instance of the `UNIClient<T>` class, where `T` is your user model.


### 2. Authenticate a User

Use the `Authenticate` method to log in a user and retrieve a token.

### 3. Validate the Token

You can validate the token using the built-in helper:

### 4. Make API Requests

Once authenticated, you can use the client to make API requests. Extend the client as needed to add custom methods for your API endpoints.

## Example Usage

Here’s a complete example:

```csharp
using System; 
using UNI.API.Client;

public class Program 
{ 
    public static async Task Main(string[] args) 
    { 
        var client = new UNIClient<MyUserModel>();
        string username = "testuser";
        string password = "password123";

        try
        {
            string token = await client.Authenticate(username, password);

            if (JWTHelper.IsTokenValid(token))
            {
                Console.WriteLine("Authentication successful!");
                // Proceed with API requests
            }
            else
            {
                Console.WriteLine("Invalid token.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
```

## Dependencies

- .NET Standard 2.0
- Any additional dependencies required by the library (e.g., `Newtonsoft.Json` for JSON handling, if applicable).

## Contributing

Contributions are welcome! If you’d like to contribute, please fork the repository and submit a pull request.

## License

This project is licensed under the MIT License.

## Support

If you encounter any issues or have questions, feel free to open an issue in the GitHub repository.

---

Happy coding!