using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EME.Application.ActorFramework;
using NetMQ;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using EME.Infrastructure.Serialization;

namespace EME.Tests.Application
{
    [TestClass]
    public class ActorTests
    {
        //[TestMethod]
        //public void EchoShimHandlerTest()
        //{
        //    //Round 1 : Should work fine
        //    EchoShimHandler echoShimHandler = new EchoShimHandler();
        //    Actor actor = new Actor(NetMQContext.Create(), echoShimHandler, new object[] { "Hello World" });
        //    actor.SendMore("ECHO");
        //    string actorMessage = "This is a string";
        //    actor.Send(actorMessage);
        //    var result = actor.ReceiveString();
        //    Trace.WriteLine("ROUND1");
        //    Trace.WriteLine("========================");
        //    string expectedEchoHandlerResult = string.Format("ECHO BACK : {0}", actorMessage);
        //    Trace.WriteLine("ExpectedEchoHandlerResult: '" + expectedEchoHandlerResult + "'\r\nGot : '" + result + "'\r\n");
        //    actor.Dispose();

        //    //Round 2 : Should NOT work, as we are now using Disposed actor
        //    //try
        //    //{
        //    //    Trace.WriteLine("ROUND2");
        //    //    Trace.WriteLine("========================");
        //    //    actor.SendMore("ECHO");
        //    //    actor.Send("This is a string");
        //    //    result = actor.ReceiveString();
        //    //}
        //    //catch (NetMQException nex)
        //    //{
        //    //    Trace.WriteLine("NetMQException : Actor has been disposed so this is expected\r\n");
        //    //}

        //    //Round 3 : Should work fine
        //    echoShimHandler = new EchoShimHandler();

        //    actor = new Actor(NetMQContext.Create(), echoShimHandler, new object[] { "Hello World" });
        //    actor.SendMore("ECHO");
        //    actorMessage = "Another Go";
        //    actor.Send(actorMessage);
        //    result = actor.ReceiveString();
        //    Trace.WriteLine("ROUND3");
        //    Trace.WriteLine("========================");
        //    expectedEchoHandlerResult = string.Format("ECHO BACK : {0}", actorMessage);
        //    Trace.WriteLine("ExpectedEchoHandlerResult: '" + expectedEchoHandlerResult + "'\r\nGot : '" + result + "'\r\n");
        //    actor.Dispose();
        //}

        //[TestMethod]
        //public void AccountShimHandlerTest()
        //{
        //    AccountShimHandler accountShimHandler = new AccountShimHandler();
        //    AccountAction accountAction = new AccountAction(TransactionType.Credit, 10);
        //    Account account = new Account(1, "Test Account", "11223", 0);

        //    Actor accountActor = new Actor(NetMQContext.Create(), accountShimHandler,
        //        new object[] { accountAction.ToJSON() });
        //    accountActor.SendMore("AMEND ACCOUNT");
        //    accountActor.Send(account.ToJSON());
        //    Account updatedAccount = accountActor.ReceiveString().FromJSON<Account>();
        //    Console.WriteLine("ROUND4");
        //    Console.WriteLine("========================");
        //    decimal expectedAccountBalance = 10.0m;
        //    Console.WriteLine(
        //        "Exected Account Balance: '{0}'\r\nGot : '{1}'\r\n" +
        //        "Are Same Account Object : '{2}'\r\n",
        //        expectedAccountBalance, updatedAccount.Balance,
        //        ReferenceEquals(accountActor, updatedAccount));
        //    accountActor.Dispose();
        //}

        [TestMethod]
        public void NativeActorTests()
        {

        }
    }
}
