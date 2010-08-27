. .\tools\psake\psake_ext.ps1
. .\tools\psake\teamcity.ps1

properties {
	$source_dir = Resolve-Path ./
	$build_dir = "$source_dir\@build"
	$checkout_dir = [Environment]::GetEnvironmentVariable("teamcity.build.checkoutDir")
	$artifact_dir = if ($checkout_dir.length -gt 0) { "$checkout_dir\@artifacts" } else { "$source_dir\@artifacts" }
	$configuration = "Release"
	$build_number = if ("$env:BUILD_NUMBER".length -gt 0) { "$env:BUILD_NUMBER" } else { "0" }
	$build_vcs_number = if ("$env:BUILD_VCS_NUMBER".length -gt 0) { "$env:BUILD_VCS_NUMBER" } else { "0" }
    $version = "0.7.$build_number"
    $info_version = "$version (rev $build_vcs_number)"
}

task default -depends Compile, ZipBinaries, ZipSource 

task ZipBinaries {
	$zip_name = "DotNetMigrations-v" + $version + "-BIN.zip"
	
	# zip the build output
    # exclude unit tests and debug symbols
    exec { ./tools/7zip/7za.exe a -tzip `"$artifact_dir\$zip_name`" `"$build_dir\*`" `
           `"-x!*.pdb`" `
           `"-x!*Tests*`" `
           `"-x!Moq.*`" `
           `"-x!nunit.*`" ` }
    
    TeamCity-PublishArtifact "@artifacts\$zip_name"
}

task ZipSource {
    $zip_name = "DotNetMigrations-v" + $version + "-SRC.zip"
    
    # zip the source code
    # exclude the cruft
    exec { ./tools/7zip/7za.exe a -tzip `"$artifact_dir\$zip_name`" `"$source_dir\*`" `
           `"-x!.git`" `
           `"-x!@build`" `
           `"-x!@artifacts`" `
           `"-x!*resharper*`" `
           `"-x!.gitignore`" `
           `"-x!TODO.txt`" `
           `"-xr!*.cache`" `
           `"-xr!*.suo`" `
           `"-xr!*.user`" }
    
    TeamCity-PublishArtifact "@artifacts\$zip_name"
}

task Compile -depends Init {
	"Building version $version for $configuration"
	
	# build the solution
	exec { msbuild ""$source_dir\DotNetMigrations.sln"" /m /nologo /t:Rebuild /p:Configuration=$configuration /p:OutDir=""$build_dir\\"" }
    
    # run tests
    $nunitLauncher = [Environment]::GetEnvironmentVariable("teamcity.dotnet.nunitlauncher")
       
    if ($nunitLauncher -eq $null) {
        "Skipping tests. The teamcity.dotnet.nunitlauncher environment variable was not found."
    } else {
        exec { & $nunitLauncher v2.0 x86 NUnit-2.4.8 $build_dir\DotNetMigrations.UnitTests.dll }
    }
}

task Init -depends Clean {
	Assert($version.length -gt 0) "No version number was specified."
    
    TeamCity-SetBuildNumber $version
    
    $year = [DateTime]::Now.ToString("yyyy")

	Generate-Assembly-Info `
        -File "$source_dir\src\SharedAssemblyInfo.cs" `
        -Title "DotNetMigrations $info_version" `
        -Description "" `
        -Company "" `
        -Product "DotNetMigrations $info_version" `
        -Version $version `
        -InfoVersion $info_version `
        -Copyright "Copyright (c) Joshua Poehls 2007-$year"

	New-Item $build_dir -ItemType directory
}

task Clean {
    if (Test-Path $build_dir) { Remove-Item -Force -Recurse $build_dir }
    if (Test-Path $artifact_dir) { Remove-Item -Force -Recurse $artifact_dir }
}