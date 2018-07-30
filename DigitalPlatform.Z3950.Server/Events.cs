using DigitalPlatform.Net;
using System;
using System.Collections.Generic;
using System.Text;
using static DigitalPlatform.Z3950.ZClient;

namespace DigitalPlatform.Z3950.Server
{
    /// <summary>
    /// 初始化阶段登录事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void InitializeLoginEventHandler(object sender,
        InitializeLoginEventArgs e);

    /// <summary>
    /// 连接状态变化事件的参数
    /// </summary>
    public class InitializeLoginEventArgs : EventArgs
    {
        public string GroupID { get; set; }
        public string ID { get; set; }
        public string Password { get; set; }

        // result.Value:
        //      -1  登录出错
        //      0   登录未成功
        //      1   登录成功
        public Result Result = new Result();    // [out]
    }

    /// <summary>
    /// 处理 请求 事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void ProcessRequestEventHandler(object sender,
        ProcessRequestEventArgs e);

    /// <summary>
    /// 处理请求事件的参数
    /// </summary>
    public class ProcessRequestEventArgs : EventArgs
    {
        public BerTree Request { get; set; }

        // 返回空表示需要立即 Close 通道
        public byte[] Response { get; set; }    // [out]
        // result.Value:
        // public Result Result = new Result();    // [out]
    }

    //
    /// <summary>
    /// 设置通道属性 事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void SetChannelPropertyEventHandler(object sender,
        SetChannelPropertyEventArgs e);

    /// <summary>
    /// 设置通道属性事件的参数
    /// </summary>
    public class SetChannelPropertyEventArgs : EventArgs
    {
        public InitRequestInfo Info { get; set; }   // [in]

        public Result Result = new Result();    // [out]
    }
    //

    /// <summary>
    /// 获得 Z39.50 配置 事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void GetZConfigEventHandler(object sender,
        GetZConfigEventArgs e);

    /// <summary>
    /// 获得 Z39.50 配置事件的参数
    /// </summary>
    public class GetZConfigEventArgs : EventArgs
    {
        public InitRequestInfo Info { get; set; }   // [in]

        public ZConfig ZConfig { get; set; }    // [out] 可能会返回 null，表示 ZConfig 没有找到。此时 Result.ErrorInfo 里面有报错信息
        // result.Value:
        public Result Result = new Result();    // [out]
    }

    /// <summary>
    /// 处理 初始化 事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void ProcessInitializeEventHandler(object sender,
        ProcessInitializeEventArgs e);

    /// <summary>
    /// 处理初始化事件的参数
    /// </summary>
    public class ProcessInitializeEventArgs : EventArgs
    {
        public BerTree Request { get; set; }

        // 返回空表示需要立即 Close 通道
        public byte[] Response { get; set; }    // [out]
        // result.Value:
        // public Result Result = new Result();    // [out]
    }

    /// <summary>
    /// 处理 检索中的检索 事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void SearchSearchEventHandler(object sender,
        SearchSearchEventArgs e);

    /// <summary>
    /// 处理检索中的检索事件的参数
    /// </summary>
    public class SearchSearchEventArgs : EventArgs
    {
        public SearchRequestInfo Request { get; set; }

        public DiagFormat Diag { get; set; }    // [out]
        // result.Value:
        public SearchResult Result = new SearchResult();    // [out]
    }

    /// <summary>
    /// 处理 检索 事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void ProcessSearchEventHandler(object sender,
        ProcessSearchEventArgs e);

    /// <summary>
    /// 处理检索事件的参数
    /// </summary>
    public class ProcessSearchEventArgs : EventArgs
    {
        public BerTree Request { get; set; }

        // 返回空表示需要立即 Close 通道
        public byte[] Response { get; set; }    // [out]
        // result.Value:
        // public Result Result = new Result();    // [out]
    }

    /// <summary>
    /// 处理 获取 事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void ProcessPresentEventHandler(object sender,
        ProcessPresentEventArgs e);

    /// <summary>
    /// 处理获取事件的参数
    /// </summary>
    public class ProcessPresentEventArgs : EventArgs
    {
        public BerTree Request { get; set; }

        // 返回空表示需要立即 Close 通道
        public byte[] Response { get; set; }    // [out]
        // result.Value:
        // public Result Result = new Result();    // [out]
    }

    /// <summary>
    /// 处理 获取中的获得记录 事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void PresentGetRecordsEventHandler(object sender,
        PresentGetRecordsEventArgs e);

    /// <summary>
    /// 处理获取中的获得记录事件的参数
    /// </summary>
    public class PresentGetRecordsEventArgs : EventArgs
    {
        public PresentRequestInfo Request { get; set; }

        public long TotalCount { get; set; }
        public DiagFormat Diag { get; set; }    // [out]
        public List<RetrivalRecord> Records { get; set; }   // [out]
        // result.Value:
        // public Result Result = new Result();    // [out]
    }

#if NO
    /// <summary>
    /// 通道释放 事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void ChannelClosedEventHandler(object sender,
        ChannelClosedEventArgs e);

    /// <summary>
    /// 通道释放事件的参数
    /// </summary>
    public class ChannelClosedEventArgs : EventArgs
    {
        public ZServerChannel Channel { get; set; }
    }
#endif
}
