@@ECHO OFF

REM ----------------------------------------------------------------------------
REM   Creates an empty assembly info file if needed
REM ----------------------------------------------------------------------------

set AssmInfoFile=SharedAssemblyInfo.cs
set AssmInfoDir=%~dp0
set AssmInfoPath=%AssmInfoDir%%AssmInfoFile%

IF NOT EXIST %~dp0SharedAssemblyInfo.cs (
  ECHO Creating empty %AssmInfoFile% in %AssmInfoDir%
  ECHO // > %AssmInfoPath%
) ELSE (
  ECHO %AssmInfoFile% already exists. Good.
)