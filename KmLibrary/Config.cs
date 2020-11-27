using System.Runtime.InteropServices;
using System.Text;

namespace KmLibrary
{
    /// <summary>
    /// 读写ini文件
    /// </summary>
    public class IniHelper
    {
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        /// <summary>
        /// 读INI文件
        /// </summary>
        /// <param name="FilePath">文件路径</param>
        /// <param name="Section">名称</param>
        /// <param name="Key">键</param>
        /// <returns></returns>
        public static string IniReadValue(string FilePath, string Section, string Key)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, "", temp, 255, FilePath);
            return temp.ToString();
        }

        /// <summary>
        /// 写INI文件
        /// </summary>
        /// <param name="FilePath">文件路径</param>
        /// <param name="Section">名称</param>
        /// <param name="Key">键</param>
        /// <param name="Value">值</param>
        public static void IniWriteValue(string FilePath, string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, FilePath);
        }
    }
}
