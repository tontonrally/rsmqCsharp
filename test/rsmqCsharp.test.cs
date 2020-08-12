using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;

using NUnit.Framework;

namespace RsmqCsharp.Test
{
    internal static class Global
    {
        public static string QueueName = "q";
        public static string WrongQueueName = "wrongQ";
        public static string WrongQueueNameFormat = @"(&!@^$(*&$@$@#)(*!@#)(*@&$(*@";

        public static async Task FlushDb()
        {
            ConnectionMultiplexer client = await ConnectionMultiplexer.ConnectAsync("127.0.0.1:6379,allowAdmin=true");
            await client.GetServer("127.0.0.1", 6379).FlushDatabaseAsync();
        }
    }

    [TestFixture]
    public class CreateQueueClass
    {
        [SetUp]
        public async Task SetUp()
        {
            await Global.FlushDb();
        }

        [DatapointSource]
        public string[] values = new string[] { $"{Global.QueueName}1", $"{Global.QueueName}2", $"{Global.QueueName}3", $"{Global.QueueName}4", $"{Global.QueueName}5" };

        [Theory]
        public void CreateQueue(string value)
        {
            var rsmq = new Rsmq();
            Assert.AreEqual(1, rsmq.CreateQueue(new CreateQueueOptions { QueueName = value }));

            var queues = rsmq.ListQueues();
            Assert.Contains(value, queues.ToArray());
        }

        [Theory]
        public async Task CreateQueueAsync(string value)
        {
            var rsmq = new Rsmq();
            Assert.AreEqual(1, await rsmq.CreateQueueAsync(new CreateQueueOptions { QueueName = value }));

            var queues = await rsmq.ListQueuesAsync();
            Assert.Contains(value, queues.ToArray());
        }
    }

    [TestFixture]
    public class CreateQueueWithErrorClass
    {
        [SetUp]
        public async Task SetUp()
        {
            await Global.FlushDb();
        }

        [Test]
        public void CreateQueueWithError()
        {
            var rsmq = new Rsmq();

            Assert.Throws<MissingParameterException>(() => rsmq.CreateQueue(new CreateQueueOptions { QueueName = null }));
            Assert.Throws<InvalidFormatException>(() => rsmq.CreateQueue(new CreateQueueOptions { QueueName = Global.WrongQueueNameFormat }));

            var queues = rsmq.ListQueues();
            Assert.Zero(queues.Count());
        }

        [Test]
        public async Task CreateQueueWithErrorAsync()
        {
            var rsmq = new Rsmq();

            Assert.ThrowsAsync<MissingParameterException>(async () => await rsmq.CreateQueueAsync(new CreateQueueOptions { QueueName = null }));
            Assert.ThrowsAsync<InvalidFormatException>(async () => await rsmq.CreateQueueAsync(new CreateQueueOptions { QueueName = Global.WrongQueueNameFormat }));

            var queues = await rsmq.ListQueuesAsync();
            Assert.Zero(queues.Count());
        }
    }

    [TestFixture]
    public class ListQueuesClass
    {
        [SetUp]
        public async Task SetUp()
        {
            await Global.FlushDb();
        }

        [Test]
        public void ListQueues()
        {
            var rsmq = new Rsmq();
            var queues = rsmq.ListQueues();

            Assert.Zero(queues.Count());

            rsmq.CreateQueue(new CreateQueueOptions { QueueName = Global.QueueName });

            queues = rsmq.ListQueues();
            Assert.AreEqual(queues.Count(), 1);
            Assert.Contains(Global.QueueName, queues.ToArray());
        }

        [Test]
        public async Task ListQueuesAsync()
        {
            var rsmq = new Rsmq();
            var queues = await rsmq.ListQueuesAsync();

            Assert.Zero(queues.Count());

            await rsmq.CreateQueueAsync(new CreateQueueOptions { QueueName = Global.QueueName });

            queues = await rsmq.ListQueuesAsync();
            Assert.AreEqual(queues.Count(), 1);
            Assert.Contains(Global.QueueName, queues.ToArray());
        }
    }

    [TestFixture]
    public class DeleteQueueClass
    {
        [SetUp]
        public async Task SetUp()
        {
            await Global.FlushDb();
        }

        [Test]
        public void DeleteQueue()
        {
            var rsmq = new Rsmq();
            rsmq.CreateQueue(new CreateQueueOptions { QueueName = Global.QueueName });
            Assert.AreEqual(1, rsmq.ListQueues().Count());
            rsmq.DeleteQueue(new DeleteQueueOptions { QueueName = Global.QueueName });
            Assert.Zero(rsmq.ListQueues().Count());

        }

