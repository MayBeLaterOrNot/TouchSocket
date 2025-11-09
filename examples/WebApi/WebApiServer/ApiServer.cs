//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Rpc;
using TouchSocket.Sockets;
using TouchSocket.WebApi;

namespace WebApiServerApp;

[CustomResponse]
public partial class ApiServer : SingletonRpcServer
{
    private readonly ILog m_logger;

    public ApiServer(ILog logger)
    {
        this.m_logger = logger;
    }

    #region 多路由配置

    [Router("[api]/[action]ab")]//此路由会以"/ApiServer/Sumab"实现
    [Router("[api]/[action]")]//此路由会以"/ApiServer/Sum"实现
    [WebApi(Method = HttpMethodType.Get)]
    public int Sum(int a, int b)
    {
        return a + b;
    }

    #endregion 多路由配置

    #region 自定义路由路径

    [Router("/api/custom/calculate")]
    [WebApi(Method = HttpMethodType.Get)]
    public int Calculate(int x, int y)
    {
        return x * y;
    }

    #endregion 自定义路由路径

    #region 类级别路由

    // 注意：此示例展示类级别路由的概念
    // 实际使用时应在类声明上添加 [Router] 特性

    #endregion 类级别路由

    #region 正则路由示例

    [RegexRouter(@"^/products/\d+$")]
    [WebApi(Method = HttpMethodType.Get)]
    public string GetProductById(IWebApiCallContext callContext)
    {
        var url = callContext.HttpContext.Request.RelativeURL;
        var id = url.Split('/').Last();
        return $"Product ID: {id}";
    }

    #endregion 正则路由示例

    #region 使用调用上下文

    [WebApi(Method = HttpMethodType.Get)]
    public int GetCallContext(IWebApiCallContext callContext, int a, int b)
    {
        if (callContext.Caller is IHttpSessionClient httpSessionClient)
        {
            Console.WriteLine($"IP:{httpSessionClient.IP}");
            Console.WriteLine($"Port:{httpSessionClient.Port}");
            Console.WriteLine($"Id:{httpSessionClient.Id}");
        }

        //http内容
        var httpContext = callContext.HttpContext;

        //http请求
        var request = httpContext.Request;
        //http响应
        var response = httpContext.Response;
        return a + b;
    }

    #endregion 使用调用上下文

    #region 返回对象

    [WebApi(Method = HttpMethodType.Get)]
    public MyClass GetMyClass()
    {
        return new MyClass()
        {
            A = 1,
            B = 2
        };
    }

    #endregion 返回对象

    #region Post传参

    [WebApi(Method = HttpMethodType.Post)]
    public int TestPost(MyClass myClass)
    {
        return myClass.A + myClass.B;
    }

    #endregion Post传参

    #region FromQuery传参

    [WebApi(Method = HttpMethodType.Get)]
    public int SumFromQuery([FromQuery(Name = "aa")] int a, [FromQuery] int b)
    {
        return a + b;
    }

    #endregion FromQuery传参

    #region FromForm传参

    [WebApi(Method = HttpMethodType.Get)]
    public int SumFromForm([FromForm] int a, [FromForm] int b)
    {
        return a + b;
    }

    #endregion FromForm传参

    #region FromHeader传参

    [WebApi(Method = HttpMethodType.Get)]
    public int SumFromHeader([FromHeader] int a, [FromHeader] int b)
    {
        return a + b;
    }

    #endregion FromHeader传参

    #region 启用跨域

    [EnableCors("cors")]//使用跨域
    [WebApi(Method = HttpMethodType.Get)]
    public int SumCallContext(IWebApiCallContext callContext, int a, int b)
    {
        return a + b;
    }

    #endregion 启用跨域

    #region 文件下载

    /// <summary>
    /// 使用调用上下文,响应文件下载。
    /// </summary>
    /// <param name="callContext"></param>
    [WebApi(Method = HttpMethodType.Get)]
    public async Task<string> DownloadFile(IWebApiCallContext callContext, string id)
    {
        if (id == "rrqm")
        {
            await callContext.HttpContext.Response.FromFileAsync(new FileInfo(@"D:\System\Windows.iso"), callContext.HttpContext.Request);
            return "ok";
        }
        return "id不正确。";
    }

    #endregion 文件下载

    #region 获取请求体

    /// <summary>
    /// 使用调用上下文，获取实际请求体。
    /// </summary>
    /// <param name="callContext"></param>
    [WebApi(Method = HttpMethodType.Post)]
    [Router("[api]/[action]")]
    public async Task<string> PostContent(IWebApiCallContext callContext)
    {
        if (callContext.Caller is IHttpSessionClient socketClient)
        {
            this.m_logger.Info($"IP:{socketClient.IP},Port:{socketClient.Port}");//获取Ip和端口
        }

        var content = await callContext.HttpContext.Request.GetContentAsync();
        this.m_logger.Info($"共计：{content.Length}");

        return "ok";
    }

    #endregion 获取请求体

    #region 上传多个小文件

    /// <summary>
    /// 使用调用上下文,上传多个小文件。
    /// </summary>
    /// <param name="callContext"></param>
    [WebApi(Method = HttpMethodType.Post)]
    public async Task<string> UploadMultiFile(IWebApiCallContext callContext, string id)
    {
        var formFiles = await callContext.HttpContext.Request.GetFormCollectionAsync();
        if (formFiles != null)
        {
            foreach (var item in formFiles.Files)
            {
                Console.WriteLine($"fileName={item.FileName},name={item.Name}");

                //写入实际数据
                File.WriteAllBytes(item.FileName, item.Data.ToArray());
            }
        }
        return "ok";
    }

    #endregion 上传多个小文件

    #region 上传大文件

    /// <summary>
    /// 使用调用上下文，上传大文件。
    /// </summary>
    /// <param name="callContext"></param>
    [WebApi(Method = HttpMethodType.Post)]
    public async Task<string> UploadBigFile(IWebApiCallContext callContext)
    {
        using (var stream = File.Create("text.file"))
        {
            await callContext.HttpContext.Request.ReadCopyToAsync(stream);
        }
        Console.WriteLine("ok");
        return "ok";
    }

    #endregion 上传大文件
}