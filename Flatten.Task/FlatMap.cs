using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Flatten.Core;
using Microsoft.Build.Framework;

namespace Flatten.Task
{

    public class FlatMap : Microsoft.Build.Utilities.Task
    {

        private const string DefaultPrefix = "flat";

        #region Task Parameters

        [Required]
        public ITaskItem[] XmlFiles { get; set; }

        [Required]
        public string ElementSelector { get; set; }

        [Required]
        public string KeyName { get; set; }

        public string ResultPrefix { get; set; }

        public string Delimiters { get; set; }

        [Output]
        public string[] Results { get; set; }

        #endregion

        public override bool Execute()
        {
            LogEnvironment();

            List<string> results = new List<string>();
            foreach (var item in XmlFiles)
            {
                var documentPath = item.ItemSpec;

                Log.LogMessage("Processing {0}...", documentPath);

                try
                {
                    var flattenedPath = GetResultPath(documentPath);
                    
                    if (File.Exists(flattenedPath)) Log.LogMessage(MessageImportance.High, "Overwriting {0}", flattenedPath);

                    results.Add(
                        WriteXDoc(
                            FlattenDocument(documentPath), 
                            flattenedPath));

                    Log.LogMessage("Processing {0} completed successfully", documentPath);
                }
                catch (Exception exception)
                {
                    Log.LogError("Processing {0} failed", documentPath);
                    Log.LogErrorFromException(exception);

                    return false;
                }

                Results = results.ToArray();
            }

            return true;                       
        }

        private void LogEnvironment()
        {
            Log.LogMessage(MessageImportance.High, "ElementSelector={0}", ElementSelector);
            Log.LogMessage(MessageImportance.High, "KeyName={0}", KeyName);
            if (!string.IsNullOrWhiteSpace(ResultPrefix)) Log.LogMessage(MessageImportance.High, "ResultPrefix={0}", ResultPrefix);
            if (!string.IsNullOrWhiteSpace(Delimiters)) Log.LogMessage(MessageImportance.High, "Delimiters={0}", new string(Delimiters.ToCharArray()));
        }

        public XDocument FlattenDocument(string documentPath)
        {
            var delimiters = string.IsNullOrWhiteSpace(Delimiters)
                                 ? Flattener.DefaultDelimiters
                                 : Delimiters.ToCharArray();
            return FlattenDocument(documentPath, ElementSelector, KeyName, delimiters);
        }

        public static XDocument FlattenDocument(string documentPath, string elementSelector, string keyName, IEnumerable<char> delimiters)
        {
            using (var stream = new FileStream(documentPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var xdocument = XDocument.Load(stream);
                return Flattener.Flatten(xdocument, elementSelector, keyName, delimiters);
            }
        }

        public string GetResultPath(string documentPath)
        {
            var prefix =  string.IsNullOrWhiteSpace(ResultPrefix)
                              ? DefaultPrefix
                              : ResultPrefix;
            return GetResultPath(documentPath, prefix);
        }

        public static string GetResultPath(string documentPath, string prefix)
        {
            var directoryName = Path.GetDirectoryName(documentPath) ?? string.Empty;
            var originalName = Path.GetFileName(documentPath);
            string flattenedName = string.Format("{0}.{1}", prefix, originalName);
            return Path.Combine(directoryName, flattenedName);
        }

        public static string WriteXDoc(XDocument xdoc, string destination)
        {
            using (var stream = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                xdoc.Save(stream, SaveOptions.None);
            }

            return destination;
        }

    }

}
