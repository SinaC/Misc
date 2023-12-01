<Query Kind="Program" />

void Main()
{
	const string path = @"C:\PRJ\Personal\EntityComparer\";
	
	RenameDirectoriesAndFilesAndContent(path);
}

private static string From = "Merge";
private static string To = "Compare";
private static bool RenameFiles = true;
private static bool RenameDirectories = true;
private static bool DeleteBuildDirectories = true;
private static bool ChangeCodeFiles = true; // cs/xaml/menu.xml/rightmenu.xml/wsdlprx/resx
private static bool ChangeProjSlnFiles = true; // csproj/sln
private static bool ChangeConfigFiles = true; // nswag/json/config/readme.txt
private static bool ChangeNugetFiles = true; // nuspec
private static bool ChangeYmlFiles = true; // yml
private static bool ChangeAdfsFiles = true; // cmd/register*/RulesService.txt

// 
private void RenameDirectoriesAndFilesAndContent(string path)
{
	var files = Directory.EnumerateFiles(path);
	foreach(var file in files)
	{
		var fileName = Path.GetFileName(file);
		// modify file
		var extension = Path.GetExtension(file);
		if (extension == ".cs" || extension == ".xaml" || fileName == "Menu.xml" || fileName == "RightMenu.xml" || extension == ".wsdlprx" || extension == ".resx")
		{
			if (ChangeCodeFiles)
			{
				$"Modifying code file content {file}".Dump();
				ChangeFileContent(file);
			}
		}
		else if (extension == ".csproj" || extension == ".sln")
		{
			if (ChangeProjSlnFiles)
			{
				$"Modifying proj/sln file content {file}".Dump();
				ChangeFileContent(file);
			}
		}
		else if (extension == ".nuspec")
		{
			if (ChangeNugetFiles)
			{
				$"Modifying nuspec file content {file}".Dump();
				ChangeFileContent(file);
			}
		}
		else if (extension == ".nswag" || extension == ".json" || extension == ".Config" || fileName == "Readme.txt")
		{
			if (ChangeConfigFiles)
			{
				$"Modifying config file content {file}".Dump();
				ChangeFileContent(file);
			}
		}
		else if (extension == ".yml")
		{
			if (ChangeYmlFiles)
			{
				$"Modifying yml file content {file}".Dump();
				ChangeFileContent(file);
			}
		}
		else if ((extension == ".cmd" && fileName.StartsWith("Register")) || fileName == "RulesService.txt")
		{
			if (ChangeAdfsFiles)
			{
				$"Modifying adfs file content {file}".Dump();
				ChangeFileContent(file);
			}
		}
		// rename file
		if (RenameFiles && fileName.Contains(From))
		{
			string newFullFileName =  Path.Combine(Path.GetDirectoryName(file), fileName.Replace(From, To));
			$"Renaming file {file} -> {newFullFileName}".Dump();
			File.Move(file, newFullFileName);
		}
	}

	var directories = Directory.EnumerateDirectories(path);
	foreach(var directory in directories)
	{
		string directoryName = Path.GetFileName(directory);
		// skip .git, .config, ...
		if (directoryName.StartsWith("."))
			$"Skipping {directory}".Dump();
		// delete bin/obj
		else if (directoryName == "bin" || directoryName == "obj")
		{
			if (DeleteBuildDirectories)
			{
				$"Deleting {directory}".Dump();
				Directory.Delete(directory,  true);
			}
		}
		// rename files and subdirectories
		else
		{
			// we first rename the files/directories in directory and sub-directories
			RenameDirectoriesAndFilesAndContent(directory);
			// then we rename the directory
			if (RenameDirectories && directoryName.Contains(From))
			{
				string newDirectoryName = Path.Combine(Path.GetDirectoryName(directory), directoryName.Replace(From, To));
				$"Renaming directory {directory} -> {newDirectoryName}".Dump();
				Directory.Move(directory, newDirectoryName);
			}
		}
	}
}

private static void ChangeFileContent(string filename)
{
	string text = File.ReadAllText(filename);
	if (text.Contains(From, StringComparison.InvariantCultureIgnoreCase))
	{
		text = text.Replace(From.ToLower(), To.ToLower()).Replace(From, To).Replace(From.ToUpper(), To.ToUpper());
		File.WriteAllText(filename, text);
	}
}
