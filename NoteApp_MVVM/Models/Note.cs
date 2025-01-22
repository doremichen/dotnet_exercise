
using System.Diagnostics;


namespace Notes.Models;

internal class Note
{

    public string Filename { get; set; }
    public string Text { get; set; }
    public DateTime Date { get; set; }

    public Note()
    {
        Filename = $"{Path.GetRandomFileName}.notes.txt";
        Date = DateTime.Now ;
        Text = string.Empty ;
    }

    
    public void Save() =>
        File.WriteAllText(System.IO.Path.Combine(FileSystem.AppDataDirectory, Filename), Text);

    public void Delete() => File.Delete(System.IO.Path.Combine(FileSystem.AppDataDirectory, Filename));


    public static Note Load(string filename)
    {
        Debug.WriteLine($"+++ Load({filename}) +++");
        filename = System.IO.Path.Combine(FileSystem.AppDataDirectory, filename);
        // check file existence
        if (!File.Exists(filename))
        {
            throw new FileNotFoundException("404: ", filename);
        }

        return
            new()
            {
                Filename = Path.GetFileName(filename),
                Text = File.ReadAllText(filename),
                Date = File.GetLastWriteTime(filename)
            };

    }

    public static IEnumerable<Note> LoadAll()
    {
        // Get folder path
        string appDataPath = FileSystem.AppDataDirectory;

        // lod the *.notes.txt files
        return Directory

            // Select the file names from the directory
            .EnumerateFiles(appDataPath, "*.notes.txt")

            // Each file name is used to load a note
            .Select(filename => Note.Load(Path.GetFileName(filename)))

            // With the final collection of notes, order them by date
            .OrderByDescending(note => note.Date);
    }
}