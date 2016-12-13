using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Threading.Tasks;

namespace exceltoxml
{
    class Program
    {
        static List<DataTable> mDataSetCollecion = new List<DataTable>();
        static List<DataTable> mServerDataSetCollecion = new List<DataTable>();
        static string TableDirectoryPath = "";
        static string ClientOutPath = "";
        static string ServerOutPath = "";
        static void Main(string[] args)
        {
            int length = args.Length;
            if (length < 3)
            {
                Debug("请依次填写3个命令行参数:");
                Debug("参数1：Excel表输入目录：");
                Debug("参数2：Client输出目录：");
                Debug("参数3：Server输出目录：");
            }
            else
            {
                TableDirectoryPath = args[0];
                ClientOutPath = args[1];
                ServerOutPath = args[2];

                Console.WriteLine("Excel表输入目录：" + TableDirectoryPath);
                Console.WriteLine("Client输出目录：" + ClientOutPath);
                Console.WriteLine("Server输出目录：" + ServerOutPath);
                if (!Directory.Exists(TableDirectoryPath))
                {
                    Debug("Excel表输入目录不存在：");
                }
                else if (!Directory.Exists(ClientOutPath))
                {
                    Debug("Client输出目录不存在：");
                }
                else if (!Directory.Exists(ServerOutPath))
                {
                    Debug("Server输出目录不存在：");
                }
                else
                {
                    ShowRules();
                    DirectoryInfo mDirectoryInfo = new DirectoryInfo(TableDirectoryPath);
                    GetAllTableByDirectory(mDirectoryInfo);
                }
            }
            Console.WriteLine("Finish");
            Console.ReadLine();

        }
        static void ShowRules()
        {
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.WriteLine("***************************Table Rules*****************************************");
            Console.WriteLine("第一行：server | client | null");
            Console.WriteLine("第二行：字段注释");
            Console.WriteLine("第三行：字段类型：int | string | int[length] | string[length] (length为数组的长度，数组值用#隔开)");
            Console.WriteLine("第四行：字段名");
            Console.WriteLine("其他行填具体数值");
            Console.WriteLine("每个表的第一个字段名是固定的：注释：序号 | 字段类型：int | 字段名：id");
            Console.WriteLine("每个表的表名必须要唯一，表中的字段名必须要唯一");
            Console.WriteLine("*******************************************************************************");
            Console.ResetColor();

        }
        static public void GetAllTableByDirectory(DirectoryInfo mDirectoryInfo)
        {
            foreach (FileInfo mFileInfo in mDirectoryInfo.GetFiles())
            {
                if (mFileInfo.Extension.Equals(".xlsx") | mFileInfo.Extension.Equals(".xls"))
                {
                    ExcelToDataSet(mFileInfo);
                }
            }

            CreateXMLDocument(mDataSetCollecion);
            CreateCSFile(mDataSetCollecion);
            CreateServerXMLDocument(mServerDataSetCollecion);
        }
        static void ExcelToDataSet(FileInfo mFileInfo)
        {
            string strCon = "";
            if (mFileInfo.Extension.Equals(".xlsx"))
            {
                strCon = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + mFileInfo.FullName + ";Extended Properties=\"Excel 12.0 Xml;HDR=NO;IMEX=1;\"";
            }
            else if (mFileInfo.Extension.Equals(".xls"))
            {
                strCon = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" +mFileInfo.FullName + ";Extended Properties='Excel 8.0;HDR=No;IMEX=1'";
            }
            else
            {
                Debug("Excel表的版本未设定");
                return;
            }
            try
            {
                OleDbConnection mOleDbConnection = new OleDbConnection(strCon);
                OleDbConnection myConn = new OleDbConnection(strCon);
                myConn.Open();

                DataTable schemaTable = myConn.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Tables, null);
                if (schemaTable.Rows.Count > 0)
                {
                    for (int i = 0; i < schemaTable.Rows.Count; i++)
                    {
                        string SheetName = schemaTable.Rows[i][2].ToString().Trim();
                        string strCom = " SELECT * FROM [" + SheetName + "]";
                        OleDbDataAdapter myCommand = new OleDbDataAdapter(strCom, myConn);
                        DataTable ds = new DataTable();
                        myCommand.Fill(ds);
                        string tablename = SheetName.Remove(SheetName.Length - 1) + "DB";
                        ds.TableName = tablename;
                        if (CheckDataTable(ds))
                        {
                            DataTable ds_Client = ds.Copy();
                            DataTable ds_Server = ds.Copy();
                            int j = 0;
                            while (true)
                            {
                                if (ds_Client.Columns.Count == j)
                                {
                                    break;
                                }
                                if (ds_Client.Rows[0][j].ToString().Trim().ToLower() == "server")
                                {
                                    ds_Client.Columns.RemoveAt(j);
                                }
                                else
                                {
                                    j++;
                                }
                            }

                            j = 0;
                            while (true)
                            {
                                if (ds_Server.Columns.Count == j)
                                {
                                    break;
                                }
                                if (ds_Server.Rows[0][j].ToString().Trim().ToLower() == "client")
                                {
                                    ds_Server.Columns.RemoveAt(j);
                                }
                                else
                                {
                                    j++;
                                }
                            }

                            if (ds_Client.Columns.Count > 1)
                            {
                                mDataSetCollecion.Add(ds_Client);
                            }

                            if (ds_Server.Columns.Count > 1)
                            {
                                mServerDataSetCollecion.Add(ds_Server);
                            }
                        }
                    }
                }
                myConn.Close();
            }
            catch (Exception e)
            {
                Debug("Excel:"+mFileInfo.Name+",读取失败：" + e.Message);
            }
        }

