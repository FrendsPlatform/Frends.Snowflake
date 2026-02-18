# Frends.Snowflake.BatchOperation

Task to run a batch operation in Snowflake

[![BatchOperation_build](https://github.com/FrendsPlatform/Frends.Snowflake/actions/workflows/BatchOperation_test_on_main.yml/badge.svg)](https://github.com/FrendsPlatform/Frends.Snowflake/actions/workflows/BatchOperation_test_on_main.yml)
![Coverage](https://app-github-custom-badges.azurewebsites.net/Badge?key=FrendsPlatform/Frends.Snowflake/Frends.Snowflake.BatchOperation|main)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT)

## Installing

You can install the Task via Frends UI Task View.

## Building

### Clone a copy of the repository

`git clone https://github.com/FrendsPlatform/Frends.Snowflake.git`

### Build the project

`dotnet build`

### Run tests

To run tests, you need to have a configured Snowflake user that has a service type and will use Key Pairs to validate.
If the user is not configured, you need to generate a new key pair.
Public one should be added to database user info
Private one should be provided as a base64-encoded string through environment variable.

Run the tests

`dotnet test`

### Create a NuGet package

`dotnet pack --configuration Release`

### StyleCop.Analyzers Version

This project uses StyleCop.Analyzers 1.2.0-beta.556, as recommended by the author, to get the latest fixes and
improvements not available in the last stable release.

### Third-party licenses

This project uses Snowflake.Data package under [the Apache 2.0 license](./Apache-2.0).
