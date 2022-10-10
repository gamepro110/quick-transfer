using Core;

namespace Tests
{
    public class Tests
    {
        private static List<string> GetFilePaths(string path, out Dictionary<string, FileInfo> info)
        {
            List<string> paths = new(Directory.GetFiles(path));

            info = new Dictionary<string, FileInfo>();
            foreach (var item in paths)
            {
                info.Add(item, new FileInfo(item));
            }

            return paths;
        }

        private static void PrintTotalTransferSize(Dictionary<string, FileInfo> info)
        {
            long size = 0;
            foreach (FileInfo item in info.Values)
            { size += item.Length; }

            Console.WriteLine($"transfer size (bytes/MB): ({size}/{size / 1024 / 1024})");
        }

        private static string GetTestDir()
        {
            string path = "";
            string[] partsOfPath = Environment.CurrentDirectory.Split('\\')[0..^3];

            foreach (string item in partsOfPath[0..^1])
            {
                path += $"{item}//";
            }
            path += partsOfPath[^1];

            return path;
        }

        [TestCase("\\testZipSource\\dev.rar", "\\testZipTarget\\", ExpectedResult = true)]
        public bool SingleFile(string srcFile, string targetDir)
        {
            string src = GetTestDir() + srcFile;
            string target = GetTestDir() + targetDir;
            return Transferer.Transfer(src, target, true);
        }

        [TestCase("\\testBulkSource", "\\testBulkTarget", 1, ExpectedResult = true, TestName = "bulk t = 01")]
        [TestCase("\\testBulkSource", "\\testBulkTarget", 2, ExpectedResult = true, TestName = "bulk t = 02")]
        [TestCase("\\testBulkSource", "\\testBulkTarget", 16, ExpectedResult = true, TestName = "bulk t = 16")]
        public bool Bulk(string sourceDir, string targetDir, int numThreads)
        {
            string src = GetTestDir() + sourceDir;
            string target = GetTestDir() + targetDir;

            List<string> files = GetFilePaths(src, out Dictionary<string, FileInfo> info);
            Console.WriteLine($"num files: {files.Count}");
            PrintTotalTransferSize(info);
            return Transferer.Transfer(files, target, numThreads, true);
        }

        [TestCase("\\testSourceDir", "\\testTargetDir", 1, ExpectedResult = true, TestName = "t = 01")]
        [TestCase("\\testSourceDir", "\\testTargetDir", 2, ExpectedResult = true, TestName = "t = 02")]
        [TestCase("\\testSourceDir", "\\testTargetDir", 4, ExpectedResult = true, TestName = "t = 04")]
        [TestCase("\\testSourceDir", "\\testTargetDir", 6, ExpectedResult = true, TestName = "t = 06")]
        [TestCase("\\testSourceDir", "\\testTargetDir", 8, ExpectedResult = true, TestName = "t = 08")]
        [TestCase("\\testSourceDir", "\\testTargetDir", 10, ExpectedResult = true, TestName = "t = 10")]
        [TestCase("\\testSourceDir", "\\testTargetDir", 12, ExpectedResult = true, TestName = "t = 12")]
        [TestCase("\\testSourceDir", "\\testTargetDir", 14, ExpectedResult = true, TestName = "t = 14")]
        [TestCase("\\testSourceDir", "\\testTargetDir", 16, ExpectedResult = true, TestName = "t = 16")]
        public bool DirContents(string srcValue, string trgValue, int numThreads)
        {
            string src = GetTestDir() + srcValue;
            string target = GetTestDir() + trgValue;

            List<string> files = GetFilePaths(src, out Dictionary<string, FileInfo> info);
            Console.WriteLine($"num files: {files.Count}");
            PrintTotalTransferSize(info);
            return Transferer.Transfer(files, target, numThreads, true);
        }
    }
}