        static bool CheckDataTable(DataTable ds_Client)
        {
            if (ds_Client.Rows.Count < 4 || !string.IsNullOrEmpty(ds_Client.Rows[0][0].ToString().Trim()) || ds_Client.Rows[2][0].ToString().Trim() != "int" || ds_Client.Rows[3][0].ToString().Trim() != "id")
            {
                return false;
            }

           foreach(DataTable dt in mDataSetCollecion)
            {
                if(dt.TableName.Trim()==ds_Client.TableName.Trim())
                {
                    Debug("Sheet Name: "+ds_Client.TableName+"  Sheet重名");
                    return false;
                }
            }
            foreach (DataTable dt in mServerDataSetCollecion)
            {
                if (dt.TableName.Trim() == ds_Client.TableName.Trim())
                {
                    Debug("Sheet Name: " + ds_Client.TableName + " Sheet重名");
                    return false;
                }
            }

            int j = 0;
            while (true)
            {
                if (ds_Client.Columns.Count == j)
                {
                    break;
                }
                if (string.IsNullOrEmpty(ds_Client.Rows[3][j].ToString().Trim()) || string.IsNullOrEmpty(ds_Client.Rows[2][j].ToString().Trim()))
                {
                    ds_Client.Columns.RemoveAt(j);
                }
                else
                {
                    j++;
                }
            }

            List<string> mFieldList = new List<string>();
            for (int i = 0; i < ds_Client.Columns.Count; i++)
            {
                string name = ds_Client.Rows[3][i].ToString().Trim();
                if (mFieldList.Contains(name))
                {
                    Debug("Sheet Name: " + ds_Client.TableName + " | " + name + "  字段重名");
                    return false;
                }
                else
                {
                    mFieldList.Add(name);
                }
            }

            j = 4;
            while (true)
            {
                if (ds_Client.Rows.Count == j)
                {
                    break;
                }
                if (string.IsNullOrEmpty(ds_Client.Rows[j][0].ToString().Trim()))
                {
                    ds_Client.Rows.RemoveAt(j);
                }
                else
                {
                    j++;
                }
            }

            return true;
        }
       
//**********************************创建客户端XML*****************************************************************************

