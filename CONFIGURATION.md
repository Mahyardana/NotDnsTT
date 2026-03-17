# NotDnsTT Configuration Guide

This guide provides detailed instructions for configuring and running NotDnsTT client and server applications.

## Overview

NotDnsTT requires careful configuration of encryption keys, domain names, and network endpoints. Configuration is primarily managed through `launchSettings.json` files in each project's `Properties` directory.

## Prerequisites

1. **Generate an Encryption Key**
   - Create a cryptographically secure random key in Base64 format (minimum 32 bytes)
   - Use a tool like `openssl` or your preferred cryptography library:
     ```bash
     openssl rand -base64 32
     ```
   - Store the key securely (use environment variables or a secure vault in production)

2. **Prepare Domain Infrastructure**
   - Own or control domain names for source and destination
   - Ensure DNS resolution is configured correctly
   - Test DNS resolution before deployment

3. **Network Requirements**
   - UDP port availability on specified addresses
   - DNS service access (typically UDP port 53)
   - Firewall rules allowing UDP traffic

## Server Configuration

### Edit `NotDnsTTServer/Properties/launchSettings.json`

```json
{
  "profiles": {
    "DnsMessengerServer": {
      "commandName": "Project",
      "commandLineArgs": "<LOCAL_ADDRESS>:<LOCAL_PORT> <ENCRYPTION_KEY> <DEST_DOMAIN> <SOURCE_DOMAIN> <BACKEND_SERVICE_ADDRESS>:<BACKEND_PORT>"
    }
  }
}
```

### Parameter Explanation

| Parameter | Example | Description |
|-----------|---------|-------------|
| `<LOCAL_ADDRESS>` | `127.0.0.1` | Local IP address to bind DNS listener to |
| `<LOCAL_PORT>` | `54` | Local UDP port for DNS listener |
| `<ENCRYPTION_KEY>` | Base64 string | Shared encryption key (must match client key) |
| `<DEST_DOMAIN>` | `mm.example.com` | Destination domain for response routing |
| `<SOURCE_DOMAIN>` | `auth.example.com` | Source domain to monitor for incoming requests |
| `<BACKEND_SERVICE_ADDRESS>` | `127.0.0.1` | Backend service IP for forwarding traffic |
| `<BACKEND_PORT>` | `22` | Backend service port (SSH, HTTP, etc.) |

### Server Configuration Example

```json
{
  "profiles": {
    "DnsMessengerServer": {
      "commandName": "Project",
      "commandLineArgs": "10.0.0.5:53 YOUR_ENCRYPTION_KEY_HERE dest.mydomain.com src.mydomain.com 10.0.0.10:22"
    }
  }
}
```

**What this does:**
- Listens on 10.0.0.5:53 for DNS queries
- Decrypts client messages using the shared key
- Forwards decrypted data to backend service at 10.0.0.10:22 (SSH)
- Responds via dest.mydomain.com

## Client Configuration

### Edit `NotDnsTTClient/Properties/launchSettings.json`

```json
{
  "profiles": {
    "DnsMessengerClient": {
      "commandName": "Project",
      "commandLineArgs": "<DNS_SERVER_ADDRESS>:<DNS_PORT> <ENCRYPTION_KEY> <SOURCE_DOMAIN> <DEST_DOMAIN> <LOCAL_PROXY_ADDRESS>:<LOCAL_PROXY_PORT>"
    }
  }
}
```

### Parameter Explanation

| Parameter | Example | Description |
|-----------|---------|-------------|
| `<DNS_SERVER_ADDRESS>` | `8.8.8.8` | DNS server to send encoded queries to |
| `<DNS_PORT>` | `53` | DNS server port |
| `<ENCRYPTION_KEY>` | Base64 string | Shared encryption key (must match server key) |
| `<SOURCE_DOMAIN>` | `auth.example.com` | Source domain for query encoding |
| `<DEST_DOMAIN>` | `mm.example.com` | Destination domain for response extraction |
| `<LOCAL_PROXY_ADDRESS>` | `127.0.0.1` | Local proxy listening address |
| `<LOCAL_PROXY_PORT>` | `1080` | Local proxy listening port (SOCKS5, HTTP, etc.) |

