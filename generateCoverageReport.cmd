@echo off

echo Cleaning up
del /s /q coverage.cobertura.xml

echo Running tests
dotnet test --collect:"XPlat Code Coverage"

echo Generating reports
reportgenerator -reports:Tests\*\TestResults\*\coverage.cobertura.xml -targetdir:Tests\Coverage -historydir:Tests\History -reporttypes:Html;TextDeltaSummary

if %1 equ open ( 
    echo Opening coverage report
    Tests\Coverage\index.htm
)
