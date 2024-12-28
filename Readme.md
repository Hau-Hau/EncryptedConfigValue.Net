EncryptedConfigValue.NET
=========================
[![Test](https://github.com/Hau-Hau/EncryptedConfigValue.Net/actions/workflows/test.yml/badge.svg?branch=main)](https://github.com/Hau-Hau/EncryptedConfigValue.Net/actions/workflows/test.yml)

> In sync with encrypted-config-value v6.0.0 ([249bbf7c5522fa1e703cadaf7be53f7e5d270bde](https://github.com/palantir/encrypted-config-value/tree/249bbf7c5522fa1e703cadaf7be53f7e5d270bde))

`EncryptedConfigValue.NET` is a .NET implementation of the Palantir's [encrypted-config-value](https://github.com/palantir/encrypted-config-value)
library.

This repository provides tooling for encrypting certain configuration parameter values in ASP.NET Core apps. This defends against accidental leaks of sensitive information such as copy/pasting a config file.

EncryptedConfigValue.AspNetCore
-----------------------------
A `EncryptedConfigValue.AspNetCore` package provides a way of using encrypted values in your ASP.NET Core _appsettings.json_ files.
 
Currently supported algorithms:
 - AES: (AES/GCM/NoPadding) with random IV
 - RSA

Install from [NuGet](https://www.nuget.org/packages/EncryptedConfigValue.Net.AspNetCore):
```powershell
Install-Package EncryptedConfigValue.Net.AspNetCore  
```

To use in your app:
```console
// If needed, set environment variable, default is var/conf/encrypted-config-value.key
my-application$ export EncryptedConfigValue_Config_KeyPath=conf/encrypted-config-value.key 
```

```json
// appsettings.json
{
  "Encrypted": "${enc:INNv4cGkVF45MLWZhgVZdIsgQ4zKvbMoJ978Es3MIKgrtz5eeTuOCLM1vPbQm97ejz2EK6M=}",
}
```

```csharp
// Program.cs
using EncryptedConfigValue.AspNetCore;
using EncryptedConfigValue.Crypto;

// Optionally you can set environment variable in application
// Environment.SetEnvironmentVariable(KeyFileUtils.KeyPathProperty, "conf/encrypted-config-value.key");

var builder = WebApplication.CreateBuilder(args).AddEncryptedConfigValueProvider();
```

 EncryptedConfigValue.Cli
-----------------------------
A `EncryptedConfigValue.Cli` project provides CLI tools for generating keys and encrypting values.

The CLI tool provides following commands:
 - `encrypt-config-value [-v <value>] [-k <keyfile>]` for encrypting values. In the case of non-symmetric algorithms (e.g. RSA) specify the public key. If `-v <value>` not provided, program will explicitly ask about value by running interactive mode. On Windows OS it is recommended to provide `keyfile` parameter as default path points to `var\conf\encrypted-config-value.key`.
 - `generate-random-key -a <algorithm> [-f <keyfile>]` for generating random keys with the specified algorithm. In the case of non-symmetric algorithms (e.g. RSA) the private key will have a .private extension. On Windows OS it is recommended to provide `keyfile` parameter as default path points to `var\conf\encrypted-config-value.key`. 
 
Currently supported algorithms:
 - AES: (AES/GCM/NoPadding) with random IV
 - RSA

Install from [NuGet](https://www.nuget.org/packages/EncryptedConfigValue.Net.Cli):
```console
dotnet tool install -g EncryptedConfigValue.Net.Cli
```

To generate keys:
 ```console
my-application$ encrypted-config-value-dotnet generate-random-key -a AES
Wrote key to var/conf/encrypted-config-value.key
```

To encrypt value:
 ```console
my-application$ encrypted-config-value-dotnet encrypt-config-value -v "secret-value"
enc:eyJUeXBlIjoiQUVTIiwiRW5jcnlwdGlvbk1vZGUiOjAsIkl2IjoiUFZkMDJqbkczQ2FCS2t4MyIsIkNpcGhlclRleHQiOiJMSXMraHNuU0dZUXVVWmc9IiwiVGFnIjoiLzRVeVN0ckpnNjRacGJUdGJRTWEzZz09In0=
```

EncryptedConfigValue.Module
-----------------------------
You can use `EncryptedConfigValue.Module` to create your own decrypt provider.

Install from [NuGet](https://www.nuget.org/packages/EncryptedConfigValue.Net.Module):
```powershell
Install-Package EncryptedConfigValue.Net.Module
```

Differences from [palantir/encrypted-config-value](https://github.com/palantir/encrypted-config-value)
-----------------------------
- Name of environmental variable containing path to key is _EncryptedConfigValue_Config_KeyPath_ instead of _palantir.config.key_path_.
- Since [palantir/encrypted-config-value v6.0.0]([https://github.com/palantir/encrypted-config-value](https://github.com/palantir/encrypted-config-value/releases/tag/6.0.0)) there is no CLI tool included in Java version. `EncryptedConfigValue.NET` still provides support for commandline tools.
- Commands _encrypt-config-value_ and _generate-random-key_ don't need explicit _-v_ parameter. If _-v_ not provided then program will start interactive mode.

Note
-----------------------------
The project has been devised to align with the original functionality. Please refrain from suggesting changes that would alter how it works compared to the original. Any adjustments, additions, or removals should be carefully considered to ensure they align seamlessly with the established framework.
