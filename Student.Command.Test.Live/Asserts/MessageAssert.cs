using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using Student.Command.Domain.Entities;
using Student.Command.Domain.Events;
using Student.Command.Domain.Models;
using System.Text;

namespace Student.Command.Test.Live.Asserts
{
    public static class MessageAssert
    {
        public static void AssertEquality<T, TData>(
          T? @event,
          ServiceBusReceivedMessage? message
           ) where T : Event<TData> where TData : IEventData
        {
            Assert.NotNull(@event);

            Assert.NotNull(@message);

            var body = JsonConvert.DeserializeObject<MessageBody>(Encoding.UTF8.GetString(message?.Body)) ?? throw new Exception();

            BaseAssert(@event, message, body);

            var eventData = @event.Data;

            var messageData = JsonConvert.DeserializeObject<TData>(value: body.Data.ToString()!);

            Assert.Equal(messageData, eventData);
        }

        private static void BaseAssert(Event @event, ServiceBusReceivedMessage? message, MessageBody? body)
        {
            Assert.NotNull(@event);
            Assert.NotNull(message);
            Assert.Equal(@event.Id.ToString(), message.CorrelationId);

            Assert.NotNull(body);
            Assert.NotNull(body.Data);

            Assert.Equal(@event.Sequence, body.Sequence);
            Assert.Equal(@event.Version, body.Version);
            Assert.Equal(@event.Type.ToString(), body.Type);
            Assert.Equal(@event.DateTime, body.DateTime, TimeSpan.FromMinutes(1));
        }
    }
}