        static void CreateXMLDocument(List<DataTable> mCollection)
        {
            Console.WriteLine("Strat Create XML Document....");
            XmlDocument mXml = new XmlDocument();
            //XmlNode mDeclaration= mXml.CreateNode(XmlNodeType.XmlDeclaration,"1.0,utf-8,no",null);
            // mXml.AppendChild(mDeclaration);
            XmlNode mComment = mXml.CreateComment("zhushi");
            mXml.AppendChild(mComment);
            XmlNode root = mXml.CreateNode(XmlNodeType.Element,"Client", null);
            mXml.AppendChild(root);

            foreach (DataTable ds in mCollection)
            {
                XmlNode mXmlNode=CreateXmlByDataSet(mXml,ds);
                root.AppendChild(mXmlNode);
            }
            SaveXmlDocument(mXml);
            Console.WriteLine("Finsh Save XML Document....");
        }
        static XmlNode CreateXmlByDataSet(XmlDocument mXml , DataTable mTable)
        {
            Console.WriteLine("Start Reading Table:"+mTable.TableName+" | "+mTable.Rows.Count+","+mTable.Columns.Count);
            XmlNode root = mXml.CreateNode(XmlNodeType.Element, mTable.TableName+"s",null);
            for (int i = 4; i < mTable.Rows.Count; i++)
            {
                XmlNode mNode= CreateNode(mXml,root, mTable.TableName, "");
                for (int j = 0; j < mTable.Columns.Count; j++)
                {
                    string name = mTable.Rows[2][j].ToString().Trim();
                    if (name.Contains("["))
                    {
                        name = name.Substring(0, name.IndexOf('['));
                        name = "Array_"+name;
                    }
                    CreateNode1(mXml,mNode,name.Trim(), mTable.Rows[3][j].ToString().Trim(),mTable.Rows[i][j].ToString().Trim());                                              
                }
            }
            return root;
        }

        static void SaveXmlDocument(XmlDocument mXml)
        {
            try
            {
                mXml.Save(Path.Combine(ClientOutPath,"DB.xml"));
            }catch(Exception e)
            {
                Debug(e.Message);
            }
        }

        public static XmlNode CreateNode(XmlDocument xmlDoc, XmlNode parentNode, string name, string value)
        {
            XmlNode node = xmlDoc.CreateNode(XmlNodeType.Element, name, null);
            node.InnerText = value;
            parentNode.AppendChild(node);
            return node;
        }
        public static XmlNode CreateNode1(XmlDocument xmlDoc, XmlNode parentNode, string name,string key, string value)
        {
            XmlElement node = xmlDoc.CreateElement(name);
            node.SetAttribute(key, value);
            parentNode.AppendChild(node);
            return node;
        }

        //**********************************创建cs文件*****************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mCollection"></param>
        static void CreateCSFile(List<DataTable> mCollection)
        {
            Console.WriteLine("Strat Create CS File....");
            string mStr = "";
            mStr += "using System.Collections;\n";
            mStr += "using System.Collections.Generic;\n";
            mStr += "namespace xk_System.Db\n{\n";
            foreach (DataTable mDataTable in mCollection)
            {
                if (mDataTable.Rows.Count > 4)
                {
                    mStr += "\tpublic class " + mDataTable.TableName.Trim() + ":DbBase\n\t{\n";
                    //字段
                    for (int j = 1; j < mDataTable.Columns.Count; j++)
                    {
                        mStr += "\t\t/// <summary>\n\t\t/// "+mDataTable.Rows[1][j].ToString().Trim()+"\n\t\t/// </summary>\n";
                        string name = mDataTable.Rows[2][j].ToString().Trim();
                        if (name.Contains("["))
                        {
                            /* string length = name.Substring(name.IndexOf('[') + 1, name.IndexOf(']')-name.IndexOf('[')-1);
                             name = name.Substring(0, name.IndexOf('['));
                             mStr += "\t\tpublic readonly " + name + "[] " + mDataTable.Rows[3][j].ToString()+"=new "+name+"[" + length+"];\n";*/
                            string fieldType = name.Substring(0, name.IndexOf('['));
                            mStr += "\t\tpublic readonly " +"List<"+fieldType+"> " + mDataTable.Rows[3][j].ToString() + "=new " + "List<" +fieldType+ ">();\n"; 
                        }
                        else
                        {
                            mStr += "\t\tpublic readonly " + name + " " + mDataTable.Rows[3][j].ToString() + ";\n";
                        }                   
                    }
                   /* //构造函数1
                    mStr += "\n\t\tpublic "+ds.DataSetName.Trim()+"(";
                    for (int j = 0; j < mDataTable.Columns.Count; j++)
                    {
                        mStr +=mDataTable.Rows[2][j].ToString().Trim()+" "+ mDataTable.Rows[3][j].ToString().Trim(); 
                        if(j<mDataTable.Columns.Count-1)
                        {
                            mStr += ",";
                        }             
                    }
                    mStr += ")\n\t\t{\n";
                    for (int j = 0; j < mDataTable.Columns.Count; j++)
                    {
                        mStr += "\t\t\tthis."+mDataTable.Rows[3][j].ToString().Trim() + "=" + mDataTable.Rows[3][j].ToString().Trim()+";\n";
                    }
                    mStr += "\t\t}\n";
                    //构造函数2
                    mStr += "\t\tpublic "+ds.DataSetName+"(ArrayList list)\n\t\t{\n" +
                        "\t\t\tFieldInfo[] mFieldInfo = this.GetType().GetFields();\n" +
                        "\t\t\tfor (int i = 0; i < mFieldInfo.Length; i++)\n\t\t\t{\n" +
                         "\t\t\t\tmFieldInfo[i].SetValue(this, list[i]);\n" +
                       "\t\t\t}\n\t\t}\n";
                       */
                    mStr += "\t}\n\n";
                }
            }
            mStr += "}";


            SaveCSFile(mStr);

            Console.WriteLine("Finsh Save CS File....");
        }

