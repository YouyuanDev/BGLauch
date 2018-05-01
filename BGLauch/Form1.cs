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

namespace BGLauch
{
    public partial class Form1 : Form
    {
        private string processPath = null;
        private string newVersion = "";
        public Form1()
        {
            InitializeComponent();

            try
            {
                AutoUpdater.ApplicationExitEvent += AutoUpdater_ApplicationExitEvent;
                string url = "http://192.168.0.200:8080/upload/clientapp/ClientAPPAutoUpdater.xml";
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
                            string processPath = Application.StartupPath + "\\bin\\YYOPInspectionClient.exe";
                            AutoUpdater.Start("http://192.168.0.200:8080/upload/clientapp/ClientAPPAutoUpdater.xml");
                            //找到更新的文件的内容
                            //Application.Exit();
                            //AutoUpdater.DownloadUpdate();
                        }
                        else
                        {
                            //MessageBox.Show("333");
                            //执行B程序
                            ExecuteBProgram();
                            //Application.Exit();
                        }
                    }
                }
            }
            catch (Exception e) {
                Console.WriteLine("检测更新时失败......");
            }
        }

        private void AutoUpdater_ApplicationExitEvent()
        {
           
            string versionPath = Application.StartupPath + "\\version.txt";
            if (!string.IsNullOrWhiteSpace(newVersion)) {
                File.WriteAllText(versionPath,newVersion);
                MessageBox.Show("更新完毕,请重启!");
                System.Environment.Exit(0);
                //ExecuteBProgram();
            }
        }
        
        public static  void ExecuteBProgram()
        {
            string processPath = Application.StartupPath + "\\bin\\YYOPInspectionClient.exe";
            //MessageBox.Show(processPath);
            if (File.Exists(processPath)) {
                //MessageBox.Show("有");
                System.Diagnostics.Process.Start(processPath);
                //Thread.Sleep(1000);
                //Application.Exit();
                System.Environment.Exit(0);
            }
           
        }

        public Version AnalysisXml(string xmlPath) {
            List<Version> tmpList = new List<Version>();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);
            XmlNodeList nodelist = xmlDoc.SelectNodes("item");
            foreach (XmlNode node in nodelist)
            {
                Version entity = new Version();
                entity.version =node["version"].InnerText;
                entity.url = node["url"].InnerText;
                entity.changelog = node["changelog"].InnerText;
                entity.mandatory = node["mandatory"].InnerText;
                tmpList.Add(entity);
            }
            if (tmpList != null && tmpList.Count > 0)
            {
                return tmpList[0];
            }
            else {
                return null;
            }
        }
        private string ReadVersion(string path) {
            StreamReader sr = new StreamReader(path);
            String str_read = sr.ReadToEnd();
            sr.Close();
            return str_read;
        }
       
        //private void GetProcessPathOfB(string dir)
        //{
        //    DirectoryInfo d = new DirectoryInfo(dir);
        //    FileSystemInfo[] fsinfos = d.GetFileSystemInfos();
        //    foreach (FileSystemInfo fsinfo in fsinfos)
        //    {
        //        if (fsinfo is DirectoryInfo)     //判断是否为文件夹
        //        {
        //            GetProcessPathOfB(fsinfo.FullName);//递归调用
        //        }
        //        else
        //        {
        //            //fsinfo.Name;
        //            if (fsinfo.Name.Equals("Chat.exe")) {
        //                processPath = fsinfo.FullName;
        //            }
        //            Console.WriteLine(fsinfo.FullName);//输出文件的全部路径
        //        }
        //    }


        //}
    }
}
