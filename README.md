# ImageDemo
.net core api 上传与加载图片 文件夹操作 swagger
 # 关于.net core API 图片上传与加载 文件夹

 [TOC]

#### 1.上传图片
#####   1.配置Swagger 与文件夹、接口添加
 
    1.建立.net core api 项目 下载 NuGet 包 Swashbuckle.AspNetCore 
    2.进入 starup 中 ConfigureServices 配置 ，添加如下代码
    3.点击项目右键属性->生成->XML 文档文件 ✔ 上，接着在取消显示警告添加1591->保存
    

    
 
 ```
             services.AddSwaggerGen(s =>

            {

                s.SwaggerDoc("v1", new Info

                {

                    Title = "图片上传",

                    Description = "图片上传测试",

                    Version = "v1"

                });



                #region XML备注



                var basePath = Path.GetDirectoryName(AppContext.BaseDirectory);

                var imagePath = Path.Combine(basePath, "ImageDemo.xml");

                s.IncludeXmlComments(imagePath,true);



                #endregion

            });

 ```

    4.进入 starup 中 Configure 配置 ，添加如下代码

```
app.UseSwagger();
app.UseSwaggerUI(s => s.SwaggerEndpoint("/swagger/v1/swagger.json", "v1版本"));
```

    5.点击 Properties 编辑为下面

```
{
  /*"$schema": "http://json.schemastore.org/launchsettings.json",
  "iisSettings": {
    "windowsAuthentication": false, 
    "anonymousAuthentication": true, 
    "iisExpress": {
      "applicationUrl": "http://localhost:59623",
      "sslPort": 44385
    }
  },*/
  "profiles": {/*
    "IIS Express": {
      "commandName": "IISExpress",
      "launchBrowser": true,
      "launchUrl": "api/values",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },*/
    "ImageDemo": {
      "commandName": "Project",
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "https://localhost:5001;http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```
    6.运行项目，便可以看到结果，以上 Swagger 配置完毕

 
#####   2.编写接口
1. 添加三个文件夹
    * **Images** 存储图片 
        * 里面在设置一个名称为 **6** 的文件夹
    * **IRepositories** 接口文件夹 
        * 新建接口类 **IImagesResource**
    * **Repositories** 实现类文件夹 
        * 新建实现类 **ImagesResource**

2. 实现类代码
```
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ImageDemo.IRepositories
{
    public interface IImagesResource
    {
        /// <summary>
        /// 加载图片
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="name">图片名</param>
        /// <returns></returns>
        FileContentResult LoadingPhoto(string path, string name);

        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="formFile">图片</param>
        /// <param name="path">路径</param>
        /// <param name="name">图片名字</param>
        /// <returns></returns>
        CustomStatusCode UpLoadPhoto(IFormFile formFile, string path);
    }
}

```



####   2.实现图片上传
_先上代码_
**实现类总代码**
```
using System;
using System.IO;
using System.Linq;
using ImageDemo.IRepositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ImageDemo.Repositories
{
    public class ImagesResource:ControllerBase,IImagesResource
    {
        public static string[] LimitPictureType = {".PNG", ".JPG", ".JPEG", ".BMP", ".ICO"};

        /// <summary>
        /// 加载图片
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public FileContentResult LoadingPhoto(string path, string name)
        {
            path = Directory.GetCurrentDirectory() + path + name + ".jpeg";
            FileInfo fi=new FileInfo(path);
            if (!fi.Exists)
            {
                return null;
            }

            FileStream fs = fi.OpenRead();
            byte[] buffer=new byte[fi.Length];
            //读取图片字节流
            //从流中读取一个字节块，并在给定的缓冲区中写入数据。
            fs.Read(buffer, 0, Convert.ToInt32(fi.Length));
            var resource = File(buffer, "image/jpeg");
            fs.Close();
            return resource;
        }

        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="formFile"></param>
        /// <param name="path">路劲</param>
        /// <returns></returns>
        public CustomStatusCode UpLoadPhoto(IFormFile formFile, string path)
        {
            CustomStatusCode code;
            var currentPictureWithoutExtension = Path.GetFileNameWithoutExtension(formFile.FileName);
            var currentPictureExtension = Path.GetExtension(formFile.FileName).ToUpper();
            path = Directory.GetCurrentDirectory() + path;
            if (LimitPictureType.Contains(currentPictureExtension))
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string name = currentPictureWithoutExtension + ".jpeg";
                path += name;
                using (var fs=System.IO.File.Create(path))
                {
                    formFile.CopyTo(fs);
                    //Stream 都有 Flush() 方法，
                    //根据官方文档的说法
                    //“使用此方法将所有信息从基础缓冲区移动到其目标或清除缓冲区，或者同时执行这两种操作”
                    fs.Flush();
                }
                code = new CustomStatusCode
                {
                    Status = "200",
                    Message = $"图片 {name} 上传成功"
                };
                return code;
            }
            code = new CustomStatusCode
            {
                Status = "400",
                Message = $"图片上传失败，格式错误"
            };
            return code;
        }
    }
}

```

