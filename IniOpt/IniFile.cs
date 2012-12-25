﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using JingFu.LogMessage;


namespace Lon.IO
{
    public class IniFile
    {
        private string fileName;
        private Dictionary<string, Dictionary<string, string>> iniCahce = new Dictionary<string, Dictionary<string, string>>();

       
        private bool largeCommentFlag = false;

        public IniFile(string fileName)
        {
            
               this.FileName = fileName;         
        }

        public bool ContainSectionName(string secName)
        {
            return this.GetAllSectionName().Contains(secName);
        }

        public ICollection<string> GetAllSectionName()
        {
            return this.iniCahce.Keys;
        }

        public bool GetBool(string section, string key, bool defValue)
        {
            if (this.iniCahce.ContainsKey(section))
            {
                Dictionary<string, string> hash = this.iniCahce[section];
                if (hash.ContainsKey(key))
                {
                    string str = hash[key].ToUpper();
                    if (string.IsNullOrEmpty(str))
                    {
                        return defValue;
                    }
                    return ((((str == "是") || (str == "YES")) || (str == "1")) || (str == "TRUE"));
                }
            }
            return defValue;
        }

        public int GetInt(string section, string key, int defValue)
        {
            if (this.iniCahce.ContainsKey(section))
            {
                Dictionary<string, string> hash = this.iniCahce[section];
                int ret;
                if (hash.ContainsKey(key))
                {
                    string str = hash[key];
                    if (string.IsNullOrEmpty(str))
                    {
                        return defValue;
                    }
                    if(int.TryParse(str,out ret))
                    {
                        return ret;
                    }

                }
            }
            return defValue;
        }

        private static string GetSecString(string str)
        {
            int len = str.Length;
            if (str[0] == '[')
            {
                int pos = str.IndexOf(']');
                if (pos > 0)
                {
                    return str.Substring(1, pos - 1).Trim();
                }
            }
            return "";
        }

        public ICollection<string> GetSectionItemName(string sectName)
        {
            if (this.iniCahce.ContainsKey(sectName))
            {
                Dictionary<string, string> tbl = this.iniCahce[sectName];
                return tbl.Keys;
            }
            return new List<string>();
        }

        public ICollection<string> GetAllValues(string sectName)
        {
            if (this.iniCahce.ContainsKey(sectName))
            {
                Dictionary<string, string> tbl = this.iniCahce[sectName];
                return tbl.Values;
            }
            return new List<String>();
        }
        public string GetString(string section, string key)
        {
            if (this.iniCahce.ContainsKey(section))
            {
                Dictionary<string, string> hash = this.iniCahce[section];
                if (hash.ContainsKey(key))
                {
                    return hash[key];
                }
            }
            return "";
        }

        public bool Load()
        {
            if (!File.Exists(this.FileName))
            {
                File.Create(this.FileName).Close();
                return false;
            }
            StreamReader rd = new StreamReader(File.Open(this.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Encoding.Default);
            if (rd == null)
            {
                return false;
            }
            bool isCurSecComment = false;
            Dictionary<string, string> curSec = null;
            string str = "";
        Label_02A6:
            str = this.ReadLine(rd);
            if (str != null)
            {
                str = str.Trim();
                if (str != "")
                {
                    if (str.StartsWith("/*"))
                    {
                        this.largeCommentFlag = true;
                    }
                    else if (this.largeCommentFlag)
                    {
                        if (str.StartsWith("*/"))
                        {
                            this.largeCommentFlag = false;
                        }
                    }
                    else if ((str.Length < 2) || ((str[0] != ';') || (str[1] != ';')))
                    {
                        string sec = GetSecString(str);
                        if (sec != "")
                        {
                            if (sec.Length >= 2)
                            {
                                if ((sec[0] == ';') && (sec[1] == ';'))
                                {
                                    isCurSecComment = true;
                                    goto Label_02A6;
                                }
                                isCurSecComment = false;
                            }
                            if (this.iniCahce.ContainsKey(sec))
                            {
                               // Output.ShowMessageBox(sec + " 段重复, 请修改配置文件！");
                                goto Label_02AE;
                            }
                            curSec = new Dictionary<string, string>();
                            this.iniCahce.Add(sec, curSec);
                        }
                        else if (!isCurSecComment)
                        {
                            int index = str.IndexOf(";;");
                            if (index >= 0)
                            {
                                str = str.Substring(0, index).Trim();
                            }
                            if (curSec == null)
                            {
                                curSec = new Dictionary<string, string>();
                                this.iniCahce.Add("", curSec);
                            }
                            string[] substr = this.SplitKeyVal(str);
                            if ((substr != null) && (substr.Length >= 2))
                            {
                                substr[0] = substr[0].Trim();
                                substr[1] = substr[1].Trim();
                                if (!curSec.ContainsKey(substr[0]))
                                {
                                    curSec.Add(substr[0], substr[1]);
                                }
                            }
                        }
                    }
                }
                goto Label_02A6;
            }
        Label_02AE:
            rd.Close();
            return true;
        }

        private string ReadLine(StreamReader rd)
        {
            string str = "";
            string s = rd.ReadLine();
            if (s == null)
            {
                return null;
            }
            s = s.Trim();
            if (!(s == ""))
            {
                str = str + s;
            }
            return str;
        }

        public void Save()
        {
            try
            {
                if (!File.Exists(this.FileName))
                {
                    File.Create(this.FileName).Close();
                }
                FileInfo fi = new FileInfo(this.FileName);
                bool isRealOnly = fi.IsReadOnly;
                if (!Directory.Exists(fi.Directory.FullName))
                {
                    Directory.CreateDirectory(fi.Directory.FullName);
                }
                if (isRealOnly && fi.Exists)
                {
                    fi.IsReadOnly = false;
                }
                StreamWriter sw = new StreamWriter(File.Open(this.FileName, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite), Encoding.Default);
                foreach (KeyValuePair<string, Dictionary<string, string>> pair in this.iniCahce)
                {
                    sw.WriteLine("[" + pair.Key + "]");
                    Dictionary<string, string> tbl = pair.Value;
                    foreach (KeyValuePair<string, string> pair2 in tbl)
                    {
                        sw.WriteLine(pair2.Key + "=" + pair2.Value);
                    }
                    sw.WriteLine("");
                }
                sw.Close();
                if (isRealOnly)
                {
                    fi.IsReadOnly = true;
                }
            }
            catch
            {
            }
        }

        private string[] SplitKeyVal(string str)
        {
            int pos = -1;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '=')
                {
                    pos = i;
                    break;
                }
            }
            if ((pos > 0) && (pos < str.Length))
            {
                return new string[] { str.Substring(0, pos), str.Substring(pos + 1) };
            }
            return null;
        }

        public void WriteInt(string section, string key, int val)
        {
            this.WriteString(section, key, val.ToString());
        }

        public void WriteString(string section, string key, string str)
        {
            if (!this.iniCahce.ContainsKey(section))
            {
                this.iniCahce.Add(section, new Dictionary<string, string>());
            }
            if (!string.IsNullOrEmpty(key))
            {
                Dictionary<string, string> secTbl = this.iniCahce[section];
                secTbl[key] = str;
            }
        }

        public string FileName
        {
            get
            {
                return this.fileName;
            }
            set
            {
                this.fileName = value;
            }
        }
    }
}
