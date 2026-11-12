# Changelog

## [3.0.0] - 2026-11-12
### Changed
- Added structured error details and configurable error handling while retaining the existing error option for compatibility.
- Upgraded the task and test projects to .NET 8.
- **[Breaking Change]** Result now rethrows original exception instead of creating a new general one.
- **[Breaking Change]** `ErrorMessage` property has been removed from the `Result` class. Use `Error.Message` instead.

## [2.0.0] - 2025-09-03
### Changed
- [Breaking] Result now returns a dynamic JToken instead of DataTable when using ExecuteReader mode.
- Added support for CancellationToken, thus enabling query cancellation.

## [1.2.0] - 2025-09-03
### Added
  - Added support for Snowflake key pair authentication:
    Input.PrivateKeyFilePath: full path to the .p8 private key file.
    Input.PrivateKeyPassphrase: optional passphrase for encrypted private keys.


## [1.1.0] - 2025-02-12
### Changed
- Update:
    Snowflake.Data        2.1.5 -> 4.3.0
    MSTest.TestAdapter    3.8.0 -> 3.8.3
    MSTest.TestFramework  3.8.0 -> 3.8.3


## [1.0.0] - 2024-01-08
### Added
- Initial implementation
