﻿using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System;

public class MakeAtlasTools
{
    #region
    [MenuItem("UnityEditor/GenerationPackage/MakeAllAtlas")]
    static void MakeAllAtlas()
    {
        string targetDir = Application.dataPath + "/ResourceABs/atlas";
        CheckAtlas(targetDir);
        Debug.Log("检查图集完毕");
    }

    public static void CheckAtlas(string path)
    {
        DirectoryInfo mDir = new DirectoryInfo(path);
        foreach (var v in mDir.GetDirectories())
        {
            foreach (var v1 in v.GetFiles())
            {
                if (v1.Extension != ".meta")
                {
                    string assetPath = "Assets/ResourceABs/atlas/" + v.Name + "/" + v1.Name;
                    CheckTextureInfoAndRepairIfNeed(assetPath, v.Name);
                }
            }
        }
    }

    /// <summary>
    /// 检测一张图片是否正确设置为sprite，sprite的参数是否符合要求//
    /// </summary>
    /// <param name="assetPath"> 图片在Unity项目中的路径（Assets/...）</param>
    /// <param name="UITypeFileName">图片的打包标签参考对象</param>
    /// <param name="needRepair">是否需要修复，需要修复的话就不打错误日志了而是修复日志</param>
    /// <returns>图片是否设置正确</returns>
    public static bool CheckTextureInfoAndRepairIfNeed(string assetPath, string UITypeFileName = null, bool needRepair = true)
    {
        TextureImporter ti = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        bool isRight = true;
        if (ti.textureType != TextureImporterType.Sprite)
        {
            if (!needRepair)
            {
                string debugLog = "TextrueType must be sprite where path is \"{0}\", maybe you forget to set it?If not ,don't move it into {1} file.";
                Debug.LogError(string.Format(debugLog, assetPath));
            }
            isRight = false;
        }
        else if (!string.IsNullOrEmpty(UITypeFileName) && ti.spritePackingTag != UITypeFileName)
        {
            if (!needRepair)
            {
                string debugLog = "The spritePackingTag of the texture is different from the name of its parent file where path is \"{0}\", if this is not your wanted, check it.";
                Debug.LogError(string.Format(debugLog, assetPath));
            }
            isRight = false;
        }
        else if (ti.mipmapEnabled)
        {
            if (!needRepair)
            {
                string debugLog = "Are you sure this sprite need mipmap where path is \"{0}\"? If not, check it.";
                Debug.LogError(string.Format(debugLog, assetPath));
            }
            // UI图都不需要mipmap//
            isRight = false;
        } else if (ti.textureFormat != TextureImporterFormat.AutomaticCompressed)
        {
            isRight = false;
        } else if(ti.spritePixelsPerUnit!=100)
        {
            isRight = false;
        }

        if (!isRight && needRepair)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.spritePackingTag = UITypeFileName;
            ti.mipmapEnabled = false;
            ti.spritePixelsPerUnit = 100;
            ti.textureFormat = TextureImporterFormat.AutomaticCompressed;
            ti.SaveAndReimport();
            Debug.Log(string.Format("The set of Texture where path is \"{0}\" has been repaired.", assetPath));
        }
        return isRight;
    }
    #endregion
}
