# Console app lists all used analyzers and performs diagnostic

Application is able to provide list of all used analyzers for selected Project or the whole Solution. The list of codes 
is aggregated if Solution is provided. 

Application is also able to run diagnostics for selected Project or the whole Solution. The diagnostic results are also
aggregated. 

As the Compile option codes are not retrievable from build, they need to be parsed from the Roslyn Github repository
beforehand. Application is also able to retrieve and store those data. 

# Application commands:
### (1) Generate all used analyzers for selected Project
`dotnet run -- print-project-config p- {path_to_.csproj} -o {path_to_output.json}`

### (2) Generate all used analyzers for selected Solution
`dotnet run -- print-solution-config p- {path_to_.sln} -o {path_to_output.json}`

### (3) Run diagnostics for selected Project
To run diagnostics you need to first run step (1) to generate analyzer list. The list is used to create
final output of diagnostic. This way all analyzers can be part of the output, without it, only those who fail would be
present.

`dotnet run -- lint-project p- {path_to_.csproj} -o {path_to_output.json} -a {path_to_result_of_step_1}`

### (4) Run diagnostics for selected Solution
To run diagnostics you need to first run step (2) to generate analyzer list. The list is used to create
final output of diagnostic. This way all analyzers can be part of the output, without it, only those who fail would be
present.

All the diagnostics are aggregated, but can be distinguished by filenames.

`dotnet run -- lint-solution p- {path_to_.sln} -o {path_to_output.json} -a {path_to_result_of_step_1}`

### (5) Generate compiler option codes
The application already has this file generated and uses it as default value. However if the need occurs for a different
compiler option file to be used, it can be generated using this command. If you want to use this new file for all steps
(1), (2), (3) or (4) you **must** use parameter `-c {path_to_file_generated_by_this_step}`

`dotnet run -- generate-compiler-codes -o {path_to_output.json}`


### (6) Run help
Run app help info screen 

`dotnet run -- --help`

Run help for selected verb

`dotnet run -- print-project-config --help`


# Dotnet tool
Application can be also run as dotnet tool.

## Install the tool
### Create dotnet tool environment

`dotnet new tool-manifest`

### Install the tool
Add the source of the package to the .nuget
https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry

`dotnet tool install AnalyzerDiagnosticInfo` 

## Run the tool

The tool accepts the same parameters as the console application. 

Run the tool to print project config

`dotnet tool run diagnostic-analyzer print-project-config p- {path_to_.csproj} -o {path_to_output.json}`
