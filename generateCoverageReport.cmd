del /s /q coverage.cobertura.xml
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:Tests\*\TestResults\*\coverage.cobertura.xml -targetdir:Tests\Coverage