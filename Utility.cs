// <copyright file="Utility.cs" company="Tetraskelion Softwares Pvt. Ltd.">
// Copyright (c) Tetraskelion Softwares Pvt. Ltd. All rights reserved.
// </copyright>

namespace TravelMint.UI
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Utility class
    /// </summary>
    public class Utility
    {
        /// <summary>
        /// The files list
        /// </summary>
        private string[] filesList;

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the temporary folder.
        /// </summary>
        /// <value>
        /// The temporary folder.
        /// </value>
        public string TempFolder { get; set; }

        /// <summary>
        /// Gets or sets the maximum file size mb.
        /// </summary>
        /// <value>
        /// The maximum file size mb.
        /// </value>
        public int MaxFileSizeMB { get; set; }

        /// <summary>
        /// original name + ".part_N.X" (N = file part number, X = total files)
        /// Objective = enumerate files in folder, look for all matching parts of split file. If found, merge and return true.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>returns boolesn value</returns>
        public bool MergeFile(string fileName)
        {
            bool result = false;

            string partToken = ".part_";
            if (fileName != null)
            {
                string baseFileName = fileName.Substring(0, fileName.IndexOf(partToken, StringComparison.Ordinal));
                if (baseFileName != null)
                {
                    string trailingTokens = fileName.Substring(fileName.IndexOf(partToken, StringComparison.Ordinal) + partToken.Length);
                    int fileIndex;
                    int fileCount;
                    fileIndex = Convert.ToInt32(trailingTokens.Substring(0, trailingTokens.IndexOf(".", StringComparison.Ordinal)), CultureInfo.CurrentCulture);
                    fileCount = Convert.ToInt32(trailingTokens.Substring(trailingTokens.IndexOf(".", StringComparison.Ordinal) + 1), CultureInfo.CurrentCulture);

                    string searchpattern = Path.GetFileName(baseFileName) + partToken + "*";
                    this.filesList = Directory.GetFiles(Path.GetDirectoryName(fileName), searchpattern);

                    if (this.filesList.Count() == fileCount)
                    {
                        if (!MergeFileManager.Instance.InUse(baseFileName))
                        {
                            MergeFileManager.Instance.AddFile(baseFileName);
                            if (File.Exists(baseFileName))
                            {
                                File.Delete(baseFileName);
                            }

                            List<SortedFile> mergeList = new List<SortedFile>();
                            foreach (string fileNames in this.filesList)
                            {
                                baseFileName = fileNames.Substring(0, fileNames.IndexOf(partToken, StringComparison.Ordinal));
                                trailingTokens = fileNames.Substring(fileNames.IndexOf(partToken, StringComparison.Ordinal) + partToken.Length);
                                fileIndex = Convert.ToInt32(trailingTokens.Substring(0, trailingTokens.IndexOf(".", StringComparison.Ordinal)), CultureInfo.CurrentCulture);
                                mergeList.Add(new SortedFile { FileName = fileNames, FileOrder = fileIndex });
                            }

                            var mergeOrder = mergeList.OrderBy(s => s.FileOrder).ToList();
                            using (FileStream fileStream = new FileStream(baseFileName, FileMode.Create))
                            {
                                foreach (var chunk in mergeOrder)
                                {
                                    using (FileStream fileChunk = new FileStream(chunk.FileName, FileMode.Open))
                                    {
                                        fileChunk.CopyTo(fileStream);
                                    }
                                }
                            }

                            result = true;

                            MergeFileManager.Instance.RemoveFile(baseFileName);
                            this.DeleteIndividualChunks();
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Deletes the individual chunks.
        /// </summary>
        private void DeleteIndividualChunks()
        {
            foreach (string files in this.filesList)
            {
                if (File.Exists(@files))
                {
                    File.Delete(@files);
                }
            }
        }
    }
}