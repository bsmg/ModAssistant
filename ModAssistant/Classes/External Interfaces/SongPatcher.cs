using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SaberSongPatcher;

namespace ModAssistant.API
{
    public class SongPatcher
    {
        public static async Task<bool> PromptAndPatchSongFromDisk(string directory)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            var supportedExtensions = new[]
            {
                "mp3",
                "m4a",
                "ogg",
                "wav",
                "flac",
                "aiff",
                "wma",
            };
            var audioExtensions = string.Join(";", supportedExtensions.Select(ext => $"*.{ext}"));
            openFileDialog.Title = "Select master song audio file";
            openFileDialog.Filter = $"Audio files ({audioExtensions})|{audioExtensions}|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

            var success = false;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                success = await PerformPatch(openFileDialog.FileName, directory);
            }
            return success;
        }

        private static async Task<bool> PerformPatch(string inputFile, string mapDirectory)
        {
            var config = ConfigParser.ParseConfig(true, mapDirectory);
            var context = new Context(config);
            var inputValidator = new InputValidator(context);
            var inputTransformer = new InputTransformer(context);

            var seemsCorrect = await inputValidator.ValidateInput(inputFile, mapDirectory);
            if (!seemsCorrect)
            {
                return false;
            }

            var outputFile = Path.Combine(mapDirectory, "song.egg");
            var success = await inputTransformer.TransformInput(inputFile, outputFile);
            if (!success)
            {
                throw new Exception("Failed to transform audio.");
            }

            return true;
        }
    }
}
