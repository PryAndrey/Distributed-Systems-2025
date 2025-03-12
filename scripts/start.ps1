$scriptDir = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent

$dotnetProjectDir = Join-Path -Path $scriptDir -ChildPath "../lw-1/Valuator"

Start-Process -FilePath "dotnet" -ArgumentList "run --urls http://localhost:5001" -WorkingDirectory $dotnetProjectDir
Start-Process -FilePath "dotnet" -ArgumentList "run --urls http://localhost:5002" -WorkingDirectory $dotnetProjectDir
Start-Process -FilePath "dotnet" -ArgumentList "run --urls http://localhost:5003" -WorkingDirectory $dotnetProjectDir
Start-Process -FilePath "dotnet" -ArgumentList "run --urls http://localhost:5004" -WorkingDirectory $dotnetProjectDir

docker start my-nginx
docker start my-redis
