#requires -Version 7.0
# Standalone integration tests; no Pester, network, or real SSH host required.
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false
$taskRoot = Split-Path -Parent $PSScriptRoot
$handoff = Join-Path $taskRoot 'handoff.ps1'
$pullFrom = Join-Path $taskRoot 'pull-from.ps1'
$fixture = Join-Path ([IO.Path]::GetTempPath()) "citron-handoff-$([Guid]::NewGuid().ToString('N'))"
$oldPath = $env:PATH

function Invoke-FixtureGit {
    param([string] $Repo, [Parameter(ValueFromRemainingArguments)][string[]] $GitArgs)
    $output = & git -C $Repo @GitArgs
    if ($LASTEXITCODE -ne 0) { throw "Fixture Git failed: $GitArgs" }
    $output
}
function Assert([bool] $Condition, [string] $Message) {
    if (-not $Condition) { throw $Message }
}
function Run-Handoff {
    param([string] $Repo, [string] $Action, [int] $Expected = 0)
    & pwsh -NoProfile -File $handoff $Action -Repository $Repo
    Assert ($LASTEXITCODE -eq $Expected) "Unexpected handoff exit for $Action"
}
function Run-PullFrom {
    param([string] $Repo, [string] $SourceRepo, [int] $Expected = 0)
    & pwsh -NoProfile -File $pullFrom OLIVER -Repository $Repo -SourceRepository $SourceRepo
    Assert ($LASTEXITCODE -eq $Expected) 'Unexpected pull-from exit'
}

