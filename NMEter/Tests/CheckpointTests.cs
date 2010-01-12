using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NMeter.Sample;

namespace NMeter
{
    [TestFixture]
    public class CheckpointTests
    {
        [Test]
        public void should_analyze_interna_classes() {
            var checkpoint = Checkpoint.For(typeof(SampleClass).Assembly);
            Assert.That(checkpoint.Classes.Any(x => x.Name.Equals("NMeter.Sample.InternalClass")), Is.True);
        }
        [Test]
        public void should_analyze_internal_methods() {
            var checkpoint = Checkpoint.For(typeof(SampleClass).Assembly);
            Assert.That(checkpoint.Methods.Any(x => x.Signature.Equals("System.Void NMeter.Sample.InternalClass::InternalMethod()")), Is.True);
        }
        [Test]
        public void should_analyze_static_methods() {
            var checkpoint = Checkpoint.For(typeof(SampleClass).Assembly);
            Assert.That(checkpoint.Methods.Any(x => x.Signature.Equals("System.Void NMeter.Sample.SampleClass::StaticMethod()")), Is.True);
        }
    }
}
