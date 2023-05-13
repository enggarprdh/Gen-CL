// See https://aka.ms/new-console-template for more information
using config = System.Configuration;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder();
builder.SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
IConfiguration config = builder.Build();        
string conns, outputPath, projectName = string.Empty;
conns = config["ConnectionStrings:GenCL"] + "";
outputPath = config["OutputPath"] + "";
projectName = config["ProjectName"] + "";
Console.WriteLine(conns);
Console.WriteLine(outputPath);
GenCL.Utilities.DataService.ConnectionString = conns;
GenCL.Core.Generator.Generate(outputPath,projectName);

