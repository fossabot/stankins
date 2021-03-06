﻿using FluentAssertions;
using Stankins.AzureDevOps;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace StankinsTestXUnit
{
    [Trait("yaml", "testGraphVizStankins")]
    [Trait("ExternalDependency", "0")]
    public class TestYamlGraphviz
    {
        [Fact]
        public async Task TestYamlGrphvizSimple()
        {
            var data = (@"# Xcode
# Build, test, and archive an Xcode workspace on macOS.
# Add steps that install certificates, test, sign, and distribute the app, save build artifacts, and more:
# https://docs.microsoft.com/azure/devops/pipelines/languages/xcode

pool:
  vmImage: 'macOS 10.13'

steps:
- task: Xcode@5
  inputs:
    scheme: ''
    sdk: 'iphoneos'
    configuration: 'Release'
    xcodeVersion: 'default' # Options: 8, 9, default, specifyPath");

            string nameFile = $"{nameof(TestYamlGrphvizSimple)}.txt";
            File.WriteAllText(nameFile, data);
            var visit = new YamlReader(nameFile, Encoding.UTF8);
            var dt = await visit.TransformData(null);
            var graph = new SenderYamlAzurePipelineToDot();
            dt = await graph.TransformData(dt);
            //await File.WriteAllTextAsync("a.txt",graph.Result());
            //Process.Start("notepad.exe","a.txt");
            var res = graph.OutputString.Rows[0]["Contents"].ToString().Replace(Environment.NewLine, "");
            res.Should().ContainAll("Xcode@5", "->");
            //res.Should().NotContain("2");// not 2 jobs, not 2 tasks

        }

        [Fact]
        public async Task TestYamlGrphviz2Jobs()
        {
            var data = (@"# Xamarin.Android and Xamarin.iOS
# Build a Xamarin.Android and Xamarin.iOS app.
# Add steps that test, sign, and distribute the app, save build artifacts, and more:
# https://docs.microsoft.com/azure/devops/pipelines/languages/xamarin

jobs:

- job: Android
  pool:
    vmImage: 'VS2017-Win2016'

  variables:
    buildConfiguration: 'Release'
    outputDirectory: '$(build.binariesDirectory)/$(buildConfiguration)'

  steps:
  - task: NuGetToolInstaller@0

  - task: NuGetCommand@2
    inputs:
      restoreSolution: '**/*.sln'

  - task: XamarinAndroid@1
    inputs:
      projectFile: '**/*droid*.csproj'
      outputDirectory: '$(outputDirectory)'
      configuration: '$(buildConfiguration)'

  - task: AndroidSigning@3
    inputs:
      apksign: false
      zipalign: false
      apkFiles: '$(outputDirectory)/*.apk'

  - task: PublishBuildArtifacts@1
    inputs:
      pathtoPublish: '$(outputDirectory)'

- job: iOS
  pool:
    vmImage: 'macOS 10.13'

  steps:
  # To manually select a Xamarin SDK version on the Hosted macOS agent, enable this script with the SDK version you want to target
  # https://go.microsoft.com/fwlink/?linkid=871629
  - script: sudo $AGENT_HOMEDIRECTORY/scripts/select-xamarin-sdk.sh 5_4_1 
    displayName: 'Select Xamarin SDK version'
    enabled: false

  - task: NuGetToolInstaller@0

  - task: NuGetCommand@2
    inputs:
      restoreSolution: '**/*.sln'

  - task: XamariniOS@2
    inputs:
      solutionFile: '**/*.sln'
      configuration: 'Release'
      buildForSimulator: true
      packageApp: false");
            string nameFile = $"{nameof(TestYamlGrphviz2Jobs)}.txt";
            File.WriteAllText(nameFile, data);
            var visit = new YamlReader(nameFile, Encoding.UTF8);
            var dt = await visit.TransformData(null);
            var graph = new SenderYamlAzurePipelineToDot();
            dt = await graph.TransformData(dt);
            //await File.WriteAllTextAsync("a.txt", graph.Result());
            //Process.Start("notepad.exe", "a.txt");
            var res = graph.OutputString.Rows[0]["Contents"].ToString().Replace(Environment.NewLine, "");
            res.Should().ContainAll("Android", "iOS", "XamarinAndroid@1", "XamariniOS@2");

        }

        [Fact]
        public async Task FullBlownTest()
        {
            var data = @"# .NET Desktop
# Build and run tests for .NET Desktop or Windows classic desktop solutions.
# Add steps that publish symbols, save build artifacts, and more:
# https://docs.microsoft.com/vsts/pipelines/apps/windows/dot-net
#https://docs.microsoft.com/en-us/azure/devops/pipelines/build/options?view=vsts&tabs=yaml

variables:
    solution: '**/StankinsV2.sln'
    buildPlatform: 'Any CPU'
    buildConfiguration: 'Release'
    year: $(Date:yyyy)
    month: $(Date:MM)
    day: $(Date:dd)
    uk: $(Date:yyyyMMdd)
    deployWindows: 1
    deployLinux: 1
    stop : 0
    

name: $(TeamProject)_$(BuildDefinitionName)_$(SourceBranchName)_$(Date:yyyyMMdd)$(Rev:.r)

jobs:

- job: Test
  pool:
    vmImage: 'vs2017-win2016'
  condition: eq(variables['stop'],1)
  steps:
    - checkout: none #skip checking out the default repository resource
    - script: |
        echo test
        dir C:\msbuild.exe /s /b
        

      
      
- job: SetProps
  pool:
    vmImage: 'vs2017-win2016'
  condition: eq(variables['stop'],0)
  steps:
    - checkout: none #skip checking out the default repository resource
    - powershell: |
        $yearPW= Get-Date -Format yyyy
        $monthPW=Get-Date -Format MM
        $dayPW=Get-Date -Format dd
        Write-Host ""##vso[task.setvariable variable=PWyear;isOutput=true]$yearPW""
        Write-Host ""##vso[task.setvariable variable=PWmonth;isOutput=true]$monthPW""
        Write-Host ""##vso[task.setvariable variable=PWday;isOutput=true]$dayPW""
        Write-Host ""##vso[task.setvariable variable=PWversion;isOutput=true]$yearPW.$monthPW.$dayPW.$env:BUILD_BUILDID""
      
      name: setvarStep


    - script: |
        echo hello from SettingVariousPropertiesMissing
        rem echo $(Build.ArtifactStagingDirectory) $(TeamProject) $(BuildDefinitionName) $(SourceBranchName) $(Date:yyyyMMdd) $(Rev:.r)   
        rem echo %BUILDDEFINITIONNAME% %SOURCEBRANCHNAME% %(DATE:yyyyMMdd)% %(REV:.r)%    
        rem echo Hello World from %AGENT_NAME%.
        rem echo My ID is %AGENT_ID%.
        rem echo AGENT_WORKFOLDER contents:
        rem @dir %AGENT_WORKFOLDER%
        rem echo AGENT_BUILDDIRECTORY contents:
        rem @dir %AGENT_BUILDDIRECTORY%
        rem echo BUILD_SOURCESDIRECTORY contents:
        rem @dir %BUILD_SOURCESDIRECTORY%      
        echo $(Date:yyyyMMdd)
        cmd /K set

- job: TestMyProps

  dependsOn: SetProps
  condition:  eq(variables['stop'],0)

  pool:
    vmImage: 'vs2017-win2016'

  
  variables:
    MyVersion: $[ dependencies.SetProps.outputs['setvarStep.PWversion'] ]  # map in the variable
 
  steps:
    - checkout: none #skip checking out the default repository resource
    - script: echo this is version $(MyVersion)
      name: echovar    

- job: DownloadFilesAndModifyBackend

  dependsOn: SetProps
  condition:  eq(variables['stop'],0)

  pool:
    vmImage: 'vs2017-win2016'

  
  variables:
    MyVersion: $[ dependencies.SetProps.outputs['setvarStep.PWversion'] ]  # map in the variable
 
  steps:
    - script: echo this is version $(MyVersion)
      name: echovar    
    
    - script: |
        dotnet tool install  --tool-path . dotnet-property   
        dotnet-property ""Stankinsv2/**/Stankins*.csproj"" Version:""%MyVersion%""
      displayName: versioning

    - task: ArchiveFiles@2
      displayName: 'arhive files'
      inputs:
        rootFolderOrFile: '$(Build.SourcesDirectory)'
        includeRootFolder: false
        archiveType: 'zip' 
        archiveFile: '$(Build.ArtifactStagingDirectory)/files-$(Build.BuildId).zip' 
        replaceExistingArchive: true 

    - task: PublishBuildArtifacts@1
      inputs:
         artifactName: dotNetModifs
      displayName: Publish files as Artifact
     

- job: BuildFrontEnd
  dependsOn: 
  - SetProps
  - DownloadFilesAndModifyBackend
  pool:
    vmImage: 'vs2017-win2016'

  variables:
    MyVersion: $[ dependencies.SetProps.outputs['setvarStep.PWversion'] ]  # map in the variable 

  steps:
  - checkout: none #skip checking out the default repository resource
  - task: DownloadBuildArtifacts@0
    displayName: 'Download Build Artifacts'
    inputs:
      artifactName: dotNetModifs
      downloadPath: $(Build.SourcesDirectory)
      #downloadPath: $(Agent.BuildDirectory)

  - script: |
       dir 
       dir dotNetModifs
       dir dotNetModifs\files-$(Build.BuildId).zip
       move dotNetModifs\files-$(Build.BuildId).zip files-$(Build.BuildId).zip 
    displayName: 'moving file artifact'

  - task: ExtractFiles@1
    displayName: 'Extract zip files'
    inputs:
        archiveFilePatterns: '*.zip' 
        destinationFolder: $(Build.SourcesDirectory)
        cleanDestinationFolder: false 

  - script: |
       rem delete old artifact files
       del files-$(Build.BuildId).zip 
       dir
       cd stankinsv2\solution\StankinsV2\StankinsAliveAngular
       echo before install angular
       rem https://docs.microsoft.com/en-us/azure/devops/pipelines/tasks/package/npm?view=vsts
       call npm install -g @angular/cli
       echo install json
       call npm install -g json
       echo before json call with %MyVersion%
       call json -I -f npm-shrinkwrap.json -e ""this.version='%MyVersion%'""
       call json -I -f package.json -e ""this.version='%MyVersion%'""
       rem type npm-shrinkwrap.json
       echo before npm
       call npm i
       echo before build
       rem see robocopy in build.bat
       build.bat 
       echo finish
    displayName: 'npm and angular publish'

  - script: |
       dir
       call npm install -g q
       call npm install -g cordova
       cd stankinsv2\solution\StankinsV2\StankinsCordova
       call cordova platform rm android
       call cordova platform rm windows
       call cordova platform add android
       call cordova platform add windows
       echo requirements
       call cordova requirements
       echo Build android
       call cordova build android
       echo dir1
       dir ""C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\amd64\*.exe""
       echo dir2
       dir ""C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\*.exe""
       echo set the MSBUILDDIR=""
       set  MSBUILDDIR=""C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\amd64\""
       rem set  MSBUILDDIR=""C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\""
       call cordova build windows -- --appx=uap
    displayName: 'cordova'

  #- task: AndroidSigning@2
  #  inputs:
  #     apkFiles: 'stankinsv2/solution/StankinsV2/StankinsCordova/**/app*.apk' #how to change name of apk file? mystery...
  #      jarsign: true
  #      jarsignerKeystoreFile: 'keystore.jks' #from library...
  #      jarsignerKeystorePassword: '$(jarsignerKeystorePassword)'
  #      jarsignerKeystoreAlias: '$(keystoreAlias)'
  #      jarsignerKeyPassword: '$(jarsignerKeyPassword)'
  #      zipalign: true  
    
    
  - task: AndroidSigning@3
    inputs:
        apkFiles: '**/debug/*.apk' #how to change name of apk file? mystery...
        #apkFiles: 'stankinsv2/solution/StankinsV2/StankinsCordova/platforms/android/app/build/outputs/apk/debug/app-debug.apk'
        apksign: true
        apksignerKeystoreFile: 'keystore.jks' #from library...
        apksignerKeystorePassword: '$(jarsignerKeystorePassword)'
        apksignerKeystoreAlias: '$(keystoreAlias)'
        apksignerKeyPassword: '$(jarsignerKeyPassword)'
        zipalign: false
  - task: CopyFiles@2
    inputs:
        contents: 'stankinsv2/solution/StankinsV2/StankinsCordova/**/app*.apk'
        targetFolder: '$(Build.ArtifactStagingDirectory)'
        flattenFolders: true
        
  - script: |
       cd $(Build.ArtifactStagingDirectory)
       dir *.apk /s /b 
       echo renaming
       ren app*.apk  StankinsCordova_%MyVersion%.apk
    displayName: 'renaming apk'

    
  - script: |
       dir
       call npm install -g electron
       call npm install -g electron-builder
       cd stankinsv2\solution\StankinsV2\StankinsElectron
       call npm install -g json
       echo before json call with %MyVersion%
       call json -I -f package.json -e ""this.version='%MyVersion%'""
       call npm install 
       echo Build electron
       electron-builder --win --x64 --publish never
       dir dist\*.* /b
    displayName: 'electron'
    
  - task: CopyFiles@2
    inputs:
        contents: 'stankinsv2/solution/StankinsV2/StankinsElectron/dist/*.*'
        targetFolder: '$(Build.ArtifactStagingDirectory)/electronwin64'
        flattenFolders: true
  

  - task: ArchiveFiles@2
    displayName: 'arhive files'
    inputs:
        rootFolderOrFile: '$(Build.SourcesDirectory)'
        includeRootFolder: false
        archiveType: 'zip' 
        archiveFile: '$(Build.ArtifactStagingDirectory)/files-$(Build.BuildId).zip' 
        replaceExistingArchive: true 


  - task: PublishBuildArtifacts@1
    displayName: Publish files as Artifact
      




- job: Windows
  dependsOn: 
  - SetProps
  - BuildFrontEnd
  condition: and( eq(variables['deployWindows'],1), succeeded('BuildFrontEnd'))
  pool:
    vmImage: 'vs2017-win2016'

  variables:
    MyVersion: $[ dependencies.SetProps.outputs['setvarStep.PWversion'] ]  # map in the variable
 
  steps:
  - checkout: none #skip checking out the default repository resource
  - task: DownloadBuildArtifacts@0
    displayName: 'Download Build Artifacts'
    inputs:
      artifactName: drop
      downloadPath: $(Build.SourcesDirectory)
      #downloadPath: $(Agent.BuildDirectory)

  - script: |
       dir
       move drop\files-$(Build.BuildId).zip files-$(Build.BuildId).zip 
    displayName: 'moving file artifact'

  - task: ExtractFiles@1
    displayName: 'Extract zip files'
    inputs:
        archiveFilePatterns: '*.zip' 
        destinationFolder: $(Build.SourcesDirectory)
        cleanDestinationFolder: false 


  - script: |
       dir
       cd stankinsv2/solution/
    displayName: 'cd to folder'

  - task: DotNetCoreInstaller@0
    displayName: 'Use .NET Core sdk 2.1.300'
    inputs:
        version: 2.1.300

  
  - task: NuGetToolInstaller@0

  - task: NuGetCommand@2
    inputs:
        restoreSolution: '$(solution)'
  
  - task: VSBuild@1
    inputs:
      solution: '$(solution)'
      platform: '$(buildPlatform)'
      configuration: '$(buildConfiguration)'

  - script: |
        dotnet tool install  --tool-path . coverlet.console
        dotnet tool install  --tool-path . dotnet-reportgenerator-globaltool 
        dotnet tool install  --tool-path . DotnetThx 
    displayName: install global tools

  #- task: DotNetCoreCLI@2
  #  inputs:
  #     command: test
  #     projects: '**/*Test*/*Test*.csproj'
  #     arguments: '--configuration $(buildConfiguration) --logger trx --collect ""Code coverage""'
  - script:  |
        dotnet test stankinsv2\solution\StankinsV2\StankinsTestXUnit\StankinsTestXUnit.csproj --logger trx --collect ""Code coverage""
    displayName: irun test coverage Automated

  - task: PublishTestResults@2
    inputs:
      testRunner: VSTest
      testResultsFiles: '**/*.trx'

  - script: |
        dir StankinsTestXUnit.dll  /s /b
        rem cd stankinsv2/solution/StankinsV2/StankinsStatusWeb
        rem dotnet-thx.exe
        rem cd ../StankinsTestXUnit
        rem dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover stankinsv2/solution/StankinsV2/StankinsTestXUnit/StankinsTestXUnit.csproj
        coverlet stankinsv2\solution\StankinsV2\StankinsTestXUnit\bin\$(buildConfiguration)\netcoreapp2.1\StankinsTestXUnit.dll --target ""dotnet"" --targetargs ""test stankinsv2\solution\StankinsV2\StankinsTestXUnit\StankinsTestXUnit.csproj --configuration $(buildConfiguration) --no-build"" --format opencover --exclude ""[xunit*]*""
        reportgenerator ""-reports:coverage.opencover.xml"" ""-targetdir:coveragereport"" -reporttypes:HTMLInline;HTMLSummary;Badges
        xcopy coveragereport  $(Build.ArtifactStagingDirectory)\cc\ /S  
        rem restoring current dir
        rem cd ../../../../ 
        dir 
    displayName: test dot net
  
  - bash: |
      curl -s https://codecov.io/bash > codecov
      chmod +x codecov
      ./codecov -f ""coverage.opencover.xml"" -t $CODECOV_TOKEN
    displayName: upload  code coverage

  - script: |
        echo start publish windows
        dotnet publish -o $(Build.ArtifactStagingDirectory)\win10-x64  -f netcoreapp2.1 -c Release -r win10-x64 stankinsv2/solution/StankinsV2/StankinsStatusWeb/StankinsAliveMonitor.csproj
        cd stankinsv2/solution/
        xcopy $(Build.ArtifactStagingDirectory)\win10-x64 win10-x64\ /I /E
        docker build . -t stankins_windows -f Dockerfile_win
        docker tag stankins_windows ignatandrei/stankins_windows
        docker image ls
        echo %MY_PASSWORD_DOCKER%| docker login --username ignatandrei --password-stdin
        docker push ignatandrei/stankins_windows
    displayName: 'docker'

  - task: PublishBuildArtifacts@1
    displayName: 'Publish Artifact: drop'

- job: Linux
  dependsOn: 
  - SetProps
  - BuildFrontEnd
  condition: and(eq(variables['deployLinux'],1), succeeded('BuildFrontEnd'))
  pool:
    vmImage: 'ubuntu-16.04'

  variables:
    MyVersion: $[ dependencies.SetProps.outputs['setvarStep.PWversion'] ]  # map in the variable
 
  
  steps:
  - checkout: none #skip checking out the default repository resource
  - task: DownloadBuildArtifacts@0
    displayName: 'Download Build Artifacts'
    inputs:
      artifactName: drop
      downloadPath: $(Build.SourcesDirectory)
      #downloadPath: $(Agent.BuildDirectory)

  - script: |
       ls -l
       mv drop/files-$(Build.BuildId).zip .
    displayName: 'moving file artifact'

  - task: ExtractFiles@1
    displayName: 'Extract zip files'
    inputs:
        archiveFilePatterns: '*.zip' 
        destinationFolder: $(Build.SourcesDirectory)
        cleanDestinationFolder: false 

  
  - script: |
       cd stankinsv2/solution/
       echo $MY_PASSWORD_DOCKER| docker login --username ignatandrei --password-stdin
       rem echo version1 : $MyVersion
       rem echo version2 : $(MyVersion)
       #docker build . --build-arg version=$(MyVersion) -t stankins_linux -f Dockerfile_linux      
       docker build . -t stankins_linux -f Dockerfile_linux      
       docker image ls
       docker tag stankins_linux ignatandrei/stankins_linux
       docker push ignatandrei/stankins_linux
       docker create --name st stankins_linux
       docker container ls
       docker cp st:/app/ $(Build.ArtifactStagingDirectory)/linux-x64
       docker container kill st
       docker container prune -f

    displayName: 'docker'

  - task: PublishBuildArtifacts@1
    displayName: 'Publish Artifact: drop'


- job: macOS
  pool:
    vmImage: 'macOS-10.13'
    variables:
        solution: '**/StankinsV2.sln'
        buildPlatform: 'Any CPU'
        buildConfiguration: 'Release'
  steps:
  - script: echo hello from macOS

  condition: false



- job: Deploy
  pool:
    vmImage: 'win1803'
  steps:
  - checkout: none #skip checking out the default repository resource
  - task: DownloadBuildArtifacts@0
    displayName: 'Download Build Artifacts'
    inputs:
      artifactName: drop
      downloadPath: $(System.ArtifactStagingDirectory)

  - script: |
        echo hello from deploy
        dir $(Build.ArtifactStagingDirectory)  
        rem echo test >>$(Build.ArtifactStagingDirectory)\a.txt
    displayName: just testing


  - task: ArchiveFiles@2
    displayName: 'arhive linux'
    condition: and(always(),eq(variables['deployLinux'],1)) #succeeded('Linux')
    inputs:
      rootFolderOrFile: '$(System.ArtifactStagingDirectory)/drop/linux-x64'
      includeRootFolder: true
      archiveType: 'zip' 
      archiveFile: '$(Build.ArtifactStagingDirectory)/linux-x64-$(Build.BuildId).zip' 
      replaceExistingArchive: true 

  - task: ArchiveFiles@2
    displayName: 'arhive windows '
    condition: and(always(),eq(variables['deployWindows'],1)) #succeeded('Windows') 
    inputs:
      rootFolderOrFile: '$(System.ArtifactStagingDirectory)/drop/win10-x64'
      includeRootFolder: true
      archiveType: 'zip' 
      archiveFile: '$(Build.ArtifactStagingDirectory)/win10-x64-$(Build.BuildId).zip' 
      replaceExistingArchive: true 

  - task: ArchiveFiles@2
    displayName: 'arhive code coverage'
    inputs:
      rootFolderOrFile: '$(System.ArtifactStagingDirectory)/drop/cc'
      includeRootFolder: true
      archiveType: 'zip' 
      archiveFile: '$(Build.ArtifactStagingDirectory)/CodeCoverage-$(Build.BuildId).zip' 
      replaceExistingArchive: true 
      
  - task: ArchiveFiles@2
    displayName: 'arhive android'
    inputs:
      rootFolderOrFile: '$(System.ArtifactStagingDirectory)/drop/*.apk'
      includeRootFolder: true
      archiveType: 'zip' 
      archiveFile: '$(Build.ArtifactStagingDirectory)/Android-$(Build.BuildId).zip' 
      replaceExistingArchive: true 

      
  - task: ArchiveFiles@2
    displayName: 'arhive electronwin64'
    inputs:
      rootFolderOrFile: '$(System.ArtifactStagingDirectory)/drop/electronwin64'
      includeRootFolder: true
      archiveType: 'zip' 
      archiveFile: '$(Build.ArtifactStagingDirectory)/StankinsWin64-$(Build.BuildId).zip' 
      replaceExistingArchive: true 

  - task: GithubRelease@0
    inputs:
      gitHubConnection: ignatandrei
      repositoryName: ignatandrei/stankins
      action: 'create'
      target: 'master'
      title: 'Automated $(Build.BuildNumber) $(Build.BuildId) BuildWindows = $(deployWindows) BuildLinux= $(deployLinux)'
      tag: '$(Build.BuildNumber)'
      addChangeLog: true
      isDraft: false
      isPreRelease: false 
      tagSource: 'manual'

  dependsOn: 
  - Windows
  - Linux
  condition: or(succeeded('Windows') , succeeded('Linux'))


- job: DeployOnAzure

  dependsOn: Windows
  condition: succeeded('Windows')

  pool:
    vmImage: 'vs2017-win2016'
 
  steps:
    - checkout: none #skip checking out the default repository resource
    - task: DownloadBuildArtifacts@0
      displayName: 'Download Build Artifacts'
      inputs:
        artifactName: drop
        downloadPath: $(Build.SourcesDirectory)
  
    - script: |
       dir
       move drop\files-$(Build.BuildId).zip files-$(Build.BuildId).zip 
      

    - task: ExtractFiles@1
      displayName: 'Extract zip files'
      inputs:
        archiveFilePatterns: '*.zip' 
        destinationFolder: $(Build.SourcesDirectory)
        cleanDestinationFolder: false 
        
    - script: |
       dir
       rmdir stankinsV1 /s /q
       dir
      displayName: removing stankinsV1

    - task: DotNetCoreCLI@2
      displayName: 'build again'
      inputs:
        command: publish
        publishWebProjects: True
        workingDirectory: 'stankinsv2/solution/StankinsV2/'
        arguments: '--configuration $(BuildConfiguration) --output $(Build.ArtifactStagingDirectory)'
        zipAfterPublish: True
        
    - script: |
       dir $(System.ArtifactsDirectory)\*.zip /s /b
      displayName: finding zip after Publish

         
    - task: AzureAppServiceManage@0
      inputs:
        azureSubscription: 'azurestankins'
        action: 'Stop Azure App Service'
        WebAppName: 'azurestankins'
        ResourceGroupName: 'stankins'
  
    - task: AzureRmWebAppDeployment@3
      inputs:
        azureSubscription: 'azurestankins'
        WebAppName: 'azurestankins'
        Package: $(System.ArtifactsDirectory)/*StankinsStatusWeb*.zip
        ResourceGroupName: 'stankins'
               
    - task: AzureAppServiceManage@0
      inputs:
        azureSubscription: 'azurestankins'
        action: 'Start Azure App Service'
        WebAppName: 'azurestankins'
        ResourceGroupName: 'stankins'
  
";
            string nameFile = $"{nameof(TestYamlGrphviz2Jobs)}.txt";
            File.WriteAllText(nameFile, data);
            var visit = new YamlReader(nameFile, Encoding.UTF8);
            var dt = await visit.TransformData(null);
            var graph = new SenderYamlAzurePipelineToDot();
            dt = await graph.TransformData(dt);
            //await File.WriteAllTextAsync("a.txt", graph.Result());
            //Process.Start("notepad.exe", "a.txt");
            var res = graph.OutputString.Rows[0]["Contents"].ToString().Replace(Environment.NewLine, "");
            res.Should().ContainAll("Publish", "macOS", "Windows", "Azure","docker");
        }
    }
}
