// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;

namespace TouchSocket.Core;

public static class AOT
{
    public const DynamicallyAccessedMemberTypes SerializerFormatterMemberType = DynamicallyAccessedMemberTypes.All;


    public const DynamicallyAccessedMemberTypes Container = DynamicallyAccessedMemberTypes.PublicConstructors;

    public const DynamicallyAccessedMemberTypes PluginMemberType = DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.Interfaces | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.NonPublicMethods;

    public const DynamicallyAccessedMemberTypes RpcInvoke = DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties;

    public const DynamicallyAccessedMemberTypes MemberAccessor = DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties;

    public const DynamicallyAccessedMemberTypes FastBinaryFormatter = DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties;

    public const DynamicallyAccessedMemberTypes RpcRegister = DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces | DynamicallyAccessedMemberTypes.NonPublicMethods;
}