        static void SaveCSFile(string classStr)
        {
            FileInfo mFileInfo = new FileInfo(Path.Combine(ClientOutPath,"DB.cs"));
            mFileInfo.Delete();
            FileStream mFileStream= mFileInfo.OpenWrite();
            byte[] mbyteStr = Encoding.UTF8.GetBytes(classStr);
            mFileStream.Write(mbyteStr,0,mbyteStr.Length);
            mFileStream.Close();
        }

        //*****************************************创建服务器XML**********************************************************************
        static void CreateServerXMLDocument(List<DataTable> mCollection)
        {
            Console.WriteLine("Strat Create Server XML Document....");
            XmlDocument mXml = new XmlDocument();
            //XmlNode mDeclaration= mXml.CreateNode(XmlNodeType.XmlDeclaration,"1.0,utf-8,no",null);
            // mXml.AppendChild(mDeclaration);
            XmlNode mComment = mXml.CreateComment("zhushi");
            mXml.AppendChild(mComment);
            XmlNode root = mXml.CreateNode(XmlNodeType.Element, "Server", null);
            mXml.AppendChild(root);

            foreach (DataTable ds in mCollection)
            {
                XmlNode mXmlNode = CreateServerXmlByDataTable(mXml, ds);
                root.AppendChild(mXmlNode);
            }
            SaveServerXmlDocument(mXml);
            Console.WriteLine("Finsh Save Server XML Document....");
        }
        static XmlNode CreateServerXmlByDataTable(XmlDocument mXml, DataTable mTable)
        {
            XmlNode root = mXml.CreateNode(XmlNodeType.Element, mTable.TableName + "s", null);
            for (int i = 1; i < mTable.Rows.Count; i++)
            {
                XmlNode mNode = CreateNode(mXml, root, mTable.TableName, "");
                for (int j = 0; j < mTable.Columns.Count; j++)
                {
                    if (i == 1)
                    {
                        CreateNode(mXml, mNode, "comment", mTable.Rows[i][j].ToString().Trim());
                    }else if(i==2)
                    {
                        string name = mTable.Rows[i][j].ToString().Trim();
                        CreateNode(mXml, mNode, "fieldtype", name);
                    }
                    else if(i==3)
                    {
                        CreateNode(mXml, mNode, "fieldname", mTable.Rows[i][j].ToString().Trim());
                    }
                    else
                    {
                        CreateNode(mXml, mNode, mTable.Rows[3][j].ToString().Trim(), mTable.Rows[i][j].ToString().Trim());
                    }
                }
            }
            return root;
        }

        static void SaveServerXmlDocument(XmlDocument mXml)
        {
            try
            {
                mXml.Save(Path.Combine(ServerOutPath, "DB_Server.xml"));
            }
            catch (Exception e)
            {
                Debug(e.Message);
            }
        }









        //***************************************************************************************************************
        static void Debug(object str)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(str);
            Console.ResetColor();
        }
    }
}
