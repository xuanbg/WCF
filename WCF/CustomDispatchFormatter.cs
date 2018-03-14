using System;
using System.IO;
using System.Net;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using Insight.Utils.Common;
using Insight.Utils.Entity;

namespace Insight.WCF
{
    public class CustomDispatchFormatter : IDispatchMessageFormatter
    {
        private readonly IDispatchMessageFormatter innerFormatter;
        private readonly string allowOrigin = Util.GetAppSetting("AllowOrigin");

        /// <summary>
        /// 构造方法，传入内置消息格式化器
        /// </summary>
        /// <param name="formatter">内置消息格式化器</param>
        public CustomDispatchFormatter(IDispatchMessageFormatter formatter)
        {
            innerFormatter = formatter;
        }

        /// <summary>
        /// 反序列化请求消息
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="parameters">请求参数集合</param>
        public void DeserializeRequest(Message message, object[] parameters)
        {
            innerFormatter.DeserializeRequest(message, parameters);
        }

        /// <summary>
        /// 序列化并压缩响应消息
        /// </summary>
        /// <param name="messageVersion">MessageVersion对象</param>
        /// <param name="parameters">参数集合</param>
        /// <param name="result">响应数据</param>
        /// <returns>Message</returns>
        public Message SerializeReply(MessageVersion messageVersion, object[] parameters, object result)
        {
            var context = WebOperationContext.Current;
            if (context == null) throw new Exception("Unknown exception");

            var encoding = context.IncomingRequest.Headers[HttpRequestHeader.AcceptEncoding] ?? "";
            var model = encoding.Contains("gzip") ? CompressType.Gzip : encoding.Contains("deflate") ? CompressType.Deflate : CompressType.None;
            var bytes = Util.JsonWrite(result, model);

            // 根据AcceptEncoding在响应头中设置ContentEncoding的值
            var headers = context.OutgoingResponse.Headers;
            headers.Add(HttpResponseHeader.ContentEncoding, encoding);

            // 设置CORS参数
            var origin = context.IncomingRequest.Headers["Origin"];
            if (!string.IsNullOrEmpty(origin) && (allowOrigin == "*" || allowOrigin.Contains(origin)))
            {
                headers.Add("Access-Control-Allow-Credentials", "true");
                headers.Add("Access-Control-Allow-Headers", "Accept, Accept-Encoding, Content-Type, Authorization");
                headers.Add("Access-Control-Allow-Methods", "GET, PUT, POST, DELETE, OPTIONS");
                headers.Add("Access-Control-Allow-Origin", origin);
            }

            return context.CreateStreamResponse(new MemoryStream(bytes), "application/json");
        }
    }
}
