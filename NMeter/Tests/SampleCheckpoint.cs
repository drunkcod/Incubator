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
                .When("I create a checkpoint for NMeter.Sample.dll", () => Checkpoint.For(typeof(SampleClass).Assembly))
                .Then("there's 2 classes", checkpoint => Assert.That(checkpoint.Classes.Count, Is.EqualTo(2)))
                .And("9 methods", checkpoint => Assert.That(checkpoint.Methods.Count, Is.EqualTo(9)))
                .And("1 of thoose methods are static", checkpoint => Assert.That(checkpoint.Methods.Count(x => x.IsStatic), Is.EqualTo(1)));
        }
    }
}
