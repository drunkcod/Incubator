﻿using System.Net;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Concoct
{
    public class MvcHost
    {
        class NullApplication : IApplication
        {
            public void Start() { }
        }

        readonly HttpListenerAcceptor acceptor;
        readonly IApplication application;

        public static MvcHost Create(IPEndPoint bindTo, string virtualPath) {
            return Create(bindTo, virtualPath, new NullApplication());
        }

        public static MvcHost Create(IPEndPoint bindTo, string virtualPath, Type applicationType) {
            return Create(bindTo, virtualPath, CreateApplicationProxy(applicationType));
        }

        static MvcHost Create(IPEndPoint bindTo, string virtualPath, IApplication application) {
            return new MvcHost(new HttpListenerAcceptor(
                    bindTo,
                    virtualPath,
                    new MvcRequestHandler(virtualPath)),
                application);
        }

        MvcHost(HttpListenerAcceptor acceptor, IApplication application) {
            this.acceptor = acceptor;
            this.application = application;
        }

        public event EventHandler<EventArgs> Starting;

        public void Start() {
            application.Start();
            OnStarting();
            acceptor.Start();
        }

        public void Stop() {
            acceptor.Stop();
        }

        void OnStarting() {
            var handler = Starting;
            if(handler != null)
                handler(this, EventArgs.Empty);
        }

        static IApplication CreateApplicationProxy(Type httpApplicationType) {
            var generated = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("Concoct.Generated"), AssemblyBuilderAccess.Run);
            var module = generated.DefineDynamicModule("Main");
            var proxy = ApplicationBuilder.CreateIn(module, httpApplicationType);
            proxy.DynamicEventWireUp(x => x.Start(), "Application_Start");
            return proxy.CreateType();
        }
    }
}