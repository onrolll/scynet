using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Scynet.HatcheryFacade.RPC;
using Xunit;
using Xunit.Abstractions;

namespace Scynet.Tests
{
    [TestCaseOrderer("Scynet.Tests.SomeOrderer", "Scynet.Tests")]
    public class HatcheryFacadeStreamingTest
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly SubscriberFacade SubscriberFacade = new SubscriberFacade(new Logger<SubscriberFacade>(new LoggerFactory()), new List<string>() { "127.0.0.1:9092" });
        public HatcheryFacadeStreamingTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async void TestSubscription()
        {
            var subscription = await SubscriberFacade.Subscribe(new SubscriptionRequest() { Id = "TestSubscription", AgetnId = "reddit_posts" }, null);
            Assert.Equal("reddit_posts", subscription.AgentId);
            var subscriptions =
                (IDictionary)typeof(SubscriberFacade).GetField("subscriptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .GetValue(SubscriberFacade);
            Assert.Collection<string>((IEnumerable<string>)subscriptions.Keys, key => Assert.Equal<string>("TestSubscription" , key));

        }

        [Fact]
        public async void TestPull()
        {
            var subscription = await SubscriberFacade.Subscribe(new SubscriptionRequest() { Id = "TestSubscription", AgetnId = "reddit_posts" }, null);

            var result = await SubscriberFacade.Pull(new PullRequest() {Id = "TestSubscription", ReturnImmediately = true, MaxMessages = 5}, null);
            await Task.Delay(1000);
            result = await SubscriberFacade.Pull(new PullRequest() {Id = "TestSubscription", ReturnImmediately = true, MaxMessages = 5}, null);
            foreach (var message in result.Messages)
            {
                _testOutputHelper.WriteLine(message.ToString());
            }
        }
    }
}
