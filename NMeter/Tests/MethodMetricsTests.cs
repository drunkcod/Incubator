using System;
using System.Collections.Generic;
using Pencil.Core;
using NUnit.Framework;
using Xlnt.NUnit;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using NMeter.Sample;
using Xlnt.Stuff;

namespace NMeter
{
    [CompilerGenerated]
    public class GeneratedClass
    {
        public void SomeMethod(){}
    }

    class MethodFactory
    {
        static public MethodInfo CreateMethod(string name, System.Type returnType, System.Type[] arguments, Action<ILGenerator> createIL) {
            const string moduleName = "NMeter.Generated";
            var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(moduleName), AssemblyBuilderAccess.ReflectionOnly);
            var module = assembly.DefineDynamicModule(moduleName);
            var method = module.DefineGlobalMethod(name, MethodAttributes.Static | MethodAttributes.Public, returnType, arguments);
            createIL(method.GetILGenerator());
            module.CreateGlobalFunctions();
            return module.GetMethod(method.Name, arguments);
        }
    }

    [TestFixture]
    public class MethodMetricsTests : ScenarioFixture
    {
        MethodMetrics first, second;

        public Scenario Fingerprinting() {
            return new Scenario("Method fingerprinting")
                .When("two identical methods", () => {
                    first = GetMetrics(x => x.Fib(42));
                    second = GetMetrics(x => x.Fib2(42));
                }).Then("their Fingerprints match", () => Assert.That(first.Fingerprint, Is.EqualTo(second.Fingerprint)))

                .When("diffrent methods", () => {
                    first = GetMetrics(x => x.Fib(42));
                    second = GetMetrics(x => x.SomeMethod());                
                }).Then("they get different fingerprints", () => Assert.That(first.Fingerprint, Is.Not.EqualTo(second.Fingerprint)))
                
                .When("when difference is \"nop\"s", () => {
                    first = GetMetrics("Return42", typeof(int), il => il
                        .Ldc(42)
                        .Ret());

                    second = GetMetrics("ReturnNop42", typeof(int), il => il
                        .Ldc(42)
                        .Nop()
                        .Ret());
                }).Then("their Fingerprints match", () => Assert.That(first.Fingerprint, Is.EqualTo(second.Fingerprint)));
        }

        public Scenario InstructionCount() {
            return new Scenario("Counting IL instructions")
                .When("the method is empty", () => GetMetrics(x => x.EmptyMethod()))
                .Then("InstructionCount is 1 (it need to ret(urn))", method => Assert.That(method.InstructionCount, Is.EqualTo(1)))

                .When("fed a sample method", () => {
                    return GetMetrics("Return42", typeof(int), il => il
                        .Ldc(42)
                        .Ret());
                }).Then("InstructionCount is number of IL instructions", method => Assert.That(method.InstructionCount, Is.EqualTo(2)))

                .When("the method contains nops", () => {
                    return GetMetrics("Nop", typeof(void), il => il
                        .Nop()
                        .Nop()
                        .Nop()
                        .Ret());
                }).Then("they're ignored", method => Assert.That(method.InstructionCount, Is.EqualTo(1)));
        }

        public Scenario IsGeneratedTests() {
            return new Scenario("Detect generated method")
                .When("given a user defined method", () => GetMetrics(x => x.SomeMethod()))
                .Then("IsGenerated is false", method => Assert.That(method.IsGenerated, Is.False))

                .When("CompilerGeneratedAttribute presten", () => GetMetrics(x => x.CompilerGenerated()))
                .Then("IsGenerated is true", method => Assert.That(method.IsGenerated, Is.True))

                .When("Method belongs to a generated class", () => GetMetrics(typeof(GeneratedClass).GetMethod("SomeMethod")))
                .Then("IsGenerated is true", method => Assert.That(method.IsGenerated, Is.True));
        }

        public Scenario SignatureTests() {
            return new Scenario("Generating method signature")
                .It("should match DefaultFormatter", () => {
                    var method = GetMethod(x => x.Duad(1, 2));
                    var actual = GetMetrics(method).Signature;

                    Assert.That(actual, Is.EqualTo(new DefaultFormatter().Format(method)));
                });
        }

        [TestCaseSource("ParameterCountTests")]
        public void ParameterCount(MethodMetrics metrics, MethodInfo method) {
            Assert.That(metrics.ParameterCount, Is.EqualTo(method.GetParameters().Length));
        }
        public IEnumerable<TestCaseData> ParameterCountTests() {            
            return Tests(
                MethodAndMetrics("Nilad").SetName("no parameters"),
                MethodAndMetrics("Duad").SetName("multiple paramters"));
        }

        TestCaseData MethodAndMetrics(string name) {
            var method = typeof(SampleClass).GetMethod(name);
            return new TestCaseData(GetMetrics(method), method);
        }

        TestCaseData[] Tests(params TestCaseData[] tests) { return tests; }

        MethodMetrics GetMetrics(string name, System.Type returnType, Action<ILGenerator> createIL) {
            return GetMetrics(MethodFactory.CreateMethod(name, returnType, System.Type.EmptyTypes, createIL));
        }

        MethodInfo GetMethod(Expression<Action<SampleClass>> expression){
            return (expression.Body as MethodCallExpression).Method;
        }

        MethodMetrics GetMetrics(Expression<Action<SampleClass>> expression) {
            return GetMetrics(GetMethod(expression));
        }

        MethodMetrics GetMetrics(MethodInfo method) { return methodMetrics.ComputeMetrics(method); }

        MethodMetricsExtractor methodMetrics = new MethodMetricsExtractor();
    }
}
