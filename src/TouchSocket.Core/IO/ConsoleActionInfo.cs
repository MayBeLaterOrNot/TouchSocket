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

namespace TouchSocket.Core;

/// <summary>
/// 控制台行为
/// </summary>

public readonly struct ConsoleActionInfo
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="action"></param>
    /// <param name="description"></param>
    /// <param name="fullOrder"></param>
    public ConsoleActionInfo(string description, string fullOrder, Func<Task> action)
    {
        this.FullOrder = fullOrder;
        this.Action = action ?? throw new ArgumentNullException(nameof(action));
        this.Description = description ?? throw new ArgumentNullException(nameof(description));
    }

    public Func<Task> Action { get; }

    public string Description { get; }
    public string FullOrder { get; }
}
