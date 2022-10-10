using System.Diagnostics;

namespace Core
{
    public class Transferer
    {
        /// <summary>
        /// transfer single file [Source] -> [Target] dir
        /// </summary>
        /// <param name="source">file to copy</param>
        /// <param name="target">dir to copy to</param>
        /// <param name="overwrite">overwrite if exists</param>
        /// <returns></returns>
        public static bool Transfer(string source, string target, bool overwrite = false)
        {
            Debug.Assert(File.Exists(source), "file not found");
            Debug.Assert(Path.IsPathFullyQualified(target), "invalid path");

            try
            {
                string targetFile = Path.Combine(target, Path.GetFileName(source));
                Debug.Assert(Path.IsPathFullyQualified(targetFile), "invalid file path");
                File.Copy(source, targetFile, overwrite);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(
                    string.Format(
                        "({0})[{1}]\n",
                        e.Source,
                        e.Message
                    )
                );
                Console.ResetColor();

                return false;
            }

            return true;
        }

        /// <summary>
        /// multi-threaded transfering
        /// </summary>
        /// <param name="sources">files to transfer</param>
        /// <param name="target">directory to copy to</param>
        /// <param name="numThreads">degree of parallelism</param>
        /// <param name="overwrite">overwrite if file exists</param>
        /// <returns></returns>
        public static bool Transfer(List<string> sources, string target, int numThreads = 4, bool overwrite = false)
        {
            bool result = true;

            sources
                .AsParallel()
                .WithDegreeOfParallelism(numThreads)
                .ForAll((source) =>
                {
                    bool r = Transfer(source, target, overwrite);

                    if (!r)
                    {
                        result = false;
                    }
                }
            );

            return result;
        }
    }
}