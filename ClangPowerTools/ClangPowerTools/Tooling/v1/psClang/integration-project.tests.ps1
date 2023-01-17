BeforeAll {
 @( "$PSScriptRoot\io.ps1"
  , "$PSScriptRoot\visualstudio-detection.ps1"
  , "$PSScriptRoot\msbuild-expression-eval.ps1"
  , "$PSScriptRoot\msbuild-project-load.ps1"
  , "$PSScriptRoot\msbuild-project-data.ps1"
  , "$PSScriptRoot\itemdefinition-context.ps1"
  ) | ForEach-Object { . $_ }
}

function LoadIntegrationProject($name)
{
  $slnPath = "$PSScriptRoot\integration-projects\CptIntegrationProjects.sln";
  if (! (VariableExists 'slnFiles'))
  {
    Set-Variable -name 'slnFiles' -value @{} -scope Global
    Set-Variable -name 'cptVisualStudioVersion' -value '2017' -scope Global
  }
  $global:slnFiles[$slnPath] = (Get-Content $slnPath)
  [string] $projPath = "$PSScriptRoot\test-projects\$name\test.vcxproj"
  LoadProject $projPath
}

Describe "Integration Project Unit Tests" {
  It "Should correctly load Item Groups" {

  #LoadIntegrationProject -name 'itemgroups'
  #$includeDirs =  Get-ProjectIncludeDirectories

  # todo validate includeDirs
  }
}
