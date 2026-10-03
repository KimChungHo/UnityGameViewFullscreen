using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameViewFullscreen.Distribution
{
    public static class GameViewFullscreenPackageExporter
    {
        public static void Export()
        {
            const string assetRoot = "Assets/Editor/GameViewFullscreen";
            if (!AssetDatabase.IsValidFolder(assetRoot))
                throw new DirectoryNotFoundException(assetRoot);

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string outputDirectory = Path.Combine(projectRoot, "Releases");
            Directory.CreateDirectory(outputDirectory);
            string packagePath = Path.Combine(outputDirectory, "GameViewFullscreen.unitypackage");

            // 기능 폴더만 포함하여 배포 도구와 다른 프로젝트 코드가 섞이지 않게 합니다.
            AssetDatabase.ExportPackage(new[] { assetRoot }, packagePath, ExportPackageOptions.Recurse);
            if (!File.Exists(packagePath) || new FileInfo(packagePath).Length == 0)
                throw new IOException(packagePath);
            Debug.Log(packagePath);
        }
    }
}