>**上面有个重点是实现类还继承了 ControllerBase，并且继承位置要在IImagesResource之前**
其中 **CustomStatusCode** 类是信息返回类下面贴下
```
namespace ImageDemo
{
    public class CustomStatusCode
    {
        public object Status;
        public object Message { get; set; }
        public object Data { get; set; }
    }
}

```


_取出里面的上传图片代码_
```
        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="formFile">图片</param>
        /// <param name="path">路劲</param>
        /// <returns></returns>
        public CustomStatusCode UpLoadPhoto(IFormFile formFile, string path)
        {
            CustomStatusCode code;
            var currentPictureWithoutExtension = Path.GetFileNameWithoutExtension(formFile.FileName);
            var currentPictureExtension = Path.GetExtension(formFile.FileName).ToUpper();
            path = Directory.GetCurrentDirectory() + path;
            if (LimitPictureType.Contains(currentPictureExtension))
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string name = currentPictureWithoutExtension + ".jpeg";
                path += name;
                using (var fs=System.IO.File.Create(path))
                {
                    formFile.CopyTo(fs);
                    //Stream 都有 Flush() 方法，
                    //根据官方文档的说法
                    //“使用此方法将所有信息从基础缓冲区移动到其目标或清除缓冲区，或者同时执行这两种操作”
                    fs.Flush();
                }
                code = new CustomStatusCode
                {
                    Status = "200",
                    Message = $"图片 {name} 上传成功"
                };
                return code;
            }
            code = new CustomStatusCode
            {
                Status = "400",
                Message = $"图片上传失败，格式错误"
            };
            return code;
        }
```
>解释阶段
1. IFormFile 
    * 是上传的文件
2. var currentPictureWithoutExtension = Path.GetFileNameWithoutExtension(formFile.FileName);
    * 获取没有后缀扩展名的文件名，如传过来的是 **image.png**,经过上面 **currentPictureWithoutExtension** = **image**
3. var currentPictureExtension = Path.GetExtension(formFile.FileName).ToUpper();
    * 得到 **formFile** 的扩展名并将其**大**写
4. path = Directory.GetCurrentDirectory() + path;
    * Directory.GetCurrentDirectory()得到当前程序的路劲，也就是和 **Starup.cs** 文件同等级的存在
5. string name = currentPictureWithoutExtension + ".jpeg";
    * 这段代码是为了保存图片将后缀名统一，因为提取图片需要后缀名，以后提取图片方便点，_这是个很次的写法，当时没想到其他的方法_，这个方法不怎么可取后面我会改，先留个坑
6. fs.Flush();
    * 这个解释是看网上的说明
    * >Stream 都有 Flush() 方法，根据官方文档的说法“使用此方法将所有信息从基础缓冲区移动到其目标或清除缓冲区，或者同时执行这两种操作”
7. 

 
#### 3.加载图片
 _贴代码_
 ```
         /// <summary>
        /// 加载图片
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public FileContentResult LoadingPhoto(string path, string name)
        {
            path = Directory.GetCurrentDirectory() + path + name + ".jpeg";
            FileInfo fi=new FileInfo(path);
            if (!fi.Exists)
            {
                return null;
            }

            FileStream fs = fi.OpenRead();
            byte[] buffer=new byte[fi.Length];
            //读取图片字节流
            //从流中读取一个字节块，并在给定的缓冲区中写入数据。
            fs.Read(buffer, 0, Convert.ToInt32(fi.Length));
            var resource = File(buffer, "image/jpeg");
            fs.Close();
            return resource;
        }
 ```
 1. **File(buffer, "image/jpeg")** 
    * 这个方法继承自 **ControllerBase** 并不是来自 using System.IO 中
    * 至于怎样用 using System.IO; 把图片取出来...我还没学，望大佬教教

