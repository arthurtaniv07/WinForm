using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using St.Mail;
using St.IO;

namespace WinForm_Test
{
    public class Pub
    {
        /// <summary>
        /// 获取内置的邮件账户
        /// </summary>
        /// <returns></returns>
        public static St.Mail.StMail GetMailAcc(string displayName)
        {

            string configFile = Environment.CurrentDirectory + "\\config\\config.ini";
            StMail mail = new StMail("1578403183@qq.com", "altntlmjsnikicig", displayName);
            if (System.IO.File.Exists(configFile))
            {
                string[] line = StFile.ReadFileLines(configFile).ToArray();
                if (line[0] == "1")
                {
                    mail = new StMail("1578403183@qq.com", line[1], displayName);
                }
                else if (line[0] == "2")
                {
                    mail = new StMail("q1578403183@163.com", line[2], displayName);
                }
            }
            return mail;
        }

    }
}