try {
    New-Item -ItemType Directory -Path $fixture | Out-Null
    $remote = (Join-Path $fixture 'remote.git').Replace('\', '/')
    $a = Join-Path $fixture 'device A'
    $b = Join-Path $fixture 'device B'
    Invoke-FixtureGit $fixture init --bare $remote | Out-Null
    Invoke-FixtureGit $fixture init $a | Out-Null
    Invoke-FixtureGit $a config user.name Test
    Invoke-FixtureGit $a config user.email test@example.invalid
    Invoke-FixtureGit $a config core.autocrlf false
    Set-Content (Join-Path $a 'file.txt') 'base'
    Set-Content (Join-Path $a '.gitignore') 'ignored.txt'
    Invoke-FixtureGit $a add .
    Invoke-FixtureGit $a commit -m base | Out-Null
    Invoke-FixtureGit $a remote add origin $remote
    Invoke-FixtureGit $a push origin HEAD | Out-Null
    Invoke-FixtureGit $fixture clone $remote $b | Out-Null
    Invoke-FixtureGit $b config user.name Test
    Invoke-FixtureGit $b config user.email test@example.invalid
    Invoke-FixtureGit $b config core.autocrlf false
    $headBefore = Invoke-FixtureGit $a rev-parse HEAD

    # An existing origin must not silently substitute for missing configuration.
    $missingConfig = & pwsh -NoProfile -File $handoff check -Repository $a 2>&1
    Assert ($LASTEXITCODE -eq 1) 'Missing handoff.remote was accepted'
    Assert (($missingConfig -join "`n") -like '*git config --local handoff.remote*') 'Missing setup instructions'
    $missingConfig = & pwsh -NoProfile -File $pullFrom OLIVER -Repository $b -SourceRepository $a 2>&1
    Assert ($LASTEXITCODE -eq 1) 'pull-from accepted missing handoff.remote'
    Assert (($missingConfig -join "`n") -like '*git config --local handoff.remote*') 'pull-from omitted setup instructions'
    & pwsh -NoProfile -File $handoff check -Repository $a -Remote origin
    Assert ($LASTEXITCODE -eq 0) 'Explicit remote requires unnecessary configuration'
    Invoke-FixtureGit $a config handoff.remote origin
    Invoke-FixtureGit $b config handoff.remote origin

    Set-Content (Join-Path $a 'file.txt') 'staged'
    Invoke-FixtureGit $a add file.txt
    Add-Content (Join-Path $a 'file.txt') 'unstaged'
    Set-Content (Join-Path $a '한글 untracked.txt') 'new file'
    Set-Content (Join-Path $a 'ignored.txt') 'local only'
    $indexBefore = Invoke-FixtureGit $a write-tree
    $contentBefore = Get-Content (Join-Path $a 'file.txt') -Raw
    Run-Handoff $a push
    Assert (@(Invoke-FixtureGit $a status --porcelain).Count -eq 0) 'Sender was not cleaned'
    Assert (Test-Path (Join-Path $a 'ignored.txt')) 'Ignored file was removed'
    Run-Handoff $b pull
    Assert ((Invoke-FixtureGit $b write-tree) -eq $indexBefore) 'Index was not restored'
    Assert ((Get-Content (Join-Path $b 'file.txt') -Raw) -ceq $contentBefore) 'Working file mismatch'
    Assert (Test-Path (Join-Path $b '한글 untracked.txt')) 'Untracked file missing'
    Assert (-not (Test-Path (Join-Path $b 'ignored.txt'))) 'Ignored file transferred'
    Run-Handoff $b pull 1
    Run-Handoff $b push
    Run-Handoff $a pull
    Assert ((Invoke-FixtureGit $a write-tree) -eq $indexBefore) 'Round trip lost staging'
    Assert ((Invoke-FixtureGit $a rev-parse HEAD) -eq $headBefore) 'HEAD moved'

    # SSH transport stand-in executes the exact encoded remote PowerShell payload.
    # Put the scripts on the source as a normal installation would.
    New-Item -ItemType Directory -Path (Join-Path $a 'task') | Out-Null
    Copy-Item -LiteralPath $handoff -Destination (Join-Path $a 'task/handoff.ps1')
    Add-Content (Join-Path $a '.git/info/exclude') 'task/'
    # Source config must be independent of the receiver's configured origin.
    Invoke-FixtureGit $a remote rename origin github
    Invoke-FixtureGit $a config handoff.remote github
    $mockBin = Join-Path $fixture 'mock-bin'
    New-Item -ItemType Directory -Path $mockBin | Out-Null
    Set-Content (Join-Path $mockBin 'ssh.ps1') @'
param($SshHost, $RemoteCommand)
if ($SshHost -ne 'OLIVER') { exit 2 }
$encoded = ($RemoteCommand -split ' ')[-1]
& pwsh -NoProfile -OutputFormat Text -EncodedCommand $encoded
exit $LASTEXITCODE
'@
    $env:PATH = $mockBin + [IO.Path]::PathSeparator + $env:PATH
    Assert ((Get-Command ssh).Source -eq (Join-Path $mockBin 'ssh.ps1')) 'SSH mock was not selected; refusing to contact a real host'
    # Dirty receiver must stop before touching the source.
    Set-Content (Join-Path $b 'block.txt') 'local change'
    $stashBefore = Invoke-FixtureGit $a rev-parse refs/stash
    Run-PullFrom $b $a 1
    Assert ((Invoke-FixtureGit $a rev-parse refs/stash) -eq $stashBefore) 'Dirty receiver contacted source'
    Remove-Item -LiteralPath (Join-Path $b 'block.txt')
    Invoke-FixtureGit $a config --unset handoff.remote
    Run-PullFrom $b $a 1
    Assert ((Invoke-FixtureGit $a rev-parse refs/stash) -eq $stashBefore) 'Missing source config changed stash'
    Assert ((Get-Content (Join-Path $a 'file.txt') -Raw) -ceq $contentBefore) 'Missing source config changed worktree'
    Invoke-FixtureGit $a config handoff.remote github
    Run-PullFrom $b $a
    Assert ((Invoke-FixtureGit $b write-tree) -eq $indexBefore) 'SSH handoff lost staging'
    Assert (@(Invoke-FixtureGit $a status --porcelain).Count -eq 0) 'SSH source not cleaned'
    # No source changes must fail rather than reapply an old remote snapshot.
    Run-PullFrom $a $a 1

    # Exercise the receiver's config as well, with origin absent on both devices.
    Invoke-FixtureGit $b remote rename origin github
    Invoke-FixtureGit $b config handoff.remote github
    Run-Handoff $b push
    Run-Handoff $a pull
    Run-PullFrom $b $a
    Assert ((Invoke-FixtureGit $b write-tree) -eq $indexBefore) 'Configured receiver lost staging'

    # An explicit -Remote still takes precedence over the device setting.
    Invoke-FixtureGit $a config handoff.remote nonexistent
    & pwsh -NoProfile -File $handoff check -Repository $a -Remote github
    Assert ($LASTEXITCODE -eq 0) 'Explicit remote did not override configuration'
    Run-Handoff $a check 1
    Invoke-FixtureGit $a config handoff.remote github

    Run-Handoff $b push
    Set-Content (Join-Path $a 'different.txt') 'different base'
    Invoke-FixtureGit $a add different.txt
    Invoke-FixtureGit $a commit -m different | Out-Null
    Run-PullFrom $b $a 1
    Run-Handoff $a pull 1
    Assert (@(Invoke-FixtureGit $a status --porcelain).Count -eq 0) 'Base mismatch modified worktree'

    # Rejecting a push after stash creation must retain a recoverable local backup.
    Set-Content (Join-Path $remote 'hooks/pre-receive') "#!/bin/sh`nexit 1`n" -NoNewline
    if (-not $IsWindows) { & chmod +x (Join-Path $remote 'hooks/pre-receive') }
    Set-Content (Join-Path $a 'recover.txt') 'recover me'
    Run-Handoff $a push 1
    Invoke-FixtureGit $a stash apply --index 'stash@{0}' | Out-Null
    Assert ((Get-Content (Join-Path $a 'recover.txt')) -eq 'recover me') 'Failed push lost work'
    if ($IsWindows) {
        # Real Git invokes a fake SSH command: no network, but the intermediate
        # shell must remove descriptor state and preserve SSH command precedence.
        $sshProbe = Join-Path $fixture "ssh probe's.sh"
        [IO.File]::WriteAllText($sshProbe, @'
printf 'SSH_PROBE_STATE=%s;ARG=%s\n' "${c28fc6f98a2c44abbbd89d6a3037d0d9_POSIX_FD_STATE-absent}" "$1" >&2
exit 1
'@.Replace("`r`n", "`n"))
        $probeCommand = 'sh ' + "'" + $sshProbe.Replace('\', '/').Replace("'", "'\''") + "'"
        Invoke-FixtureGit $a remote set-url github 'probe@example.invalid:repo'
        Invoke-FixtureGit $a config core.sshCommand "$probeCommand config"
        $oldSshCommand = $env:GIT_SSH_COMMAND
        $stateName = 'c28fc6f98a2c44abbbd89d6a3037d0d9_POSIX_FD_STATE'
        $oldState = [Environment]::GetEnvironmentVariable($stateName)
        try {
            [Environment]::SetEnvironmentVariable($stateName, 'inherited-state')
            foreach ($mode in @('config', 'environment', 'executable-path')) {
                $oldSsh = $env:GIT_SSH
                try {
                    $env:GIT_SSH_COMMAND = if ($mode -eq 'environment') { "$probeCommand environment" } else { $null }
                    if ($mode -eq 'executable-path') {
                        Invoke-FixtureGit $a config --unset core.sshCommand
                        $env:GIT_SSH = $sshProbe.Replace('\', '/')
                    }
                    $probeOutput = & pwsh -NoProfile -File $handoff status -Repository $a 2>&1
                    Assert ($LASTEXITCODE -eq 1) 'Fake SSH unexpectedly succeeded'
                    Assert (($probeOutput -join "`n") -like '*SSH_PROBE_STATE=absent*') "Descriptor state reached SSH ($mode)"
                    if ($mode -ne 'executable-path') {
                        Assert (($probeOutput -join "`n") -like "*ARG=$mode*") "Wrong SSH command selected ($mode)"
                    }
                }
                finally { $env:GIT_SSH = $oldSsh }
            }
        }
        finally {
            $env:GIT_SSH_COMMAND = $oldSshCommand
            [Environment]::SetEnvironmentVariable($stateName, $oldState)
        }
    }
    Write-Host 'PASS: round trip, staging, untracked/ignored files, dirty receiver, SSH payload, missing local/source config, per-device remotes, explicit remote override, empty source, base mismatch, failed push backup, Windows SSH descriptor-state workaround and SSH command precedence.'
}
finally {
    $env:PATH = $oldPath
    $resolvedFixture = [IO.Path]::GetFullPath($fixture)
    $tempRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
    if ($resolvedFixture.StartsWith($tempRoot, [StringComparison]::OrdinalIgnoreCase) -and
        (Split-Path -Leaf $resolvedFixture) -like 'citron-handoff-*') {
        Remove-Item -LiteralPath $resolvedFixture -Recurse -Force -ErrorAction SilentlyContinue
    }
}
exit 0
