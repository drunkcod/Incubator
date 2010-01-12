using System;
using System.Runtime.CompilerServices;

namespace NMeter.Sample
{
    public class SampleClass
    {
        public int Fib(int n){
            int a = 1, b = 1;
            for(int i = 0; i != n; ++i) {
                var t = a + b;
                a = b;
                b = t;
            }
            return a;
        }
        public int Fib2(int number) {
            var result = 1;
            var next = 1;
            for(int i = 0; i != number; ++i){
                var sum = result + next;
                result = next;
                next = sum;
            }
            return result;
        }
        public void SomeMethod() {
            var i = 42;
        }
        public void Nilad() { }
        public void Duad(int a, int b) { }
        public void EmptyMethod() { }

        [CompilerGenerated]
        public void CompilerGenerated() { }
    }
}
