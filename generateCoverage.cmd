@echo off
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:include="[*]NaoBlocks.*"
reportgenerator -reports:*.Tests\coverage.cobertura.xml -targetdir:coverage
coverage\index.htm
