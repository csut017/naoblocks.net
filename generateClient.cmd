@echo off
echo Building Client
cd client
call ng build --prod
cd ..

echo Cleaning old folder
del /F/Q/S NaoBlocks.Web\wwwroot\*.* 
rmdir /Q/S NaoBlocks.Web\wwwroot

echo Copying to server folder
mkdir /Q/S NaoBlocks.Web\wwwroot
xcopy client\dist\client\*.* NaoBlocks.Web\wwwroot\ /S