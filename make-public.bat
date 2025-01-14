@REM Defines the certificate to use for signatures.
@REM Use Powershell to find the available certs:
@REM
@REM PS> Get-ChildItem -Path Cert:CurrentUser\My
@REM
set cert=73E7B9D1F72EDA033E7A9D6B17BC37A96CE8513A
set timestamp=http://timestamp.sectigo.com

@REM ==================
@REM Compile C# sources
@REM ==================
copy /Y Global.public.cs Global.cs
msbuild /p:Configuration=Release

@REM ==================
@REM Build an installer
@REM ==================
iscc.exe /Opublic FlexConfirmMail.iss

@REM ==================
@REM Sign the installer
@REM ==================
signtool sign /t %timestamp% /fd SHA256 /sha1 %cert% public\FlexConfirmMailSetup*.exe

@REM ==================
@REM Add suffix to name
@REM ==================
powershell -C "Get-ChildItem public\*.exe | rename-item -newname { $_.Name -replace  '.exe', '-Free.exe' }"