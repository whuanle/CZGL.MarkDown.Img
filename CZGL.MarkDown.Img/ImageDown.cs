using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CZGL.MarkDown.Img
{

    /// <summary>
    /// 捕获摄像头实时图像
    /// </summary>
    public class ImageDown
    {
        // 设置 client 
        private readonly HttpClient client;
        private readonly string _ImagePath;

        public ImageDown(string path)
        {
            client = new HttpClient();
            _ImagePath = path;
        }


        public async void GetImgAsync(string cameraUrl, string name)
        {
            try
            {
                byte[] response = await client.GetByteArrayAsync(cameraUrl);

                using (MemoryStream memory = new MemoryStream(response))
                {
                    byte[] image = memory.ToArray();
                    FileStream fs = new FileStream(Path.Combine(_ImagePath, name), FileMode.Create, FileAccess.Write);
                    await fs.WriteAsync(image, 0, image.Length);
                    await fs.FlushAsync();
                    fs.Close();
                }
            }
            catch
            {
                Console.WriteLine("图片下载失败,图片地址：" + cameraUrl);
            }
        }

    }
}
