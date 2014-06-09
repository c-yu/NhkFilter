/*
 * created by c.yu
 * 前置:
 * ①安装.net版本的mysql connector
 * ②添加Mysql的引用
 * 
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Net;
using System.IO;
using System.Xml;

namespace NhkFilter
{
    struct DataItem
    {
        public string title;
        public string url;
    }

    class Program
    {
        static void Main(string[] args)
        {
            //ReadDatabase();
            string content = DownLoadRss();
            List<DataItem> items = ProcessRssContent(content);
            SaveToDatabase(items);
            Console.Read();
        }

        static string DownLoadRss()
        {
            string content = null;
            string url = "http://www.nhk.or.jp/r-news/podcast/nhkradionews.xml";
            try
            {
                WebRequest request = WebRequest.Create(url);
                WebResponse response = request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                content = reader.ReadToEnd();
            }
            catch (Exception e)
            {
                Console.WriteLine("下载rss内容出错:"+e.Message);
            }
            Console.WriteLine("Rss下载完成");
            return content;
        }

        static List<DataItem> ProcessRssContent(string content)
        {
            List<DataItem> itemList = new List<DataItem>();
            XmlDocument xd = new XmlDocument();
            try
            {
                xd.LoadXml(content);
                XmlNodeList itemNodes = xd.SelectNodes("/rss/channel/item");
                foreach (XmlNode itemNode in itemNodes)
                {
                    XmlNode titleNode = itemNode.SelectSingleNode("title");
                    XmlNode enclosureNode = itemNode.SelectSingleNode("enclosure");
                    DataItem item;// = new DataItem();
                    item.title = titleNode.InnerText;
                    item.url = enclosureNode.Attributes["url"].Value;
                    itemList.Add(item);
                }
            }
            catch (Exception e)
            {
                Console.Write("处理rss内容出错:"+e.Message);
            }
            Console.WriteLine("Rss分析完成");
            return itemList;
        }

        static void SaveToDatabase(List<DataItem> items)
        {
            string connStr = "server=localhost;uid=root;pwd=passpass;database=nhk;charset=utf8";
            string sql = null;
            try
            {
                MySqlConnection conn = new MySqlConnection(connStr);
                conn.Open();
                foreach (DataItem item in items)
                {
                    sql = "insert into radionews(title,url) values(\"" + item.title + "\",\"" + item.url + "\")";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                    Console.WriteLine(item.title);
                    Console.WriteLine(item.url);
                }
                conn.Close();
            }
            catch (Exception e) {
                Console.WriteLine("保存数据出错:"+sql);
            }
            Console.WriteLine("保存数据库完成");

        }

        static void ReadDatabase()
        {
            string connStr = "server=localhost;uid=root;pwd=passpass;database=nhk";
            try
            {
                MySqlConnection conn = new MySqlConnection(connStr);
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("select * from radionews", conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read()) {
                    Console.WriteLine(reader["title"]);
                    Console.WriteLine(reader["url"]);
                }
                conn.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.WriteLine("读取数据库完成");

        }

        
    }
}