        [Test]
        public async Task DeleteQueueAsync()
        {
            var rsmq = new Rsmq();
            await rsmq.CreateQueueAsync(new CreateQueueOptions { QueueName = Global.QueueName });
            Assert.AreEqual(1, (await rsmq.ListQueuesAsync()).Count());
            await rsmq.DeleteQueueAsync(new DeleteQueueOptions { QueueName = Global.QueueName });
            Assert.Zero((await rsmq.ListQueuesAsync()).Count());

        }
    }

    [TestFixture]
    public class DeleteQueueWithErrorClass
    {
        [SetUp]
        public async Task SetUp()
        {
            await Global.FlushDb();
        }

        [Test]
        public void DeleteQueueWithError()
        {
            var rsmq = new Rsmq();
            Assert.Throws<InvalidFormatException>(() => rsmq.DeleteQueue(new DeleteQueueOptions { QueueName = Global.WrongQueueNameFormat }));

        }

        [Test]
        public void DeleteQueueWithErrorAsync()
        {
            var rsmq = new Rsmq();
            Assert.ThrowsAsync<InvalidFormatException>(async () => await rsmq.DeleteQueueAsync(new DeleteQueueOptions { QueueName = Global.WrongQueueNameFormat }));
        }
    }

    [TestFixture]
    public class GetQueueAtributesClass
    {
        [SetUp]
        public async Task SetUp()
        {
            await Global.FlushDb();
        }

        [DatapointSource]
        public CreateQueueOptions[] values = new CreateQueueOptions[] {
            new CreateQueueOptions { QueueName = $"{Global.QueueName}1", MaxSize = 1024 },
            new CreateQueueOptions { QueueName = $"{Global.QueueName}2", MaxSize = 10000, Delay = 30 },
            new CreateQueueOptions { QueueName = $"{Global.QueueName}3", MaxSize = -1, Delay = 1, VisibilityTimer = 10 },
            new CreateQueueOptions { QueueName = $"{Global.QueueName}4", MaxSize = 30000, Delay = 13, VisibilityTimer = 15 },
            new CreateQueueOptions { QueueName = $"{Global.QueueName}5", MaxSize = 45234, Delay = 17, VisibilityTimer = 3 }
        };

        [Theory]
        public void GetQueueAttributes(CreateQueueOptions value)
        {
            var rsmq = new Rsmq();
            rsmq.CreateQueue(value);
            var attributes = rsmq.GetQueueAttributes(new GetQueueAttributesOptions { QueueName = value.QueueName });

            Assert.AreEqual(attributes.Delay, value.Delay);
            Assert.AreEqual(attributes.MaxSize, value.MaxSize);
            Assert.AreEqual(attributes.VisibilityTimer, value.VisibilityTimer);
            Assert.Zero(attributes.TotalSent);

            rsmq.SendMessage(new SendMessageOptions { QueueName = value.QueueName, Message = "Halo" });
            attributes = rsmq.GetQueueAttributes(new GetQueueAttributesOptions { QueueName = value.QueueName });
            Assert.AreEqual(1, attributes.TotalSent);
        }

        [Theory]
        public async Task GetQueueAttributesAsync(CreateQueueOptions value)
        {
            var rsmq = new Rsmq();
            await rsmq.CreateQueueAsync(value);
            var attributes = await rsmq.GetQueueAttributesAsync(new GetQueueAttributesOptions { QueueName = value.QueueName });

            Assert.AreEqual(attributes.Delay, value.Delay);
            Assert.AreEqual(attributes.MaxSize, value.MaxSize);
            Assert.AreEqual(attributes.VisibilityTimer, value.VisibilityTimer);
            Assert.AreEqual(0, attributes.TotalSent);

            await rsmq.SendMessageAsync(new SendMessageOptions { QueueName = value.QueueName, Message = "Halo" });
            attributes = await rsmq.GetQueueAttributesAsync(new GetQueueAttributesOptions { QueueName = value.QueueName });
            Assert.AreEqual(1, attributes.TotalSent);
        }
    }

    [TestFixture]
    public class GetQueueAtributesWithErrorClass
    {
        [SetUp]
        public async Task SetUp()
        {
            await Global.FlushDb();
        }

        [Test]
        public void GetQueueAttributesWithError()
        {
            var rsmq = new Rsmq();
            Assert.Throws<QueueNotFoundException>(() => rsmq.GetQueueAttributes(new GetQueueAttributesOptions { QueueName = Global.QueueName }));
            Assert.Throws<InvalidFormatException>(() => rsmq.GetQueueAttributes(new GetQueueAttributesOptions { QueueName = Global.WrongQueueNameFormat }));
        }

