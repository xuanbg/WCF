using System.ServiceModel.Channels;

namespace Insight.WCF
{
    public class CustomContentTypeMapper : WebContentTypeMapper
    {
        public override WebContentFormat GetMessageFormatForContentType(string contentType)
        {
            return WebContentFormat.Raw;
        }
    }
}
