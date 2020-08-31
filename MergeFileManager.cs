// <copyright file="MergeFileManager.cs" company="Tetraskelion Softwares Pvt. Ltd.">
// Copyright (c) Tetraskelion Softwares Pvt. Ltd. All rights reserved.
// </copyright>

namespace TravelMint.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Merge File Manager
    /// </summary>
    public class MergeFileManager
    {
        /// <summary>
        /// The instance
        /// </summary>
        private static MergeFileManager instance;

        /// <summary>
        /// The merge file list
        /// </summary>
        private List<string> mergeFileList;

        /// <summary>
        /// Prevents a default instance of the <see cref="MergeFileManager"/> class from being created.
        /// </summary>
        private MergeFileManager()
        {
            this.mergeFileList = new List<string>();
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static MergeFileManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MergeFileManager();
                }

                return instance;
            }
        }

        /// <summary>
        /// Adds the file.
        /// </summary>
        /// <param name="baseFileName">Name of the base file.</param>
        public void AddFile(string baseFileName)
        {
            this.mergeFileList.Add(baseFileName);
        }

        /// <summary>
        /// Ins the use.
        /// </summary>
        /// <param name="baseFileName">Name of the base file.</param>
        /// <returns>returns boolean value</returns>
        public bool InUse(string baseFileName)
        {
            return this.mergeFileList.Contains(baseFileName);
        }

        /// <summary>
        /// Removes the file.
        /// </summary>
        /// <param name="baseFileName">Name of the base file.</param>
        /// <returns>returns boolean value</returns>
        public bool RemoveFile(string baseFileName)
        {
            return this.mergeFileList.Remove(baseFileName);
        }
    }
}