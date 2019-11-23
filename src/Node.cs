﻿using System;
using System.IO;
using YA.TenantWorker.Constants;

namespace YA.TenantWorker
{
    internal static class Node
    {
        internal static readonly string Id;

        static Node()
        {
            string filePath = Path.Combine(Program.RootPath, General.AppDataFolderName, "nodeid");

            if (!File.Exists(filePath))
            {
                Id = Guid.NewGuid().ToString("N");
                File.WriteAllText(filePath, Id);
            }
            else
            {
                Id = File.ReadAllText(filePath).Trim();
            }
        }
    }
}
