using System;
using System.Collections.Generic;
using Pencil.Core;
using NUnit.Framework;
using Xlnt.NUnit;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace NMeter
{
    public class SampleClass
    {
        public void DuplicateMethod1() { }
        public void DuplicateMethod2() { }
        public void SomeMethod() {
            var i = 42;
        }
        public void Nilad() { }
        public void Duad(int a, int b) { }
        public void EmptyMethod() { }

        [System.Runtime.CompilerServices.CompilerGenerated]
        public void CompilerGenerated() { }
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
    public class MethodMetricsTests
    {
        MethodMetrics first, second;

        [TestCaseSource("FingerprintingTests")]
        [TestCaseSource("InstructionCountTests")]
        [TestCaseSource("IsGeneratedTests")]
        public void Scenarios(Action verify) { verify(); }
        
        public IEnumerable<TestCaseData> FingerprintingTests() {
            return new Scenario()
                .When("two identical methods", () => {
                    first = GetMetrics(x => x.DuplicateMethod1());
                    second = GetMetrics(x => x.DuplicateMethod2());
                }).Then("their Fingerprints match", () => Assert.That(first.Fingerprint, Is.EqualTo(second.Fingerprint)))

                .When("diffrent methods", () => {
                    first = GetMetrics(x => x.DuplicateMethod1());
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

        public IEnumerable<TestCaseData> InstructionCountTests() {
            return new Scenario()
                .When("the method is empty", () => GetMetrics(x => x.EmptyMethod()))
                .Then("InstructionCount is 1 (it need to ret(urn))", method => Assert.That(method.InstructionCount, Is.EqualTo(1)))

                .When("fed a sample method", () => {
                    first = GetMetrics("Return42", typeof(int), il => il
                        .Ldc(42)
                        .Ret());
                }).Then("InstructionCount is number of IL instructions", () => Assert.That(first.InstructionCount, Is.EqualTo(2)))

                .When("the method contains nops", () => {
                    first = GetMetrics("Nop", typeof(void), il => il
                        .Nop()
                        .Nop()
                        .Nop()
                        .Ret());
                }).Then("they're ignored", () => Assert.That(first.InstructionCount, Is.EqualTo(1)));
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

        public IEnumerable<TestCaseData> IsGeneratedTests() {
            return new Scenario()
                .When("given a user defined function", () => GetMetrics(x => x.SomeMethod()))
                .Then("IsGenerated is false", method => Assert.That(method.IsGenerated, Is.False))
                
                .When("CompilerGeneratedAttribute presten", () => GetMetrics(x => x.CompilerGenerated()))
                .Then("IsGenerated is true", method => Assert.That(method.IsGenerated, Is.True));
        }

        TestCaseData MethodAndMetrics(string name) {
            var method = typeof(SampleClass).GetMethod(name);
            return new TestCaseData(GetMetrics(method), method);
        }

        TestCaseData[] Tests(params TestCaseData[] tests) { return tests; }

        MethodMetrics GetMetrics(string name, System.Type returnType, Action<ILGenerator> createIL) {
            return GetMetrics(MethodFactory.CreateMethod(name, returnType, System.Type.EmptyTypes, createIL));
        }

        MethodMetrics GetMetrics(Expression<Action<SampleClass>> expression) {
            return MethodMetrics.For((expression.Body as MethodCallExpression).Method);
        }

        MethodMetrics GetMetrics(MethodInfo method) { return MethodMetrics.For(method); }

    }
}
