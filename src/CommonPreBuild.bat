@@ECHO OFF

REM ----------------------------------------------------------------------------
REM   Creates an empty assembly info file if needed
REM ----------------------------------------------------------------------------

set AssmInfoFile=CommonAssemblyInfo.cs
set AssmInfoDir=%~dp0
set AssmInfoPath=%AssmInfoDir%%AssmInfoFile%

IF NOT EXIST %~dp0CommonAssemblyInfo.cs (
  ECHO Creating empty %AssmInfoFile% in %AssmInfoDir%
  ECHO // > %AssmInfoPath%
) ELSE (
  ECHO %AssmInfoFile% already exists. Good.
)