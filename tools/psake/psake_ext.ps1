function Generate-Assembly-Info
{
param(
	[string]$Title,
	[string]$Description,
	[string]$Company,
	[string]$Product,
	[string]$Copyright,
	[string]$Version,
    [string]$InfoVersion = $Version,
	[string]$File = $(throw "File is a required parameter.")
)
 
  $asmInfo = "using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: ComVisibleAttribute(false)]
[assembly: AssemblyTitleAttribute(""$Title"")]
[assembly: AssemblyDescriptionAttribute(""$Description"")]
[assembly: AssemblyCompanyAttribute(""$Company"")]
[assembly: AssemblyProductAttribute(""$Product"")]
[assembly: AssemblyCopyrightAttribute(""$Copyright"")]
[assembly: AssemblyVersionAttribute(""$Version"")]
[assembly: AssemblyInformationalVersionAttribute(""$InfoVersion"")]
[assembly: AssemblyFileVersionAttribute(""$Version"")]
[assembly: AssemblyDelaySignAttribute(false)]"
 
	$dir = [System.IO.Path]::GetDirectoryName($file)
	if ([System.IO.Directory]::Exists($dir) -eq $false)
	{
		Write-Host "Creating directory $dir"
		[System.IO.Directory]::CreateDirectory($dir)
	}
	Write-Host "Generating assembly info file: $file"
	(Get-Item $file).Attributes = 'Normal'
	Write-Output $asmInfo > $file
}