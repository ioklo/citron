#requires -Version 7.0
<#
.SYNOPSIS
Pulls unfinished work from an SSH host through the Git remote.
.DESCRIPTION
One-time path configuration on the receiving device:
  git config --local handoff.OLIVER.repository 'Z:/Proj/Citron'
Each device selects its own Git remote independently:
  git config --local handoff.remote github
Without configuration or -Remote, execution fails with setup instructions. -Remote overrides only
the receiving device; the source reads its own handoff.remote setting.
Then run:
  ./task/pull-from.ps1 OLIVER
Both devices need pwsh and git on the SSH/session PATH, a configured Git remote,
and these task scripts installed. SSH authentication uses the normal SSH config.
The receiver must be clean and both devices must have the same HEAD.
The sender is cleaned by stash; its stash backup is retained. Ignored files and
submodule changes are not transferred. Do not edit either worktree during use.
If receiving fails after sending, retry handoff.ps1 pull with the printed Ref.
.EXAMPLE
./task/pull-from.ps1 OLIVER
.EXAMPLE
./task/pull-from.ps1 macbook -SourceRepository '/Users/me/Projects/Citron'
#>
[CmdletBinding()]
param(
    [Parameter(Position = 0, Mandatory)]
    [ValidatePattern('^[A-Za-z0-9][A-Za-z0-9._-]*$')]
    [string] $Source,
    [string] $SourceRepository,
    [string] $Remote,
    [string] $Repository = (Split-Path -Parent $PSScriptRoot)
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false

function Write-PullStep([string] $Message) {
    Write-Host ('[{0:HH:mm:ss}] [{1} pull-from] {2}' -f (Get-Date), [Environment]::MachineName, $Message)
}

function ConvertTo-PSLiteral([string] $Value) {
    "'" + $Value.Replace("'", "''") + "'"
}

try {
    Write-PullStep "Script entered. Source: $Source; local repository: $Repository"
    if (-not $Remote) {
        Write-PullStep 'START git config --get handoff.remote'
        $Remote = & git -C $Repository config --get handoff.remote
        Write-PullStep "END git config --get handoff.remote (exit $LASTEXITCODE)"
        if ($LASTEXITCODE -notin @(0, 1)) { throw 'Could not read handoff.remote configuration.' }
        if (-not $Remote) {
            throw 'handoff.remote is not configured. Set it with: git config --local handoff.remote <remote-name> (or specify -Remote).'
        }
    }
    $handoff = Join-Path $PSScriptRoot 'handoff.ps1'
    # A separate process isolates handoff.ps1's exit statements.
    Write-PullStep 'START local preflight'
    & pwsh -NoProfile -File $handoff check -Repository $Repository -Remote $Remote
    Write-PullStep "END local preflight (exit $LASTEXITCODE)"
    if ($LASTEXITCODE -ne 0) { throw 'Local preflight failed. Source was not contacted.' }

    if (-not $SourceRepository) {
        Write-PullStep "START git config --get handoff.$Source.repository"
        $SourceRepository = & git -C $Repository config --get "handoff.$Source.repository"
        Write-PullStep "END git config --get handoff.$Source.repository (exit $LASTEXITCODE)"
        if ($LASTEXITCODE -notin @(0, 1)) { throw 'Could not read handoff path configuration.' }
    }
    if (-not $SourceRepository) {
        throw "Set the source repository path first: git config --local handoff.$Source.repository '<path on $Source>'"
    }
    Write-PullStep 'START git rev-parse --verify HEAD'
    $headOid = & git -C $Repository rev-parse --verify HEAD
    Write-PullStep "END git rev-parse --verify HEAD (exit $LASTEXITCODE)"
    if ($LASTEXITCODE -ne 0) { throw 'Could not read local HEAD.' }
    # Each invocation owns its ref so concurrent handoffs cannot replace it.
    $transferRef = "refs/wip/handoff/$([Guid]::NewGuid().ToString('N'))"
    Write-Host "Transfer ref: $transferRef"
    Write-Host "If receiving fails, retry: ./task/handoff.ps1 pull -Remote $Remote -Ref $transferRef"
    $payload = @"
`$ErrorActionPreference = 'Stop'
try {
    Write-Host ('[{0:HH:mm:ss}] [{1} pull-from remote] PowerShell entered; loading handoff.ps1.' -f (Get-Date), [Environment]::MachineName)
    `$repo = $(ConvertTo-PSLiteral $SourceRepository)
    & (Join-Path `$repo 'task/handoff.ps1') push -Repository `$repo -Ref $(ConvertTo-PSLiteral $transferRef) -ExpectedHead $(ConvertTo-PSLiteral $headOid) -RequireChanges
    exit `$LASTEXITCODE
} catch {
    Write-Error `$_ -ErrorAction Continue
    exit 1
}
"@
    # Only a fixed command and Base64 reach the remote login shell (PowerShell/zsh).
    $encoded = [Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($payload))
    Write-PullStep "START SSH to $Source; source repository: $SourceRepository"
    & ssh $Source "pwsh -NoLogo -NoProfile -NonInteractive -OutputFormat Text -EncodedCommand $encoded"
    Write-PullStep "END SSH to $Source (exit $LASTEXITCODE)"
    if ($LASTEXITCODE -ne 0) { throw 'Source handoff failed. Local worktree was not changed; inspect source output for its stash backup.' }

    Write-PullStep 'START local restore'
    & pwsh -NoProfile -File $handoff pull -Repository $Repository -Remote $Remote -Ref $transferRef
    Write-PullStep "END local restore (exit $LASTEXITCODE)"
    if ($LASTEXITCODE -ne 0) { throw "Receiving failed. Snapshot remains at ${Remote}:${transferRef}." }
    Write-Host "Handoff from $Source completed. Remote snapshot retained at $transferRef."
}
catch {
    Write-Error $_ -ErrorAction Continue
    exit 1
}
