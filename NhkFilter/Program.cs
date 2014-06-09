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
        public DateTime puttime;
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
                Console.WriteLine("下载rss内容出错:" + e.Message);
            }
            Console.WriteLine("Rss下载完成");
            return content;
        }

        static List<DataItem> ProcessRssContent(string content)
        {
            DateTime lastTime = GetLasttime();
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
                    XmlNode dateNode = itemNode.SelectSingleNode("pubDate");
                    DataItem item;
                    item.title = titleNode.InnerText;
                    item.url = enclosureNode.Attributes["url"].Value;
                    item.puttime = DateTime.Parse(dateNode.InnerText);
                    if (lastTime.CompareTo(item.puttime) < 0)
                    {
                        itemList.Add(item);
                    }
                }
            }
            catch (Exception e)
            {
                Console.Write("处理rss内容出错:" + e.Message);
            }
            Console.WriteLine("Rss分析完成");
            if (itemList.Count == 0) {
                Console.WriteLine("Rss内容没有更新");
                Console.Read();
                System.Environment.Exit(0);
            }
            return itemList;
        }

        static MySqlConnection GetConnection()
        {
            MySqlConnection conn = null;
            string connStr = "server=localhost;uid=root;pwd=passpass;database=nhk;charset=utf8";
            conn = new MySqlConnection(connStr);
            return conn;
        }

        static DateTime GetLasttime()
        {
            MySqlConnection conn = GetConnection();
            DateTime lasttime = new DateTime();
            try
            {
                conn.Open();
                string sql = "select max(puttime) from radionews";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                lasttime = (DateTime)cmd.ExecuteScalar();
            }
            catch (Exception e)
            {
                Console.WriteLine("获取数据库最后更新时间失败:" + e.Message);
            }
            return lasttime;
        }

        static void SaveToDatabase(List<DataItem> items)
        {
            string sql = null;
            try
            {
                MySqlConnection conn = GetConnection();
                conn.Open();
                foreach (DataItem item in items)
                {
                    sql = "insert into radionews(title,url,puttime) values(\"" + item.title + "\",\"" + item.url + "\",\"" + item.puttime.ToString() + "\")";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                    Console.WriteLine(item.title);
                    Console.WriteLine(item.url);
                    Console.WriteLine(item.puttime);
                }
                conn.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("保存数据出错:" + sql);
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
                while (reader.Read())
                {
                    Console.WriteLine(reader["title"]);
                    Console.WriteLine(reader["url"]);
                    Console.WriteLine(reader["puttime"]);
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
