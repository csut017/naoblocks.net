@echo off
echo Building Client
cd Clients\Angular
call ng build --source-map
cd ..
cd ..

echo Cleaning old folder
del /F/Q/S Server\NaoBlocks.Web\wwwroot\*.* 
rmdir /Q/S Server\NaoBlocks.Web\wwwroot

echo Copying to server folder
mkdir Server\NaoBlocks.Web\wwwroot
xcopy Clients\Angular\dist\angular\*.* Server\NaoBlocks.Web\wwwroot\ /S