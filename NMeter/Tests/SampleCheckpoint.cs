using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xlnt.NUnit;
using NMeter.Sample;
using System.Reflection;
using NUnit.Framework;

namespace NMeter
{   
    [TestFixture]
    public class SampleCheckpoint : ScenarioFixture
    {
        public Scenario SampleCheckpointResults() {
            return new Scenario()
                .When("I create a checkpoint for NMeter.Sample", () => Checkpoint.For(typeof(SampleClass).Assembly))
                .Then("there's 1 class", checkpoint => Assert.That(checkpoint.ClassCount, Is.EqualTo(1)))
                .And("7 methods", checkpoint => Assert.That(checkpoint.MethodCount, Is.EqualTo(7)));
        }
    }
}
