#requires -Version 7.0
<#
.SYNOPSIS
Transfers unfinished work through a single custom Git ref.
.DESCRIPTION
push saves tracked, staged and untracked files with stash push -u, then sends
the snapshot to origin:refs/wip/citron. The worktree becomes clean; the local
stash is retained even if transmission fails. Ignored files are excluded.
pull requires a clean worktree and the same HEAD as the snapshot's base, then
uses stash apply --index. It never switches branches or moves HEAD.
Submodule changes must be handled separately. Do not edit files while running.
The remote ref is shared by all devices; it is not a private storage area.
.EXAMPLE
pwsh -NoProfile -File ./task/handoff.ps1 push
.EXAMPLE
pwsh -NoProfile -File ./task/handoff.ps1 pull
.EXAMPLE
pwsh -NoProfile -File ./task/handoff.ps1 status
.EXAMPLE
pwsh -NoProfile -File ./task/handoff.ps1 push -Remote origin -Ref refs/wip/other
#>
[CmdletBinding()]
param(
    [Parameter(Position = 0, Mandatory)]
    [ValidateSet('push', 'pull', 'status', 'check')]
    [string] $Action,
    [string] $Remote = 'origin',
    [string] $Ref = 'refs/wip/citron',
    [string] $Repository = (Split-Path -Parent $PSScriptRoot),
    [string] $ExpectedHead,
    [switch] $RequireChanges
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
# Check native exit codes explicitly, including when the caller enables this.
$PSNativeCommandUseErrorActionPreference = $false

function Invoke-Git {
    param([Parameter(ValueFromRemainingArguments)][string[]] $GitArgs)
    $result = & git -C $Repository @GitArgs
    if ($LASTEXITCODE -ne 0) {
        throw "git $($GitArgs -join ' ') failed (exit $LASTEXITCODE)."
    }
    $result
}

function Get-RemoteSnapshot {
    $lines = @(Invoke-Git ls-remote --refs -- $Remote $Ref)
    if ($lines.Count -eq 0) { return '' }
    if ($lines.Count -ne 1) { throw 'Expected exactly one remote snapshot ref.' }
    ($lines[0] -split '\s+')[0]
}

try {
    if ($Ref -notlike 'refs/wip/*') { throw '-Ref must be under refs/wip/.' }
    Invoke-Git check-ref-format $Ref | Out-Null
    # Accept a configured remote name, not an option or arbitrary URL.
    if ($Remote -notin @(Invoke-Git remote)) { throw "Unknown remote: $Remote" }
    $Repository = [string](Invoke-Git rev-parse --show-toplevel)
    $headOid = [string](Invoke-Git rev-parse --verify HEAD)
    if ($ExpectedHead -and $headOid -ne $ExpectedHead) {
        throw "HEAD differs from receiving device ($ExpectedHead). No snapshot was created."
    }
    $changes = @(Invoke-Git status --porcelain=v1 --untracked-files=all --ignore-submodules=none)

    if ($Action -eq 'status') {
        $remoteOid = Get-RemoteSnapshot
        Write-Host "HEAD: $headOid"
        Write-Host "${Remote}:${Ref}: $(if ($remoteOid) { $remoteOid } else { '(no snapshot)' })"
        $changes | ForEach-Object { Write-Host $_ }
        exit 0
    }

    # stash does not capture submodule worktrees or reliably restore gitlinks.
    $withoutSubmodules = @(Invoke-Git status --porcelain=v1 --untracked-files=all --ignore-submodules=all)
    if (($changes -join "`n") -cne ($withoutSubmodules -join "`n")) {
        throw 'Submodule changes detected. Commit or stash them separately first.'
    }
    foreach ($state in @('MERGE_HEAD', 'CHERRY_PICK_HEAD', 'REVERT_HEAD', 'rebase-merge', 'rebase-apply', 'sequencer')) {
        $statePath = [string](Invoke-Git rev-parse --git-path $state)
        if (-not [IO.Path]::IsPathRooted($statePath)) { $statePath = Join-Path $Repository $statePath }
        if (Test-Path -LiteralPath $statePath) { throw "Finish the active Git operation first ($state)." }
    }
    if (@(Invoke-Git ls-files --unmerged).Count -gt 0) { throw 'Resolve index conflicts first.' }

    if ($Action -eq 'check') {
        if ($changes.Count -gt 0) { throw 'Receiving requires a clean worktree (including untracked files).' }
        exit 0
    }

    if ($Action -eq 'push') {
        if ($changes.Count -eq 0) {
            if ($RequireChanges) { throw 'Source has no unfinished work to send.' }
            Write-Host 'No changes to send. The remote snapshot was not changed.'
            exit 0
        }
        # Read before stashing so an unavailable remote leaves local work alone.
        $expected = Get-RemoteSnapshot
        $previous = @(& git -C $Repository rev-parse --verify --quiet refs/stash)
        Invoke-Git stash push --include-untracked -m "handoff $([Environment]::MachineName) $([DateTimeOffset]::Now.ToString('o'))" | Out-Host
        $snapshot = [string](Invoke-Git rev-parse --verify refs/stash)
        if ($previous.Count -gt 0 -and $snapshot -eq $previous[0]) { throw 'No new stash was created.' }
        Write-Host "Local backup: $snapshot (retained in git stash list)."
        Write-Host "Restore locally with: git stash apply --index $snapshot"
        Write-Host "Retry transmission with: git push $Remote ${snapshot}:$Ref --force-with-lease=${Ref}:$expected"
        if (@(Invoke-Git status --porcelain=v1 --untracked-files=all --ignore-submodules=none).Count -gt 0) {
            throw 'Worktree is still dirty after stashing; snapshot was not sent. Local stash is retained.'
        }
        # An explicit lease prevents overwriting a snapshot changed during this run.
        Invoke-Git -c push.followTags=false push "--force-with-lease=${Ref}:$expected" -- $Remote "${snapshot}:$Ref" | Out-Host
        Write-Host "Sent $snapshot to ${Remote}:${Ref}. Worktree is clean; local stash retained."
    }
    else {
        if ($changes.Count -gt 0) { throw 'Receiving requires a clean worktree (including untracked files).' }
        # A dedicated ref keeps the fetched object recoverable after later fetches.
        $incoming = "refs/handoff/incoming/$([Guid]::NewGuid().ToString('N'))"
        Invoke-Git fetch --no-tags --no-recurse-submodules -- $Remote "${Ref}:$incoming" | Out-Host
        $snapshot = [string](Invoke-Git rev-parse --verify $incoming)
        Write-Host "Received snapshot: $snapshot (backup: $incoming)."
        $base = [string](Invoke-Git rev-parse --verify "${snapshot}^1")
        if ($headOid -ne $base) {
            throw "HEAD differs from snapshot base $base. Switch to that commit/branch first, then retry pull. Backup retained at $incoming."
        }
        Invoke-Git stash apply --index $snapshot | Out-Host
        Write-Host "Restored staged, unstaged and untracked work. Backup retained at $incoming."
    }
}
catch {
    Write-Error $_ -ErrorAction Continue
    exit 1
}