### Client Configuration Example

```json
{
  "profiles": {
    "DnsMessengerClient": {
      "commandName": "Project",
      "commandLineArgs": "10.0.0.5:53 YOUR_ENCRYPTION_KEY_HERE src.mydomain.com dest.mydomain.com 127.0.0.1:1080"
    }
  }
}
```

**What this does:**
- Queries DNS server at 10.0.0.5:53
- Encodes outbound data in src.mydomain.com subdomains
- Listens on 127.0.0.1:1080 for local proxy connections
- Extracts responses from dest.mydomain.com

## Running the Applications

### Build the Solution

```bash
cd NotDnsTT-1
dotnet build NotDnsTT.slnx
```

### Run the Server

```bash
cd NotDnsTTServer
dotnet run
```

Output will indicate the DNS server is listening on the configured address and port.

### Run the Client

In a separate terminal:

```bash
cd NotDnsTTClient
dotnet run
```

The proxy will be available on the configured local address and port for connections.

## Security Best Practices

### Key Management
- **Never hardcode keys** in source repositories
- Use environment variables:
  ```bash
  set ENCRYPTION_KEY=your_key_here
  dotnet run
  ```
- Rotate keys periodically
- Use different keys for different deployments

### Network Security
- **Isolate networks** - Deploy in controlled network segments
- **Firewall rules** - Restrict DNS traffic to authorized hosts
- **Monitor traffic** - Log and analyze DNS queries for anomalies
- **Rate limiting** - Implement query rate limits to detect abuse

### Operational Security
- **Disable logging** in production environments (review source code)
- **Use HTTPS/TLS** for any management interfaces
- **Principle of least privilege** - Run with minimal required permissions
- **Secure boot** - Ensure code integrity and authenticity

### Domain and DNS
- Use subdomains that blend with legitimate traffic
- Implement DNS query randomization
- Monitor for unusual DNS patterns
- Consider using DNS over HTTPS (DoH) for stealth

## Troubleshooting

### Connection Failed
- Verify DNS server is reachable: `ping <DNS_SERVER_ADDRESS>`
- Check firewall rules for UDP port restrictions
- Confirm encryption keys match between client and server

### Decryption Errors
- Ensure encryption key is identical on both sides
- Verify key encoding (Base64 format)
- Check for data corruption in transit

### Proxy Not Responding
- Verify client is running and listening on configured address
- Check local firewall for proxy port restrictions
- Confirm domain names are correctly configured

### Performance Issues
- Reduce query frequency if network constrained
- Monitor DNS server for rate limiting
- Check for packet fragmentation on large transfers

## Environment Variables (Optional)

For improved security, pass configuration via environment variables:

```bash
# Linux/macOS
export LOCAL_ADDRESS=127.0.0.1
export LOCAL_PORT=54
export ENCRYPTION_KEY=your_key
export DEST_DOMAIN=dest.example.com
export SOURCE_DOMAIN=src.example.com
export BACKEND_ADDRESS=127.0.0.1
export BACKEND_PORT=22
dotnet run

# Windows PowerShell
$env:ENCRYPTION_KEY = "your_key"
dotnet run
```

## Production Deployment

### Checklist
- [ ] Encryption keys generated and securely stored
- [ ] Domain infrastructure configured and tested
- [ ] Firewall rules implemented
- [ ] Logging and monitoring configured
- [ ] Security audit completed
- [ ] Disaster recovery plan in place
- [ ] Keys backed up securely
- [ ] Documentation updated with deployment details

### Scaling Considerations
- Load balance DNS servers for high traffic
- Implement certificate pinning if using DNS-over-HTTPS
- Monitor resource usage (CPU, memory, bandwidth)
- Plan for key rotation procedures

## Additional Resources

- DNS Protocol Reference: RFC 1035
- Base32 Encoding: RFC 4648
- .NET 8.0 Documentation: https://docs.microsoft.com/en-us/dotnet/

---

**Last Updated:** 2026
**Configuration Version:** 1.0
