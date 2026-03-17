# GitHub Actions Workflows - Documentation

This document describes the GitHub Actions workflows configured for NotDnsTT.

## Workflows Overview

### 1. Build and Release (`build-and-release.yml`)

**Trigger**: 
- Automatically on version tags (v*.*)
- Manual workflow dispatch

**Platforms Built**:
- **Windows**: x64, x86, ARM64
- **Linux**: x64, ARM, ARM64, musl-x64

**What It Does**:
1. Checks out the code
2. Builds self-contained executables for each platform using:
   - PublishSingleFile (single executable)
   - PublishTrimmed (reduced file size)
   - PublishReadyToRun (pre-compiled MSIL)
3. Packages binaries:
   - Windows: ZIP archives
   - Linux: TAR.GZ archives
4. Creates a GitHub Release with all artifacts

**Artifacts Produced**:
- `NotDnsTTClient-{platform}.{zip|tar.gz}`
- `NotDnsTTServer-{platform}.{zip|tar.gz}`

**Release Details**:
- Auto-generates release notes
- Marks pre-releases for tags with `rc`, `beta`, or `alpha`
- Includes platform compatibility information

### 2. Build Only (`build-only.yml`)

**Trigger**: 
- Push to main, master, develop branches
- Pull requests to main, master, develop branches

**Platforms**:
- Windows (latest)
- Linux (latest)

**What It Does**:
1. Builds solution in Release and Debug configurations
2. Checks for compilation warnings
3. Uploads build artifacts for 7 days
4. Runs on every PR to ensure code quality

**Purpose**: Quick validation that code compiles on both major platforms

### 3. CodeQL Analysis (`codeql-analysis.yml`)

**Trigger**:
- Push to main, master, develop branches
- Pull requests
- Weekly schedule (Sunday at midnight UTC)

**What It Does**:
1. Initializes GitHub's CodeQL engine for C#
2. Analyzes code for:
   - Security vulnerabilities
   - Code quality issues
   - Best practice violations
3. Reports findings in GitHub Security tab

**Purpose**: Automated security scanning without external dependencies

## How to Use These Workflows

### Creating a Release

1. **Create a Version Tag**
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```

2. **GitHub Actions Automatically**:
   - Builds all 7 platform variants
   - Creates release artifacts
   - Publishes GitHub Release
   - Takes ~10-15 minutes

3. **Results Available At**:
   - GitHub Releases page
   - Download executables for your platform

### Pre-Release Versions

For release candidates, betas, or alphas:
```bash
git tag v1.0.0-rc1
git tag v1.0.0-beta1
git tag v1.0.0-alpha1
git push origin v1.0.0-rc1
```

These are automatically marked as pre-releases.

### Manual Workflow Dispatch

Manually trigger the build-and-release workflow:
1. Go to "Actions" tab on GitHub
2. Select "Build and Release"
3. Click "Run workflow"
4. (Optional) Enter custom release name
5. Click "Run workflow"

## Artifact Information

### Executables per Platform

**Windows**
- `win-x64` - 64-bit Intel/AMD
- `win-x86` - 32-bit Intel/AMD
- `win-arm64` - ARM64 (Surface Pro X, etc.)

**Linux**
- `linux-x64` - 64-bit Intel/AMD (glibc)
- `linux-arm` - 32-bit ARM v7 (glibc)
- `linux-arm64` - 64-bit ARM v8+ (glibc)
- `linux-musl-x64` - 64-bit Intel/AMD (musl, Alpine Linux)

### Archive Contents

Each archive contains:
- Main executable
- Required .NET runtime libraries
- Dependencies (Ae.Dns.Protocol.dll, etc.)
- Everything needed to run standalone

## Configuration and Customization

### Build Options

Modify `build-and-release.yml` to:

**Add new platforms**:
```yaml
- os: ubuntu-latest
  target: linux-s390x
  rid: linux-s390x
  artifact_suffix: ''
```

**Change output locations**:
Modify `--output ./publish/...` paths

**Adjust trimming**:
Toggle `PublishTrimmed=true/false` for larger/smaller binaries

### Release Notes

Auto-generated from workflow file. To customize:
1. Edit the `body:` section in `create-release` job
2. Add dynamic content using GitHub variables
3. Include changelog information

### Build Secrets

If you need to:
- Sign executables
- Upload to package repositories
- Notify external systems

Add secrets in GitHub Settings → Secrets and Variables → Actions, then use:
```yaml
env:
  MY_SECRET: ${{ secrets.MY_SECRET_NAME }}
```

## Troubleshooting

### Build Failures

1. **Check build logs**:
   - Go to "Actions" → Recent run → Click specific job
   - Review "Build" step output
   - Look for compiler errors

2. **Common issues**:
   - `.NET SDK not found`: Verify `dotnet-version` in workflow
   - `NuGet restore failed`: Check internet connectivity
   - `Platform not supported`: Verify RID (Runtime Identifier) is valid

### Release Not Created

1. **Verify tag format**: Must match `v*.*` pattern
2. **Check workflow permissions**: Settings → Actions → General
   - `Read and write permissions` should be enabled
3. **Monitor Actions tab**: See if workflow is running

### Artifacts Not Uploading

1. **Check `upload-artifact` step**: Verify paths exist
2. **Verify storage quota**: Organizations have storage limits
3. **Review retention policy**: 30-day retention may delete old artifacts

## Best Practices

### Version Tags
- Use semantic versioning: `v1.2.3`
- Tag only on main branch after PR review
- Include release notes in summary

### Build Artifacts
- Always test downloaded binaries before release
- Verify executables run on target platform
- Document any platform-specific requirements

### Security
- Review code before tagging
- Ensure no secrets in commits
- Enable branch protection rules

## Performance Notes

- Build times: ~5-15 minutes per platform
- Total run with all platforms: ~20-30 minutes
- Parallel builds: Up to 7 simultaneous (may be limited by plan)

## Maintenance

- Review workflows quarterly
- Update .NET version as needed
- Add new platforms as they gain adoption
- Remove deprecated OS versions

---

For more information, see:
- [README.md](README.md)
- [CONFIGURATION.md](CONFIGURATION.md)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
