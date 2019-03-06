using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;

namespace Insight.WCF
{
    public class CustomWebHttpBehavior : WebHttpBehavior
    {
        /// <summary>
        /// 设置请求消息反序列化器
        /// </summary>
        /// <param name="operationDescription"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        protected override IDispatchMessageFormatter GetRequestDispatchFormatter(OperationDescription operationDescription, ServiceEndpoint endpoint)
        {
            // no change for GET operations or nothing in the body, still use the default
            if (isGetOperation(operationDescription) || operationDescription.Messages[0].Body.Parts.Count == 0)
            {
                return base.GetRequestDispatchFormatter(operationDescription, endpoint);
            }

            return new CustomDispatchFormatter(operationDescription, true);
        }

        /// <summary>
        /// 设置响应消息序列化器
        /// </summary>
        /// <param name="operationDescription"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        protected override IDispatchMessageFormatter GetReplyDispatchFormatter(OperationDescription operationDescription, ServiceEndpoint endpoint)
        {
            if (operationDescription.Messages.Count == 1 || operationDescription.Messages[1].Body.ReturnValue.Type == typeof(void))
            {
                return base.GetReplyDispatchFormatter(operationDescription, endpoint);
            }

            return new CustomDispatchFormatter(operationDescription, false);
        }

        /// <summary>
        /// ServiceEndpoint 检查器
        /// </summary>
        /// <param name="endpoint"></param>
        public override void Validate(ServiceEndpoint endpoint)
        {
            base.Validate(endpoint);

            var elements = endpoint.Binding.CreateBindingElements();
            var webEncoder = elements.Find<WebMessageEncodingBindingElement>();
            if (webEncoder == null)
            {
                throw new InvalidOperationException("This behavior must be used in an endpoint with the WebHttpBinding (or a custom binding with the WebMessageEncodingBindingElement).");
            }

            foreach (var operation in endpoint.Contract.Operations)
            {
                validateOperation(operation);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="operation"></param>
        private void validateOperation(OperationDescription operation)
        {
            if (operation.Messages.Count > 1)
            {
                if (operation.Messages[1].Body.Parts.Count > 0)
                {
                    throw new InvalidOperationException("Operations cannot have out/ref parameters.");
                }
            }

            var bodyStyle = getBodyStyle(operation);
            var inputParameterCount = operation.Messages[0].Body.Parts.Count;
            if (!isGetOperation(operation))
            {
                var wrappedRequest = bodyStyle == WebMessageBodyStyle.Wrapped || bodyStyle == WebMessageBodyStyle.WrappedRequest;
                if (inputParameterCount == 1 && wrappedRequest)
                {
                    throw new InvalidOperationException("Wrapped body style for single parameters not implemented in this behavior.");
                }
            }

            var wrappedResponse = bodyStyle == WebMessageBodyStyle.Wrapped || bodyStyle == WebMessageBodyStyle.WrappedResponse;
            var isVoidReturn = operation.Messages.Count == 1 || operation.Messages[1].Body.ReturnValue.Type == typeof(void);
            if (!isVoidReturn && wrappedResponse)
            {
                throw new InvalidOperationException("Wrapped response not implemented in this behavior.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        private WebMessageBodyStyle getBodyStyle(OperationDescription operation)
        {
            var wga = operation.Behaviors.Find<WebGetAttribute>();
            if (wga != null)
            {
                return wga.BodyStyle;
            }

            var wia = operation.Behaviors.Find<WebInvokeAttribute>();
            if (wia != null)
            {
                return wia.BodyStyle;
            }

            return DefaultBodyStyle;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        private bool isGetOperation(OperationDescription operation)
        {
            var wga = operation.Behaviors.Find<WebGetAttribute>();
            if (wga != null)
            {
                return true;
            }

            var wia = operation.Behaviors.Find<WebInvokeAttribute>();
            if (wia != null)
            {
                return wia.Method == "HEAD";
            }

            return false;
        }
    }
}
