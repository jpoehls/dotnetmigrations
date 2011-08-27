REM ----------------------------------------------------------------------------
REM   CommonPreBuild.bat is run during the pre-build event of every project.
REM ----------------------------------------------------------------------------

@@ECHO OFF

REM ----------------------------------------------------------------------------
REM   Create an empty CommonAssemblyInfo.cs file if it doesn't already exist.
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