        [Test]
        public void GetQueueAttributesWithErrorAsync()
        {
            var rsmq = new Rsmq();
            Assert.ThrowsAsync<QueueNotFoundException>(async () => await rsmq.GetQueueAttributesAsync(new GetQueueAttributesOptions { QueueName = Global.QueueName }));
            Assert.ThrowsAsync<InvalidFormatException>(async () => await rsmq.GetQueueAttributesAsync(new GetQueueAttributesOptions { QueueName = Global.WrongQueueNameFormat }));
        }
    }

    [TestFixture]
    public class SetQueueQttributesClass
    {
        [SetUp]
        public async Task SetUp()
        {
            await Global.FlushDb();
        }

        [DatapointSource]
        public SetQueueAttributesOptions[] values = new SetQueueAttributesOptions[] {
            new SetQueueAttributesOptions { QueueName = $"{Global.QueueName}1", MaxSize = 1024 },
            new SetQueueAttributesOptions { QueueName = $"{Global.QueueName}2", MaxSize = 10000, Delay = 30 },
            new SetQueueAttributesOptions { QueueName = $"{Global.QueueName}3", MaxSize = 2000, Delay = 1, VisibilityTimer = 10 },
            new SetQueueAttributesOptions { QueueName = $"{Global.QueueName}4", MaxSize = 30000, Delay = 13, VisibilityTimer = 15 },
            new SetQueueAttributesOptions { QueueName = $"{Global.QueueName}5", MaxSize = 45234, Delay = 17, VisibilityTimer = 3 }
        };

        [Theory]
        public void SetQueueAttributes(SetQueueAttributesOptions value)
        {
            var rsmq = new Rsmq();
            var defaultAttributes = new CreateQueueOptions { QueueName = value.QueueName };
            rsmq.CreateQueue(defaultAttributes);

            var queueAttributes = rsmq.GetQueueAttributes(new GetQueueAttributesOptions { QueueName = value.QueueName });

            Assert.AreEqual(defaultAttributes.Delay, queueAttributes.Delay);
            Assert.AreEqual(defaultAttributes.MaxSize, queueAttributes.MaxSize);
            Assert.AreEqual(defaultAttributes.VisibilityTimer, queueAttributes.VisibilityTimer);

            queueAttributes = rsmq.SetQueueAttributes(value);

            Assert.AreEqual(value.Delay ?? defaultAttributes.Delay, queueAttributes.Delay);
            Assert.AreEqual(value.MaxSize ?? defaultAttributes.MaxSize, queueAttributes.MaxSize);
            Assert.AreEqual(value.VisibilityTimer ?? defaultAttributes.VisibilityTimer, queueAttributes.VisibilityTimer);
        }

        [Theory]
        public async Task SetQueueAttributesAsync(SetQueueAttributesOptions value)
        {
            var rsmq = new Rsmq();
            var defaultAttributes = new CreateQueueOptions { QueueName = value.QueueName };
            await rsmq.CreateQueueAsync(defaultAttributes);

            var queueAttributes = await rsmq.GetQueueAttributesAsync(new GetQueueAttributesOptions { QueueName = value.QueueName });

            Assert.AreEqual(defaultAttributes.Delay, queueAttributes.Delay);
            Assert.AreEqual(defaultAttributes.MaxSize, queueAttributes.MaxSize);
            Assert.AreEqual(defaultAttributes.VisibilityTimer, queueAttributes.VisibilityTimer);

            queueAttributes = await rsmq.SetQueueAttributesAsync(value);

            Assert.AreEqual(value.Delay ?? defaultAttributes.Delay, queueAttributes.Delay);
            Assert.AreEqual(value.MaxSize ?? defaultAttributes.MaxSize, queueAttributes.MaxSize);
            Assert.AreEqual(value.VisibilityTimer ?? defaultAttributes.VisibilityTimer, queueAttributes.VisibilityTimer);
        }
    }

    [TestFixture]
    public class SetQueueAttributesWithErrorClass
    {
        [SetUp]
        public async Task SetUp()
        {
            await Global.FlushDb();
        }

        [Test]
        public void SetQueueAttributesWithError()
        {
            var rsmq = new Rsmq();
            Assert.Throws<QueueNotFoundException>(() => rsmq.SetQueueAttributes(new SetQueueAttributesOptions { QueueName = Global.QueueName, MaxSize = 7453 }));

            rsmq.CreateQueue(new CreateQueueOptions { QueueName = Global.QueueName });

            Assert.Throws<QueueNotFoundException>(() => rsmq.SetQueueAttributes(new SetQueueAttributesOptions { QueueName = Global.WrongQueueName, MaxSize = 3000 }));
            Assert.Throws<NoAttributeSuppliedException>(() => rsmq.SetQueueAttributes(new SetQueueAttributesOptions { QueueName = Global.QueueName }));
            Assert.Throws<InvalidValueException>(() => rsmq.SetQueueAttributes(new SetQueueAttributesOptions { QueueName = Global.QueueName, MaxSize = -34 }));
        }

