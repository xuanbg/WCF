using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Text;
using Insight.Utils.Common;
using Insight.Utils.Entity;
using Newtonsoft.Json;

namespace Insight.WCF
{
    public class CustomDispatchFormatter : IDispatchMessageFormatter
    {
        private readonly string allowOrigin = Util.getAppSetting("AllowOrigin");
        private readonly OperationDescription operation;
        private readonly Dictionary<string, int> parameterNames;

        /// <summary>
        /// 构造方法，传入内置消息格式化器
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="isRequest"></param>
        public CustomDispatchFormatter(OperationDescription operation, bool isRequest)
        {
            this.operation = operation;
            if (!isRequest) return;

            var operationParameterCount = operation.Messages[0].Body.Parts.Count;
            if (operationParameterCount <= 1) return;

            parameterNames = new Dictionary<string, int>();
            for (var i = 0; i < operationParameterCount; i++)
            {
                parameterNames.Add(operation.Messages[0].Body.Parts[i].Name, i);
            }
        }

        /// <summary>
        /// 反序列化请求消息
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="parameters">请求参数集合</param>
        public void DeserializeRequest(Message message, object[] parameters)
        {
            if (!message.Properties.TryGetValue(WebBodyFormatMessageProperty.Name, out var bodyFormatProperty) || ((WebBodyFormatMessageProperty)bodyFormatProperty).Format != WebContentFormat.Raw)
            {
                throw new InvalidOperationException("Incoming messages must have a body format of Raw. Is a ContentTypeMapper set on the WebHttpBinding?");
            }

            var context = WebOperationContext.Current;
            if (context == null) throw new Exception("Unknown exception");

            var encoding = context.IncomingRequest.Headers[HttpRequestHeader.ContentEncoding] ?? "";
            var model = encoding.Contains("gzip") ? CompressType.GZIP : encoding.Contains("deflate") ? CompressType.DEFLATE : CompressType.NONE;

            var bodyReader = message.GetReaderAtBodyContents();
            var bytes = Util.decompress(bodyReader.ReadContentAsBase64(), model);
            using (var stream = new MemoryStream(bytes))
            {
                using (var sr = new StreamReader(stream, Encoding.UTF8))
                {
                    var serializer = new JsonSerializer();
                    if (parameters.Length == 1)
                    {
                        parameters[0] = serializer.Deserialize(sr, operation.Messages[0].Body.Parts[0].Type);
                        return;
                    }

                    using (var reader = new JsonTextReader(sr))
                    {
                        reader.Read();
                        if (reader.TokenType != JsonToken.StartObject)
                        {
                            throw new InvalidOperationException("Input needs to be wrapped in an object");
                        }

                        reader.Read();
                        while (reader.TokenType == JsonToken.PropertyName)
                        {
                            var parameterName = reader.Value as string;
                            reader.Read();
                            if (parameterNames.ContainsKey(parameterName ?? throw new InvalidOperationException()))
                            {
                                var parameterIndex = parameterNames[parameterName];
                                parameters[parameterIndex] = serializer.Deserialize(reader, operation.Messages[0].Body.Parts[parameterIndex].Type);
                            }
                            else
                            {
                                reader.Skip();
                            }

                            reader.Read();
                        }
                    }
                }
            }
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
            var model = encoding.Contains("gzip") ? CompressType.GZIP : encoding.Contains("deflate") ? CompressType.DEFLATE : CompressType.NONE;
            var bytes = Util.jsonWrite(result, model);

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
