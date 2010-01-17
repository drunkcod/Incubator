using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NMeter.Sample;
using Xlnt.NUnit;

namespace NMeter
{
    [TestFixture]
    public class CheckpointTests : ScenarioFixture
    {
        public Scenario Specs() {
            return new Scenario()
            .Describe<Checkpoint>()
                .It("should receive a unique id when created", () => {
                    var first = new Checkpoint();
                    var second = new Checkpoint();
                    Assert.That(first.Id, Is.Not.EqualTo(second.Id));
                });
        }

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