        [Test]
        public async Task SetQueueAttributesWithErrorAsync()
        {
            var rsmq = new Rsmq();
            Assert.ThrowsAsync<QueueNotFoundException>(async () => await rsmq.SetQueueAttributesAsync(new SetQueueAttributesOptions { QueueName = Global.QueueName, MaxSize = 7453 }));

            await rsmq.CreateQueueAsync(new CreateQueueOptions { QueueName = Global.QueueName });

            Assert.ThrowsAsync<QueueNotFoundException>(async () => await rsmq.SetQueueAttributesAsync(new SetQueueAttributesOptions { QueueName = Global.WrongQueueName, MaxSize = 3000 }));
            Assert.ThrowsAsync<NoAttributeSuppliedException>(async () => await rsmq.SetQueueAttributesAsync(new SetQueueAttributesOptions { QueueName = Global.QueueName }));
            Assert.ThrowsAsync<InvalidValueException>(async () => await rsmq.SetQueueAttributesAsync(new SetQueueAttributesOptions { QueueName = Global.QueueName, MaxSize = -34 }));
        }
    }

    [TestFixture]
    public class ReceiveMessageClass
    {
        [SetUp]
        public async Task SetUp()
        {
            await Global.FlushDb();
        }

        [Test]
        public void ReceiveMessage()
        {
            var rsmq = new Rsmq();
            rsmq.CreateQueue(new CreateQueueOptions { QueueName = Global.QueueName });

            var msgId1 = rsmq.SendMessage(new SendMessageOptions { QueueName = Global.QueueName, Message = "hello1" });
            var msgId2 = rsmq.SendMessage(new SendMessageOptions { QueueName = Global.QueueName, Message = "hello2" });

            var message1 = rsmq.ReceiveMessage(new ReceiveMessageOptions { QueueName = Global.QueueName });
            var message2 = rsmq.ReceiveMessage(new ReceiveMessageOptions { QueueName = Global.QueueName });

            Assert.NotNull(message1);
            Assert.NotNull(message2);

            Assert.AreEqual("hello1", message1.Message);
            Assert.AreEqual("hello2", message2.Message);

            Assert.AreEqual(msgId1, message1.Id);
            Assert.AreEqual(msgId2, message2.Id);

            Assert.AreEqual(1, rsmq.DeleteMessage(new DeleteMessageOptions { QueueName = Global.QueueName, Id = msgId1 }));
            Assert.AreEqual(1, rsmq.DeleteMessage(new DeleteMessageOptions { QueueName = Global.QueueName, Id = msgId2 }));
        }

        [Test]
        public async Task ReceiveMessageAsync()
        {
            var rsmq = new Rsmq();
            await rsmq.CreateQueueAsync(new CreateQueueOptions { QueueName = Global.QueueName });

            var msgId1 = await rsmq.SendMessageAsync(new SendMessageOptions { QueueName = Global.QueueName, Message = "hello1" });
            var msgId2 = await rsmq.SendMessageAsync(new SendMessageOptions { QueueName = Global.QueueName, Message = "hello2" });

            var message1 = await rsmq.ReceiveMessageAsync(new ReceiveMessageOptions { QueueName = Global.QueueName });
            var message2 = await rsmq.ReceiveMessageAsync(new ReceiveMessageOptions { QueueName = Global.QueueName });

            Assert.NotNull(message1);
            Assert.NotNull(message2);

            Assert.AreEqual("hello1", message1.Message);
            Assert.AreEqual("hello2", message2.Message);

            Assert.AreEqual(msgId1, message1.Id);
            Assert.AreEqual(msgId2, message2.Id);

            Assert.AreEqual(1, await rsmq.DeleteMessageAsync(new DeleteMessageOptions { QueueName = Global.QueueName, Id = msgId1 }));
            Assert.AreEqual(1, await rsmq.DeleteMessageAsync(new DeleteMessageOptions { QueueName = Global.QueueName, Id = msgId2 }));
        }
    }

    [TestFixture]
    public class DeleteMessageClass
    {
        [SetUp]
        public async Task SetUp()
        {
            await Global.FlushDb();
        }

