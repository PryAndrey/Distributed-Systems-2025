$scriptDir = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent

$dotnetProjectDir = Join-Path -Path $scriptDir -ChildPath "../lw-1/Valuator"
$dotnetProjectDir1 = Join-Path -Path $scriptDir -ChildPath "../lw-1/RankCalculator"
$dotnetProjectDir2 = Join-Path -Path $scriptDir -ChildPath "../lw-1/EventsLogger"

Start-Process -FilePath "dotnet" -ArgumentList "run --urls http://localhost:5001" -WorkingDirectory $dotnetProjectDir
Start-Process -FilePath "dotnet" -ArgumentList "run --urls http://localhost:5002" -WorkingDirectory $dotnetProjectDir
Start-Process -FilePath "dotnet" -ArgumentList "run --urls http://localhost:5005" -WorkingDirectory $dotnetProjectDir1
Start-Process -FilePath "dotnet" -ArgumentList "run --urls http://localhost:5006" -WorkingDirectory $dotnetProjectDir2
Start-Process -FilePath "dotnet" -ArgumentList "run --urls http://localhost:5007" -WorkingDirectory $dotnetProjectDir2

docker start my-redis 
docker start my-nginx 
docker start my-centrifugo
docker start rabbitmq