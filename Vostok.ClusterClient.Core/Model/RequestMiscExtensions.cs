namespace Vostok.Clusterclient.Core.Model
{
    public static class RequestMiscExtensions
    {
        public static bool ContainsAlreadyUsedStream(this Request request)
        {
            return request.StreamContent is SingleUseStreamContent streamContent && streamContent.WasUsed;
        }

        public static bool ContainsAlreadyUsedContent(this Request request)
        {
            return request.ContentProducer is UserContentProducerWrapper contentProducer && contentProducer.WasUsed;
        }
    }
}