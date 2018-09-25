﻿using System;
using NSubstitute;
using NUnit.Framework;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Modules;

namespace Vostok.ClusterClient.Core.Tests.Modules
{
    [TestFixture]
    internal class ClientApplicationIdentityModule_Tests
    {
        private IRequestContext context;
        private ClientApplicationIdentityModule module;

        [SetUp]
        public void TestSetup()
        {
            context = Substitute.For<IRequestContext>();
            context.Request.Returns(Request.Get("foo/bar"));

            module = new ClientApplicationIdentityModule();
        }

        [TestCase("qwe")]
        [TestCase("xyz")]
        public void Should_add_client_app_name_from_context(string name)
        {
            context.ClientApplicationName.Returns(name);
            
            module.ExecuteAsync(context, requestContext =>
            {
                Console.WriteLine(requestContext.Request.Headers);
                return null;
            });
        }
    }
}