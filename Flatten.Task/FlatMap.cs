using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Flatten.Core;
using Microsoft.Build.Framework;

namespace Flatten.Task
{

    public class FlatMap : Microsoft.Build.Utilities.Task
    {

        private const string DefaultPrefix = "flat";

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

        private void LogExecutionState()
        {
            Log.LogMessage(MessageImportance.High, "ElementSelector={0}", ElementSelector);
            Log.LogMessage(MessageImportance.High, "KeyName={0}", KeyName);
            if (!string.IsNullOrWhiteSpace(ResultPrefix)) Log.LogMessage(MessageImportance.High, "ResultPrefix={0}", ResultPrefix);
            if (!string.IsNullOrWhiteSpace(Delimiters)) Log.LogMessage(MessageImportance.High, "Delimiters={0}", string.Join("", Delimiters.ToCharArray()));
        }

        public override bool Execute()
        {
            LogExecutionState();

            List<string> results = new List<string>();
            foreach (var item in XmlFiles)
            {
                Log.LogMessage("Processing {0}...", item.ItemSpec);

                try
                {
                    XDocument flattenedDocument;
                    using (var stream = new FileStream(item.ItemSpec, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var xdocument = XDocument.Load(stream);
                        var delimiters = string.IsNullOrWhiteSpace(Delimiters)
                                             ? Flattener.DefaultDelimiters
                                             : Delimiters.ToCharArray();
                        flattenedDocument = Flattener.Flatten(xdocument, ElementSelector, KeyName, delimiters);
                    }

                    var directoryName = Path.GetDirectoryName(item.ItemSpec);
                    var originalName = Path.GetFileName(item.ItemSpec);
                    string flattenedName = string.Format("{0}.{1}",
                                               string.IsNullOrWhiteSpace(ResultPrefix) 
                                                   ? DefaultPrefix 
                                                   : ResultPrefix,
                                               originalName);
                    var flattenedPath = Path.Combine(directoryName, flattenedName);
                    
                    if (File.Exists(flattenedPath)) Log.LogMessage(MessageImportance.High, "Overwriting {0}", flattenedPath);

                    using (var stream = new FileStream(flattenedPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        flattenedDocument.Save(stream, SaveOptions.None);
                    }

                    results.Add(flattenedPath);

                    Log.LogMessage("Processing {0} completed successfully", item.ItemSpec);
                }
                catch (Exception exception)
                {
                    Log.LogError("Processing {0} failed", item.ItemSpec);
                    Log.LogErrorFromException(exception);

                    return false;
                }

                Results = results.ToArray();
            }

            return true;                       
        }

    }

}
