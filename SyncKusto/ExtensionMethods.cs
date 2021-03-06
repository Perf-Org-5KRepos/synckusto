﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using Kusto.Data.Common;
using SyncKusto.Kusto;

namespace SyncKusto
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Write the function to the file system.
        /// </summary>
        /// <param name="functionSchema">The function to write</param>
        /// <param name="rootFolder">The root folder for all the CSL files</param>
        /// <returns></returns>
        public static void WriteToFile(this FunctionSchema functionSchema, string rootFolder)
        {
            string filename = functionSchema.Name + ".csl";

            // First remove any other files with this name. In the case where you moved an object to a new folder, this will handle cleaning up the old file
            string[] existingFiles = Directory.GetFiles(rootFolder, filename, SearchOption.AllDirectories);
            if (existingFiles.Length > 0)
            {
                foreach (string file in existingFiles)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        // It's not the end of the world if this call fails
                    }
                }
            }

            // Now add write the new file to the correct location.
            string funcFolder = Path.Combine(rootFolder, "Functions");
            if (!string.IsNullOrEmpty(functionSchema.Folder))
            {
                string cleanedFolder = string.Join("", functionSchema.Folder.Split(Path.GetInvalidPathChars()));
                funcFolder = Path.Combine(funcFolder, cleanedFolder);
            }
            
            string destinationFile = Path.Combine(funcFolder, filename);
            if (!Directory.Exists(funcFolder))
            {
                Directory.CreateDirectory(funcFolder);
            }

            File.WriteAllText(destinationFile, CslCommandGenerator.GenerateCreateOrAlterFunctionCommand(functionSchema, true));
        }

        /// <summary>
        /// Write a function to Kusto
        /// </summary>
        /// <param name="functionSchema">The function to write</param>
        /// <param name="kustoQueryEngine">An initialized query engine for issuing the Kusto command</param>
        public static void WriteToKusto(this FunctionSchema functionSchema, QueryEngine kustoQueryEngine)
        {
            kustoQueryEngine.CreateOrAlterFunctionAsync(CslCommandGenerator.GenerateCreateOrAlterFunctionCommand(functionSchema, true), functionSchema.Name).Wait();
        }

        /// <summary>
        /// Delete a function from the file system
        /// </summary>
        /// <param name="functionSchema">The function to remove</param>
        /// <param name="rootFolder">The root folder for all the CSL files</param>
        public static void DeleteFromFolder(this FunctionSchema functionSchema, string rootFolder)
        {
            string funcFolder = Path.Combine(rootFolder, "Functions");
            if (!string.IsNullOrEmpty(functionSchema.Folder))
            {
                funcFolder = Path.Combine(funcFolder, functionSchema.Folder);
            }
            string destinationFile = Path.Combine(funcFolder, functionSchema.Name + ".csl");
            File.Delete(destinationFile);
        }

        /// <summary>
        /// Delete a function from Kusto
        /// </summary>
        /// <param name="functionSchema">The function to remove</param>
        /// <param name="kustoQueryEngine">An initialized query engine for issuing the Kusto command</param>
        public static void DeleteFromKusto(this FunctionSchema functionSchema, QueryEngine kustoQueryEngine)
        {
            kustoQueryEngine.DropFunction(functionSchema);
        }

        /// <summary>
        /// Write a table to the file system
        /// </summary>
        /// <param name="tableSchema">The table to write</param>
        /// <param name="rootFolder">The root folder for all the CSL files</param>
        public static void WriteToFile(this TableSchema tableSchema, string rootFolder)
        {
            string tableFolder = rootFolder;
            if (!string.IsNullOrEmpty(tableSchema.Folder))
            {
                string cleanedFolder = string.Join("", tableSchema.Folder.Split(Path.GetInvalidPathChars()));
                tableFolder = Path.Combine(rootFolder, "Tables", cleanedFolder);
            }
            string destinationFile = Path.Combine(tableFolder, tableSchema.Name + ".csl");
            if (!Directory.Exists(tableFolder))
            {
                Directory.CreateDirectory(tableFolder);
            }

            File.WriteAllText(destinationFile, CslCommandGenerator.GenerateTableCreateCommand(tableSchema, true));
        }

        /// <summary>
        /// Write a table to Kusto
        /// </summary>
        /// <param name="tableSchema">The table to write</param>
        /// <param name="kustoQueryEngine">An initialized query engine for issuing the Kusto command</param>
        public static void WriteToKusto(this TableSchema tableSchema, QueryEngine kustoQueryEngine)
        {
            kustoQueryEngine.CreateOrAlterTableAsync(CslCommandGenerator.GenerateTableCreateCommand(tableSchema, false), tableSchema.Name).Wait();
        }

        /// <summary>
        /// Delete a table from the file system
        /// </summary>
        /// <param name="tableSchema">The table to remove</param>
        /// <param name="rootFolder">The root folder for all the CSL files</param>
        public static void DeleteFromFolder(this TableSchema tableSchema, string rootFolder)
        {
            string tableFolder = rootFolder;
            if (!string.IsNullOrEmpty(tableSchema.Folder))
            {
                tableFolder = Path.Combine(rootFolder, "Tables", tableSchema.Folder);
            }
            string destinationFile = Path.Combine(tableFolder, tableSchema.Name + ".csl");
            File.Delete(destinationFile);
        }

        /// <summary>
        /// Delete a table from Kusto
        /// </summary>
        /// <param name="tableSchema">The table to remove</param>
        /// <param name="kustoQueryEngine">An initialized query engine for issuing the Kusto command</param>
        public static void DeleteFromKusto(this TableSchema tableSchema, QueryEngine kustoQueryEngine)
        {
            kustoQueryEngine.DropTable(tableSchema.Name);
        }
    }
}
