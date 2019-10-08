using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;


namespace CZGL.MarkDown.Img
{
    class Program
    {
        static string dirName = "";
        static bool isDown = false;
        static Exclude exclude = new Exclude();

        static void Main(string[] args)
        {
            try
            {
                exclude = JsonConvert.DeserializeObject<Exclude>(File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "SetRe.json")));
            }
            catch
            {
                exclude.Dir_Exclude = new string[] { ".git", "image", "images" };
                exclude.File_Exclude = new string[] { "toc.yml", "intro.md" };
            }
            Console.WriteLine("快速迁移Markdown文档的远程图片到本地");
            Console.WriteLine("输入Markdown文件目录地址");
            string dirpath = Console.ReadLine();

            Console.WriteLine("输入存储图片的目录名称");
            dirName = Console.ReadLine();

            Console.WriteLine("是否需要下载图片(y/n)");
            while (true)
            {
                string vc = Console.ReadLine();
                if (vc.ToLower() == "y")
                {
                    isDown = true;
                    break;
                }
                else if (vc.ToLower() == "n")
                {
                    break;
                }
                Console.WriteLine("输入无效，请重新输入");
            }

            Traverse(dirpath);


            Console.WriteLine("处理完成");
            Console.ReadKey();
        }
        // 遍历
        public static void Traverse(string dirpath)
        {
            string[] dirs = Directory.GetDirectories(dirpath);
            foreach (var item in exclude.Dir_Exclude)
            {
                dirs = dirs.Where(x => x.ToLower() != (Path.Combine(dirpath, item)).ToLower()).ToArray();
            }
            dirs = dirs.Where(x => x.ToLower() != dirName.ToLower()).ToArray();

            if (dirs.Length != 0)
            {
                foreach (var item in dirs)
                {
                    Traverse(item);
                }
            }
            Console.WriteLine($"处理目录：" + dirpath);
            DownWork(dirpath);

        }
        public static async void DownWork(string dirpath)
        {
            ImageDown down = new ImageDown(dirpath);
            string[] files = Directory.GetFiles(dirpath);
            foreach (var item in exclude.File_Exclude)
            {
                files = files.Where(x => x.ToLower() != (Path.Combine(dirpath, item)).ToLower()).ToArray();
            }
            if (files.Length != 0)
            {
                if (!Directory.Exists(Path.Combine(dirpath, dirName)))
                {
                    Directory.CreateDirectory(Path.Combine(dirpath, dirName));
                }
            }
            foreach (var item in files)
            {
                if (isDown)
                    await ReadMd(item);
                else
                    await ReadMd(item, down);
            }

        }
        public static async Task ReadMd(string mdName, ImageDown down = null)
        {
            Console.WriteLine($"处理文件：{mdName}");

            StreamReader streamReader = File.OpenText(mdName);

            string lines = streamReader.ReadToEnd();
            string lines2 = lines;
            streamReader.Close();
            streamReader.Dispose();

            if (lines.Length < 10) return;

            bool yes = false;
            bool yesL = false;
            int start = 0;
            int lineStart = 0;
            int end = 0;

            for (int i = 0; i < lines.Length - 1; i++)
            {
                if (lines[i] == '!' && lines[i + 1] == '[')
                {
                    start = i;
                    yes = true;
                }
                if (yes == true && start != 0 && lines[i] == '(')
                {
                    lineStart = i;
                    yesL = true;
                }
                if (yes == true && yesL == true && lines[i] == ')')
                {
                    end = i;
                    yes = false;
                    yesL = false;
                    string url = lines.Substring(lineStart + 1, end - lineStart - 1);
                    Console.WriteLine("处理图片地址：" + url);
                    // 图片名称
                    string imageName = url.Substring(url.LastIndexOf("/") + 1);

                    string filePath = Path.Combine(Path.Combine(Path.GetDirectoryName(mdName), dirName), imageName);
                    if (isDown == true)
                        down.GetImgAsync(url, filePath);
                    string rpStr = "./" + dirName + "/" + imageName;
                    lines2 = lines2.Replace(url, rpStr);

                    start = 0;
                    lineStart = 0;
                    end = 0;
                }
            }

            using (var fileStream = new FileStream(mdName, FileMode.Create))
            {
                await fileStream.WriteAsync(Encoding.Default.GetBytes(lines2));
                fileStream.Flush();
                fileStream.Close();
            }
        }

    }

    public class Exclude
    {
        public string[] Dir_Exclude { get; set; }
        public string[] File_Exclude { get; set; }
    }
}
