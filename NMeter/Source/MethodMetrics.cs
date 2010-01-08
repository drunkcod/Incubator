using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Reflection;
using Pencil.Core;

namespace NMeter
{
    public class MethodMetrics
    {
        public byte[] Fingerprint;

        public static MethodMetrics For<T>(Expression<Action<T>> action) { return For((action.Body as MethodCallExpression).Method); }

        public static MethodMetrics For(MethodInfo method) {
            var bytes = new MemoryStream();
            var writer = new StreamWriter(bytes);
            foreach(var item in Disassembler.Decode(method)) {
                writer.WriteLine(item);
            }
            writer.Flush();
            bytes.Position = 0;
            using(var hash = MD5.Create())
                return new MethodMetrics { Fingerprint = hash.ComputeHash(bytes) };
        }
    }
}