using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Virtuplex.SampleCalculator.Calculations;

namespace Virtuplex.SampleCalculator.Services
{
    /// <summary>
    /// Service used to generat file with random expressions.
    /// </summary>
    public class FileGeneratorService
    {
        /// <summary>
        /// Generates a random file and fills the mwith expressions. The file is around 3 gig. 
        /// This functionality was only created for generating a file and is not performance optimized. 
        /// </summary>
        /// <param name="fileName">Output file.</param>
        /// <param name="progress">Progress reported to ui.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public async Task<bool> GenerateFile(string fileName, IProgress<string> progress, CancellationToken token)
        {
            try
            {
                // clear existing file
                if(File.Exists(fileName))
                {
                    using (var fs = File.Open(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        lock (fs)
                        {
                            fs.SetLength(0);
                        }
                    }
                }

                using (var fs = File.OpenWrite(fileName))
                {
                    using (var sw = new StreamWriter(fs))
                    {
                        var random = new Random();
                        var currentExpression = new StringBuilder();
                        var expressions = 10000; // Should be around 2 gigs
                        var ops = new char[4] { '+', '-', '*', '/' };

                        for (var line = 0; line < expressions; line++)
                        {
                            token.ThrowIfCancellationRequested(); 

                            var maxOperations = random.Next(2, 10); 

                            // Add some invalid lines
                            if (line % 10 == 0)
                            {
                                sw.WriteLine("3 + a");
                                continue;
                            }

                            // Add some blanks
                            if (line % 15 == 0)
                            {
                                sw.WriteLine(" ");
                            }

                            for (var i = 0; i <= maxOperations; i++)
                            { 
                                // some negtive numbers 
                                if (i > 0 && i % random.Next(1, 5) == 0)
                                {
                                    currentExpression.Append("-");
                                }

                                currentExpression.Append(random.Next(0, int.MaxValue));

                                if (i < maxOperations)
                                {
                                    // add radom operation
                                    currentExpression.AppendFormat(" {0} ", ops[random.Next(0, 3)]);
                                }
                            }

                            currentExpression.Append(Environment.NewLine);

                            progress.Report($"{line} / {expressions}");

                            await sw.WriteLineAsync(currentExpression.ToString());
                        }
                    }
                }

                return true;
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Operation cancelled by user.");

                return false; 
            }
            catch (Exception e)
            {
                MessageBox.Show($"Unable to create file: {e.Message}");

                return false;
            }
        } 
    }
}
