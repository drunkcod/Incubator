using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using NUnit.Framework;
using Xlnt.NUnit;
using System.Reflection;
using System.IO;
using Pencil.Core;
using System.Security.Cryptography;

namespace NMeter
{


    public class SampleClass
    {
        public void DuplicateMethod1() { }
        public void DuplicateMethod2() { }
        public void SomeMethod() {
            var i = 42;
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
                .When("fingerprinting to identical methods", () => {
                    first = MethodMetrics.For<SampleClass>(x => x.DuplicateMethod1());
                    second = MethodMetrics.For<SampleClass>(x => x.DuplicateMethod2());
                })
                .Then("their Fingerprints match", () => Assert.That(first.Fingerprint, Is.EqualTo(second.Fingerprint)))

                .When("fingerprinting diffrent methods", () => {
                    first = MethodMetrics.For<SampleClass>(x => x.DuplicateMethod1());
                    second = MethodMetrics.For<SampleClass>(x => x.SomeMethod());
                
                })
                .Then("they get different fingerprints", () => Assert.That(first.Fingerprint, Is.Not.EqualTo(second.Fingerprint)))
            ;
        }
    }
}
