using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Specialized;
using System.Diagnostics;

namespace WindowsAzureCompanion.VMManagerService
{
    public static class FileUtils
    {
        public static void ReplaceStringInFile(string fileName, string source, string target)
        {
            try
            {
                StreamReader streamReader = null;
                StreamWriter streamWriter = null;
                string contents = null;

                streamReader = File.OpenText(fileName);
                contents = streamReader.ReadToEnd();
                streamReader.Close();
                streamWriter = File.CreateText(fileName);
                streamWriter.Write(contents.Replace(source, target));
                streamWriter.Close();
            }
            catch (Exception ex)
            {
                Trace.TraceError("File I/O Error: {0}, StackTrace: {1}", ex.Message, ex.StackTrace);
            }
        }

        public static void AppendToFile(string fileName, string line)
        {
            try
            {
                using (StreamWriter sw = File.AppendText(fileName)) 
                {
                    sw.WriteLine(line);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("File I/O Error: {0}, StackTrace: {1}", ex.Message, ex.StackTrace);
            }
        }

        public static void WriteToFile(string fileName, string line)
        {
            try
            {
                using (StreamWriter sw = File.CreateText(fileName))
                {
                    sw.WriteLine(line);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("File I/O Error: {0}, StackTrace: {1}", ex.Message, ex.StackTrace);
            }
        }

        public static string SubstituteParameters(NameValueCollection parameters, string input)
        {
            string output = input;
            try
            {
                foreach (var parameter in parameters.AllKeys)
                {
                    output = output.Replace("{" + parameter + "}", parameters[parameter]);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("File I/O Error: {0}, StackTrace: {1}", ex.Message, ex.StackTrace);
            }
            return output;
        }

        public static void WriteConfigFile(NameValueCollection parameters, string inPath, string outPath)
        {
            try
            {
                File.WriteAllText(outPath, SubstituteParameters(parameters, File.ReadAllText(inPath)));
            }
            catch (Exception ex)
            {
                Trace.TraceError("File I/O Error: {0}, StackTrace: {1}", ex.Message, ex.StackTrace);
            }
        }
    }
}
