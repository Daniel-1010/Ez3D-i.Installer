$inputJson = [Console]::In.ReadToEnd()
if ([string]::IsNullOrWhiteSpace($inputJson)) {
    Write-Output '{"decision":"allow"}'
    exit 0
}

try {
    $payload = $inputJson | ConvertFrom-Json
    $cmd = $payload.toolCall.args.CommandLine
    if ($null -ne $cmd) {
        if ($cmd -match '(?i)\bdotnet\s+test\b') {
            $resp = @{
                decision = "deny"
                reason   = "Rule 7 Violation: Do not run 'dotnet test' from the AI IDE (triggers Windows UIPI Access is denied). Ask the user to run it in their Administrator VS Code Terminal."
            } | ConvertTo-Json -Compress
            Write-Output $resp
            exit 0
        }
        if ($cmd -match '(?i)\bgit\s+push\b') {
            $resp = @{
                decision = "force_ask"
                reason   = "Rule 4 Guard: Explicit user permission is required before running 'git push'."
            } | ConvertTo-Json -Compress
            Write-Output $resp
            exit 0
        }
    }
}
catch {
    # Fallback to allow if JSON parsing fails
}

Write-Output '{"decision":"allow"}'
