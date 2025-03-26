param (
    [Parameter(Mandatory=$false)]
    [string]$Host = "localhost",
    
    [Parameter(Mandatory=$false)]
    [int]$Port = 9042,

    [Parameter(Mandatory=$false)]
    [string]$Username = "babbly_user",

    [Parameter(Mandatory=$false)]
    [string]$Password = "babbly_password",

    [Parameter(Mandatory=$false)]
    [string]$File
)

$ScriptPath = Split-Path -Parent $MyInvocation.MyCommand.Definition
$InitScriptsPath = Join-Path -Path $ScriptPath -ChildPath "init-scripts"

if ($File) {
    # Execute a specific file
    $TargetFile = $File
    Write-Host "Applying CQL script: $TargetFile"
    
    docker exec -it $(docker ps -q -f name=cassandra) cqlsh -u $Username -p $Password -f /scripts/$(Split-Path -Leaf $TargetFile)
}
else {
    # Execute all scripts in order
    Write-Host "Applying all CQL scripts in $InitScriptsPath"
    
    Get-ChildItem -Path $InitScriptsPath -Filter "*.cql" | Sort-Object Name | ForEach-Object {
        Write-Host "Applying CQL script: $($_.Name)"
        docker exec -it $(docker ps -q -f name=cassandra) cqlsh -u $Username -p $Password -f /scripts/$($_.Name)
    }
} 