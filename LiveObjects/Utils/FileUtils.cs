using System;
using System.IO;

namespace LiveObjects.Utils
{
    public class FileUtils
    {
        public static string ProvidePath(string filename)
        {
            filename = GetFullPath(filename);
            var di = new DirectoryInfo(Path.GetDirectoryName(filename));
            di.Create();
            return filename;
        }

        public static string GetFullPath(string path, string baseDir = null)
        {
            baseDir = baseDir ?? AppDomain.CurrentDomain.BaseDirectory;
            path = Path.IsPathRooted(path) ? path : Path.Combine(baseDir, path);
            return path;
        }

        public static string GetNextFilename(string fn)
        {
            var currFn = fn;
            var path = Path.GetDirectoryName(fn);
            var fnWoExt = Path.GetFileNameWithoutExtension(fn);
            var ext = Path.GetExtension(fn);
            
            var idx = 1;
            while (File.Exists(currFn))
                currFn = Path.Combine(path, fnWoExt + "_" + (idx++) + ext);

            return currFn;
        }

        public static string ProvideRotatedFileName(string fn)
        {
            var nextFileName = GetNextFilename(fn);
            if (nextFileName != fn)
                File.Move(fn, nextFileName);

            return ProvidePath(fn);
        }

        public static string Cache(string filename, TimeSpan cacheTime, Func<string> action)
        {
            string result;
            if(!File.Exists(filename) || (DateTime.Now - new FileInfo(filename).LastWriteTime) > cacheTime)
                File.WriteAllText(filename, result = action());
            else
                result = File.ReadAllText(filename);
            return result;
        }
    }
}
