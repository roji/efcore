// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

#nullable disable

public static class TestEnvironment
{
    private static readonly string _emulatorAuthToken =
        "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

    public static IConfiguration Config { get; } = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("cosmosConfig.json", optional: true)
        .AddJsonFile("cosmosConfig.test.json", optional: true)
        .AddEnvironmentVariables()
        .Build()
        .GetSection("Test:Cosmos");

    public static string DefaultConnection { get; } = "https://rojitest.documents.azure.com:443";

    public static string AuthToken { get; } = string.IsNullOrEmpty(Config["AuthToken"])
        ? _emulatorAuthToken
        : Config["AuthToken"];

    public static string ConnectionString { get; } = $"AccountEndpoint={DefaultConnection}";

    public static bool UseTokenCredential { get; } = true;

    public static TokenCredential TokenCredential { get; } = new AzureCliCredential();

    public static string SubscriptionId { get; } = "a8a5e977-272e-4e06-9bad-ff9b686a99d4";

    public static string ResourceGroup { get; } = "test";

    public static AzureLocation AzureLocation { get; } = string.IsNullOrEmpty(Config["AzureLocation"])
        ? AzureLocation.NorthEurope
        : Enum.Parse<AzureLocation>(Config["AzureLocation"]);

    public static bool IsEmulator { get; } = !UseTokenCredential && (AuthToken == _emulatorAuthToken);
}