#### 4.Controller 控制器
_贴代码_
```
```
using ImageDemo.Db;
using ImageDemo.IRepositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;

namespace ImageDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        #region Inistial

        private readonly DbContext _dbContext;
        private readonly IImagesResource _imagesResource;

        private readonly ILogger<ValuesController> _logger;

        public ValuesController(
            ILogger<ValuesController> logger,
            DbContext dbContext,
            IImagesResource imagesResource)
        {
            _logger = logger;
            _dbContext = dbContext;
            _imagesResource = imagesResource;
        }

        #endregion

        /// <summary>
        /// 批量上传图片
        /// </summary>
        /// <param name="file"></param>
        /// <param name="formCollection"></param>
        [HttpPost]
        public IActionResult Post([FromForm] IFormFileCollection formCollection)
        {
            IList<CustomStatusCode> code=new List<CustomStatusCode>();
                if (formCollection.Count > 0)
                foreach (IFormFile file in formCollection)
                {
                    var currentCode=_imagesResource.UpLoadPhoto(file, @"\images\6\");
                    code.Add(currentCode);
                }

                return StatusCode(200,code);
        }

        /// <summary>
        /// 获取图片
        /// </summary>
        /// <param name="imgName"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get(string imgName)
        {
            var image = _imagesResource.LoadingPhoto("\\Images\\6\\", imgName);
            if (image == null)
            {
                _logger.LogInformation($"图片 {imgName} 不存在");
                var code = new CustomStatusCode
                {
                    Status = "404",
                    Message = $"图片 {imgName} 加载不存在"
                };
                return StatusCode(404, code);
            }

            return image;

            #region MyRegion

            /*string[] LimitPictureType =
                {".PNG", ".JPG", ".JPEG", ".BMP", ".GIF", ".ICO"};
            GetFileName(di);
            

            

            
            string path = Directory.GetCurrentDirectory()+ $@"\Images\6\{imgName}"+".jpeg";

            FileInfo fi = new FileInfo(path);
            FileStream fs = fi.OpenRead(); ;
            byte[] buffer = new byte[fi.Length];
            //读取图片字节流
            fs.Read(buffer, 0, Convert.ToInt32(fi.Length));
            var response = File(buffer, "image/jpeg");
            fs.Close();
            return response;
            */

            #endregion
        }

        private static IList<string> path = new List<string>(); //保存你图片名称
        DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory()+@"\Images\6\");

        /// <summary>
        /// 加载目录内文件夹名
        /// </summary>
        /// <param name="info"></param>
        public static void GetFileName(DirectoryInfo info)
        {
            //获取该路径下的所有文件的列表
            FileInfo[] fileInfo = info.GetFiles();

            //开始得到图片名称
            foreach (FileInfo subinfo in fileInfo)
            {
                //判断扩展名是否相同
                // if (subinfo.Extension == extension)
                // {
                string strname = subinfo.Name; //获取文件名称

                path.Add(strname); //把文件名称保存在泛型集合中
                // }
            }
        }
    }
}

---




#### 5.想法，问题与 [GitHub](https://github.com/1045683477)

>问题：
>1. 上传图片可以多张上传，加载图片的方法本来也想多张加载，当时多张的话，图片就会变成字符串的形式，不知为何，求解
>2. 如果不用 **ControllerBse** 中的 **File** 那么该如何向页面传递图片

>有两个想法，朋友提的：
>1. 那个加载图片直接建立一个html网页，将图片放进去，前端这样可以访问图片，应该可行，不知效率怎样
>2. 那个改后缀的实属下策，可以用建立数据库，两列，一列存储图片名，一列存储图片后缀，存储的时候可以将图片的名字改下，如果是用户头像，可以改为用户 **Id** 这样数字就唯一了
