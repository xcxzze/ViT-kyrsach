#if UNITY_EDITOR

using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace _ViT.CodeBase.Debug
{
    public class ScriptCollector : EditorWindow
    {
        [MenuItem("ViT kyrsach Tools/Export All Scripts")]
        static void ExportBombermanScripts()
        {
            string codebasePath = Path.Combine(Application.dataPath, "_ViT/CodeBase");
            string debugFolderPath = Path.Combine(codebasePath, "Debug");
            string outputPath = Path.Combine(debugFolderPath, "ViTScripts.txt");
        
            // Создаем папку Debug если её нет
            if (!Directory.Exists(debugFolderPath))
            {
                Directory.CreateDirectory(debugFolderPath);
            }
        
            if (!Directory.Exists(codebasePath))
            {
                UnityEngine.Debug.LogError("CodeBase path not found: " + codebasePath);
                return;
            }
        
            StringBuilder allScripts = new StringBuilder();
            allScripts.AppendLine($"Exported: {System.DateTime.Now}");
            allScripts.AppendLine();
        
            int fileCount = CollectScriptsFromFolder(codebasePath, debugFolderPath, allScripts);
        
            File.WriteAllText(outputPath, allScripts.ToString());
            UnityEngine.Debug.Log($"Exported {fileCount} scripts from CodeBase to: {outputPath}");
        
            // Показываем файл в проводнике
            EditorUtility.RevealInFinder(outputPath);
        }
    
        static int CollectScriptsFromFolder(string folderPath, string excludeFolder, StringBuilder output)
        {
            int count = 0;
        
            // Пропускаем папку Debug
            if (folderPath.Equals(excludeFolder, System.StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }
        
            // Добавляем все .cs файлы в текущей папке
            string[] csFiles = Directory.GetFiles(folderPath, "*.cs");
            foreach (string file in csFiles)
            {
                output.AppendLine(File.ReadAllText(file));
                output.AppendLine();
                count++;
            }
        
            // Рекурсивно обрабатываем подпапки
            string[] subFolders = Directory.GetDirectories(folderPath);
            foreach (string folder in subFolders)
            {
                // Пропускаем папку Debug и её подпапки
                if (!folder.StartsWith(excludeFolder))
                {
                    count += CollectScriptsFromFolder(folder, excludeFolder, output);
                }
            }
        
            return count;
        }
    }
}

#endif