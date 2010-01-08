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
        public void EmptyMethod() { }
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

    public static class ILGeneratorExtensions
    {
        public static ILGenerator Ldc(this ILGenerator il, int value) {
            il.Emit(OpCodes.Ldc_I4, value);
            return il;
        }

        public static ILGenerator Nop(this ILGenerator il) {
            il.Emit(OpCodes.Nop);
            return il;
        }

        public static ILGenerator Ret(this ILGenerator il) {
            il.Emit(OpCodes.Ret);
            return il;
        }
    }

    [TestFixture]
    public class MethodMetricsTests
    {
        MethodMetrics first, second;

        [TestCaseSource("FingerprintingTests")]
        public void Fingerprinting(Action verify) { verify(); }
        public IEnumerable<TestCaseData> FingerprintingTests() {
            return new Scenario()
                .When("fingerprinting two identical methods", () => {
                    first = GetMetrics(x => x.DuplicateMethod1());
                    second = GetMetrics(x => x.DuplicateMethod2());
                }).Then("their Fingerprints match", () => Assert.That(first.Fingerprint, Is.EqualTo(second.Fingerprint)))

                .When("fingerprinting diffrent methods", () => {
                    first = MethodMetrics.For<SampleClass>(x => x.DuplicateMethod1());
                    second = MethodMetrics.For<SampleClass>(x => x.SomeMethod());                
                }).Then("they get different fingerprints", () => Assert.That(first.Fingerprint, Is.Not.EqualTo(second.Fingerprint)));
        }

        [TestCaseSource("InstructionCountTests")]
        public void InstructionCount(Action verify) { verify(); }
        public IEnumerable<TestCaseData> InstructionCountTests() {
            return new Scenario()
                .When("the method is empty", () => first = GetMetrics(x => x.EmptyMethod()))
                .Then("InstructionCount is 1 (it need to ret(urn))", () => Assert.That(first.InstructionCount, Is.EqualTo(1)))

                .When("fed a sample method", () => {
                    first = GetMetrics(MethodFactory.CreateMethod("Return42", typeof(int), System.Type.EmptyTypes, il => il
                        .Ldc(42)
                        .Ret()));
                }).Then("InstructionCount is number of IL instructions", () => Assert.That(first.InstructionCount, Is.EqualTo(2)))

                .When("the method contains nops", () => {
                    first = GetMetrics(MethodFactory.CreateMethod("Nop", typeof(void), System.Type.EmptyTypes, il => il
                        .Nop()
                        .Nop()
                        .Nop()
                        .Ret()));
                }).Then("they're ignored", () => Assert.That(first.InstructionCount, Is.EqualTo(1)));
        }

        MethodMetrics GetMetrics(Expression<Action<SampleClass>> expression) {
            return MethodMetrics.For(expression);
        }

        MethodMetrics GetMetrics(MethodInfo method) { return MethodMetrics.For(method); }

    }
}
