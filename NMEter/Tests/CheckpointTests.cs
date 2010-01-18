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
                })
                .It("has 0 metrics when created", () => {
                    var checkpoint = new Checkpoint();
                    Assert.That(checkpoint.MetricsCount, Is.EqualTo(0));
                })
            .Given("a newly created Checkpoint", () => new Checkpoint())
                .When("a new metric is added", checkpoint => { checkpoint.AddMetric<ClassMetrics>("Classes"); })
                .Then("it can be retreived", checkpoint => Assert.That(checkpoint.GetMetric("Classes"), Is.AssignableTo<IList<ClassMetrics>>()))
                .And("MetricCount is increased", checkpoint => Assert.That(checkpoint.MetricsCount, Is.EqualTo(1)))
                .And("trying to access a missing metric throws ArgumentException", checkpoint => Assert.That(() => checkpoint.GetMetric("Missing"), Throws.ArgumentException))

                .When("adding a new metric", checkpoint => new { Checkpoint = checkpoint, Metric = checkpoint.AddMetric<ClassMetrics>("Classes") })
                .Then("the return value is of type IList´1", context => Assert.That(context.Metric, Is.AssignableTo<IList<ClassMetrics>>()))
                .And("the return value is the same as when retreiving the metric", context => Assert.That(context.Metric, Is.SameAs(context.Checkpoint.GetMetric("Classes"))))
                
                .When("metrics are added", checkpoint => {
                    checkpoint.AddMetric<ClassMetrics>("Classes");
                    checkpoint.AddMetric<MethodMetrics>("Methods"); })
                .Then("they can be enumerated with EachMetric", checkpoint => {
                    var metrics = new List<string>();
                    checkpoint.EachMetric((name, items) => metrics.Add(name));
                    Assert.That(metrics, Is.EquivalentTo(new[]{ "Classes", "Methods"}));
                });
        }

        [Test]
        public void should_analyze_interna_classes() {
            var checkpoint = Checkpoint.For(typeof(SampleClass).Assembly);
            Assert.That(checkpoint.GetMetric<ClassMetrics>("Classes").Any(x => x.Name.Equals("NMeter.Sample.InternalClass")), Is.True);
        }
        [Test]
        public void should_analyze_internal_methods() {
            var checkpoint = Checkpoint.For(typeof(SampleClass).Assembly);
            Assert.That(checkpoint.GetMetric<MethodMetrics>("Methods").Any(x => x.Signature.Equals("System.Void NMeter.Sample.InternalClass::InternalMethod()")), Is.True);
        }
        [Test]
        public void should_analyze_static_methods() {
            var checkpoint = Checkpoint.For(typeof(SampleClass).Assembly);
            Assert.That(checkpoint.GetMetric<MethodMetrics>("Methods").Any(x => x.Signature.Equals("System.Void NMeter.Sample.SampleClass::StaticMethod()")), Is.True);
        }
    }
}
