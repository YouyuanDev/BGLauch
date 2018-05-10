using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoUpdaterDotNET;
using System.Net;
using System.IO;
using System.Xml;
using System.Threading;
using System.Diagnostics;

namespace BGLauch
{
    public partial class Form1 : Form
    {
        private string processPath = null;
        private string newVersion = "";
        #region 构造函数
        public Form1()
        {
            InitializeComponent();
            try
            {
                this.Hide();
                AutoUpdater.ApplicationExitEvent += AutoUpdater_ApplicationExitEvent;
                string ipAndPort = GetServerIPAndPort();
                if (ipAndPort != null)
                {
                    string url = "http://"+ipAndPort.Trim()+"/upload/clientapp/ClientAPPAutoUpdater.xml";
                    string filePath = Application.StartupPath + "\\version.xml";
                    string versionPath = Application.StartupPath + "\\version.txt";
                    HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
                    request.Method = "GET";
                    HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                    // 转换为byte类型
                    System.IO.Stream stream = response.GetResponseStream();
                    //创建本地文件写入流
                    Stream fs = new FileStream(filePath, FileMode.Create);
                    byte[] bArr = new byte[1024];
                    int size = stream.Read(bArr, 0, (int)bArr.Length);
                    while (size > 0)
                    {
                        fs.Write(bArr, 0, size);
                        size = stream.Read(bArr, 0, (int)bArr.Length);
                    }
                    fs.Close();
                    stream.Close();
                    Version entiy = AnalysisXml(filePath);
                    string serverVersion = entiy.version.Trim();
                    newVersion = serverVersion;
                    string nowVersion = ReadVersion(versionPath).Trim();
                    //MessageBox.Show(.Length+":"+nowVersion.Trim().Length);
                    if (entiy != null)
                    {
                        if (!string.IsNullOrWhiteSpace(nowVersion))
                        {

                            if (!serverVersion.Equals(nowVersion))
                            {
                                AutoUpdater.Start("http://"+ipAndPort.Trim()+"/upload/clientapp/ClientAPPAutoUpdater.xml");
                                Application.Exit();
                            }
                            else
                            {
                                //Application.Exit();
                                string sourcePath = Application.StartupPath + "\\bin";
                                string destPath = Application.StartupPath + "\\OPClientBin\\bin";
                                if (Directory.Exists(sourcePath))
                                {
                                    if (Directory.Exists(destPath))
                                    {
                                        MoveFolder(sourcePath, destPath);
                                        Directory.Delete(sourcePath, true);
                                    }
                                    else
                                    {
                                        Directory.CreateDirectory(destPath);
                                    }
                                }
                                ExecuteBProgram();
                            }
                        }
                    }
                }
                else {
                    MessageBox.Show("连接服务器失败,请稍后重启!");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("检测更新时失败......");
            }
        } 
        #endregion

        #region 更新完成事件
        private void AutoUpdater_ApplicationExitEvent()
        {

            string versionPath = Application.StartupPath + "\\version.txt";
            if (!string.IsNullOrWhiteSpace(newVersion))
            {
                File.WriteAllText(versionPath, newVersion);
                MessageBox.Show("更新完毕,请重启!");
                Application.Exit();
                //System.Environment.Exit(0);
                //Application.Exit();
            }
        } 
        #endregion

        #region 执行B程序
        public static void ExecuteBProgram()
        {
            string processPath = Application.StartupPath + "\\OPClientBin\\bin\\YYOPInspectionClient.exe";
            if (File.Exists(processPath))
            {
                System.Diagnostics.Process.Start(processPath);
            }
            System.Environment.Exit(0);
            Application.Exit();
        } 
        #endregion

        #region 解析XML文件
        public Version AnalysisXml(string xmlPath)
        {
            List<Version> tmpList = new List<Version>();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);
            XmlNodeList nodelist = xmlDoc.SelectNodes("item");
            foreach (XmlNode node in nodelist)
            {
                Version entity = new Version();
                entity.version = node["version"].InnerText;
                entity.url = node["url"].InnerText;
                entity.changelog = node["changelog"].InnerText;
                entity.mandatory = node["mandatory"].InnerText;
                tmpList.Add(entity);
            }
            if (tmpList != null && tmpList.Count > 0)
            {
                return tmpList[0];
            }
            else
            {
                return null;
            }
        } 
        #endregion

        #region 读取配置文件版本号
        private string ReadVersion(string path)
        {
            StreamReader sr = new StreamReader(path);
            String str_read = sr.ReadToEnd();
            sr.Close();
            return str_read;
        } 
        #endregion

        #region 移动文件夹到指定位置
        public void MoveFolder(string sourcePath, string destPath)
        {
            if (Directory.Exists(sourcePath))
            {
                if (!Directory.Exists(destPath))
                {
                    //目标目录不存在则创建
                    try
                    {
                        Directory.CreateDirectory(destPath);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("创建目标目录失败：" + ex.Message);
                    }
                }
                //获得源文件下所有文件
                List<string> files = new List<string>(Directory.GetFiles(sourcePath));
                files.ForEach(c =>
                {
                    string destFile = Path.Combine(new string[] { destPath, Path.GetFileName(c) });
                    //覆盖模式
                    if (File.Exists(destFile))
                    {
                        File.Delete(destFile);
                    }
                    File.Move(c, destFile);
                });
                //获得源文件下所有目录文件
                List<string> folders = new List<string>(Directory.GetDirectories(sourcePath));

                folders.ForEach(c =>
                {
                    string destDir = Path.Combine(new string[] { destPath, Path.GetFileName(c) });
                    //Directory.Move必须要在同一个根目录下移动才有效，不能在不同卷中移动。
                    //Directory.Move(c, destDir);

                    //采用递归的方法实现
                    MoveFolder(c, destDir);
                });
            }
            else
            {
                Console.WriteLine("源目录不存在！");
            }
        }
        #endregion

        #region 获取配置文件的IP和Port
        private string GetServerIPAndPort()
        {
            string ipAndPort =null;
            try {
                string configPath = Application.StartupPath + "\\config.txt";
                string str = File.ReadAllText(configPath);
                str = str.Replace("\n", "");
                string[] strIPArray = str.Split('\r');
                if (strIPArray.Length > 0)
                {
                    ipAndPort = strIPArray[0];
                }
            }
            catch (Exception e) {
                Console.WriteLine("获取IP和Port时出错!");
            }
            return ipAndPort;
        } 
        #endregion
    }
}
