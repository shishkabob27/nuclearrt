using System.IO;

public static class FileUtils
{
	public static void CopyFilesRecursively(string sourcePath, string targetPath)
	{
		// create all directories
		foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
		{
			Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
		}

		// copy all files & replace any files with the same name
		foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
		{
			SaveFile(newPath.Replace(sourcePath, targetPath), File.ReadAllText(newPath));
		}
	}

	public static void SaveFile(string path, string content)
	{
		// check if the content is different from the file (helps with compile times)
		if (!File.Exists(path))
		{
			File.WriteAllText(path, content);
		}
		else
		{
			string currentContent = File.ReadAllText(path);
			if (currentContent != content)
			{
				File.WriteAllText(path, content);
			}
		}
	}
}
