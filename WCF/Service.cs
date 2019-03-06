using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Xml;
using Insight.Utils.Common;

namespace Insight.WCF
{
    public class Service
    {
        private readonly List<ServiceHost> hosts = new List<ServiceHost>();

        /// <summary>
        /// 读取服务目录下的WCF服务库创建WCF服务主机
        /// </summary>
        public void createHosts()
        {
            var dirInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "Services");
            var files = dirInfo.GetFiles("*.dll", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var assembly = Assembly.LoadFrom(file.FullName);
                var attributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.FirstOrDefault() is AssemblyProductAttribute att && att.Product != "WCF Service") continue;

                var name = assembly.GetName();
                var type = assembly.GetTypes().Single(i => i.Name == name.Name);
                var ln = name.Name.ToLower();
                var api = ln.EndsWith("s") ? ln.Substring(0, ln.Length - 1) : ln;
                var uri = new Uri($"{Util.getAppSetting("Address")}/{api}api/v{name.Version.Major}.{name.Version.Minor}");
                createHost(type, uri);
            }
        }

        /// <summary>
        /// 启动服务列表中的全部服务
        /// </summary>
        public void startService()
        {
            var list = hosts.Where(h => h.State == CommunicationState.Created || h.State == CommunicationState.Closed);
            foreach (var host in list)
            {
                host.Open();
            }
        }

        /// <summary>
        /// 启动服务列表中的服务
        /// </summary>
        /// <param name="service">服务名称</param>
        public void startService(string service)
        {
            var host = hosts.SingleOrDefault(h => h.Description.Name == service);
            if (host == null || (host.State != CommunicationState.Created && host.State != CommunicationState.Closed)) return;

            host.Open();
        }

        /// <summary>
        /// 停止服务列表中的全部服务
        /// </summary>
        public void stopService()
        {
            foreach (var host in hosts.Where(host => host.State == CommunicationState.Opened))
            {
                host.Abort();
                host.Close();
            }
        }

        /// <summary>
        /// 停止服务列表中的服务
        /// </summary>
        /// <param name="service">服务名称</param>
        public void stopService(string service)
        {
            var host = hosts.SingleOrDefault(h => h.Description.Name == service);
            if (host == null || host.State != CommunicationState.Opened) return;

            host.Abort();
            host.Close();
        }

        /// <summary>
        /// 创建WCF服务主机
        /// </summary>
        /// <param name="type">TypeInfo</param>
        /// <param name="uri">Uri</param>
        private void createHost(Type type, Uri uri)
        {
            var host = new ServiceHost(type, uri);
            var binding = initBinding();
            var endpoint = host.AddServiceEndpoint(type.GetInterfaces().First(), binding, "");
            var behavior = new CustomWebHttpBehavior {AutomaticFormatSelectionEnabled = true};
            endpoint.Behaviors.Add(behavior);
            /* Windows Server 2008 需要设置MaxItemsInObjectGraph值为2147483647
            foreach (var operation in endpoint.Contract.Operations)
            {
                var behavior = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();
                if (behavior != null)
                {
                    behavior.MaxItemsInObjectGraph = 2147483647;
                }
            }*/
            hosts.Add(host);
            logToEvent($"WCF 服务{type.Name}已绑定于：{uri}");
        }

        /// <summary>
        /// 初始化基本HTTP服务绑定
        /// </summary>
        private WebHttpBinding initBinding()
        {
            var binding = new WebHttpBinding
            {
                SendTimeout = TimeSpan.FromSeconds(600),
                ReceiveTimeout = TimeSpan.FromSeconds(600),
                ReaderQuotas = new XmlDictionaryReaderQuotas {MaxArrayLength = 67108864, MaxStringContentLength = 67108864},
                TransferMode = TransferMode.Streamed,
                MaxReceivedMessageSize = 1073741824,
                ContentTypeMapper = new CustomContentTypeMapper()
            };

            return binding;
        }

        /// <summary>
        /// 将事件消息写入系统日志
        /// </summary>
        /// <param name="message"></param>
        public void logToEvent(string message)
        {
            if (!EventLog.SourceExists("WCF Service"))
            {
                EventLog.CreateEventSource("WCF Service", "应用程序");
            }

            EventLog.WriteEntry("WCF Service", message, EventLogEntryType.Information);
        }
    }
}