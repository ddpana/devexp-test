# Files/Folders in this repo

- stock_price_data_files -> folder with source files, in the same hierarchy as the zip file provided
- Controllers/StockDataController.cs -> the two exposed API endpoints
- Models -> data models used based on the requirement
- Services/StockDataProcessingService.cs -> service with dependency injection, rather than writing simple functions
- Config.cs -> configuring the app with appsettings.json data, for dependency injection
- Program.cs -> main entrypoint of the app
- RunMe.exe -> pre-compiled binary, ready to run, without any dependencies.


# Install / Run / Build

- ### First, you need to have .NET Runtime 8 (or SDK) -> [here](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-aspnetcore-8.0.11-windows-x64-installer)
- ### Clone this repo, and ```cd Lseg.Devexp.Test```
- ### ```dotnet watch run``` (for development mode)
- ### Go to [http://localhost:5000/swagger/index.html](http://localhost:5000/swagger/index.html)


## For production

- ### ```dotnet publish``` -> you will find the binary in `%Project Folder%\Lseg.Devexp.Test\bin\Release\net8.0\publish\Lseg.Devexp.Test.exe`
- ### You need to run the executable.

## I already added one compiled windows version, ready to run (you do not need any runtime) as the ```RunMe.exe``` in the main folder of the repo.

# Using it

### In the swagger UI, or trough API, there are two methods:

- GET http://localhost:5000/stockdata/datapoints/{numberoffilestoprocess} -> this method will parse all the files in the folder ```stock_price_data_files``` and return exactly 30 records for each file from each index from each stock exchange
- GET http://localhost:5000/stockdata/outliers/{numberoffilestoprocess} -> this method will show all the outliers from each file for each index for each stock exchange


# Considerations

### Due to time constraints, I wasn't able to add more, but I thought of over 30 ways to improve this code including:
- Adding authentication (at least JWT if not AzureAD)
- Error handling for the data in the files are wrongly formatted (strings instead of numbers, etc)
- Error handling for cases where some files are too large
- Error handling for loops
- Error handling for system permissions (while running, some other process might modify permissions over the files)
- Improved logging (I used Console.WriteLine only to display the output in a better format than the native dotnet logging)
- Storing secrets (if needed) in KeyVault/Hashicorp vault
- Creating multiple methods / classes to scope the code better
- Optimize for performance
