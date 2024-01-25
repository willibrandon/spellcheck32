using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;

namespace spellcheck32;

internal class DictionaryHelper(SpellChecker spellChecker)
{
    private const string Dictionary = "Dictionary";
    private const string SpellCheck32 = "spellcheck32";
    private const string Spelling = "Spelling";
    private const string USEnglish = "en-US";
    private const string USEnglishDictionary = "en_US.dic";
    private const string USEnglishDictionaryZip = "en_US.zip";
    private const string USEnglishResource = $"{SpellCheck32}.{Dictionary}.{USEnglishDictionaryZip}";

    private readonly string _usEnglishDictionaryPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        nameof(Microsoft),
        Spelling,
        USEnglish,
        USEnglishDictionary);

    /// <summary>
    ///  Converts the specified file to UTF-16 LE plaintext with the required Byte Order Mark (BOM).
    /// </summary>
    /// <param name="filePath">The path of the file to convert.</param>
    public void ConvertToUtf16WithBOM(string filePath)
    {
        const string tempExt = ".tmp";

        using (StreamReader streamReader = new(filePath))
        {
            using FileStream fileStream = File.Create(filePath + tempExt);

            // UTF-16 format encoding using little-endian byte order.
            using StreamWriter streamWriter = new(fileStream, Encoding.Unicode);

            // Write the Byte Order Mark (BOM).
            streamWriter.Write(Encoding.Unicode.GetPreamble());

            string line = string.Empty;
            while ((line = streamReader.ReadLine()) is not null)
            {
                streamWriter.WriteLine(line);
            }

            streamWriter.Flush();
        }

        File.Delete(filePath);
        File.Move(filePath + tempExt, filePath);
    }

    public void ExtractUSEnglishDictionary()
    {
        string usEnglishDictionaryZip = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            nameof(Microsoft),
            Spelling,
            USEnglish,
            USEnglishDictionaryZip);

        WriteResourceToFile(USEnglishResource, usEnglishDictionaryZip);

        if (File.Exists(usEnglishDictionaryZip))
        {
            using ZipArchive zipArchive = ZipFile.OpenRead(usEnglishDictionaryZip);
            ZipArchiveEntry zipArchiveEntry = zipArchive.GetEntry(USEnglishDictionary);
            zipArchiveEntry.ExtractToFile(_usEnglishDictionaryPath, true);
        }

        if (File.Exists(usEnglishDictionaryZip))
        {
            File.Delete(usEnglishDictionaryZip);
        }
    }

    public void InstallUSEnglishDictionary() => InstallDictionary(_usEnglishDictionaryPath);

    public void InstallDictionary(string filePath)
    {
        try
        {
            FileInfo fileInfo = new(filePath);

            if (!fileInfo.Exists)
            {
                ExtractUSEnglishDictionary();
            }

            fileInfo.Refresh();

            if (fileInfo.Exists && fileInfo.Length == 0)
            {
                File.Delete(filePath);
                ExtractUSEnglishDictionary();
            }

            fileInfo.Refresh();

            if (!fileInfo.Exists || fileInfo.Length == 0)
            {
                throw new InvalidOperationException($"Failed to extract '{filePath}'.");
            }

            if (ValidateDictionary(_usEnglishDictionaryPath))
            {
                RegisterUSEnglishDictionary();
            }
        }
        catch
        {
            throw;
        }
    }

    public void RegisterUSEnglishDictionary()
    {
        if (File.Exists(_usEnglishDictionaryPath))
        {
            spellChecker.RegisterUserDictionary(_usEnglishDictionaryPath, USEnglish);
        }
    }

    /// <summary>
    ///  Checks whether the specified dictionary is UTF-16 LE plaintext and starts with the required Byte Order Mark (BOM).
    /// </summary>
    /// <remarks>
    /// <para>
    ///  Inspects the first two bytes of the file for the Byte Order Mark (0xFF 0xFE).
    /// </para>
    /// </remarks>
    /// <param name="dictionaryPath">The file path of the dictionary to validate.</param>
    /// <returns>
    ///  <see langword="true"/> if the specified dictionary is valid, otherwise <see langword="false"/>.
    /// </returns>
    public bool ValidateDictionary(string dictionaryPath)
    {
        byte[] bom = new byte[4];
        using (FileStream fileStream = new(dictionaryPath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            int bytesRead = fileStream.Read(bom, 0, bom.Length);
        }

        return bom[0] == 0xff && bom[1] == 0xfe;
    }

    /// <summary>
    ///  Writes the specified resource to the specified file.
    /// </summary>
    /// <param name="resourceName">The name of the resource.</param>
    /// <param name="filePath">The file path to write the resource to.</param>
    public void WriteResourceToFile(string resourceName, string filePath)
    {
        using Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"The resource '{resourceName}' was not found.");
        using FileStream fileStream = new(filePath, FileMode.Create, FileAccess.Write);
        resourceStream.CopyTo(fileStream);
    }
}