        [Test]
        public void DeleteMessage()
        {
            var rsmq = new Rsmq();
            rsmq.CreateQueue(new CreateQueueOptions { QueueName = Global.QueueName });

            var msgId = rsmq.SendMessage(new SendMessageOptions { QueueName = Global.QueueName, Message = "hello" });

            Assert.AreEqual(1, rsmq.DeleteMessage(new DeleteMessageOptions { QueueName = Global.QueueName, Id = msgId }));
            Assert.Zero(rsmq.DeleteMessage(new DeleteMessageOptions { QueueName = Global.QueueName, Id = "0123456789abcdefghijklmnopqrstuv" }));
        }

        [Test]
        public async Task DeleteMessageAsync()
        {
            var rsmq = new Rsmq();
            await rsmq.CreateQueueAsync(new CreateQueueOptions { QueueName = Global.QueueName });

            var msgId = await rsmq.SendMessageAsync(new SendMessageOptions { QueueName = Global.QueueName, Message = "hello" });

            Assert.AreEqual(1, await rsmq.DeleteMessageAsync(new DeleteMessageOptions { QueueName = Global.QueueName, Id = msgId }));
            Assert.Zero(await rsmq.DeleteMessageAsync(new DeleteMessageOptions { QueueName = Global.QueueName, Id = "0123456789abcdefghijklmnopqrstuv" }));
        }
    }

    [TestFixture]
    public class PopMessageClass
    {
        [SetUp]
        public async Task SetUp()
        {
            await Global.FlushDb();
        }

        [Test]
        public void PopMessage()
        {
            var rsmq = new Rsmq();
            rsmq.CreateQueue(new CreateQueueOptions { QueueName = Global.QueueName });

            var msgId = rsmq.SendMessage(new SendMessageOptions { QueueName = Global.QueueName, Message = "hello" });

            var poppedMessage = rsmq.PopMessage(new PopMessageOptions { QueueName = Global.QueueName });
            Assert.AreEqual(msgId, poppedMessage.Id);

            Assert.Zero(rsmq.DeleteMessage(new DeleteMessageOptions { QueueName = Global.QueueName, Id = msgId }));
        }

        [Test]
        public async Task PopMessageAsync()
        {
            var rsmq = new Rsmq();
            await rsmq.CreateQueueAsync(new CreateQueueOptions { QueueName = Global.QueueName });

            var msgId = await rsmq.SendMessageAsync(new SendMessageOptions { QueueName = Global.QueueName, Message = "hello" });

            var poppedMessage = await rsmq.PopMessageAsync(new PopMessageOptions { QueueName = Global.QueueName });
            Assert.AreEqual(msgId, poppedMessage.Id);

            Assert.Zero(await rsmq.DeleteMessageAsync(new DeleteMessageOptions { QueueName = Global.QueueName, Id = msgId }));
        }
    }

    [TestFixture]
    public class ChangeMessageVisibilityClass
    {
        [SetUp]
        public async Task SetUp()
        {
            await Global.FlushDb();
        }

        [Test]
        public void ChangeMessageVisibility()
        {
            var rsmq = new Rsmq();
            rsmq.CreateQueue(new CreateQueueOptions { QueueName = Global.QueueName });

            var msgId = rsmq.SendMessage(new SendMessageOptions { QueueName = Global.QueueName, Message = "hello" });

            rsmq.ChangeMessageVisibility(new ChangeMessageVisibilityOptions { QueueName = Global.QueueName, Id = msgId, VisibilityTimer = 2 });

            Assert.IsNull(rsmq.ReceiveMessage(new ReceiveMessageOptions { QueueName = Global.QueueName }));
            Task.WaitAll(Task.Delay(2500));
            Assert.IsNotNull(rsmq.ReceiveMessage(new ReceiveMessageOptions { QueueName = Global.QueueName }));
        }

        [Test]
        public async Task ChangeMessageVisibilityAsync()
        {
            var rsmq = new Rsmq();
            await rsmq.CreateQueueAsync(new CreateQueueOptions { QueueName = Global.QueueName });

            var msgId = await rsmq.SendMessageAsync(new SendMessageOptions { QueueName = Global.QueueName, Message = "hello" });

            await rsmq.ChangeMessageVisibilityAsync(new ChangeMessageVisibilityOptions { QueueName = Global.QueueName, Id = msgId, VisibilityTimer = 2 });

            Assert.IsNull(await rsmq.ReceiveMessageAsync(new ReceiveMessageOptions { QueueName = Global.QueueName }));
            await Task.Delay(2500);
            Assert.IsNotNull(await rsmq.ReceiveMessageAsync(new ReceiveMessageOptions { QueueName = Global.QueueName }));
        }
    }
}