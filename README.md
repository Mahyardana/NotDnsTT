# NotDnsTT - DNS-based Covert Tunneling

A .NET 8.0 application that implements a DNS-based covert communication tunnel, enabling secure command and control (C&C) or proxy functionality through DNS queries and responses.

## Overview

NotDnsTT consists of three main components:

- **DnsMessengerEncryption**: A cryptographic library handling encryption, decryption, and encoding/decoding for tunneled data
- **NotDnsTTClient**: Client application that initiates tunneled connections through DNS
- **NotDnsTTServer**: Server application that receives and routes tunneled traffic

## Features

- DNS-based covert channel for bypassing network restrictions
- End-to-end encryption of tunneled data
- Base32 encoding for DNS-compatible data transmission
- CRC32 checksum validation for data integrity
- Support for UDP-based sockets and async operations
- Modular architecture with reusable encryption components

## Requirements

- .NET 8.0 or later
- Windows or Linux (tested on Windows)
- Network access to DNS services
- Administrator/root privileges for network operations

## Project Structure

```
NotDnsTT/
├── DnsMessengerEncryption/          # Encryption/encoding library
│   ├── Encryption.cs                # Main encryption implementation  
│   ├── DnsMessengerEncryption.csproj
├── NotDnsTTClient/                  # Client application
│   ├── Program.cs                   # Client entry point
│   ├── NotDnsTTClient.csproj
│   ├── Properties/
│   │   └── launchSettings.json      # Client configuration
├── NotDnsTTServer/                  # Server application
│   ├── Program.cs                   # Server entry point
│   ├── NotDnsTTServer.csproj
│   ├── Properties/
│   │   └── launchSettings.json      # Server configuration
└── NotDnsTT.slnx                    # Solution file
```

## Building

Navigate to the workspace root and build using .NET CLI:

```bash
dotnet build NotDnsTT.slnx
```

Or build individual projects:

```bash
dotnet build DnsMessengerEncryption/DnsMessengerEncryption.csproj
dotnet build NotDnsTTClient/NotDnsTTClient.csproj
dotnet build NotDnsTTServer/NotDnsTTServer.csproj
```

## Configuration

See [CONFIGURATION.md](CONFIGURATION.md) for detailed setup and configuration instructions.

## Dependencies

- Ae.Dns.Protocol - DNS protocol implementation for parsing and constructing DNS messages
- System namespaces for networking, cryptography, and threading

## Security Considerations

- **Encryption Key**: Must be generated securely and kept confidential
- **Domain Configuration**: Use domains under your control for IP resolution
- **Network Isolation**: Deploy in isolated networks or with strict firewall rules
- **Audit Logging**: Implement comprehensive logging for monitoring and forensics

⚠️ **Warning**: This tool implements covert communication channels. Ensure all usage complies with applicable laws and organizational policies. Unauthorized use may violate laws and regulations.

## License

Check LICENSE file (if present) for usage terms.

## Support

For issues, questions, or contributions, refer to the project repository documentation.
