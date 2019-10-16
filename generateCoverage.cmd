dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
reportgenerator -reports:*.Tests\coverage.cobertura.xml -targetdir:coverage
coverage\index.htm
