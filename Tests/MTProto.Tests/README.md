# How to add Tests to a csharp project.


* Create the MTProto.Tests project by running the following command:

```sh
dotnet new xunit -o MTProto.Tests
```

* Add the test project to the solution file by running the following command:
```sh
dotnet sln add ./MTProto.Tests/MTProto.Tests.csproj
```

* Add the MTProto project as a dependency to the MTProto.Tests project:
```sh
dotnet add ./Tests/MTProto.Tests/MTProto.Tests.csproj reference ./MTProto/MTProto.csproj
```


<hr/>

* References:
 - [Unit testing in .NET Core](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test)
