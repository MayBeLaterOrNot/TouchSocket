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

    [EnableCors("cors")]//使用跨域
    [Router("[api]/[action]ab")]//此路由会以"/ApiServer/Sumab"实现
    [Router("[api]/[action]")]//此路由会以"/ApiServer/Sum"实现
    [WebApi(Method = HttpMethodType.Get)]
    public int Sum(int a, int b)
    {
        return a + b;
    }

    [WebApi(Method = HttpMethodType.Get)]
    public int SumCallContext(IWebApiCallContext callContext, int a, int b)
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

    [WebApi(Method = HttpMethodType.Get)]
    public MyClass GetMyClass()
    {
        return new MyClass()
        {
            A = 1,
            B = 2
        };
    }

    [WebApi(Method = HttpMethodType.Post)]
    public int TestPost(MyClass myClass)
    {
        return myClass.A + myClass.B;
    }

    /// <summary>
    /// 使用调用上下文，响应文件下载。
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

    /// <summary>
    /// 使用调用上下文，上传多个小文件。
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

    [WebApi(Method = HttpMethodType.Get)]
    public string GetString()
    {
        Console.WriteLine("GetString");
        return "hello";
    }

    [WebApi(Method = HttpMethodType.Get)]
    public int SumFromForm([FromForm] int a, [FromForm] int b)
    {
        return a + b;
    }

    [WebApi(Method = HttpMethodType.Get)]
    public int SumFromQuery([FromQuery(Name = "aa")] int a, [FromQuery] int b)
    {
        return a + b;
    }

    [WebApi(Method = HttpMethodType.Get)]
    public int SumFromHeader([FromHeader] int a, [FromHeader] int b)
    {
        return a + b;
    }
}


