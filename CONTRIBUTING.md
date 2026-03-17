# Contributing to NotDnsTT

We appreciate your interest in contributing to NotDnsTT. This document provides guidelines for contributions.

## Code of Conduct

- Be respectful and inclusive
- No harassment, discrimination, or abusive language
- Constructive criticism and feedback only

## Getting Started

1. **Fork the Repository**
   ```bash
   git clone https://github.com/yourusername/NotDnsTT.git
   cd NotDnsTT
   ```

2. **Set Up Development Environment**
   - Install .NET 8.0 SDK or later
   - Install Visual Studio Code or Visual Studio
   - Recommended extensions: C#, .NET Extension Pack

3. **Create a Branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

## Development Guidelines

### Code Standards
- Follow Microsoft C# coding conventions
- Use meaningful variable and method names
- Add comments for complex logic
- Keep functions small and focused

### Building & Testing
```bash
# Build the solution
dotnet build NotDnsTT.slnx

# Build Release configuration
dotnet build NotDnsTT.slnx -c Release

# Test specific project
dotnet build NotDnsTTClient/NotDnsTTClient.csproj
```

### Commit Messages
- Use clear, descriptive commit messages
- Format: `[type] brief description`
- Types: `[feature]`, `[fix]`, `[docs]`, `[refactor]`, `[test]`
- Example: `[feature] Add DNS query caching`

### Pull Request Process

1. **Before Submitting**
   - Ensure code compiles without errors
   - Test on both Windows and Linux if possible
   - Update documentation as needed
   - Follow code style guidelines

2. **Submission**
   - Create PR against `main` or `develop` branch
   - Use descriptive title and detailed description
   - Link related issues: `Closes #123`
   - Include any breaking changes clearly

3. **Review Process**
   - Maintainers will review your changes
   - Address feedback and suggestions
   - Ensure CI/CD pipeline passes
   - All conversations must be professional

## Issue Reporting

**Bug Reports**: Use the [bug report template](.github/ISSUE_TEMPLATE/bug_report.md)
- Include reproduction steps
- Provide environment details (.NET version, OS, architecture)
- Do NOT include sensitive data (keys, domains, IPs)

**Feature Requests**: Use the [feature request template](.github/ISSUE_TEMPLATE/feature_request.md)
- Describe use case clearly
- Suggest implementation approach

## Security

⚠️ **IMPORTANT**: If you discover a security vulnerability:
1. **DO NOT** open a public issue
2. **DO NOT** include vulnerability details in pull requests
3. Email security details to maintainers privately
4. Allow time for a patch before public disclosure

## Documentation

When contributing code changes:
- Update relevant documentation
- Add examples for new features
- Update README.md if appropriate
- Keep CONFIGURATION.md current

## Architecture Overview

```
NotDnsTT/
├── DnsMessengerEncryption/  # Encryption and encoding
│   ├── Base32Lower          # Custom Base32 encoding
│   ├── Crc32                # Checksum validation
│   ├── Encryption           # Core encryption logic
│
├── NotDnsTTClient/          # Client tunnel endpoint
│   ├── UDPSocket            # Socket management
│   ├── Main logic           # Proxy and routing
│
└── NotDnsTTServer/          # Server tunnel endpoint
    ├── UDPSocket            # Socket management
    └── Main logic           # Request handling
```

## Key Components

### Encryption Layer
- Located in: `DnsMessengerEncryption/Encryption.cs`
- Handles: Key generation, encryption/decryption
- Consider: Performance, crypto best practices

### Network Layer
- Located in: `NotDnsTTClient/Program.cs`, `NotDnsTTServer/Program.cs`
- Handles: UDP sockets, DNS protocol, threading
- Consider: Connection handling, error recovery

## Testing

Currently no automated tests. If adding tests:
- Use xUnit or NUnit framework
- Place in `*.Tests` projects
- Ensure >80% code coverage
- Run: `dotnet test`

## Release Process

1. **Version Numbering**: Semantic Versioning (MAJOR.MINOR.PATCH)
2. **Tags**: Format as `v1.2.3` (e.g., `v1.0.0`, `v1.1.0-rc1`)
3. **Build**: GitHub Actions automatically builds on tags
4. **Release Notes**: Update CHANGELOG.md
5. **Publishing**: Artifacts automatically created and published

## Performance Considerations

- Minimize allocations in hot paths
- Use object pooling for high-frequency operations
- Profile before optimizing
- Document performance-critical sections

## Questions?

- Check existing issues and discussions
- Read the [CONFIGURATION.md](CONFIGURATION.md)
- Review the [README.md](README.md)

---

Thank you for contributing! 🎉
