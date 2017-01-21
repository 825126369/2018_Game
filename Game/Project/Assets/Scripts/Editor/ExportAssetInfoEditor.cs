using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;
using xk_System.AssetPackage;

public class ExportAssetInfoEditor : MonoBehaviour
{
    static string extention = AssetBundlePath.ABExtention;
    static string BuildAssetPath = "Assets/ResourceABs";
    static string CsOutPath = "Assets/Scripts/auto";

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~创建AB文件所有的信息~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [MenuItem("UnityEditor/GenerationPackage/Generation AssetInfo Cs File")]
    public static void GenericAssetCSInfo()
    {
        Debug.Log("Start Generation AssetInfo Cs Info");
        CreateABCSFile();
        Debug.Log("Finish Generation AssetInfo Cs Info");
    }

    private static void CreateABCSFile()
    {
        string m = "";
        m += "namespace xk_System.AssetPackage\n{\n";
        DirectoryInfo mDir = new DirectoryInfo(BuildAssetPath);
        m += "\tpublic class " + mDir.Name + "Folder : Singleton<" + mDir.Name + "Folder>\n\t{\n";
        string s = "";
        foreach (var v in mDir.GetDirectories())
        {
            FileInfo[] mFileInfos1 = v.GetFiles();
            int mFilesLength1 = 0;
            foreach (var v1 in mFileInfos1)
            {
                if (v1.Extension != ".meta")
                {
                    mFilesLength1++;
                    break;
                }
            }
            if (mFilesLength1 > 0 || v.GetDirectories().Length > 0)
            {
                string fieldName = v.Name + "Folder";
                m += "\t\t public " + fieldName + " " + v.Name + "=new " + fieldName + "();\n";
               // s += CreateDirClass(v, v.Name.ToLower());
            }
        }
        foreach (var v in mDir.GetDirectories())
        {
            m += CreateDirClass(v, v.Name.ToLower());
        }
        m += "\t}\n";
       // m += s;
        m += "}\n";
        string fileName = CsOutPath + "/" + mDir.Name + ".cs";
        StreamWriter mSw = new StreamWriter(fileName, false);
        mSw.Write(m);
        mSw.Close();
    }

    private static string CreateDirClass(DirectoryInfo mDir, string bundleName)
    {
        string tStr = GetTStr(mDir);
        string m = "";
        string s = "";
        FileInfo[] mFileInfos = mDir.GetFiles();
        int mFilesLength = 0;
        foreach (var v in mFileInfos)
        {
            if (v.Extension != ".meta")
            {
                mFilesLength++;
                break;
            }
        }
        if (mFilesLength > 0)
        {
            string bundleName1 = bundleName+ extention;
            m = tStr+"public class " + mDir.Name + "Folder\n"+tStr+"{\n";
            foreach (var v in mFileInfos)
            {
                if (v.Extension != ".meta")
                {
                    string assetPath = GetAssetPath(v.FullName);
                    string fileName = v.Name.Substring(0, v.Name.LastIndexOf(v.Extension));
                    m += tStr+"\t public AssetInfo m" + fileName + "=new AssetInfo(\""+assetPath+"\",\"" + bundleName1 + "\",\"" + v.Name + "\");\n";
                }
            }
            m += tStr+"}\n";
        }
        else
        {
            if (mDir.GetDirectories().Length > 0)
            {

                m = tStr+"public class " + mDir.Name + "Folder\n"+tStr+"{\n";
                foreach (var v in mDir.GetDirectories())
                {
                    FileInfo[] mFileInfos1 = v.GetFiles();
                    int mFilesLength1 = 0;
                    foreach (var v1 in mFileInfos1)
                    {
                        if (v1.Extension != ".meta")
                        {
                            mFilesLength1++;
                            break;
                        }
                    }
                    if (mFilesLength1 > 0 || v.GetDirectories().Length > 0)
                    {
                        string fieldName = v.Name + "Folder";
                        m += tStr+"\t public " + fieldName + " " + v.Name + "=new " + fieldName + "();\n";
                    }
                }
                foreach (var v in mDir.GetDirectories())
                {
                    m += CreateDirClass(v, bundleName + "_" + v.Name.ToLower());
                }
                m += tStr+"}\n";
               // m += s;
            }
        }
        return m;
    }
    public static string GetTStr(DirectoryInfo mDir)
    {
        int coutT = 0;
        int index = mDir.FullName.IndexOf(@"ResourceABs\");
        if (index >= 0)
        {
            for(int j=0;j<mDir.FullName.Length;j++)
            {
                if (j > index)
                {
                    var v = mDir.FullName[j];
                    if (v.Equals('\\'))
                    {
                        coutT++;
                    }
                }
            }
        }
        coutT++;
        string tStr = "";
        int i = 0;
        while(i<coutT)
        {
            tStr += "\t";
            i++;
        }
        return tStr;
    }

    public static string GetAssetPath(string filePath)
    {
        string assetPath = "";
        int index = filePath.IndexOf(@"Assets\");
        if (index >= 0)
        {
            assetPath = filePath.Remove(0, index);
            assetPath = assetPath.Replace(@"\","/");
        }
        return assetPath;
    }

}
